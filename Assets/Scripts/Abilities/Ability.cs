using UnityEngine;
using System.Collections;
using InControl;

public abstract class Ability : MonoBehaviour, IUsable
{
    #region Class members
    // The energy cost of the ability.
    [Header("Energy Cost")]
    [SerializeField]
    protected int energyCost = 40;
    [Space(5)]

    // Audiosource reference
    protected AudioSource abilityAudioSource;

	// defines the cooldown of the ability
	public float abilityCoolDown;

    // Specifies if the use of the ability is allowed.
	protected bool useIsAllowed;

    // The owner of the projectile.
    protected MonoBehaviour ownerScript;
    #endregion

    // the inputdevice of the owner
    protected InputDevice inputDevice;

    // the playeractions of the inputDevice;
    protected PlayerControlActions playerActions;

    protected RumbleManager rumbleManager;
    #region Properties
    


    /// <summary>
    /// Gets useIsAllowed.
    /// </summary>
    public bool UseIsAllowed
    {
        get { return this.useIsAllowed; }
        set { this.useIsAllowed = value; }
    }

    /// <summary>
    /// Gets or sets the name.
    /// </summary>
	public string Name
	{
		get
		{
			return gameObject.name;
		}
		set
		{ 
			gameObject.name = value;
		}
	}

    /// <summary>
    /// Gets the energy cost.
    /// </summary>
    public int EnergyCost
    {
        get { return this.energyCost; }
    }

    /// <summary>
    /// Gets or sets the owner script.
    /// </summary>
    public MonoBehaviour OwnerScript
    {
        get { return this.ownerScript; }
        set { this.ownerScript = value; }
    }


    public InputDevice InputDevice
    {
        set
        {
            inputDevice = value;
        }
    }

    public PlayerControlActions PlayerActions
    {
        set
        {
            playerActions = value;
        }
    }

    public RumbleManager RumbleManager
    {
        set
        {
            rumbleManager = value;
        }
    }

	protected virtual void Start()
	{
		useIsAllowed = true;

        abilityAudioSource = GetComponent<AudioSource>();
	}

    void OnEnable()
    {
        useIsAllowed = true;
    }
    #endregion


        #region Methods
        /// <summary>
        /// Implements the use behaviour of the ability.
        /// </summary>
    public virtual void Use()
    {
        abilityAudioSource.Play();
    }

    /// <summary>
	/// Waits for the next use of the ability based on the ability active time Fire rate and sets "useIsAllowed" to true.
	/// </summary>
	/// <returns></returns>
	protected IEnumerator WaitForNextAbility()
	{
		yield return new WaitForSeconds(abilityCoolDown);
		useIsAllowed = true;
    }
    #endregion
}
