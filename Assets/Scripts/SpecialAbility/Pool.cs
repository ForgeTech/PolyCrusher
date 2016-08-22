using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Pool : MonoBehaviour {

    //static reference
    public static Pool current;


    //which object should be pooled
    public GameObject objectToPool;


    //how many objects should be instantiated
    public int amount;


    //list that holds the pooled objects
    private List<GameObject> pool;


    void Awake()
    {
        current = this;

        StartCoroutine(InitializePool());
    }




    public GameObject getPooledObject()
    {

        for(int i = 0; i < pool.Count; i++)
        {
            if (!pool[i].activeInHierarchy)
            {                
                return pool[i];
            }

        }
        return null;


    }

    
    private IEnumerator InitializePool()
    {
        yield return new WaitForSeconds(2.5f);
      

        pool = new List<GameObject>();

        for (int i = 0; i < amount; i++)
        {
            GameObject go = (GameObject)Instantiate(objectToPool);
            go.transform.SetParent(this.transform);
            go.SetActive(false);
            pool.Add(go);

        }


    }


	
}
