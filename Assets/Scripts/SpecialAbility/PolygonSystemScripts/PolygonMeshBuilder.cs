using UnityEngine;
using System.Collections;

public class PolygonMeshBuilder : MonoBehaviour {

    #region variable
    private PolygonProperties polygonProperties;
    #endregion

    #region property
    public PolygonProperties PolygonProperties
    {
        set { polygonProperties = value; }
    }
    #endregion

    #region methods

    #region updateMeshInformation (new vertices, indices, normals, ...)

    public void UpdateMeshInformations(GameObject[] playerGameObjects, Vector3 middlePoint, MeshFilter[] meshFilters, Mesh[] polygonMeshes, int donkey, float[] polygonPartHeightOffsets)
    {
        Vector3[] vbot;
        Vector3[] vtop;
        int jumper = 1;

        if (donkey != -1)
        {
            vbot = new Vector3[playerGameObjects.Length];
            vtop = new Vector3[playerGameObjects.Length];

            vbot[playerGameObjects.Length - 1] = middlePoint;
            vtop[playerGameObjects.Length - 1] = middlePoint + polygonProperties.polygonThickness;

        }
        else if (playerGameObjects.Length == 2)
        {
            jumper = 2;

            Vector3 direction = playerGameObjects[0].transform.position - playerGameObjects[1].transform.position;
            Vector3 pos = (playerGameObjects[0].transform.position + playerGameObjects[1].transform.position) * 0.5f;
            Vector3 pointA = pos + (Quaternion.Euler(0, -90, 0) * direction * 0.5f);
            Vector3 pointB = pos + (Quaternion.Euler(0, 90, 0) * direction * 0.5f);

            vbot = new Vector3[playerGameObjects.Length + 3];
            vtop = new Vector3[playerGameObjects.Length + 3];

            vbot[1] = pointA + polygonProperties.heightOffset;
            vtop[1] = pointA + polygonProperties.heightOffset;

            vbot[3] = pointB + polygonProperties.heightOffset;
            vtop[3] = pointB + polygonProperties.heightOffset;

            vbot[playerGameObjects.Length + 2] = middlePoint;
            vtop[playerGameObjects.Length + 2] = middlePoint + polygonProperties.polygonThickness;
        }
        else
        {
            vbot = new Vector3[playerGameObjects.Length + 1];
            vtop = new Vector3[playerGameObjects.Length + 1];

            vbot[playerGameObjects.Length] = middlePoint;
            vtop[playerGameObjects.Length] = middlePoint + polygonProperties.polygonThickness;
        }

        int index = 0;
        for (int i = 0; i < playerGameObjects.Length; i++)
        {
            if (i != donkey)
            {
                vbot[index * jumper] = playerGameObjects[i].transform.position + polygonProperties.heightOffset;
                vtop[index * jumper] = playerGameObjects[i].transform.position + polygonProperties.heightOffset + polygonProperties.polygonThickness;
                index++;
            }
        }
        ApplyMeshChanges(vbot, vtop, meshFilters, polygonMeshes, polygonPartHeightOffsets);
    }


    private void ApplyMeshChanges(Vector3[] vbot, Vector3[] vtop, MeshFilter[] meshFilters, Mesh[] polygonMeshes, float[] polygonPartHeightOffsets)
    {
        int polyPartsIndex = 4;
        if (vbot.Length == 4)
        {
            polyPartsIndex = 3;
            polygonMeshes[3].Clear();
        }

        for (int i = 0; i < polyPartsIndex; i++)
        {
            polygonMeshes[i].vertices = new Vector3[] { new Vector3(vbot[i].x, vbot[i].y + polygonPartHeightOffsets[i], vbot[i].z), new Vector3(vbot[(i + 1) % polyPartsIndex].x, vbot[(i + 1) % polyPartsIndex].y + polygonPartHeightOffsets[i], vbot[(i + 1) % polyPartsIndex].z), new Vector3(vbot[polyPartsIndex].x, vbot[polyPartsIndex].y + polygonPartHeightOffsets[i], vbot[polyPartsIndex].z), new Vector3(vtop[i].x, vtop[i].y + polygonPartHeightOffsets[i], vtop[i].z), new Vector3(vtop[(i + 1) % polyPartsIndex].x, vtop[(i + 1) % polyPartsIndex].y + polygonPartHeightOffsets[i], vtop[(i + 1) % polyPartsIndex].z), new Vector3(vtop[polyPartsIndex].x, vtop[polyPartsIndex].y + polygonPartHeightOffsets[i], vtop[polyPartsIndex].z) };
            polygonMeshes[i].normals = new Vector3[] { Vector3.up, Vector3.up, Vector3.up, Vector3.up, Vector3.up, Vector3.up };
            polygonMeshes[i].uv = new Vector2[] { new Vector2(0, 0), new Vector2(0, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 1), new Vector2(1, 0.5f), new Vector2(1, 1) };
            polygonMeshes[i].SetTriangles(polygonProperties.meshIndices, 0);
            polygonMeshes[i].Optimize();
            polygonMeshes[i].RecalculateBounds();
            meshFilters[i].sharedMesh = polygonMeshes[i];
        }
    }
    #endregion

    #region clearMeshes
    public void ClearMeshes(MeshFilter[] meshFilter, Mesh[] meshes)
    {
        for(int i = 0; i < meshes.Length; i++)
        {
            meshes[i].Clear();
            meshFilter[i].sharedMesh = null;
            meshFilter[i].sharedMesh = meshes[i];
        }

    }
    #endregion

    #endregion
}
