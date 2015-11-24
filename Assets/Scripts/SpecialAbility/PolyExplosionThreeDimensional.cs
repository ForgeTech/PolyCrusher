using UnityEngine;
using System.Collections.Generic;

public class PolyExplosionThreeDimensional : MonoBehaviour
{



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
    private Vector3 scaleFactor;
    private Pool pool;
    

    private int matchedIndex;

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

        
        

    }

    // Update is called once per frame
    void FixedUpdate()
    {

        if (explode)
        {


            explode = false;
            ExplodePartial(0);
            
            //part2 = true;
        }
    
    }


    private void GetTriangles(int[] indices)
    {
        triList = new List<Tri>();
        for(int i = 0; i <indices.Length; i+=3)
        {
            triList.Add(new Tri(indices[i], indices[i + 1], indices[i + 2]));
        }
    }


    private bool GetMatchingTri(int x, int y, int z)
    {
        foreach(Tri tri in triList)
        {
            if(((tri.x == x && tri.y ==y)|| (tri.x == y && tri.y == x))&& tri.z!=z) {// || (tri.x == x && tri.z == y) || (tri.x == y && tri.z == x)||(tri.y ==x && tri.z == y) || (tri.y ==y && tri.z == x))
                
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

            if(((tri.y == x && tri.z == y) || (tri.y == y && tri.z == x)) && tri.x != z)
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

            for (int i = triList.Count-1; i>= 0; i--) //grandStep)
            {
                Tri tri = triList[i];
                 
                Vector3 direction1 = Vector3.Normalize(-normals[tri.x]);
                Vector3 direction2;
                int[] triangles= new int[0];

                mesh = new Mesh();
                Vector2 uvZero = new Vector2(0, 0);

                if (GetMatchingTri(tri.x, tri.y, tri.z))
                {
                    newVerts = new Vector3[8];
                    newNormals = new Vector3[8];
                    newUvs = new Vector2[8];
                    direction2 = Vector3.Normalize(-normals[matchedIndex]);


                    //for (int n = 0; n < 3; n++)
                    //{
                    //    int index = indices[i + n];
                    //    newVerts[n] = verts[index];
                    //    newUvs[n] = uvs[index];
                    //    newNormals[n] = normals[index];

                    //    newVerts[n + 4] = newVerts[n]+ direction1;
                    //    newUvs[n + 4] = uvZero;
                    //    newNormals[n + 4] = normals[index];
                    //}

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
                    newUvs[4] = uvZero;
                    newNormals[4] = newNormals[0];

                    newVerts[5] = newVerts[1] + direction1;
                    newUvs[5] = uvZero;
                    newNormals[5] = newNormals[1];

                    newVerts[6] = newVerts[2] + direction1;
                    newUvs[6] = uvZero;
                    newNormals[6] = newNormals[2];


                    newVerts[7] = newVerts[3] + direction2;
                    newUvs[7] = uvZero;
                    newNormals[7] = newNormals[3];

                    triangles = new int[] { 0, 1, 2,  3,1,0, /*up tris*/  6,4,2,   0,2,4,  /*side1 */ 5,2,1,  2,5,6,  /*side2*/ 0,4,7,  7,3,0, /*side3*/ 7,5,1, 7,1,3, /*side4*/ 6,5,4,  4,5,7  /*down*/};

                }
                else
                {
                    newVerts = new Vector3[6];
                    newNormals = new Vector3[6];
                    newUvs = new Vector2[6];


                    //for (int n = 0; n < 3; n++)
                    //{
                    //    int index = indices[i + n];
                    //    newVerts[n] = verts[index];
                    //    newUvs[n] = uvs[index];
                    //    newNormals[n] = normals[index];

                    //    newVerts[n + 3] = newVerts[n] + direction1;
                    //    newUvs[n + 3] = uvs[index];
                    //    newNormals[n + 3] = normals[index];
                    //}


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
                    newUvs[3] = uvZero;
                    newNormals[3] = newNormals[0];

                    newVerts[4] = newVerts[1] + direction1;
                    newUvs[4] = uvZero;
                    newNormals[4] = newNormals[1];

                    newVerts[5] = newVerts[2] + direction1;
                    newUvs[5] = uvZero;
                    newNormals[5] = newNormals[2];


                    triangles = new int[] { 0, 1, 2, /*up*/   4, 1, 0,  0, 3, 4,  /*side1*/  0, 2, 5,    1, 4, 5, /*side2*/     5, 4, 3,   5, 3, 0,   /*side3*/  5, 2, 1 /*down*/};

                }

                


               


                //newVerts[0] = new Vector3(0, 0, 0);
                //newNormals[0] = normals[indices[i]];
                //newUvs[0] = uvs[indices[i]];

              
              
               

               

               
                

                
                mesh.vertices = newVerts;
                mesh.normals = newNormals;
                mesh.uv = newUvs;
                //Debug.Log(newVerts[0] + "  " + newVerts[1]);
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



                    GO.AddComponent<MeshCollider>();
                    GO.GetComponent<MeshCollider>().convex = true;

                    //deactivator.attachedRigid.AddExplosionForce(0, new Vector3(transform.position.x, transform.position.y, transform.position.z), 1, 0.0f,ForceMode.Force);
                    deactivator.enabled = false;



                }




            }
        }
        //MR.enabled = false;







    }

}

