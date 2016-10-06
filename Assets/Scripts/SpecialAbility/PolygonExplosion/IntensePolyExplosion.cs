using UnityEngine;

public class IntensePolyExplosion : PolyExplosion {

    // Use this for initialization
    public override void Start()
    {
        base.Start();
        SetAttributes();
        base.ExplodePartial(Random.Range(0, 6));
    }

    private void SetAttributes()
    {
        minimumAliveTime = 2.0f;
        maximumAliveTime = 3.5f;
        grandStep = step * 4;
        scaleFactor = 16.0f;      
    }

    private void OnDestroy()
    {
        Destroy(gameObject);
    }
}
