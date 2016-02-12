using UnityEngine;
using System.Collections;

public class TriangleCollision : MonoBehaviour {

    private PolygonSystem polygonSystemScript;


	// Use this for initialization
	void Start () {
        polygonSystemScript = GetComponentInParent<PolygonSystem>();
	}
	
    void OnTriggerEnter(Collider coll)
    {
        if(polygonSystemScript.detonate == true && coll.tag=="Enemy")
        {
            if (coll.GetComponent<MonoBehaviour>() is BossEnemy)
            {
                coll.GetComponent<BossEnemy>().TakeDamage(polygonSystemScript.currentBossDamage, null);              
                polygonSystemScript.currentBossDamage = 0;
            }
            else
            {
                coll.tag = "SentencedToDeath";
                coll.GetComponent<BaseEnemy>().CanShoot = false;
                coll.GetComponent<BaseEnemy>().MeleeAttackDamage = 0;
                polygonSystemScript.enemies.Add(coll.gameObject);                
            }           
        }
    }

}
