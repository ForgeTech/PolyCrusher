using UnityEngine;

public class PolyExplosion : MonoBehaviour {

    #region variables
    private int vertexCount;
    protected int step;
    protected int grandStep;
    protected float scaleFactor;
    protected float upwardsModifier = 6.0f;
    protected float explosionForce = 50.0f;
    protected float minimumAliveTime = 5.5f;
    protected float maximumAliveTime = 10.0f;
    protected bool changeForwardVector = false;
    protected bool bulletKill = false;
    protected bool useGravity = true;
    protected float drag = 0.0f;
    protected float angularDrag = 0.0f;
    protected Vector3 explosionOrigin;
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
        explosionOrigin = transform.position;

        MR = GetComponentInChildren<SkinnedMeshRenderer>();
        M = MR.sharedMesh;
        verts = M.vertices;
        normals = M.normals;
        uvs = M.uv;
        vertexCount = M.vertexCount;
       
        step = vertexCount / 90;
        scaleFactor = 8;
        while (step % 3 != 0)
        {
            step++;
        }
        grandStep = step * 12;       
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

                    
                    
                    deactivator.attachedRenderer.material = MR.materials[submesh];                   
                    deactivator.attachedFilter.mesh = mesh;

                    GO.transform.position = transform.position;
                    GO.transform.rotation = transform.rotation;
                    GO.transform.localScale = new Vector3(MR.transform.localScale.x * scaleFactor, MR.transform.localScale.y, MR.transform.localScale.z * scaleFactor);

                    if (changeForwardVector)
                    {
                        GO.transform.forward = transform.up;
                    }

                    GO.AddComponent<BoxCollider>();

                    deactivator.attachedRigid.AddExplosionForce(explosionForce, explosionOrigin, 50, upwardsModifier);
                    deactivator.attachedRigid.useGravity = useGravity;
                    deactivator.attachedRigid.drag = drag;
                    deactivator.attachedRigid.angularDrag = angularDrag;

                    deactivator.TriggerDeactivation(Random.Range(minimumAliveTime, maximumAliveTime));
                }
            }
        }
        Destroy(this);
    }

    #endregion
}
