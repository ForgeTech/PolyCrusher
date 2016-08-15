using UnityEngine;
using System.Collections;

public class AbilityPie : Ability
{
    [SerializeField]
    [Tooltip("The prefab of the pie.")]
    protected GameObject piePrefab;

    protected override void Start()
    {
        base.Start();
    }

    public override void Use()
    {
        if (useIsAllowed)
        {
            if (GameObject.FindGameObjectWithTag("Pie") == null)
            {
                base.Use();

                GameObject obj = Instantiate(piePrefab);

                BasePlayer p = OwnerScript.GetComponent<BasePlayer>();
                PieBehaviour pie = obj.GetComponent<PieBehaviour>();

                pie.OwnerScript = this.OwnerScript;
                pie.PlayerActions = p.PlayerActions;

                obj.transform.position = transform.position - transform.forward * 1.5f;
                obj.transform.rotation = transform.rotation;

                useIsAllowed = false;

                StartCoroutine(WaitForNextAbility());
            }
            else
            {
                BasePlayer player = OwnerScript.GetComponent<BasePlayer>();

                if (player != null)
                    player.Energy += EnergyCost;
            }
        }
    }
}
