﻿using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MeshModifier : BaseMeshEffect
{
    [Range(0,50)]
    public float size = 30.0f;

    [SerializeField]
    private Material imageMaterial;

    [SerializeField]
    private Material textMaterial;
    
    private static readonly List<UIVertex> s_tempVertices = new List<UIVertex>();

    protected override void Start()
    {
        base.Start();
        Image img = gameObject.GetComponent<Image>();
        Text txt = gameObject.GetComponent<Text>();

        if (img != null)
        {
            Material instancedImageMaterial = new Material(imageMaterial);
            img.material = instancedImageMaterial;
        }
        else if (txt != null)
        {
            Material instancedTextMaterial = new Material(textMaterial);
            txt.material = instancedTextMaterial;
        }
    }

    public override void ModifyMesh(Mesh mesh)
    {
        using (VertexHelper vertexHelper = new VertexHelper(mesh))
        {
            vertexHelper.GetUIVertexStream(s_tempVertices);
        
        for (var i = 0; i <= s_tempVertices.Count - 3; i += 3)
        {
            UIVertex v0 = s_tempVertices[i + 0];
            UIVertex v1 = s_tempVertices[i + 1];
            UIVertex v2 = s_tempVertices[i + 2];
            
            var xy0 = new Vector2(v0.position.x, v0.position.y);
            var xy1 = new Vector2(v1.position.x, v1.position.y);
            var xy2 = new Vector2(v2.position.x, v2.position.y);
            // build two vectors
            Vector2 deltaA = (xy1 - xy0).normalized;
            Vector2 deltaB = (xy2 - xy1).normalized;
            Vector2 vecUvX;
            Vector2 vecUvY;
            Vector2 vecX;
            Vector2 vecY;
            // calculate UV vectors for the X and Y axes
            if (Mathf.Abs(Vector2.Dot(deltaA, Vector2.right)) > Mathf.Abs(Vector2.Dot(deltaB, Vector2.right)))
            {
                vecX = xy1 - xy0;
                vecY = xy2 - xy1;
                vecUvX = v1.uv0 - v0.uv0;
                vecUvY = v2.uv0 - v1.uv0;
            }
            else
            {
                vecX = xy2 - xy1;
                vecY = xy1 - xy0;
                vecUvX = v2.uv0 - v1.uv0;
                vecUvY = v1.uv0 - v0.uv0;
            }
            // retrieve UV minimum and maximum
            Vector2 uvMin = Min(v0.uv0, v1.uv0, v2.uv0);
            Vector2 uvMax = Max(v0.uv0, v1.uv0, v2.uv0);
            // also retrieve the XY mininum and maximum
            float xMin = Min(v0.position.x, v1.position.x, v2.position.x);
            float yMin = Min(v0.position.y, v1.position.y, v2.position.y);
            float xMax = Max(v0.position.x, v1.position.x, v2.position.x);
            float yMax = Max(v0.position.y, v1.position.y, v2.position.y);
            var xyMin = new Vector2(xMin, yMin);
            var xyMax = new Vector2(xMax, yMax);
            // store UV min. and max. in the tangent of each vertex
            var tangent = new Vector4(uvMin.x, uvMin.y, uvMax.x, uvMax.y);
            // calculate center of UV and pos
            Vector2 xyCenter = (xyMin + xyMax) * 0.5f;
            // we need the vector lengths inside our loop, precalculate them here
            float vecXLen = vecX.magnitude;
            float vecYLen = vecY.magnitude;
            // now manipulate each vertex
            for (var v = 0; v < 3; ++v)
            {
                UIVertex vertex = s_tempVertices[i + v];
                // extrude each vertex to the outside 'm_size' pixels wide.
                // we need the extrude to create more space for the glow
                var posOld = new Vector2(vertex.position.x, vertex.position.y);
                Vector2 posNew = posOld;
                float addX = (vertex.position.x > xyCenter.x) ? size : -size;
                float addY = (vertex.position.y > xyCenter.y) ? size : -size;
                float signX = Vector2.Dot(vecX, Vector2.right) > 0 ? 1 : -1;
                float signY = Vector2.Dot(vecY, Vector2.up) > 0 ? 1 : -1;
                posNew.x += addX;
                posNew.y += addY;
                vertex.position = new Vector3(posNew.x, posNew.y, 0);
                // re-calculate UVs accordingly to prevent scaled texts
                Vector2 uvOld = vertex.uv0;
                vertex.uv0 += vecUvX / vecXLen * addX * signX;
                vertex.uv0 += vecUvY / vecYLen * addY * signY;
                // set the tangent so we know the UV boundaries. We use this to
                // prevent smearing into other characters in the texture atlas
                vertex.tangent = tangent;
                s_tempVertices[i + v] = vertex;
            }
        }
            vertexHelper.AddUIVertexTriangleStream(s_tempVertices);
            vertexHelper.FillMesh(mesh);
        }
    }

#if UNITY_EDITOR
    protected override void OnValidate()
    {
        base.OnValidate();
        graphic.SetVerticesDirty();
    }
#endif

    private static float Min(float _a, float _b, float _c)
    {
        return Mathf.Min(_a, Mathf.Min(_b, _c));
    }
    private static float Max(float _a, float _b, float _c)
    {
        return Mathf.Max(_a, Mathf.Max(_b, _c));
    }
    private static Vector2 Min(Vector2 _a, Vector2 _b, Vector2 _c)
    {
        return new Vector2(Min(_a.x, _b.x, _c.x), Min(_a.y, _b.y, _c.y));
    }
    private static Vector2 Max(Vector2 _a, Vector2 _b, Vector2 _c)
    {
        return new Vector2(Max(_a.x, _b.x, _c.x), Max(_a.y, _b.y, _c.y));
    }
}