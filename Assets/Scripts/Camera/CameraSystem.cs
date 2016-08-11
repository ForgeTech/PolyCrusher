using UnityEngine;
using System.Collections;

/// <summary>
/// This script uses a camera proxy gameobject and a Camera gameobject as child.
/// The script must be attached to the proxy gameobject!
/// 
/// Regulates the smooth following behaviour and tracks all players with a bounding
/// box. The camera changes the height and the angle dynamically, depending on the
/// player position.
/// </summary>
/// 
//[ExecuteInEditMode]
public class CameraSystem : MonoBehaviour 
{
    [Header("Camera movement settings")]
    //Player array for the bounding box calculation.
    protected GameObject[] players;

    //The whole bounding box of all players
    public static Bounds playerBounds;

    // Motion vector of the player walking direction
    public static Vector3 playerMotionVector = Vector3.forward;
    private Vector3 lastPlayerCenterPosition = Vector3.zero;

    [SerializeField]
    protected float motionVectorAdaptionSpeed = 12f;

    //Velocity
    private Vector3 velocity = Vector3.zero;

    //The camera damping factor.
    [SerializeField]
    protected float cameraDamping = 1f;

    //Max speed of the camera
    [SerializeField]
    protected float maxSpeed = 1f;

    //The height of the camera.
    [SerializeField]
    protected float cameraHeight = 35f;

    //The angle of the camera.
    [SerializeField]
    protected float cameraAngle = 315f;

    //Min camera height.
    [SerializeField]
    protected float minCameraHeight = 35f;

    //Max camera height.
    [SerializeField]
    protected float maxCameraHeight = 42f;

    //Minimum camera angle.
    [SerializeField]
    protected float minCameraAngle = 300f;

    //Maximum camera angle.
    [SerializeField]
    protected float maxCameraAngle = 330f;

    //Min square size of the player bounding box (for angle and height calculation)
    [SerializeField]
    protected float minSquareSize = 3.6f;

    //Max square size of the player bounding box (for angle and height calculation)
    [SerializeField]
    protected float maxSquareSize = 80f;

    //Speed for the rotation and height change.
    [SerializeField]
    protected float cameraAdaptSpeed = 6f;

    //Adapt velocity
    private float cameraAdaptVelocityRotation = 0f;
    private float cameraAdaptVelocityHeight = 0f;

    //Camera adapt damping time.
    [SerializeField]
    protected float cameraAdaptDampingTime = 1f;

    [SerializeField]
    protected Vector2 middlePointOffset = Vector2.zero;

    //Variables for the custom delta time calculation.
    float lastFrame = 0f;
    float currentFrame = 0f;
    float myDelta = 0f;
    float avg = 0f;
    //=================================================

    // Reference of the child camera object
    private Camera cam;
    
    /// <summary>
    /// Gets the cam of the camera system.
    /// </summary>
    public Camera Cam
    {
        get { return this.cam; }
    }

    [Space(5)]
    [Header("Camera Shake settings")]
    [Tooltip("Time of the shake.")]
    public float shakeTime = 0.6f;

    [Tooltip("Specifies how much the camera should shake.")]
    public float shakeStrength = 6f;

    [Tooltip("Shake damping of the camera shake.")]
    public float shakeDamping = 0.3f;

    private float shakeX = 0;
    private float shakeZ = 0;

    void Awake()
    {
        // Register "FindAllPlayers" to the player join delegate.
        PlayerManager.PlayerJoinedEventHandler += FindAllPlayers;
        BasePlayer.PlayerDied += FindAllPlayers;
        BasePlayer.PlayerSpawned += FindAllPlayers;
    }

	// Use this for initialization
	void Start () 
    {
        GetComponent<MeshRenderer>().enabled = false;

        FindChildCameraObject();

        //Find all players.
        FindAllPlayers();

        playerBounds = new Bounds();
        
        if (players.Length == 0)
            players = null;
	}
	
	/// <summary>
	/// Delta time and bounding box calculation.
	/// </summary>
	void Update () 
    {
        //Calc delta time
        CalculateDeltaTime();

        if (players != null)
        {
            CalculatePlayerBoundingBox();
            CalculatePlayerMotionVector();
        }
	}

    /// <summary>
    /// Camera movement.
    /// </summary>
    void LateUpdate()
    {
        if (players != null)
            MoveCamera(); 

        SetCameraValues();
    }

    /*/// <summary>
    /// Testing
    /// </summary>
    void OnGUI()
    {
        if (GUI.Button(new Rect(20, 40, 80, 20), "Shake"))
            ShakeOnce();
    }*/

    /// <summary>
    /// Shakes the camera once based on the shake values.
    /// </summary>
    public void ShakeOnce()
    {
        StartCoroutine(Shake());
    }

    /// <summary>
    /// The actual Shake as IEnumerator.
    /// </summary>
    /// <returns></returns>
    protected IEnumerator Shake()
    {
        float elapsedTime = 0f;

        while (elapsedTime < shakeTime)
        {
            elapsedTime += Time.deltaTime;

            // Current position of the shake in percent.
            float completed = elapsedTime / shakeTime;
            float shakeDamper = 1f - Mathf.Clamp(shakeDamping * completed - (shakeDamping - 1f), 0f, 1f);

            // Map the random value between -1 and +1
            shakeX = Random.value * 2f - 1f;
            shakeZ = Random.value * 2f - 1f;

            // Apply strength and damping.
            shakeX *= shakeStrength * shakeDamper;
            shakeZ *= shakeStrength * shakeDamper;

            yield return null;
        }

        shakeX = 0f;
        shakeZ = 0f;
    }

