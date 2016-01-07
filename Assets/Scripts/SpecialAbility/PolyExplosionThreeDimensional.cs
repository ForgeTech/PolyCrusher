using UnityEngine;
using System.Collections.Generic;

public class PolyExplosionThreeDimensional : MonoBehaviour
{
    public GameObject explosion;


    public bool explode = false;
    public float extrudeFactor;
    public int hitsTillExplosion;
    
    public bool explodeable = true;
    public bool respawn = false;
    public int timeTillRespawn = 10;

    private int vertexCount;
    private int step;
    private int grandStep;
    private Vector3 scaleFactor;
    private Pool pool;
    private int health;


    private int matchedIndex;
    private DestructibleRespawn respawnScript;
    public List<Deactivator> deactivators;

    [Header("Power Up")]
    [SerializeField]
    [Tooltip("Power up prefab for line cutting.")]
    protected GameObject powerUpPrefab;

    [SerializeField]
    [Tooltip("Probability for a power up spawn.")]
    protected float powerUpProbability = 0.05f;

    public struct Tri
    {
        public int x;
        public int y;
        public int z;

        public Tri(int x, int y, int z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }
    }

    private List<Tri> triList = new List<Tri>();
    private bool found;
    


    MeshFilter MF;
    MeshRenderer MR;
    Mesh M;
    Vector3[] verts;
    Vector3[] normals;
    Vector2[] uvs;





    GameObject GO;
    Mesh mesh;
    Vector3[] newVerts;
    Vector3[] newNormals;
    Vector2[] newUvs;

    // Use this for initialization
    void Start()
    {
        pool = Pool.current;
        //explode = true;
        MF = GetComponent<MeshFilter>();
        MR = GetComponent<MeshRenderer>();
        M = MF.sharedMesh;
        verts = M.vertices;
        normals = M.normals;
        uvs = M.uv;
        vertexCount = M.vertexCount;

        step = vertexCount / 90;
        while (step % 3 != 0)
        {
            step++;
        }

        grandStep = step * 20;
        //grandStep = 21;

        //scaleFactor = 0.03f * step * 2;
        scaleFactor = transform.localScale;
        found = false;

        health = hitsTillExplosion;

        if (respawn)
        {
            respawnScript = gameObject.AddComponent<DestructibleRespawn>();
        }
        deactivators = new List<Deactivator>();
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (explode)
        {
            explode = false;
            ExplodePartial(0);
        }
    }


    private void GetTriangles(int[] indices)
    {
        triList = new List<Tri>();
        for (int i = 0; i < indices.Length; i += 3)
        {
            triList.Add(new Tri(indices[i], indices[i + 1], indices[i + 2]));
        }
    }


    private bool GetMatchingTri(int x, int y, int z)
    {
        foreach (Tri tri in triList)
        {
            if (((tri.x == x && tri.y == y) || (tri.x == y && tri.y == x)) && tri.z != z) {// || (tri.x == x && tri.z == y) || (tri.x == y && tri.z == x)||(tri.y ==x && tri.z == y) || (tri.y ==y && tri.z == x))

                matchedIndex = tri.z;
                triList.Remove(tri);
                return true;
            }

            if (((tri.x == x && tri.z == y) || (tri.x == y && tri.z == x)) && tri.y != z)
            {
                matchedIndex = tri.y;
                triList.Remove(tri);
                return true;
            }

            if (((tri.y == x && tri.z == y) || (tri.y == y && tri.z == x)) && tri.x != z)
            {
                matchedIndex = tri.x;
                triList.Remove(tri);
                return true;
            }
        }
        return false;
    }



