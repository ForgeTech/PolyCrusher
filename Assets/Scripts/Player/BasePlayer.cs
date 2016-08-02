using UnityEngine;
using System.Collections;

/// <summary>
/// Eventhandler for player deaths.
/// </summary>
public delegate void PlayerDiedEventHandler();

/// <summary>
/// Eventhandler for player spawns.
/// </summary>
/// <param name="player">The reference of the player.</param>
public delegate void PlayerSpawnedEventHandler(BasePlayer player);

/// <summary>
/// Eventhandler for the player ability.
/// When the ability is useable, this event will be fired.
/// </summary>
/// <param name="player"></param>
public delegate void AbilityUseableEventHandler(BasePlayer player);

/// <summary>
/// The base form of the player script.
/// </summary>
public class BasePlayer : MonoBehaviour, IAttackable, IMoveable, IDamageable
{
    #region Class Members

    [Header("Health Values")]
    //The actual health value.
    [SerializeField]
    protected int health = 100;
    
    //The minimum health
    [SerializeField]
    protected int minHealth = 0;

    //The maximum health
    [SerializeField]
    protected int maxHealth = 100;

    // Specifies if the player is invincible.
    protected bool invincible = false;

    [Header("Energy Values")]
    //The maximum energy
    [SerializeField]
    protected int energy = 0;

    // The minimum energy
    [SerializeField]
    protected int minEnergy = 0;

    // The maximum energy
    [SerializeField]
    protected int maxEnergy = 100;

    //The refill rate of the energy in seconds.
    [SerializeField]
    protected float energyRefillRate = 0.1f;

    // Specifies if the refill is allowed; Is used with the refill rate.
    protected bool allowEnergyRefill = true;

    // Stops the energy refill if it is true; Is used in combination with the LineSystem.
    protected bool stopEnergyRefill = true;

    // The value which will be added to the actual energy.
    [SerializeField]
    protected int energyIncrement = 5;

    [Space(10)]
    [Header("Misc")]
    //Player name
    [SerializeField]
    protected string playerName;
    [System.Obsolete("Use 'PlayerIdentifier' instead.")]
    public string PlayerName{
        get { return playerName; }
    }

    [SerializeField]
    protected PlayerEnum playerIdentifier = PlayerEnum.Birdman;
    public PlayerEnum PlayerIdentifier {
        get { return this.playerIdentifier; }
    }


    //Player prefix for the controller input.
    [SerializeField]
    protected string playerPrefix = "P1_";

    // Initial death time.
    [Tooltip("The time the player stays on screen after he dies.")]
    [SerializeField]
    protected float deathTime = 3.3f;

    // The actual death time which will be used.
    protected float currentDeathTime;

	//Phone player slot for the mobile controller input.
	//[SerializeField]
	protected int phonePlayerSlot = -1;

	//Network script for the mobile controller input.
	//[SerializeField]
	protected PlayerNetCommunicate network;


    [Space(10)]
    [Header("Movement")]
    //The movement speed of the player.
    [SerializeField]
    protected float movementSpeed;

    //Tolerance of the analog stick.
    [SerializeField]
    private float analogStickTolerance = 0.2f;
    
    //The rotation speed of the character.
    [SerializeField]
    protected float rotationSpeed = 1.7f;

    //Determines if the right stick is used or not.
    protected bool rightAnalogStickIsUsed = false;

    // Determines if the player is dead or not.
    protected bool isDead;

    //======================================
    [Space(10)]
    [Header("Weapon & Ability References")]
    // Weapon of the player.
    [SerializeField]
    protected Weapon weapon;
    
    // Ability of the player.
    [SerializeField]
    protected Ability ability;
    //======================================



    //=========HUD Elements=================
    [Space(10)]
    [Header("HUD Element References")]
    [SerializeField]
    protected UnityEngine.UI.Image energyLevel;

    [SerializeField]
    protected UnityEngine.UI.Image healthLevel;

    [SerializeField]
    protected UnityEngine.UI.Image innerEnergyCircle;

