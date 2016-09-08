using UnityEngine;
using System.Collections.Generic;
using System.Collections;


public delegate void DetectionFinishedHandler();

public class TriangleCollision : MonoBehaviour {

    #region variables
    private List<GameObject> detectedEnemies;
   
    private BossEnemy[] bossEnemy;
   
    private MeshCollider meshCollider;
    private MeshFilter meshFilter;
    private static int collidersFinished = 0;
    private static int currentColliderCount = 0;
    private WaitForFixedUpdate fixedWait;
    private bool isDetecting = false;
    public static event DetectionFinishedHandler DetectionDone;
    #endregion

    #region properties
    public List<GameObject> DetectedEnemies
    {
        set { detectedEnemies = value; }
    } 

    public BossEnemy[] BossEnemy
    {
        set { bossEnemy = value; }
    }
    #endregion

    #region methods
    void Awake()
    {
        LevelEndManager.levelExitEvent += ResetValues;
        PolygonCoreLogic.PolyExecuted += HandleEnemyDetection;
        meshCollider = GetComponent<MeshCollider>();
        if(meshCollider == null)
        {
            Debug.LogError("Meshcollider is null!");
        }
        meshFilter = GetComponent<MeshFilter>();
        if(meshFilter == null)
        {
            Debug.LogError("Meshfilter is null!");
        }
        fixedWait = new WaitForFixedUpdate();
        currentColliderCount++;
    }

    private void HandleEnemyDetection()
    {
        if (!isDetecting)
        {
            isDetecting = true;
            StartCoroutine(DetectEnemies());
        }
        else
        {
            Debug.Log("detecting started while detecting");
        }
    }

    private IEnumerator DetectEnemies()
    {
        meshCollider.sharedMesh = null;
        meshCollider.sharedMesh = meshFilter.mesh;
        meshCollider.enabled = true;
        
        yield return fixedWait;

        meshCollider.enabled = false;
        meshCollider.sharedMesh = null;

        yield return fixedWait;
        isDetecting = false;
        OnDetectionDone();
    }

    void OnTriggerEnter(Collider coll)
    {
        if (coll.tag == "Enemy")
        {
            if (coll.GetComponent<MonoBehaviour>() is BossEnemy)
            {
                if (bossEnemy[0] == null)
                {
                    bossEnemy[0] = coll.GetComponent<BossEnemy>();
                }
            }
            else
            {
                coll.tag = "SentencedToDeath";
                coll.GetComponent<BaseEnemy>().CanShoot = false;
                coll.GetComponent<BaseEnemy>().MeleeAttackDamage = 0;
                detectedEnemies.Add(coll.gameObject);
            }
        }
    }

    #region eventMethod
    public void OnDetectionDone()
    {
        collidersFinished++;
        if (collidersFinished >= currentColliderCount)
        {
             if (DetectionDone != null)
            {
                collidersFinished = 0;
                DetectionDone();
            }
        }
    }
    #endregion

    #region eventReset
    private void ResetValues()
    {
        DetectionDone = null;
        currentColliderCount = 0;
        collidersFinished = 0;
    }
    #endregion

    #endregion



}
