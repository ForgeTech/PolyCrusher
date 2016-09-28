using UnityEngine;

public class BigScalePolyExplosion : PolyExplosion {

    // Use this for initialization
    public override void Start()
    {
        base.Start();
        SetAttributes();
        base.ExplodePartial(Random.Range(0, 3));
    }

    private void SetAttributes()
    {
        scaleFactor = 35.0f;
        explosionForce = 25.0f;
        upwardsModifier = -3.0f;
        changeForwardVector = true;
    }
}