    // The original health level scale.
    private Vector3 originalHealthLevelScale;
    //======================================


    [Space(10)]
    [Header("Camera Edge detection")]
    [Tooltip("The offset value of the edge detection. (Screen space value!)")]
    [SerializeField]
    protected float cameraEdgeDetectionOffset = 0.02f;


    [Space(10)]
    [Header("Blood particle prefab")]
    [Tooltip("Prefab of the blood particles.")]
    [SerializeField]
    protected GameObject bloodParticles;

    // Specifies if the object can shoot or not.
    protected bool canShoot;

	//Says if ability was used or not
	protected bool abilityUseable = true;

    // Cam limiter
    protected LimitToCameraFrustum camLimiter;

    //Reference to the animator.
    protected Animator playerAnimator;

    //===============Sounds====================
    [Space(10)]
    [Header("Character sounds")]
    [SerializeField]
    protected GameObject characterVoice;

    protected MultipleAudioclips abilityCharacterSound;
    protected MultipleAudioclips hurtCharacterSound;
    protected MultipleAudioclips deathCharacterSound;
    protected MultipleAudioclips spawnCharacterSound;
    //=========================================

    [Space(10)]
    [Header("Special particles")]
    [SerializeField]
    protected GameObject respawnParticles;

    //Event handler for player deaths.
    public static event PlayerDiedEventHandler PlayerDied;

    //Event handler for player spawn.
    public static event PlayerSpawnedEventHandler PlayerSpawned;

    // Event handler for the use of the ability.
    public static event AbilityUseableEventHandler AbilityUseable;
    #endregion


    #region Properties
    /// <summary>
    /// Gets or sets the actual health value.
    /// </summary>
    public int Health
    {
        get
        {
            return this.health;
        }
        set
        {
            if (value >= minHealth && value <= maxHealth)
                this.health = value;

            if (value > maxHealth)
                this.health = maxHealth;

            if (value <= 0)
            {
                this.health = minHealth;
                KillPlayer();
            }
        }
    }

    /// <summary>
    /// Gets or sets the max health.
    /// The max health must be greater than 0 and greater than the min health.
    /// </summary>
    public int MaxHealth
    {
        get { return this.maxHealth; }
        set
        {
            if(value > this.minHealth && value > 0)
                this.maxHealth = value;
        }
    }

    /// <summary>
    /// Gets or sets the max energy.
    /// The max energy must be greater than 0 and greater than the min energy.
    /// </summary>
    public int MaxEnergy
    {
        get { return this.maxEnergy; }
        set
        {
            if (value > this.minEnergy && value > 0)
                this.maxEnergy = value;
        }
    }

    /// <summary>
    /// Sets or gets the invincibility.
    /// </summary>
    public bool Invincible
    {
        get
        {
            return this.invincible;
        }
        set
        {
            this.invincible = value;
        }
    }

    /// <summary>
    /// Gets if the player is dead.
    /// </summary>
    public bool IsDead
    {
        get { return this.isDead; }
    }

    /// <summary>
    /// Specifies if the object can shoot or not.
    /// </summary>
    public bool CanShoot
    {
        get
        {
            return this.canShoot;
        }
        set
        {
            this.canShoot = value;
        }
    }

    /// <summary>
    /// Gets the player weapon.
    /// </summary>
    public Weapon PlayerWeapon
    {
        get
        {
            return this.weapon;
        }
    }

    /// <summary>
    /// Gets or sets the current death time.
    /// </summary>
    public float CurrentDeathTime
    {
        get { return this.currentDeathTime; }
        set { this.currentDeathTime = value; }
    }

    /// <summary>
    /// Gets or sets the player prefix.
    /// </summary>
    public string PlayerPrefix
    {
        get { return this.playerPrefix; }
        set
        {
            this.playerPrefix = value;
        }
    }

	/// <summary>
	/// Gets or sets the phone player slot.
	/// </summary>
	public int PhonePlayerSlot
	{
		get { return this.phonePlayerSlot; }
		set
		{
			this.phonePlayerSlot = value;
		}
	}

