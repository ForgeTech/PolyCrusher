using UnityEngine;
using System.Collections;

/// <summary>
/// Ability script for the timesphere.
/// </summary>
public class AbilityTimeSphere : Ability 
{
	[SerializeField]
	private GameObject timeSphere;

    protected override void Start()
    {
        base.Start();
    }

	public override void Use() 
    {
		if (useIsAllowed)
        {
            base.Use();

			Instantiate(timeSphere, transform.position, transform.rotation);

			useIsAllowed = false;
			StartCoroutine(WaitForNextAbility());
		}
	}
}
