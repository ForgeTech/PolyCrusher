using UnityEngine;
using System.Collections;

public class PolyExplosion : MonoBehaviour {

    #region variables
    private int vertexCount;
    private int step;
    private int grandStep;
    private float scaleFactor;
    private string pooledObjectName = "FragmentObject";

    SkinnedMeshRenderer MR;
    Mesh M;
    Vector3[] verts;
    Vector3[] normals;
    Vector2[] uvs;

    GameObject GO;
    Mesh mesh;
    Vector3[] newVerts;
    Vector3[] newNormals;
    Vector2[] newUvs;
    int[] triangles = new int[] { 0, 1, 2, 2, 1, 0 };
    #endregion

    #region methods
    // Use this for initialization
    void Start ()
    {
        newVerts = new Vector3[3];
        newNormals = new Vector3[3];
        newUvs = new Vector2[3];

        MR = GetComponentInChildren<SkinnedMeshRenderer>();
        M = MR.sharedMesh;
        verts = M.vertices;
        normals = M.normals;
        uvs = M.uv;
        vertexCount = M.vertexCount;
       
        step = vertexCount / 90;
        while(step % 3 != 0)
        {
            step++;
        }
        grandStep = step * 20;
        scaleFactor = 3+((step/3)* 0.28f);

        StartCoroutine(ExplodeOverTime());
    }


    private IEnumerator ExplodeOverTime()
    {
        for(int i = 0; i < 11; i++)
        {
            ExplodePartial(i);
            yield return null;
        }
        Destroy(this);
    }


    private void ExplodePartial(int start)
    {
        for (int submesh = 0; submesh < M.subMeshCount; submesh++)
        {
            int[] indices = M.GetTriangles(submesh);

            for (int i = start; i < indices.Length; i += grandStep)
            {
                for (int n = 0; n < 3; n++)
                {
                    int index = indices[i + n];
                    newVerts[n] = verts[index];
                    newUvs[n] = uvs[index];
                    newNormals[n] = normals[index];
                }
                mesh = new Mesh();
                mesh.vertices = newVerts;
                mesh.normals = newNormals;
                mesh.uv = newUvs;

                mesh.triangles = triangles;

                GO = ObjectsPool.Spawn(pooledObjectName, Vector3.zero, Quaternion.identity);
               

                if(GO != null)
                {
                    GO.SetActive(true);

                    Deactivator deactivator = GO.GetComponent<Deactivator>();
                                       
                    GO.layer = LayerMask.NameToLayer("Fragments");

                    GO.transform.position = transform.position;
                    GO.transform.rotation = transform.rotation;
                    GO.transform.localScale = new Vector3(MR.transform.localScale.x*scaleFactor, MR.transform.localScale.y, MR.transform.localScale.z*scaleFactor);
                    
                    deactivator.attachedRenderer.material = MR.materials[submesh];                   
                    deactivator.attachedFilter.mesh = mesh;

                    GO.AddComponent<BoxCollider>();

                    deactivator.attachedRigid.AddExplosionForce(50, new Vector3(transform.position.x, transform.position.y, transform.position.z), 50, 0.0f);                   
                    deactivator.TriggerDeactivation(Random.Range(5.5f, 10.0f));
                }
            }
        }
    }

    #endregion
}