    /// <summary>
    /// Gets or sets the energy.
    /// </summary>
    public int Energy
    {
        get { return this.energy; }
        set
        {
            if (value >= minEnergy && value <= maxEnergy)
                this.energy = value;

            if (value > maxEnergy)
                this.energy = maxEnergy;
        }
    }

    /// <summary>
    /// Gets the energy refill rate.
    /// </summary>
    public float EnergyRefillRate
    {
        get { return this.energyRefillRate; }
    }

    /// <summary>
    /// Gets or sets the energy increment value.
    /// </summary>
    public int EnergyIncrement
    {
        get { return this.energyIncrement; }
        set { this.energyIncrement = value; }
    }

    /// <summary>
    /// Gets or sets the Movement speed.
    /// </summary>
    public float MovementSpeed
    {
        get { return this.movementSpeed; }
        set { this.movementSpeed = value; }
    }

    /// <summary>
    /// Gets if the player is moved by user input or not.
    /// </summary>
    public bool IsMoving
    {
        get 
        {
            // -1 -> Player is controlled by a controller
            if (PhonePlayerSlot == -1)
            {
                float leftStickHorizontal = Input.GetAxis(playerPrefix + "Horizontal");
                float leftStickVertical = Input.GetAxis(playerPrefix + "Vertical");

                // Horizontal check
                if (leftStickHorizontal < analogStickTolerance && leftStickHorizontal > -analogStickTolerance)
                {
                    // Verticals check
                    if (leftStickVertical < analogStickTolerance && leftStickVertical > -analogStickTolerance)
                        return false;   // Player is not moving.
                }
            }
            else  // Player is controlled by phone.
            {
                float leftStickHorizontal = network.horizontal[phonePlayerSlot];
                float leftStickVertical = network.vertical[phonePlayerSlot];

                // Horizontal check
                if (leftStickHorizontal < analogStickTolerance && leftStickHorizontal > -analogStickTolerance)
                {
                    // Verticals check
                    if (leftStickVertical < analogStickTolerance && leftStickVertical > -analogStickTolerance)
                        return false;   // Player is not moving.
                }
            }

            // Player is moving.
            return true;
        }
    }

    /// <summary>
    /// Gets or sets the energy refill parameter.
    /// </summary>
    public bool StopEnergyRefill
    {
        get { return this.stopEnergyRefill; }
        set { this.stopEnergyRefill = value; }
    }
    #endregion


    #region Methods
    void Awake()
    {
        LevelEndManager.levelExitEvent += ResetValues;
    }

    // Use this for initialization
	void Start () 
    {
        // Set the gameobject name.
        gameObject.name = this.playerName;

        // Set dead to default (false).
        this.isDead = false;

        // Set death time
        this.currentDeathTime = deathTime;

        // Init character sounds.
        InitializeCharacterSounds();

        // Get an animator reference.
        playerAnimator = GetComponent<Animator>();

        // Init the cam limiter.
        camLimiter = new LimitToCameraFrustum(this, cameraEdgeDetectionOffset);

        // Init network.
		network = GameObject.FindObjectOfType<PlayerNetCommunicate>();

        // Set the owner of the weapon and the ability.
        if (weapon != null)
            weapon.OwnerScript = this;

        if (ability != null)
            ability.OwnerScript = this;

        //Set original health level.
        originalHealthLevelScale = healthLevel.transform.localScale;

        Debug.Log(playerPrefix + "Player added!");

        // Fire spawn event.
        OnPlayerSpawn();
	}

    void OnEnable()
    {
        this.CurrentDeathTime = deathTime;
        this.allowEnergyRefill = true;
        this.invincible = false;

        Rigidbody rigid = GetComponent<Rigidbody>();
        if (rigid != null)
            rigid.mass = 0.2f;
}

        // Update is called once per frame
    void Update () 
    {
        if (!IsDead)
        {
            // Refill Energy
            RefillEnergy(energyIncrement);

			if (phonePlayerSlot == -1) {
				HandleAbilityInput();
			} else {
				HandleAbilityInputPhone();
			}

        }

        // HUD Elements
        UpdateHUDElements();
	}

