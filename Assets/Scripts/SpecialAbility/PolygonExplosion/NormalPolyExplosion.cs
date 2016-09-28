using UnityEngine;
using System.Collections;

public class NormalPolyExplosion : PolyExplosion {

	// Use this for initialization
	public override void Start () {
        base.Start();
        base.ExplodePartial(Random.Range(0,6));
	}

    private void OnDestroy()
    {
        Destroy(gameObject);
    }
	
}
