using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// This is a an optimized class. :P
/// </summary>
public class CutUpMesh : MonoBehaviour
{
    public bool explode = false;

    private int vertexCount;
    private int step;
    private int grandStep;
   
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

    MeshFilter MF;
    MeshRenderer MR;
    SkinnedMeshRenderer SMR;
    Mesh M;    
    Vector3[] normals;
    Vector2[] uvs;

    GameObject GO;
    Mesh mesh;
    Vector3[] newVerts;
    Vector3[] newNormals;
    Vector2[] newUvs;

    GameObject upper;
    GameObject lower;

    private float cutHeight = 0.9f;

    private int upperVertices;
    private List<Vector3> upperVertexList = new List<Vector3>();
    private List<Vector3> lowerVertexList = new List<Vector3>();

    private Vector3[] upVertices;
    private Vector3[] lowVertices;

    private List<Vector2> lowUVs;
    private List<Vector2> upperUVs;

    private List<int>[] lowIndices;
    private List<int>[] upIndices;

    private Material[] lowMaterials;
    private Material[] upMaterials;

    private List<Vector3> lowNormals;
    private List<Vector3> upNormals;

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

        if (GetComponent<MeshFilter>() != null)
        {
            MF = GetComponent<MeshFilter>();
            M = MF.mesh;
        }
        else if (GetComponentInChildren<MeshFilter>() != null)
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
        normals = M.normals;
        uvs = M.uv;
       
        vertexCount = M.vertexCount;
       
        SMR.transform.InverseTransformDirection(cutPoint);

        vertexPosChange = new int[vertexCount];

        lowUVs = new List<Vector2>();
        upperUVs = new List<Vector2>();

        lowIndices = new List<int>[M.subMeshCount + 1];
        upIndices = new List<int>[M.subMeshCount + 1];

        upMaterials = new Material[SMR.sharedMaterials.Length + 1];
        lowMaterials = new Material[SMR.sharedMaterials.Length + 1];

        for (int i = 0; i < lowIndices.Length; i++)
        {
            lowIndices[i] = new List<int>();
            upIndices[i] = new List<int>();
            if (i < lowIndices.Length - 1)
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

        upNormals = new List<Vector3>();
        lowNormals = new List<Vector3>();
        explode = true;
    }

    void DetermineVertexPositions(Vector3[] vertices)
    {
        for (int i = 0; i < vertices.Length; i++)
        {
            if (vertices[i].y <= cutHeight)
            {
                lowerVertices++;
                lowerVertexList.Add(vertices[i]);
                lowUVs.Add(uvs[i]);

                lowNormals.Add(normals[i]);
                vertexPosChange[i] = lowerVertexList.Count - 1;

            }
            else
            {
                upperVertices++;
                upperVertexList.Add(vertices[i]);
                upperUVs.Add(uvs[i]);

                upNormals.Add(normals[i]);
                vertexPosChange[i] = upperVertexList.Count - 1;
            }
        }
    }

    void FixedUpdate()
    {
        if (explode)
        {

            explode = false;

            SplitInTwo();
            SMR.enabled = false;

            if (playerScript != null)
            {
                playerScript.InstantKill(this);
            }
            else if (enemyScript != null)
            {
                enemyScript.InstantKill(this);
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
        for (int i = 0; i < indices.Count; i++)
        {
            indexArray[i] = lookUp[indices[i]];
        }

        return indexArray;
    }

    private void SplitInTwo()
    {
        int above = 0;
        int below = 0;

        Vector3[] vertices = M.vertices;

        for (int submesh = 0; submesh < M.subMeshCount; submesh++)
        {
            int[] subMeshIndices = M.GetIndices(submesh);

            for (int i = 0; i < subMeshIndices.Length; i += 3)
            {
                above = 0;
                below = 0;

                if (vertices[subMeshIndices[i]].y <= cutHeight)
                {
                    below++;
                }
                else
                {
                    above++;
                }

                if (vertices[subMeshIndices[i + 1]].y <= cutHeight)
                {
                    below++;
                }
                else
                {
                    above++;
                }
                if (vertices[subMeshIndices[i + 2]].y <= cutHeight)
                {
                    below++;
                }
                else
                {
                    above++;
                }

                if (above != 3 && below != 3)
                {


                }
                else
                {
                    if (above == 3)
                    {
                        upIndices[submesh].Add(subMeshIndices[i]);
                        upIndices[submesh].Add(subMeshIndices[i + 1]);
                        upIndices[submesh].Add(subMeshIndices[i + 2]);

                        upIndices[upIndices.Length - 1].Add(subMeshIndices[i + 2]);
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
                }
            }
        }
        DetermineVertexPositions(vertices);

        if(upperVertices != 0)
        {
            upper = pool.getPooledObject();
            upper.layer = LayerMask.NameToLayer("Fragments");
            upper.transform.position = transform.position;

            upper.transform.rotation = SMR.transform.rotation;
            upper.transform.localScale = transform.localScale;

            upper.SetActive(true);
            upper.name = "upper";

            Mesh mesh = new Mesh();
            mesh.SetVertices(upperVertexList);

            mesh.subMeshCount = upIndices.Length;
            for (int i = 0; i < upIndices.Length; i++)
            {
                mesh.SetIndices(ApplyIndexChange(upIndices[i], vertexPosChange), MeshTopology.Triangles, i);
            }

            mesh.SetNormals(upNormals);
            mesh.SetUVs(0, upperUVs);

            Deactivator deactivator = upper.GetComponent<Deactivator>();
            
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

            deactivator.attachedRigid.AddForceAtPosition(upper.transform.forward * 0.5f, upper.transform.position, ForceMode.Impulse);
        }

        if (lowerVertices != 0)
        {
            lower = pool.getPooledObject();
            lower.layer = LayerMask.NameToLayer("Fragments");
            lower.transform.position = transform.position;

            lower.transform.rotation = SMR.transform.rotation;
            lower.transform.localScale = transform.localScale;

            lower.SetActive(true);
            lower.name = "lower";

            Mesh meshLow = new Mesh();
            meshLow.SetVertices(lowerVertexList);
            meshLow.subMeshCount = lowIndices.Length;


            for (int i = 0; i < lowIndices.Length; i++)
            {
                meshLow.SetIndices(ApplyIndexChange(lowIndices[i], vertexPosChange), MeshTopology.Triangles, i);
            }

            meshLow.SetNormals(lowNormals);
            meshLow.SetUVs(0, lowUVs);

            Deactivator deactivatorLow = lower.GetComponent<Deactivator>();

            Destroy(lower.GetComponent<MeshFilter>());
            Destroy(lower.GetComponent<MeshRenderer>());
            SkinnedMeshRenderer lowSMR = lower.AddComponent<SkinnedMeshRenderer>();
            lowSMR.sharedMaterials = lowMaterials;

            lowSMR.sharedMesh = meshLow;
            lowSMR.sharedMesh.RecalculateBounds();

            BoxCollider boxLow = lower.AddComponent<BoxCollider>();
            boxLow.center = lowSMR.sharedMesh.bounds.center;
            boxLow.size = lowSMR.sharedMesh.bounds.size * 0.95f;

            deactivatorLow.attachedRigid.AddForceAtPosition(lower.transform.forward * 0.5f, lower.transform.position, ForceMode.Impulse);
        }
    }
}