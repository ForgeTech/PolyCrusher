using UnityEngine;

public class AbilityFallout : Ability
{
    #region variable
    [SerializeField]
    private GameObject falloutCloud;
    #endregion

    #region methods
    protected override void Start()
    {
        base.Start();
    }

    public override void Use()
    {
        if (useIsAllowed)
        {
            base.Use();

            GameObject obj = Instantiate(falloutCloud,transform.position, Quaternion.identity) as GameObject;
            FalloutBehaviour chi = obj.GetComponent<FalloutBehaviour>();
            chi.OwnerScript = this.OwnerScript;
            chi.RumbleManager = this.rumbleManager;

            useIsAllowed = false;
            StartCoroutine(WaitForNextAbility());
        }
    }
    #endregion
}
