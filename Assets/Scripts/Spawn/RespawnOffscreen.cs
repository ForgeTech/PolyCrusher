using UnityEngine;
using System.Collections;

/// <summary>
/// Respawns the gameobject with a NavMesh agent offscreen if it was too loung outside
/// of the camera view.
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
public class RespawnOffscreen : MonoBehaviour
{
    [SerializeField]
    [Tooltip("Maximum time to respawn (in seconds)")]
    private float maxTimeToRespawn = 8f;

    [SerializeField]
    [Tooltip("Minimum time to respawn (in seconds)")]
    private float minTimeToRespawn = 4f;

    [SerializeField]
    [Tooltip("Spawn distance from the center point of all players.")]
    private float spawnDistanceFromPlayers = 25f;
    
    [SerializeField]
    [Tooltip("Delta position for spawning with on NavMesh.")]
    private float spawnPositionDelta = 10f;

    private float timeToRespawn = 0f;
    private float currentTimeUntilRespawn = 0f;
    private NavMeshAgent navAgent = null;

    private Camera cam = null;

	void Start ()
    {
        Initialize();
	}
	
	void Update ()
    {
        if (cam == null)
            FetchCamerSystem();

        HandleViewportCheck();
	}

    private void Initialize()
    {
        navAgent = GetComponent<NavMeshAgent>();
        timeToRespawn = Random.Range(minTimeToRespawn, maxTimeToRespawn);
        FetchCamerSystem();
    }

    private void HandleViewportCheck()
    {
        if (isOffscreen())
            currentTimeUntilRespawn += Time.deltaTime;
        else
            currentTimeUntilRespawn = 0f;

        // Respawn and reset time.
        if (currentTimeUntilRespawn > timeToRespawn) {
            RespawnOutsideCameraView();
            currentTimeUntilRespawn = 0f;
        }
    }

    private void RespawnOutsideCameraView()
    {
        Vector3 playerMotion = CameraSystem.playerMotionVector;
        Vector3 spawnPosition = CameraSystem.playerBounds.center + (playerMotion.normalized * spawnDistanceFromPlayers);

        NavMeshHit hit;
        NavMesh.SamplePosition(spawnPosition, out hit, spawnPositionDelta, NavMesh.AllAreas);

        if (hit.hit)
            RespawnAt(hit.position);
    }

    private void RespawnAt(Vector3 position)
    {
        gameObject.SetActive(false);
        transform.position = position;
        gameObject.SetActive(true);

        // Enemy hack, since the OnEnable funtion is not called properly :/
        BaseEnemy enemy = GetComponent<BaseEnemy>();

        if (enemy != null)
        {
            enemy.EnemySpawnScaleTween();
            navAgent.speed = enemy.InitialMovementSpeed;
        }
    }

    /// <summary>
    /// Checks if the gameobject is in the camera viewport or offscreen.
    /// </summary>
    private bool isOffscreen()
    {
        Vector3 viewPortCoordinates = cam.WorldToViewportPoint(transform.position);
        bool isOffscreen = !(viewPortCoordinates.z > 0f
            && viewPortCoordinates.x > 0f && viewPortCoordinates.x < 1f
            && viewPortCoordinates.y > 0f && viewPortCoordinates.y < 1f);

        return isOffscreen;
    }

    /// <summary>
    /// Gets the camera component of the camera system.
    /// </summary>
    private void FetchCamerSystem()
    {
        CameraSystem camSys = GameObject.FindObjectOfType<CameraSystem>();

        if (camSys != null)
            cam = camSys.Cam;
    }
}