    void FixedUpdate()
    {
        if (!IsDead) {
			HandleInput();
			CheckAbilityStatusReady();
		}

        // Check camera boundings
        camLimiter.CheckCameraBounding(transform.position);
    }

	/// <summary>
	/// Checks if ability is ready to use.
	/// </summary>
	void CheckAbilityStatusReady() {
		if (!abilityUseable && phonePlayerSlot != -1 && CheckEnergyLevelWithoutSubtraction ()) {
			abilityUseable = true;
			network.sendData("1002", phonePlayerSlot);
		}
	}

    /// <summary>
    /// Handles the player input.
    /// </summary>
    protected virtual void HandleInput()
    {
        //Handles the movement and rotation/shoot input.
        //playerAnimator.speed = 1f;

		if (phonePlayerSlot == -1) {
			HandleMovement ();
		} else {
			HandlePhoneMovement();
		}

    }

    /// <summary>
    /// Handles the shoot mechanism.
    /// </summary>
    public virtual void Shoot()
    {
        if (weapon != null)
        {
            weapon.Use();
            //StartCoroutine(WaitForAnimator("Shoot"));
        }
    }

    /// <summary>
    /// Handles the close quarter attack (if neccesary).
    /// </summary>
    public virtual void Attack()
    {
        
    }

    /// <summary>
    /// Handles the input of the ability.
    /// </summary>
    public virtual void HandleAbilityInput()
    {
        // Player presses ability button.
        if (Input.GetButtonDown(playerPrefix + "Ability"))
        {
            if ( ability != null)
            {
                if (ability.UseIsAllowed && CheckEnergyLevel())
                {
                    playerAnimator.SetTrigger("Ability");
                    ability.Use();

                    // Play ability sound
                    if (abilityCharacterSound != null)
                        abilityCharacterSound.PlayRandomClip();

                    // save ability event
                    new Event(Event.TYPE.ability).addPos(this.transform).addCharacter(playerName).addWave().send();
                }
            }
        }
    }

    /// <summary>
    /// Handles when the object should move (for example when there is user input).
    /// </summary>
    public virtual void HandleMovement()
    {
        float leftStickHorizontal = Input.GetAxis(playerPrefix + "Horizontal");
        float leftStickVertical = Input.GetAxis(playerPrefix + "Vertical");
        float verticalRotation = Input.GetAxis(playerPrefix + "VerticalRotation");
        float horizontalRotation = Input.GetAxis(playerPrefix + "HorizontalRotation");

        // Set animator value
        float magnitude = new Vector2(leftStickHorizontal, leftStickVertical).magnitude;
        //playerAnimator.speed = magnitude;
        playerAnimator.SetFloat("MoveValue", magnitude);

        //Debug.Log("LeftStickHorizontal: " + leftStickHorizontal + ", LeftStickVertical: " + leftStickVertical);

        //Reverse animation if neccessary.
        /*if (leftStickHorizontal < 0 && horizontalRotation > 0 || horizontalRotation < 0 && leftStickHorizontal > 0)
            playerAnimator.speed = -1;
        else if (leftStickVertical < 0 && verticalRotation > 0 || leftStickVertical > 0 && verticalRotation < 0)
            playerAnimator.speed = -1;
        else
            playerAnimator.speed = 1;*/

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
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(angle), Time.deltaTime * rotationSpeed);

            //Shoot when you rotate
            Shoot();