    private void CalculatePlayerMotionVector()
    {
        playerMotionVector = Vector3.RotateTowards(playerMotionVector, playerBounds.center - lastPlayerCenterPosition, 
            Time.deltaTime * motionVectorAdaptionSpeed, 0f);

        playerMotionVector.Normalize();
        playerMotionVector.Set(playerMotionVector.x, 0f, playerMotionVector.z);

        lastPlayerCenterPosition = playerBounds.center;
    }

    /// <summary>
    /// Follow the center of the bounding box.
    /// </summary>
    private void MoveCamera()
    {
        avg = (Time.deltaTime + Time.smoothDeltaTime + myDelta) * 0.3333333f;
        Vector3 newPosition = new Vector3(playerBounds.center.x + middlePointOffset.x,
            transform.position.y,
            playerBounds.center.z + middlePointOffset.y);

        transform.position = Vector3.SmoothDamp(transform.position, newPosition, ref velocity, cameraDamping, maxSpeed, avg);
    }

    /// <summary>
    /// Sets the rotation and the height of the camera.
    /// </summary>
    private void SetCameraValues()
    {
        // Square size of the player bounding box.
        float squareSize = Mathf.Abs(playerBounds.max.x - playerBounds.min.x) * Mathf.Abs(playerBounds.max.z - playerBounds.min.z);
        float clampValue = ScaleClamp(squareSize, minSquareSize, maxSquareSize);
        float newAngle;
        float newHeight;

        //========Calculate height based on player position=====================================
        //cam.transform.localPosition = new Vector3(0f, cameraHeight, 0f);
        newHeight = Mathf.Lerp(minCameraHeight, maxCameraHeight, clampValue);
        float calcValue = Mathf.SmoothDamp(cam.transform.localPosition.y, newHeight, ref cameraAdaptVelocityHeight, cameraAdaptDampingTime, cameraAdaptSpeed, avg);
        cam.transform.localPosition = new Vector3(shakeX, calcValue, shakeZ);
        //======================================================================================

        //========Calculate rotation based on player position===================================
        newAngle = Mathf.Lerp(maxCameraAngle, minCameraAngle, clampValue);
        transform.rotation = Quaternion.Euler(Mathf.SmoothDamp(transform.rotation.eulerAngles.x, newAngle, ref cameraAdaptVelocityRotation, cameraAdaptDampingTime, cameraAdaptSpeed, avg), transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z);
        //======================================================================================

        //Debug.Log("Height: " + newHeight + "CalcValue: " + calcValue);
        //Debug.Log("CameraSystem: SquareSize: " + squareSize + ", ClampValue: " + clampValue);
        
    }

    /// <summary>
    /// Calculate bounding box over all players.
    /// </summary>
    private void CalculatePlayerBoundingBox()
    {
        if (players.Length > 1)
        {
            float smallestX = players[0].transform.position.x;
            float smallestZ = players[0].transform.position.z;

            float biggestX = players[0].transform.position.x;
            float biggestZ = players[0].transform.position.z;

            for (int i = 0; i < players.Length; i++)
            {
                if (players[i].transform.position.x < smallestX)
                    smallestX = players[i].transform.position.x;

                if (players[i].transform.position.z < smallestZ)
                    smallestZ = players[i].transform.position.z;

                if (players[i].transform.position.x > biggestX)
                    biggestX = players[i].transform.position.x;

                if (players[i].transform.position.z > biggestZ)
                    biggestZ = players[i].transform.position.z;

                playerBounds.SetMinMax(new Vector3(smallestX, 0.5f, biggestZ), new Vector3(biggestX, 0.5f, smallestZ));
            }
            //Debug.Log("SmallestX: " + smallestX + ", BiggestX: " + biggestX);
        }
        else if (players.Length != 0)
        {
                playerBounds.SetMinMax(players[0].transform.position, players[0].transform.position);
        }
    }

    /// <summary>
    /// Calculate custom delta time
    /// </summary>
    private void CalculateDeltaTime()
    {
        currentFrame = Time.realtimeSinceStartup;
        myDelta = currentFrame - lastFrame;
        lastFrame = currentFrame;
    }

    /// <summary>
    /// Searches for the camera object in the child objects of the proxy gameobject.
    /// </summary>
    private void FindChildCameraObject()
    {
        // Loop through the children.
        foreach (Transform t in this.transform)
        {
            if (t.tag == "MainCamera")
                cam = t.gameObject.GetComponent<Camera>();
        }
    }

    /// <summary>
    /// Finds all players.
    /// </summary>
    /// <param name="player"></param>
    protected void FindAllPlayers(BasePlayer player)
    {
        FindAllPlayers();
    }

    /// <summary>
    /// Finds all players.
    /// </summary>
    protected void FindAllPlayers()
    {
        //Find all players.
        players = GameObject.FindGameObjectsWithTag("Player");

        Debug.Log("SimpleCamera: FindAllObjects()");
    }

    /// <summary>
    /// Debug draw the player bounding box.
    /// </summary>
    void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(playerBounds.center, playerBounds.size);
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(playerBounds.center + new Vector3(0, 1f, 0), playerBounds.center + playerMotionVector * 4f);
    }

    /// <summary>
    /// Clamps a value between min and max and scales the value between 0 and 1.
    /// </summary>
    /// <param name="value">The value which should be clamped.</param>
    /// <param name="min">Min clamp value.</param>
    /// <param name="max">Max clamp value</param>
    /// <returns>Value between 0 and 1.</returns>
    private float ScaleClamp(float value, float min, float max)
    {
        return Mathf.Clamp01((value - min) / (max - min));
    }
}
