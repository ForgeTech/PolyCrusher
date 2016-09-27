using UnityEngine;
using System.Collections;

public class SmallPolyExplosion : PolyExplosion {

	// Use this for initialization
	public override void Start () {
        base.Start();
        scaleFactor =  6 * step / (float)(step * step);
        grandStep = step * 9;
        explosionForce = 20;
        upwardsModifier = 1.0f;
        base.ExplodePartial(Random.Range(0,6));
	}
	
	
}
