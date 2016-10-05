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
        grandStep = step * 3;
        scaleFactor = 12.0f;      
    }

    private void OnDestroy()
    {
        Destroy(gameObject);
    }
}
