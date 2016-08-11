using UnityEngine;
using System.Collections;

/// <summary>
/// Base Weapon class which implements the basic 
/// weapon mechanic.
/// </summary>
public abstract class Weapon : MonoBehaviour, IUsable
{
    #region Class Members
    // The damage of the weapon.
    [SerializeField]
    protected int weaponDamage;

    // The fire rate of the weapon.
    [SerializeField]
    protected float weaponFireRate;

    // The range of the weapon as bullet life time.
    [SerializeField]
    protected float bulletLifeTime;

    // The accuracy of the weapon in degree.
    [SerializeField]
    protected float weaponAccuracy;

    //Name of the weapon
    [SerializeField]
    protected string weaponName;

    [SerializeField]
    [Tooltip("Prefab reference to the bullet")]
    protected GameObject bulletPrefab;

    [Space(10)]
    [Header("Light properties")]
    [Tooltip("Muzzle flash light")]
    [SerializeField]
    protected GameObject muzzleFlashLight;

    [SerializeField]
    protected float muzzleFlashTime = 0.1f;

    // Audiosource of the weapon.
    protected AudioSource weaponAudioSource;

    // Specifies if the weapon is allowed to shoot; Works in combination with the fire rate.
    protected bool shootIsAllowed;

    // Reference to the audiosource component.
    protected AudioSource gunSound;

    // The owner of the weapon.
    protected MonoBehaviour ownerScript;
    #endregion

    #region Properties
    /// <summary>
    /// Gets or sets the weapons name.
    /// </summary>
    public string Name
    {
        get
        {
            return this.weaponName;
        }
        set
        {
            this.weaponName = value;
        }
    }

    /// <summary>
    /// Gets or sets the weapon damage.
    /// </summary>
    public int WeaponDamage
    {
        get { return this.weaponDamage; }
        set { this.weaponDamage = value; }
    }

    /// <summary>
    /// Gets or sets the fire rate.
    /// </summary>
    public float WeaponFireRate
    {
        get { return this.weaponFireRate; }
        set { this.weaponFireRate = value; }
    }

    /// <summary>
    /// Gets or sets the owner script.
    /// </summary>
    public MonoBehaviour OwnerScript
    {
        get { return this.ownerScript; }
        set { this.ownerScript = value; }
    }
    #endregion

    #region Methods

    protected virtual void Start()
    {
        shootIsAllowed = true;
        gunSound = GetComponent<AudioSource>();
    }

    void OnEnable()
    {
        shootIsAllowed = true;
    }

    /// <summary>
    /// Implements the shoot behaviour of the weapon.
    /// </summary>
    public virtual void Use()
    {
        gunSound.Play();
        StartCoroutine(HandleMuzzleFlash());
        //SteamManager.Instance.LogAchievementData(AchievementID.ACH_A_MILLION_SHOTS);
    }

    /// <summary>
    /// Waits for the next shot based on the weapon Fire rate and sets "shootIsAllowed" to true.
    /// </summary>
    /// <returns></returns>
    protected IEnumerator WaitForNextShot()
    {
        yield return new WaitForSeconds(weaponFireRate);
        shootIsAllowed = true;
    }

    /// <summary>
    /// Handles the light of the muzzle flash.
    /// </summary>
    /// <returns></returns>
    private IEnumerator HandleMuzzleFlash()
    {
        Light l = muzzleFlashLight.GetComponent<Light>();

        l.enabled = true;
        yield return new WaitForSeconds(muzzleFlashTime);
        l.enabled = false;
    }
    #endregion

}