    private void ExplodePartial(int start)
    {
        for (int submesh = 0; submesh < M.subMeshCount; submesh++)
        {
            int[] indices = M.GetTriangles(submesh);
            GetTriangles(indices);

            for (int i = triList.Count - 1; i >= 0; i -= 3) //grandStep)
            {
                Tri tri = triList[i];

                Vector3 direction1 = Vector3.Normalize(-normals[tri.x]) * extrudeFactor;
                Vector3 direction2;
                int[] triangles = new int[0];

                mesh = new Mesh();
                Vector2 uvZero = new Vector2(0, 0);

                if (GetMatchingTri(tri.x, tri.y, tri.z))
                {
                    newVerts = new Vector3[8];
                    newNormals = new Vector3[8];
                    newUvs = new Vector2[8];
                    direction2 = Vector3.Normalize(-normals[matchedIndex]) * extrudeFactor;

                    newVerts[0] = verts[tri.x];
                    newUvs[0] = uvs[tri.x];
                    newNormals[0] = normals[tri.x];

                    newVerts[1] = verts[tri.y];
                    newUvs[1] = uvs[tri.y];
                    newNormals[1] = normals[tri.y];

                    newVerts[2] = verts[tri.z];
                    newUvs[2] = uvs[tri.z];
                    newNormals[2] = normals[tri.z];

                    newVerts[3] = verts[matchedIndex];
                    newUvs[3] = uvs[matchedIndex];
                    newNormals[3] = normals[matchedIndex];

                    newVerts[4] = newVerts[0] + direction1;
                    newUvs[4] = uvs[tri.x];
                    newNormals[4] = newNormals[0];

                    newVerts[5] = newVerts[1] + direction1;
                    newUvs[5] = uvs[tri.y];
                    newNormals[5] = newNormals[1];

                    newVerts[6] = newVerts[2] + direction1;
                    newUvs[6] = uvs[tri.z];
                    newNormals[6] = newNormals[2];


                    newVerts[7] = newVerts[3] + direction2;
                    newUvs[7] = uvs[matchedIndex];
                    newNormals[7] = newNormals[3];

                    triangles = new int[] { 0, 1, 2, 3, 1, 0, /*up tris*/  6, 4, 2, 0, 2, 4,  /*side1 */ 5, 2, 1, 2, 5, 6,  /*side2*/ 0, 4, 7, 7, 3, 0, /*side3*/ 7, 5, 1, 7, 1, 3, /*side4*/ 6, 5, 4, 4, 5, 7  /*down*/};

                }
                else
                {
                    newVerts = new Vector3[6];
                    newNormals = new Vector3[6];
                    newUvs = new Vector2[6];

                    newVerts[0] = verts[tri.x];
                    newUvs[0] = uvs[tri.x];
                    newNormals[0] = normals[tri.x];

                    newVerts[1] = verts[tri.y];
                    newUvs[1] = uvs[tri.y];
                    newNormals[1] = normals[tri.y];

                    newVerts[2] = verts[tri.z];
                    newUvs[2] = uvs[tri.z];
                    newNormals[2] = normals[tri.z];

                    newVerts[3] = newVerts[0] + direction1;
                    newUvs[3] = uvs[tri.x];
                    newNormals[3] = newNormals[0];

                    newVerts[4] = newVerts[1] + direction1;
                    newUvs[4] = uvs[tri.y];
                    newNormals[4] = newNormals[1];

                    newVerts[5] = newVerts[2] + direction1;
                    newUvs[5] = uvs[tri.z];
                    newNormals[5] = newNormals[2];


                    triangles = new int[] { 0, 1, 2, /*up*/   4, 1, 0, 0, 3, 4,  /*side1*/  0, 2, 5, 1, 4, 5, /*side2*/     5, 4, 3, 5, 3, 0,   /*side3*/  5, 2, 1 /*down*/};

                }



                mesh.vertices = newVerts;
                mesh.normals = newNormals;
                mesh.uv = newUvs;
                mesh.triangles = triangles;

                GO = pool.getPooledObject();

                if (GO != null)
                {
                    GO.SetActive(true);

                    Deactivator deactivator = GO.GetComponent<Deactivator>();

                    GO.layer = LayerMask.NameToLayer("Fragments");

                    GO.transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.z);
                    GO.transform.rotation = transform.rotation;
                    GO.transform.localScale = new Vector3(scaleFactor.x, scaleFactor.y, scaleFactor.z);


                    deactivator.attachedRenderer.material = MR.materials[submesh];
                    deactivator.attachedFilter.mesh = mesh;
                    deactivators.Add(deactivator);

                    
                    GO.AddComponent<BoxCollider>();
                    
                }
            }
        }

        // Spawn PowerUp
        SpawnPowerUp();

        MR.enabled = false;
        GetComponent<Rigidbody>().isKinematic = true;
        GetComponent<Collider>().enabled = false;
        if (GetComponent<NavMeshObstacle>() != null)
        {
            GetComponent<NavMeshObstacle>().enabled = false;
        }
       
        if (respawn)
        {
            Invoke("TriggerRespawn", timeTillRespawn);
        }
        
    }



    void OnTriggerEnter(Collider coll)
    {        
        if (coll.GetComponent<Collider>().tag == "Bullet" || coll.GetComponent<Collider>().tag == "EnemyBullet")
        {
            health--;
            if (health <= 0 && explodeable)
            {
                explode = true;
                explodeable = false;
            }
        }        
    }



    private void TriggerRespawn()
    {
        respawnScript.Respawn();       
        explodeable = true;
        health = hitsTillExplosion;
    }

    /// <summary>
    /// Spawns the line cut power up.
    /// </summary>
    protected void SpawnPowerUp()
    {
        // Spawn based on the probability
        if (powerUpPrefab != null && Random.value < powerUpProbability)
        {
            // Instantiate
            Instantiate(powerUpPrefab, transform.position, powerUpPrefab.transform.rotation);
        }
    }

    void OnDisable()
    {
        CancelInvoke();
    }
}

