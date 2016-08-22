using UnityEngine;
using System.Collections;

/// <summary>
/// Ability class for the chicken.
/// </summary>
public class AbilityChicken : Ability
{
    [SerializeField]
    protected GameObject chickenPrefab;

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


            GameObject obj = Instantiate(chickenPrefab);

            ChickenBehaviour chi = obj.GetComponent<ChickenBehaviour>();
            chi.OwnerScript = this.OwnerScript;
            chi.RumbleManager = this.rumbleManager;
            obj.SetActive(false);

            NavMeshHit hit;
            bool found = NavMesh.SamplePosition(transform.position, out hit, 10f, NavMesh.AllAreas);

            if (found)
                obj.transform.position = hit.position;
            else
            {
                Debug.Log("false");
                obj.transform.position = transform.position;
            }

            obj.transform.rotation = transform.rotation;
            obj.SetActive(true);

            useIsAllowed = false;

            StartCoroutine(WaitForNextAbility());
        }
    }

    protected void Rumble()
    {
        rumbleManager.Rumble(inputDevice, RumbleType.BasicRumbleShort);
    }
}