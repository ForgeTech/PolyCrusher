using UnityEngine;
using System.Collections;

public class TriangleCollision : MonoBehaviour {

    private PolygonSystem polygonSystemScript;


	// Use this for initialization
	void Start () {
        polygonSystemScript = GetComponentInParent<PolygonSystem>();


	}
	
	// Update is called once per frame
	void Update () {
	
	}


    void OnTriggerStay(Collider coll)
    {
        if(polygonSystemScript.detonate == true && coll.tag=="Enemy")
        {
            coll.tag = "SentencedToDeath";
            coll.GetComponent<BaseEnemy>().CanShoot = false;
            coll.GetComponent<BaseEnemy>().MeleeAttackDamage = 0;
            polygonSystemScript.enemies.Add(coll.gameObject);

        }
        



    }

}
