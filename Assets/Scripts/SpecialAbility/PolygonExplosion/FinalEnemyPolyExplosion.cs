using UnityEngine;

public class FinalEnemyPolyExplosion : PolyExplosion {

    // Use this for initialization
    public override void Start()
    {
        base.Start();
        SetAttributes();
        base.ExplodePartial(Random.Range(0, 3));
    }

    private void SetAttributes()
    {        
        useGravity = false;
        drag = 2.5f;
        angularDrag = 0.25f;
    }

    private void OnDestroy()
    {
        Destroy(gameObject);
    }
}
