using UnityEngine;
using System.Collections;
using InControl;
using System;

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
    [SerializeField]
    protected int health = 100;
    
    [SerializeField]
    protected int minHealth = 0;

    [SerializeField]
    protected int maxHealth = 100;

    [SerializeField]
    protected int lowHealth = 25;

    [SerializeField]
    protected float healthRefillRate = 0.3f;

    [SerializeField]
    protected int healthRefillIncrement = 2;

    private bool allowHealthRefill = true;

    private WaitForSeconds waitForHealthRefill;

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

    private WaitForSeconds waitForEnergyRefill;
    // The value which will be added to the actual energy.
    [SerializeField]
    protected int energyIncrement = 5;

    [Space(10)]
    [Header("Misc")]
    [SerializeField]
    protected PlayerEnum playerIdentifier = PlayerEnum.Birdman;
    public PlayerEnum PlayerIdentifier { get { return this.playerIdentifier; } }

    [SerializeField]
    protected Color playerColor = Color.white;
    public Color PlayerColor { get { return playerColor; } }

    #region Input
    //Input Device for accessing Rumble
    protected InputDevice inputDevice;

    //Player Actions for accessing this players input
    protected PlayerControlActions playerActions;

    protected RumbleManager rumbleManager;

    protected bool lowHealthRumbleActive = false;
    #endregion

    // Initial death time.
    [Tooltip("The time the player stays on screen after he dies.")]
    [SerializeField]
    protected float deathTime = 3.3f;

    // The actual death time which will be used.
    protected float currentDeathTime;

    [Space(10)]
    [Header("Movement")]
    [SerializeField]
    protected PlayerInputHandler playerInput;

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
    public Ability ability;
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

    [Space(10)]
    [Header("Death particle prefab")]
    [Tooltip("Prefab of the death particles.")]
    [SerializeField]
    protected GameObject deathParticles;

    [Space(10)]
    [Header("Enemy kill particle prefab")]
    [Tooltip("Prefab of the enemy kill particles.")]
    [SerializeField]
    public GameObject killParticles;

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

    public MultipleAudioclips abilityCharacterSound;
    protected MultipleAudioclips hurtCharacterSound;
    protected MultipleAudioclips deathCharacterSound;
    protected MultipleAudioclips spawnCharacterSound;
    //=========================================

    [Space(10)]
    [Header("Special particles")]
    [SerializeField]
    protected GameObject respawnParticles;

    #region Delegates and Events
    //Event handler for player deaths.
    public static event PlayerDiedEventHandler PlayerDied;

    //Event handler for player spawn.
    public static event PlayerSpawnedEventHandler PlayerSpawned;

    // Event handler for the use of the ability.
    public static event AbilityUseableEventHandler AbilityUseable;

    public delegate void TakeDamageEventHandler(int damageDealed);
    public event TakeDamageEventHandler DamageTaken;
    #endregion
    #endregion


    #region Properties
    /// <summary>
    /// Gets or sets the actual health value.
    /// </summary>
    public int Health
    {
        get { return this.health; }
        set
        {
            if (value >= minHealth && value <= maxHealth)
                this.health = value;

            if (value > maxHealth)
                this.health = maxHealth;

            if (value <= 0)
            {
                this.health = minHealth;
                if(!IsDead)
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

    public bool Invincible
    {
        get { return this.invincible; }
        set { this.invincible = value; }
    }

    public bool IsDead
    {
        get { return this.isDead; }
    }

    /// <summary>
    /// Specifies if the object can shoot or not.
    /// </summary>
    public bool CanShoot
    {
        get { return this.canShoot; }
        set { this.canShoot = value; }
    }

    public Weapon PlayerWeapon
    {
        get { return this.weapon; }
    }

    public float CurrentDeathTime
    {
        get { return this.currentDeathTime; }
        set { this.currentDeathTime = value; }
    }

    public InputDevice InputDevice
    {
        get { return inputDevice; }
        set
        {
            inputDevice = value;
            playerActions.Device = value;
            ability.InputDevice = inputDevice;
        }
    }

    public PlayerControlActions PlayerActions
    {
        get { return playerActions; }
        set
        {
            playerActions.Destroy();
            playerActions = value;
            ability.PlayerActions = playerActions;
        }
    }   

    public RumbleManager RumbleManager
    {
        get { return rumbleManager; }
        set
        {
            rumbleManager = value;
            ability.RumbleManager = rumbleManager;
        }
    }

    /// <summary>
    /// Special ability energy.
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

    public float MovementSpeed
    {
        get { return playerInput.movementSpeed; }
        set { playerInput.movementSpeed = value; }
    }

    public bool IsMoving { get { return playerInput.IsMoving; } }

    public bool StopEnergyRefill
    {
        get { return this.stopEnergyRefill; }
        set { this.stopEnergyRefill = value; }
    }
    #endregion

    #region Methods
    private void Awake()
    {
        LevelEndManager.levelExitEvent += ResetValues;

        playerActions = PlayerControlActions.CreateWithGamePadBindings();
        if (inputDevice != null)
            playerActions.Device = inputDevice;

        InitializeWaitForSeconds();
    }

	private void Start () 
    {
        // Set the gameobject name.
        gameObject.name = PlayerIdentifier.ToString("g");

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
		//network = GameObject.FindObjectOfType<PlayerNetCommunicate>();

        // Set the owner of the weapon and the ability.
        if (weapon != null)
            weapon.OwnerScript = this;

        //Set original health level.
        originalHealthLevelScale = healthLevel.transform.localScale;

        // Fire spawn event.
        OnPlayerSpawn();

        if (ability != null)
        {
            ability.OwnerScript = this;
            ability.InputDevice = inputDevice;
            ability.PlayerActions = playerActions;
        }
        playerInput = new PlayerInputHandler(this, playerAnimator);
    }

    private void OnEnable()
    {
        this.CurrentDeathTime = deathTime;
        this.allowEnergyRefill = true;
        this.invincible = false;

        Rigidbody rigid = GetComponent<Rigidbody>();
        if (rigid != null)
            rigid.mass = 0.2f;
    }

    private void InitializeWaitForSeconds()
    {
        waitForHealthRefill = new WaitForSeconds(healthRefillRate);
        waitForEnergyRefill = new WaitForSeconds(energyRefillRate);
    }

    private void Update () 
    {
        if (!IsDead)
        {
            // Refill Energy
            RefillEnergy(energyIncrement);
            
            // Only refill in a single player game
            if(PlayerManager.PlayerCountInGameSession == 1)
                RefillHealth();

			HandleAbilityInput();
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
    void CheckAbilityStatusReady()
    {
        if (!abilityUseable && CheckEnergyLevelWithoutSubtraction())
            abilityUseable = true;
    }

    /// <summary>
    /// Handles the player input.
    /// </summary>
    protected virtual void HandleInput()
    {
        //Handles the movement and rotation/shoot input.
		HandleMovement ();
    }

    /// <summary>
    /// Handles the shoot mechanism.
    /// </summary>
    public virtual void Shoot()
    {
        if (weapon != null)
            weapon.Use();
    }

    public virtual void Attack() { /*Nothing to do here*/ }

    /// <summary>
    /// Handles the input of the ability.
    /// </summary>
    public virtual void HandleAbilityInput()
    {
        playerInput.HandleAbilityInput();
    }

    public void HandleMovement()
    {
        playerInput.HandleMovement();
    }

    public void ManipulateMovement(float speedFactor, Vector3 direction)
    {
        playerInput.ManipulateMovement(speedFactor, direction);
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
            if(damageDealer is BaseEnemy)
                DamageTakenRumble();

            // Health Level tween.
            Vector3 originalScale = originalHealthLevelScale;
            healthLevel.transform.localScale = Vector3.zero;
            LeanTween.scale(healthLevel.rectTransform, originalScale, 0.5f).setEase(AnimCurveContainer.AnimCurve.pingPong);

            // Send event if player will be dead
            if (Health - damage <= 0 && !IsDead)
            {
                CancelInvoke();

                string enemyName = "undefined";
                if (damageDealer is BaseEnemy)
                    enemyName = ((BaseEnemy)damageDealer).EnemyName;
                else
                    enemyName = damageDealer.GetType().Name;

                // Instantiate particles if the prefab reference isn't null.
                if (deathParticles != null)
                {
                    GameObject deathParticle = Instantiate(deathParticles);
                    deathParticle.transform.position = transform.position;
                }
                new Event(Event.TYPE.death).addPos(transform).addCharacter(PlayerIdentifier.ToString("g")).addWave().addEnemy(enemyName).addLevel().addPlayerCount().send();
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
            
            Health -= damage;
            OnDamageTaken(damage);

            if(health < lowHealth && !lowHealthRumbleActive)
            {
                lowHealthRumbleActive = true;
                InvokeRepeating("LowHealthRumble", 0.0f, 5.0f);
            }
        }
    }

    /// <summary>
    /// Lowers the health to zero.
    /// </summary>
    public virtual void InstantKill(MonoBehaviour trigger)
    {
        this.Health = minHealth;
        new Event(Event.TYPE.death).addPos(this.transform).addCharacter(PlayerIdentifier.ToString("g")).addWave().addEnemy(trigger.GetType().Name).addLevel().addPlayerCount().send();
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

                LeanTween.scale(energyLevel.rectTransform, originalScale, 0.4f).setEase(AnimCurveContainer.AnimCurve.upscale);
            }

            Energy += addEnergy;
            StartCoroutine(WaitForEnergyRefill());
        }
    }

    private void RefillHealth()
    {
        if (allowHealthRefill && !IsMoving)
        {
            Health += healthRefillIncrement;
            StartCoroutine(WaitForHealthRefill());
        }
    }

    public void PowerUpPickUpRumble()
    {
        if (rumbleManager != null)
            rumbleManager.Rumble(inputDevice, RumbleType.BasicRumbleShort);
    }

    private void DamageTakenRumble()
    {
        if (rumbleManager != null && !IsDead)
            rumbleManager.Rumble(inputDevice, RumbleType.BasicRumbleExtraShort);
    }

    private void LowHealthRumble()
    {
        if (rumbleManager != null && !IsDead)
        {
            if (this.health < lowHealth)
            {
                rumbleManager.Rumble(inputDevice, RumbleType.LowHealth);
            }
            else
            {
                CancelInvoke();
                lowHealthRumbleActive = false;
            }
        }
    }

    protected IEnumerator WaitForEnergyRefill()
    {
        allowEnergyRefill = false;
        yield return waitForEnergyRefill;
        allowEnergyRefill = true;
    }

    protected IEnumerator WaitForHealthRefill()
    {
        allowHealthRefill = false;
        yield return waitForHealthRefill;
        allowHealthRefill = true;
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

        // Set RigidBody to isKinematic. (So the enemies does not move the player after death).
        GetComponent<Rigidbody>().isKinematic = true;

        // Set death boolean.
        this.isDead = true;

        // Play death sound.
        if (deathCharacterSound != null)
            deathCharacterSound.PlayRandomClip();

        // Tween HUD elements
        LeanTween.scale(energyLevel.rectTransform, Vector3.zero, CurrentDeathTime * 0.5f).setEase(LeanTweenType.easeOutSine);
        LeanTween.scale(healthLevel.rectTransform, Vector3.zero, CurrentDeathTime * 0.5f).setEase(LeanTweenType.easeOutSine);
        LeanTween.scale(innerEnergyCircle.rectTransform, Vector3.zero, CurrentDeathTime * 0.5f).setEase(LeanTweenType.easeOutSine);

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
        // Tween HUD elements
        LeanTween.scale(energyLevel.rectTransform, Vector3.one, CurrentDeathTime * 0.5f).setEase(LeanTweenType.easeOutSine);
        LeanTween.scale(healthLevel.rectTransform, originalHealthLevelScale, CurrentDeathTime * 0.5f).setEase(LeanTweenType.easeOutSine);
        LeanTween.scale(innerEnergyCircle.rectTransform, Vector3.one, CurrentDeathTime * 0.5f).setEase(LeanTweenType.easeOutSine);

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
        {
            PlayerDied();
            if (rumbleManager != null && inputDevice != null)
                rumbleManager.Rumble(inputDevice, RumbleType.PlayerDeath);
        }
    }

    /// <summary>
    /// Event method for player spawn.
    /// </summary>
    protected virtual void OnPlayerSpawn()
    {
        if (PlayerSpawned != null)
        {
            PlayerSpawned(this);
            if (rumbleManager != null && inputDevice !=null)
                rumbleManager.Rumble(inputDevice, RumbleType.BasicRumbleLong);
            Debug.Log("Player <b>" + PlayerIdentifier + "</b> spawned!");
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
    public bool CheckEnergyLevel()
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
			return true;
	}

    /// <summary>
    /// Updates the values of the HUD elements.
    /// </summary>
    private void UpdateHUDElements()
    {
        // Update Energy
        if (energyLevel != null)
            energyLevel.fillAmount = (float)Energy / (float)maxEnergy;
        
        // Update Health
        if (healthLevel != null)
            healthLevel.fillAmount = (float)Health / (float)maxHealth;

        // Set the inner energy circle to visible or not.
        if (innerEnergyCircle != null)
        {
            if (Energy == MaxEnergy)
                innerEnergyCircle.enabled = true;
            else if (Energy < MaxHealth)
                innerEnergyCircle.enabled = false;
        }
    }

    protected void OnDamageTaken(int damageTaken)
    {
        if (DamageTaken != null)
            DamageTaken(damageTaken);
    }

    /// <summary>
    /// Resets all neccessary values.
    /// </summary>
    protected void ResetValues()
    {
        PlayerDied = null;
        PlayerSpawned = null;
        AbilityUseable = null;
        playerActions.Destroy();

        if (inputDevice != null)
            inputDevice.StopVibration();
    }
    #endregion
}