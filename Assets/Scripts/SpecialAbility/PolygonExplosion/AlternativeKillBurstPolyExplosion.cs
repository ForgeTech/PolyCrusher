using UnityEngine;
using System.Collections;

public class AlternativeKillBurstPolyExplosion : PolyExplosion {

	// Use this for initialization
	public override void Start () {
        base.Start();
        SetAttributes();
        base.ExplodePartial(Random.Range(0, 3));
    }

    private void SetAttributes()
    {
        scaleFactor = 20.0f;
        grandStep = step * 5;
        explosionForce = 20;
        upwardsModifier = 0.0f;
        minimumAliveTime = 1.0f;
        maximumAliveTime = 3.0f;
        changeForwardVector = true;
    }

    private void OnDestroy()
    {
        Destroy(gameObject);
    }
}
