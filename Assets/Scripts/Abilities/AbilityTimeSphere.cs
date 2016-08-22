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

            Rumble();

			GameObject g = Instantiate(timeSphere, transform.position, transform.rotation) as GameObject;
            g.GetComponent<TimeSphereScript>().RumbleManager = rumbleManager;

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
