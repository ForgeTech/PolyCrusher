using UnityEngine;


public class PolyExplosion : MonoBehaviour {


    
    public GameObject explosion;
    public bool explode = false;
    private bool part2 = false;
    private bool part3 = false;
    private bool part4 = false;
    private bool part5 = false;
    private bool part6 = false;
    private bool part7 = false;
    private bool part8 = false;
    private bool part9 = false;
    private bool part10 = false;

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

    // Use this for initialization
    void Start () {
        explode = true;
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
    }
	
	// Update is called once per frame
	void FixedUpdate () {

        if (explode)
        { 

            explode = false;
            ExplodePartial(0);
            part2 = true;
        }else
        {
            if (part2)
            {
                part2 = false;
                ExplodePartial(step);
                part3 = true;

            }else
            {
                if (part3)
                {
                    part3 = false;
                    ExplodePartial(step*2);
                    part4 = true;
                }else
                {
                    if (part4)
                    {
                        part4 = false;
                        ExplodePartial(step*3);
                        part5 = true;
                    }else
                    {
                        if (part5)
                        {
                            part5 = false;
                            ExplodePartial(step*4);
                            part6 = true;

                        }else
                        {
                            if (part6)
                            {
                                part6 = false;
                                ExplodePartial(step * 5);
                                part7 = true;


                            }
                            else
                            {
                                if (part7)
                                {
                                    part7 = false;
                                    ExplodePartial(step * 6);
                                    part8 = true;

                                }else
                                {

                                    if (part8)
                                    {
                                        part8 = false;
                                        ExplodePartial(step * 7);
                                        part9 = true;

                                    }else
                                    {
                                        if (part9)
                                        {
                                            part9 = false;
                                            ExplodePartial(step * 8);
                                            part10 = true;
                                        }else
                                        {
                                            if (part10)
                                            {
                                                part10 = false;
                                                ExplodePartial(step * 9);
                                                Destroy(gameObject);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
	}


    private void ExplodePartial(int start)
    {
        for (int submesh = 0; submesh < M.subMeshCount; submesh++)
        {
            int[] indices = M.GetTriangles(submesh);

            for (int i = start; i < indices.Length; i += grandStep)
            {
                newVerts = new Vector3[3];
                newNormals = new Vector3[3];
                newUvs = new Vector2[3];
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

                mesh.triangles = new int[] { 0, 1, 2, 2, 1, 0 };

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
}
