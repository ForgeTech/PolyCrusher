using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CutUpMesh : MonoBehaviour {


    public bool explode = false;   

    private int vertexCount;
    private int step;
    private int grandStep;
    private Vector3 scaleFactor;
    private Pool pool;

    private int[] vertexPosChange;

    private int matchedIndex;


    public Material insideMeshMaterial;

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
    SkinnedMeshRenderer SMR;
    Mesh M;
    Vector3[] verts;
    Vector3[] normals;
    Vector2[] uvs;
    Vector2[] uvs2;
    Color[] colors;

    GameObject GO;
    Mesh mesh;
    Vector3[] newVerts;
    Vector3[] newNormals;
    Vector2[] newUvs;


    GameObject upper;

    GameObject lower;

    public float cutHeight;

    private int upperVertices;
    private List<Vector3> upperVertexList = new List<Vector3>();
    private List<Vector3> lowerVertexList = new List<Vector3>();

    private Vector3[] upVertices;
    private Vector3[] lowVertices;



    private List<int> lowerIndices;
    private List<int> upperIndices;

    private List<Vector2> lowUVs;
    private List<Vector2> upperUVs;

    private List<Vector2> lowUVs2;
    private List<Vector2> upperUVs2;




    private List<Color> lowColors;
    private List<Color> upperColors;


    private List<int>[] lowIndices;
    private List<int>[] upIndices;

    private Material[] lowMaterials;
    private Material[] upMaterials;


    private List<Vector3> lowNormals;
    private List<Vector3> upNormals;

    private Transform root;



    private int lowerVertices;

    private BasePlayer playerScript;
    private BaseEnemy enemyScript;


    private Vector3 cutPoint;


    void Start()
    {

        cutPoint = new Vector3(0, cutHeight, 0);
       
        playerScript = GetComponent<BasePlayer>();
        enemyScript = GetComponent<BaseEnemy>();

        insideMeshMaterial = Resources.Load("Material/MeshSplit/MeshInsideMaterial", typeof(Material)) as Material;
        pool = Pool.current;
        //explode = true;

        if (GetComponent<MeshFilter>() != null)
        {
            MF = GetComponent<MeshFilter>();
            M = MF.mesh;
        }
        else if(GetComponentInChildren<MeshFilter>() != null)
        {
            MF = GetComponentInChildren<MeshFilter>();
            M = MF.mesh;
        }
        else
        {
            SMR = GetComponentInChildren<SkinnedMeshRenderer>();
            if (SMR != null)
            {
                Mesh old = new Mesh();
                SMR.BakeMesh(old);
                M = (Mesh)Instantiate(old);
            }
            else
            {
                Debug.Log("smr notfound");
            }
            

        }

      
        verts = M.vertices;
        normals = M.normals;
        uvs = M.uv;
        uvs2 = M.uv2;
        vertexCount = M.vertexCount;
        colors = M.colors;

        transform.InverseTransformDirection(cutPoint);

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


        vertexPosChange = new int[vertexCount];

        lowerIndices = new List<int>();
        upperIndices = new List<int>();


        lowUVs = new List<Vector2>();
        upperUVs = new List<Vector2>();

        lowUVs2 = new List<Vector2>();
        upperUVs2 = new List<Vector2>();


        lowColors = new List<Color>();
        upperColors = new List<Color>();


        lowIndices = new List<int>[M.subMeshCount + 1];
        upIndices = new List<int>[M.subMeshCount + 1];

        upMaterials = new Material[SMR.sharedMaterials.Length + 1];
        lowMaterials = new Material[SMR.sharedMaterials.Length + 1];


        for (int i = 0; i < lowIndices.Length; i++)
        {
            lowIndices[i] = new List<int>();
            upIndices[i] = new List<int>();
            if(i< lowIndices.Length - 1)
            {
                upMaterials[i] = SMR.sharedMaterials[i];
                lowMaterials[i] = SMR.sharedMaterials[i];
            }
            else
            {
                upMaterials[i] = insideMeshMaterial;
                lowMaterials[i] = insideMeshMaterial;
            }
            

        }

        

        

        //upMaterials = SMR.sharedMaterials;
        //lowMaterials = SMR.sharedMaterials;

        upNormals = new List<Vector3>();
        lowNormals = new List<Vector3>();

        root = SMR.rootBone;
        

    }
    // Update is called once per frame
    void Update () {
	
	}


    void DetermineVertexPositions(Vector3[] vertices)
    {
        for(int i=0; i < vertices.Length; i++)
        {
            if(vertices[i].y <= cutHeight)
            {
                lowerVertices++;
                lowerVertexList.Add(vertices[i]);
                lowUVs.Add(uvs[i]);
                //lowUVs2.Add(uvs2[i]);
                lowNormals.Add(normals[i]);
                vertexPosChange[i] = lowerVertexList.Count-1;
               
            }
            else
            {
                upperVertices++;
                upperVertexList.Add(vertices[i]);
                upperUVs.Add(uvs[i]);
                //upperUVs2.Add(uvs2[i]);
                upNormals.Add(normals[i]);
                vertexPosChange[i] = upperVertexList.Count - 1;
            }
        }

    }



    void printUvs()
    {
        foreach(Vector2 v in lowUVs)
        {
            Debug.Log(v);


        }

    }


    void FixedUpdate()
    {
        if (explode)
        {

            explode = false;
            //ExplodePartial(0);
            //if(MF!=null)
            SplitInTwo();
            SMR.enabled = false;

            if(playerScript != null)
            {
                playerScript.InstantKill();
            }
            else if(enemyScript!= null)
            {
                enemyScript.InstantKill();
            }
            else
            {
                Destroy(gameObject);
            }   


        }
    }


    private int[] ApplyIndexChange(List<int> indices, int[] lookUp)
    {
        int[] indexArray = new int[indices.Count];
        for(int i = 0; i < indices.Count; i++)
        {
            indexArray[i] = lookUp[indices[i]];
        }

        return indexArray;
    }


    private void SplitInTwo()
    {
        float start = Time.realtimeSinceStartup;
        float end;

        //ArrayList indexList = new ArrayList();
       
        int above = 0;
        int below = 0;

        
       // DetermineVertexPositions(M.vertices);
        Vector3[] vertices = M.vertices;
        //int[] indices;

        for (int submesh = 0; submesh < M.subMeshCount; submesh++)
        {
            int[] subMeshIndices = M.GetIndices(submesh);

            for (int i = 0; i < subMeshIndices.Length; i+=3)
            {
                above = 0;
                below = 0;
               
               
                if(vertices[subMeshIndices[i]].y <= cutHeight)
                {
                    below++;
                }
                else
                {
                    above++;
                }

                if (vertices[subMeshIndices[i+1]].y <= cutHeight)
                {
                    below++;
                }
                else
                {
                    above++;
                }
                if (vertices[subMeshIndices[i+2]].y <= cutHeight)
                {
                    below++;
                }
                else
                {
                    above++;
                }

                if(above != 3 && below != 3)
                {
                    //allAbove++;
                    //triList.RemoveAt(i);
                    //trisToRemove++;

                }
                else
                {
                    if (above == 3)
                    {
                        upIndices[submesh].Add(subMeshIndices[i]);
                        upIndices[submesh].Add(subMeshIndices[i+1]);
                        upIndices[submesh].Add(subMeshIndices[i+2]);

                        upIndices[upIndices.Length - 1].Add(subMeshIndices[i+2]);
                        upIndices[upIndices.Length - 1].Add(subMeshIndices[i + 1]);
                        upIndices[upIndices.Length - 1].Add(subMeshIndices[i]);


                    }
                    else
                    {
                        lowIndices[submesh].Add(subMeshIndices[i]);
                        lowIndices[submesh].Add(subMeshIndices[i + 1]);
                        lowIndices[submesh].Add(subMeshIndices[i + 2]);

                        lowIndices[lowIndices.Length - 1].Add(subMeshIndices[i + 2]);
                        lowIndices[lowIndices.Length - 1].Add(subMeshIndices[i + 1]);
                        lowIndices[lowIndices.Length - 1].Add(subMeshIndices[i]);

                    }
                    //triInverse.x = tri.z;
                    //triInverse.y = tri.y;
                    //triInverse.z = tri.x;
                    //triList.Add(triInverse);


                }
            }

            //indices = new int[triList.Count * 3];

            //for (int i = 0; i < triList.Count; i++)
            //{
            //    Tri tri = triList[i];
            //    indices[i*3] = tri.x;
            //    indices[i*3 + 1] = tri.y;
            //    indices[i*3 + 2] = tri.z;
            //}

            //M.SetIndices(indices, MeshTopology.Triangles, submesh);
        }




        DetermineVertexPositions(vertices);


       
       



        upper = pool.getPooledObject();
        upper.transform.position = transform.position;
        upper.transform.rotation = transform.rotation;
        upper.transform.localScale = transform.localScale;

       

        upper.SetActive(true);
        upper.name = "upper";


        Mesh mesh = new Mesh();
        mesh.SetVertices(upperVertexList);
        //mesh.SetIndices(ApplyIndexChange(upperIndices, vertexPosChange), MeshTopology.Triangles, 0);
        mesh.subMeshCount = upIndices.Length;
        for(int i =0; i < upIndices.Length; i++)
        {
            mesh.SetIndices(ApplyIndexChange(upIndices[i], vertexPosChange), MeshTopology.Triangles, i);
        }

        mesh.SetNormals(upNormals);
        mesh.SetUVs(0,upperUVs);
        //mesh.SetUVs(1, upperUVs2);

        //mesh.SetColors(upperColors);


        Deactivator deactivator = upper.GetComponent<Deactivator>();

        //deactivator.attachedRenderer = null;

        Destroy(upper.GetComponent<MeshFilter>());
        Destroy(upper.GetComponent<MeshRenderer>());
        SkinnedMeshRenderer upSMR = upper.AddComponent<SkinnedMeshRenderer>();
        upSMR.sharedMesh = mesh;
        upSMR.sharedMaterials = upMaterials;
        
        upSMR.sharedMesh.RecalculateBounds();
        upSMR.rootBone = upper.transform;



        BoxCollider box = upper.AddComponent<BoxCollider>();
        box.center = upSMR.sharedMesh.bounds.center;
        box.size = upSMR.sharedMesh.bounds.size;






        lower = pool.getPooledObject();
        lower.transform.position = transform.position;
        lower.transform.rotation = transform.rotation;
        lower.transform.localScale = transform.localScale;

        

        lower.SetActive(true);
        lower.name = "lower";


        Mesh meshLow = new Mesh();
        meshLow.SetVertices(lowerVertexList);
        meshLow.subMeshCount = lowIndices.Length;
        //meshLow.SetIndices(ApplyIndexChange(lowerIndices, vertexPosChange), MeshTopology.Triangles, 0);

        for (int i = 0; i < lowIndices.Length; i++)
        {
            meshLow.SetIndices(ApplyIndexChange(lowIndices[i], vertexPosChange), MeshTopology.Triangles, i);
            Debug.Log("indices at submesh: " + i + " : " + lowIndices[i].Count);
        }

        meshLow.SetNormals(lowNormals);
        meshLow.SetUVs(0, lowUVs);
        //mesh.SetUVs(1, lowUVs2);

        //meshLow.SetColors(lowColors);
        Deactivator deactivatorLow = lower.GetComponent<Deactivator>();


        Destroy(lower.GetComponent<MeshRenderer>());
        SkinnedMeshRenderer lowSMR = lower.AddComponent<SkinnedMeshRenderer>();
        lowSMR.sharedMaterials = lowMaterials;
       
        lowSMR.sharedMesh = meshLow;
        lowSMR.sharedMesh.RecalculateBounds();
        
        BoxCollider boxLow = lower.AddComponent<BoxCollider>();
        boxLow.center = lowSMR.sharedMesh.bounds.center;
        boxLow.size = lowSMR.sharedMesh.bounds.size;



        end = Time.realtimeSinceStartup;
        Debug.Log("Split In Two method took: " + (end - start) + " seconds");
    
    }

  



    private bool GetMatchingTri(int x, int y, int z)
    {
        foreach (Tri tri in triList)
        {
            if (((tri.x == x && tri.y == y) || (tri.x == y && tri.y == x)) && tri.z != z)
            {
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



   


}
