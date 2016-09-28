using UnityEngine;

public class SmallPolyExplosion : PolyExplosion {

    public Vector3 ForceDirection
    {
        set { explosionOrigin = value; }
    }

	// Use this for initialization
	public override void Start () {
        base.Start();
        SetAttributes();
        base.ExplodePartial(Random.Range(0, 6));
    }
	
    private void SetAttributes()
    {
        scaleFactor = 1.3f;
        grandStep = step * 5;
        explosionForce = 20;
        upwardsModifier = 0.0f;
        minimumAliveTime = 1.0f;
        maximumAliveTime = 3.0f;
    }

  
}
