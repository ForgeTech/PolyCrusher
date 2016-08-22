using UnityEngine;
using System.Collections;

public class AbilityPantomime : Ability
{
    [SerializeField]
    [Tooltip("The prefab of the pantomime wall.")]
    protected GameObject wallPrefab;

    protected override void Start()
    {
        base.Start();
    }

    public override void Use()
    {
        if (useIsAllowed)
        {
            base.Use();

            Rumble();

            GameObject wall = Instantiate(wallPrefab);
            BasePlayer p = OwnerScript.GetComponent<BasePlayer>();
            p.name = "PantomimeWall";

            wall.transform.position = transform.position + transform.forward * 2f;
            wall.transform.rotation = transform.rotation;

            useIsAllowed = false;
            StartCoroutine(WaitForNextAbility());
        }
    }

    protected void Rumble()
    {
        if (rumbleManager != null)
        {
            rumbleManager.Rumble(inputDevice, RumbleType.BasicRumbleShort);
        }
    }
}