            playerAnimator.SetBool("Shoot", true);
        }
        else
        {
            playerAnimator.SetBool("Shoot", false);
            //Debug.Log("BasePlayer: False");
        }

        if (!rightAnalogStickIsUsed)
        {
            //Check the analog stick tolerance
            if (leftStickHorizontal > analogStickTolerance || leftStickHorizontal < -analogStickTolerance
                || leftStickVertical > analogStickTolerance || leftStickVertical < -analogStickTolerance)
            {
                Vector3 angle = new Vector3(0, Mathf.Atan2(Input.GetAxis(playerPrefix + "Horizontal"), -Input.GetAxis(playerPrefix + "Vertical")) * Mathf.Rad2Deg, 0);
                transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(angle), Time.deltaTime * rotationSpeed);
            }
        }

        //playerAnimator.speed = 1;
        rightAnalogStickIsUsed = false;
        //==========================================
    }

	/// <summary>
	/// Handles the input of the ability for phone.
	/// </summary>
	public virtual void HandleAbilityInputPhone()
	{
		// Player presses ability button.
		if (network.actionButton[phonePlayerSlot] == 1)
		{
			if ( ability != null && CheckEnergyLevel() )
			{
				if (ability.UseIsAllowed)
				{
					network.sendData("1000", phonePlayerSlot);
					playerAnimator.SetTrigger("Ability");
					ability.Use();
				}
			} else {
				abilityUseable = false;
				network.sendData("1001", phonePlayerSlot);
			}

			network.actionButton[phonePlayerSlot] = 0;
		}
	}

	/// <summary>
	/// Handles when the object should move (for example when there is user input) for phones.
	/// </summary>
	public virtual void HandlePhoneMovement()
	{
		float leftStickHorizontal = network.horizontal[phonePlayerSlot];
		float leftStickVertical = network.vertical[phonePlayerSlot];

		// Set animator value
		float magnitude = new Vector2(leftStickHorizontal, leftStickVertical).magnitude;
		//playerAnimator.speed = magnitude;
		playerAnimator.SetFloat("MoveValue", magnitude);
		
		//Debug.Log("LeftStickHorizontal: " + leftStickHorizontal + ", LeftStickVertical: " + leftStickVertical);
		
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
		if (network.verticalRotation[phonePlayerSlot] > analogStickTolerance || network.verticalRotation[phonePlayerSlot] < -analogStickTolerance || network.horizontalRotation[phonePlayerSlot] > analogStickTolerance || network.horizontalRotation[phonePlayerSlot] < -analogStickTolerance)
		{
			rightAnalogStickIsUsed = true;
			Vector3 angle = new Vector3(0, Mathf.Atan2(network.horizontalRotation[phonePlayerSlot] , -network.verticalRotation[phonePlayerSlot] ) * Mathf.Rad2Deg, 0);
			transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(angle), Time.deltaTime * rotationSpeed);
			
			//Shoot when you rotate
			Shoot();
			
			playerAnimator.SetBool("Shoot", true);
		}
		else
		{
			playerAnimator.SetBool("Shoot", false);
			//Debug.Log("BasePlayer: False");
		}
		
		if (!rightAnalogStickIsUsed)
		{
			//Check the analog stick tolerance
			if (leftStickHorizontal > analogStickTolerance || leftStickHorizontal < -analogStickTolerance
			    || leftStickVertical > analogStickTolerance || leftStickVertical < -analogStickTolerance)
			{
				Vector3 angle = new Vector3(0, Mathf.Atan2(network.horizontal[phonePlayerSlot], -network.vertical[phonePlayerSlot]) * Mathf.Rad2Deg, 0);
				transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(angle), Time.deltaTime * rotationSpeed);
			}
		}
		
		//playerAnimator.speed = 1;
		rightAnalogStickIsUsed = false;
		//==========================================
	}

    /// <summary>
    /// Handles the actual movement with a speed in a certain direction.
    /// </summary>
    /// <param name="speedFactor">Speed of the movement.</param>
    /// <param name="direction">Direction.</param>
    public virtual void ManipulateMovement(float speedFactor, Vector3 direction)
    {
        //Debug.Log("SpeedFactor: " + speedFactor + ", Direction: " + direction);
        GetComponent<Rigidbody>().AddForce(direction * speedFactor);
    }

    /// <summary>
    /// Draws the object some damage and lowers the health.
    /// </summary>
    /// <param name="damage">The damage dealt.</param>
    public virtual void TakeDamage(int damage, MonoBehaviour damageDealer)
    {
        // Only take damage if the player is not invincible.
        if (!Invincible)
        {
            // Health Level tween.
            Vector3 originalScale = originalHealthLevelScale;
            healthLevel.transform.localScale = Vector3.zero;
            StartCoroutine(healthLevel.transform.ScaleTo(originalScale, 0.5f, AnimCurveContainer.AnimCurve.pingPong.Evaluate));

            // send event if player will be dead
            if (Health - damage < 0 && !IsDead)
            {
                string enemyName = "undefined";

                if (damageDealer is BaseEnemy)
                {
                    enemyName = ((BaseEnemy)damageDealer).EnemyName;
                }

                new Event(Event.TYPE.death).addPos(this.transform).addCharacter(PlayerIdentifier.ToString("g")).addWave().addEnemy(enemyName).addLevel().addPlayerCount().send();
            }

            // Instantiate particles if the prefab reference isn't null.
            if (bloodParticles != null)
            {
                GameObject particle = Instantiate(bloodParticles);
                particle.transform.position = transform.position;
            }

            //Play sound only if the player does not die.
            if (hurtCharacterSound != null && this.Health - damage > 0)
                hurtCharacterSound.PlayRandomClip();
            
            this.Health -= damage;
        }
    }

    /// <summary>
    /// Lowers the health to zero.
    /// </summary>
    public virtual void InstantKill()
    {
        this.Health = minHealth;
    }

    /// <summary>
    /// Refills the energy with the given parameter.
    /// </summary>
    /// <param name="addEnergy">Energy which should be added.</param>
    public void RefillEnergy(int addEnergy)
    {
        if (allowEnergyRefill && !StopEnergyRefill)
        {
            // Check if the ability can be used and trigger a tween.
            if(Energy < ability.EnergyCost && (Energy + addEnergy) >= ability.EnergyCost)
            {
                Vector3 originalScale = energyLevel.transform.localScale;
                energyLevel.transform.localScale = Vector3.zero;

                StartCoroutine(energyLevel.transform.ScaleTo(originalScale, 0.4f, AnimCurveContainer.AnimCurve.upscale.Evaluate));
            }

            Energy += addEnergy;

            allowEnergyRefill = false;
            StartCoroutine(WaitForEnergyRefill());
        }
    }

    /// <summary>
    /// Waits before the refill is allowed.
    /// </summary>
    /// <returns></returns>
    protected IEnumerator WaitForEnergyRefill()
    {
        yield return new WaitForSeconds(EnergyRefillRate);
        allowEnergyRefill = true;
    }

    /// <summary>
    /// Sets the boolean param of the player animator to true and after that to false again.
    /// </summary>
    /// <param name="animParam">Animator parameter.</param>
    /// <returns></returns>
    protected IEnumerator WaitForAnimator(string animParam)
    {
        playerAnimator.SetBool(animParam, true);
        yield return null;
        playerAnimator.SetBool(animParam, false);
    }


    /// <summary>
    /// Kills the player.
    /// </summary>
    protected virtual void KillPlayer()
    {
        if(!playerAnimator.GetBool("Dead"))
            playerAnimator.SetBool("Dead", true);

		//Let mobile know you died
		if (phonePlayerSlot != -1 && !isDead) {
			network.sendData("1003", phonePlayerSlot);
		}

        // Set RigidBody to isKinematic. (So the enemies does not move the player after death).
        GetComponent<Rigidbody>().isKinematic = true;

        // Set death boolean.
        this.isDead = true;

        // Play death sound.
        if (deathCharacterSound != null)
            deathCharacterSound.PlayRandomClip();
        
        // Start disabling routine
        StartCoroutine(WaitForDisable(CurrentDeathTime));

        // Set invincible to false
        if (Invincible)
            Invincible = false;
    }

    /// <summary>
    /// Revives the player and sets variables.
    /// </summary>
    public virtual void RevivePlayer()
    {
        this.isDead = false;

        // Set isKinematic to false.
        GetComponent<Rigidbody>().isKinematic = false;

        // Trigger player spawn event.
        OnPlayerSpawn();

        // Play character spawn sound.
        if (spawnCharacterSound != null)
            spawnCharacterSound.PlayRandomClip();

        Health = maxHealth;
        playerAnimator.SetBool("Dead", false);

        // Respawn particles
        if (respawnParticles != null)
            Instantiate(respawnParticles, transform.position, transform.rotation);
    }

    /// <summary>
    /// Event method for player deaths.
    /// </summary>
    protected virtual void OnPlayerDeath()
    {
        if (PlayerDied != null)
            PlayerDied();
    }

    /// <summary>
    /// Event method for player spawn.
    /// </summary>
    protected virtual void OnPlayerSpawn()
    {
        if (PlayerSpawned != null)
        {
            PlayerSpawned(this);
            Debug.Log("Player Spawned!!!");
        }
    }

    /// <summary>
    /// Event method for the ability is useable event handler.
    /// </summary>
    protected virtual void OnAbilityUseable()
    {
        if (AbilityUseable != null)
            AbilityUseable(this);
    }

    /// <summary>
    /// Waits before the player will be disabled.
    /// </summary>
    /// <param name="time">Time before disable.</param>
    /// <returns></returns>
    protected IEnumerator WaitForDisable(float time)
    {
        yield return new WaitForSeconds(time);
        DisablePlayer();
    }

    /// <summary>
    /// Disables the player.
    /// </summary>
    protected void DisablePlayer()
    {
        ability.UseIsAllowed = true;

        gameObject.SetActive(false);
        OnPlayerDeath();
        StopAllCoroutines();
    }

    /// <summary>
    /// Checks if the ability can be used based on the actual energy level.
    /// </summary>
    /// <returns>True: Ability can be used. False: Ability can't be used.</returns>
    protected bool CheckEnergyLevel()
    {
        if ((Energy - ability.EnergyCost) < minEnergy)
            return false;
        else
        {
            // Subtract energy.
            Energy -= ability.EnergyCost;
            return true;
        }
    }

    /// <summary>
    /// Initializes the character sounds.
    /// </summary>
    protected void InitializeCharacterSounds()
    {
        if (characterVoice != null)
        {
            MultipleAudioclips[] clips = characterVoice.GetComponents<MultipleAudioclips>();

            for (int i = 0; i < clips.Length; i++)
            {
                if (clips[i].AudioCategory == "Ability")
                    abilityCharacterSound = clips[i];
                if (clips[i].AudioCategory == "Death")
                    deathCharacterSound = clips[i];
                if (clips[i].AudioCategory == "Hurt")
                    hurtCharacterSound = clips[i];
                if (clips[i].AudioCategory == "Spawn")
                    spawnCharacterSound = clips[i];
            }
        }
    }

	/// <summary>
	/// Checks if the ability can be used based on the actual energy level without subtracting value.
	/// </summary>
	/// <returns>True: Ability can be used. False: Ability can't be used.</returns>
	protected bool CheckEnergyLevelWithoutSubtraction()
	{
		if ((Energy - ability.EnergyCost) < minEnergy)
			return false;
		else
		{
			return true;
		}
	}

    /// <summary>
    /// Updates the values of the HUD elements.
    /// </summary>
    private void UpdateHUDElements()
    {
        // Update Energy
        if (energyLevel != null)
        {
            energyLevel.fillAmount = (float)Energy / (float)maxEnergy;
        }
        
        // Update Health
        if (healthLevel != null)
        {
            healthLevel.fillAmount = (float)Health / (float)maxHealth;
        }

        // Set the inner energy circle to visible or not.
        if (innerEnergyCircle != null)
        {
            if (Energy == MaxEnergy)
                innerEnergyCircle.enabled = true;
            else if (Energy < MaxHealth)
                innerEnergyCircle.enabled = false;
        }
    }

    /// <summary>
    /// Resets all neccessary values.
    /// </summary>
    protected void ResetValues()
    {
        PlayerDied = null;
        PlayerSpawned = null;
        AbilityUseable = null;
    }
    #endregion
}
