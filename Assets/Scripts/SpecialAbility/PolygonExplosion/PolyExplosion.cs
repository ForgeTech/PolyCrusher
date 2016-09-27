using UnityEngine;
using System.Collections;

public class PolyExplosion : MonoBehaviour {

    #region variables
    private int vertexCount;
    protected int step;
    protected int grandStep;
    protected float scaleFactor;
    protected float upwardsModifier = 10.0f;
    protected float explosionForce = 50.0f;
    private string pooledObjectName = "FragmentObject";
    private string layerName = "Fragments";

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
    public virtual void Start ()
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
        scaleFactor = 12*step/ (step* step);
        while (step % 3 != 0)
        {
            step++;
        }
        grandStep = step * 12;
       
        //ExplodePartial(Random.Range(0,6));
    }
       
    public virtual void ExplodePartial(int start)
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
                                       
                    GO.layer = LayerMask.NameToLayer(layerName);

                    GO.transform.position = transform.position;
                    GO.transform.rotation = transform.rotation;
                    GO.transform.localScale = new Vector3(MR.transform.localScale.x*scaleFactor, MR.transform.localScale.y, MR.transform.localScale.z*scaleFactor);
                    
                    deactivator.attachedRenderer.material = MR.materials[submesh];                   
                    deactivator.attachedFilter.mesh = mesh;

                    GO.AddComponent<BoxCollider>();

                    deactivator.attachedRigid.AddExplosionForce(explosionForce, new Vector3(transform.position.x, transform.position.y, transform.position.z), 50, upwardsModifier);                   
                    deactivator.TriggerDeactivation(Random.Range(5.5f, 10.0f));
                }
            }
        }

        Destroy(this);
    }

    #endregion
}
