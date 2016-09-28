using UnityEngine;

public class BossPolyHitExplosion : PolyExplosion {

	// Use this for initialization
	public override void Start () {
        base.Start();
        SetAttributes();
        base.ExplodePartial(Random.Range(0,3));
	}
	
    private void SetAttributes()
    {
        grandStep = step;
        scaleFactor = 2.0f;
        explosionForce = 35.0f;
        minimumAliveTime = 0.5f;
        maximumAliveTime = 1.5f;
    }
	
}
