using UnityEngine;
using System.Collections;

/// <summary>
/// Ability class for the chicken.
/// </summary>
public class AbilityChicken : Ability
{
    // The prefab path to the chicken ability prefab.
    [SerializeField]
    protected string prefabString = "Abilities/ChickenAbility";

    protected override void Start()
    {
        base.Start();
    }

    public override void Use()
    {
        if (useIsAllowed)
        {
            base.Use();

            GameObject obj = Instantiate(Resources.Load<GameObject>(prefabString));

            obj.GetComponent<ChickenBehaviour>().OwnerScript = this.OwnerScript;
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
}