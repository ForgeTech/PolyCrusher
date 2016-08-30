using UnityEngine;

[System.Serializable]
public class PlayerInputHandler
{
    #region Internal members
    private Animator playerAnimator;
    private PlayerControlActions playerAction;
    private BasePlayer player;
    private Rigidbody playerRigidBody;
    #endregion

    #region Inspector members
    //The movement speed of the player.
    [SerializeField]
    public float movementSpeed = 20f;

    //Tolerance of the analog stick.
    [SerializeField]
    public float analogStickTolerance = 0.1f;

    //The rotation speed of the character.
    [SerializeField]
    public float rotationSpeed = 6f;

    //Determines if the right stick is used or not.
    [HideInInspector]
    public bool rightAnalogStickIsUsed = false;
    #endregion

    /// <summary>
    /// Gets if the player is moved by user input or not.
    /// </summary>
    public bool IsMoving
    {
        get
        {
            float leftStickHorizontal = playerAction.LeftHorizontal;
            float leftStickVertical = playerAction.LeftVertical;

            // Horizontal check
            if (leftStickHorizontal < analogStickTolerance && leftStickHorizontal > -analogStickTolerance)
            {
                // Verticals check
                if (leftStickVertical < analogStickTolerance && leftStickVertical > -analogStickTolerance)
                    return false;   // Player is not moving.
            }
            // Player is moving.
            return true;
        }
    }

    public PlayerInputHandler(BasePlayer player, Animator playerAnimator, PlayerControlActions playerAction)
    {
        this.playerAnimator = playerAnimator;
        this.playerAction = playerAction;
        this.player = player;
        this.playerRigidBody = player.GetComponent<Rigidbody>();
    }

    /// <summary>
    /// Handles when the object should move (for example when there is user input).
    /// </summary>
    public virtual void HandleMovement()
    {
        float leftStickHorizontal = playerAction.LeftHorizontal;
        float leftStickVertical = playerAction.LeftVertical;
        float verticalRotation = playerAction.RightVertical;
        float horizontalRotation = playerAction.RightHorizontal;

        // Set animator value
        float magnitude = new Vector2(leftStickHorizontal, leftStickVertical).magnitude;
        //playerAnimator.speed = magnitude;
        playerAnimator.SetFloat("MoveValue", magnitude);

        //==============Movement====================
        if (leftStickHorizontal > analogStickTolerance)
            ManipulateMovement(movementSpeed * Mathf.Abs(leftStickHorizontal), Vector3.right);
        else if (leftStickHorizontal < -analogStickTolerance)
            ManipulateMovement(movementSpeed * Mathf.Abs(leftStickHorizontal), -Vector3.right);

        if (leftStickVertical > analogStickTolerance)
            ManipulateMovement(movementSpeed * Mathf.Abs(leftStickVertical), -Vector3.forward);
        else if (leftStickVertical < -analogStickTolerance)
            ManipulateMovement(movementSpeed * Mathf.Abs(leftStickVertical), Vector3.forward);
        //==========================================

        //=============Rotation=====================
        if (verticalRotation > analogStickTolerance || verticalRotation < -analogStickTolerance || horizontalRotation > analogStickTolerance || horizontalRotation < -analogStickTolerance)
        {
            rightAnalogStickIsUsed = true;
            Vector3 angle = new Vector3(0, Mathf.Atan2(horizontalRotation, -verticalRotation) * Mathf.Rad2Deg, 0);
            player.transform.rotation = Quaternion.Lerp(player.transform.rotation, Quaternion.Euler(angle), Time.deltaTime * rotationSpeed);

            //Shoot when you rotate
            player.Shoot();

            playerAnimator.SetBool("Shoot", true);
        }
        else
        {
            playerAnimator.SetBool("Shoot", false);
        }

        if (!rightAnalogStickIsUsed)
        {
            //Check the analog stick tolerance
            if (leftStickHorizontal > analogStickTolerance || leftStickHorizontal < -analogStickTolerance
                || leftStickVertical > analogStickTolerance || leftStickVertical < -analogStickTolerance)
            {
                Vector3 angle = new Vector3(0, Mathf.Atan2(playerAction.LeftHorizontal, -playerAction.LeftVertical) * Mathf.Rad2Deg, 0);
                player.transform.rotation = Quaternion.Lerp(player.transform.rotation, Quaternion.Euler(angle), Time.deltaTime * rotationSpeed);
            }
        }

        //playerAnimator.speed = 1;
        rightAnalogStickIsUsed = false;
        //==========================================
    }

    /// <summary>
    /// Handles the actual movement with a speed in a certain direction.
    /// </summary>
    public virtual void ManipulateMovement(float speedFactor, Vector3 direction)
    {
        playerRigidBody.AddForce(direction * speedFactor);
    }

    /// <summary>
    /// Handles the input of the ability.
    /// </summary>
    public virtual void HandleAbilityInput()
    {
        // Player presses ability button.
        if (playerAction.Ability)
        {
            if (player.ability != null)
            {
                if (player.ability.UseIsAllowed && player.CheckEnergyLevel())
                {
                    playerAnimator.SetTrigger("Ability");
                    player.ability.Use();

                    //inputDevice.Vibrate(0.5f, 0.4f);

                    // Play ability sound
                    if (player.abilityCharacterSound != null)
                        player.abilityCharacterSound.PlayRandomClip();

                    // save ability event
                    new Event(Event.TYPE.ability).addPos(player.transform).addCharacter(player.PlayerIdentifier.ToString("g")).addWave().send();
                }
            }
        }
    }
}
