using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(PerlinLine))]
public class PerlinLineEditor : Editor
{
    // Reference to the script
    private PerlinLine perlinLineScript;

    // Serialized version of the script
    private SerializedObject serializedPerlinLineScript;

    // Styles
    private GUIStyle titleStyle;

    /// <summary>
    /// Is called every time the inspected object is selected.
    /// </summary>
    private void OnEnable()
    {
        perlinLineScript = (PerlinLine)target;
        serializedPerlinLineScript = new SerializedObject(perlinLineScript);

        InitStyles();
    }

    public override void OnInspectorGUI()
    {
        DrawTargetInformation();
        EditorGUILayout.Space();
        DrawParticleSettings();
        EditorGUILayout.Space();
        DrawPerlinNoiseSettings();
        EditorGUILayout.Space();
        DrawAnimationSettings();

        if (GUI.changed)
            EditorUtility.SetDirty(perlinLineScript);

        serializedPerlinLineScript.ApplyModifiedProperties();
    }

    private void DrawTargetInformation()
    {
        SerializedProperty startPosition = serializedPerlinLineScript.FindProperty("startPosition");
        SerializedProperty endPosition = serializedPerlinLineScript.FindProperty("endPosition");

        // Draw GUI
        EditorGUILayout.LabelField("Targets", titleStyle);

        EditorGUILayout.BeginVertical("box");
            EditorGUILayout.PropertyField(startPosition, new GUIContent("Start Position"));
            EditorGUILayout.PropertyField(endPosition, new GUIContent("End Position"));
        EditorGUILayout.EndVertical();
    }

    private void DrawParticleSettings()
    {
        SerializedProperty particleSize = serializedPerlinLineScript.FindProperty("particleSize");
        SerializedProperty particleQuantity = serializedPerlinLineScript.FindProperty("particleQuantity");

        EditorGUILayout.LabelField("Particle Settings", titleStyle);

        EditorGUILayout.BeginVertical("box");
            EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(particleQuantity, new GUIContent("Quantity"));
                //if (GUILayout.Button("Re-Init", GUILayout.Height(Screen.height * 0.02f)))
                //{
                //    particles = perlinLineScript.ReInitializeEmitter();
                //}
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.PropertyField(particleSize, new GUIContent("Particle Size"));
        EditorGUILayout.EndVertical();
    }

    private void DrawPerlinNoiseSettings()
    {
        SerializedProperty speed = serializedPerlinLineScript.FindProperty("speed");
        SerializedProperty scale = serializedPerlinLineScript.FindProperty("scale");

        EditorGUILayout.LabelField("Perlin Noise Settings", titleStyle);

        EditorGUILayout.BeginVertical("box");
            EditorGUILayout.PropertyField(speed, new GUIContent("Speed"));
            EditorGUILayout.PropertyField(scale, new GUIContent("Scale"));
        EditorGUILayout.EndVertical();
    }

    private void DrawAnimationSettings()
    {
        SerializedProperty rayAnimationSpeed = serializedPerlinLineScript.FindProperty("rayAnimationSpeed");
        SerializedProperty sizeCurve = serializedPerlinLineScript.FindProperty("sizeCurve");

        // Draw GUI
        EditorGUILayout.LabelField("Animation Settings", titleStyle);

        EditorGUILayout.BeginVertical("box");
            EditorGUIUtility.labelWidth = Screen.width * 0.7f;
            EditorGUILayout.PropertyField(rayAnimationSpeed, new GUIContent("Ray Anim. Speed"));
            EditorGUIUtility.labelWidth = 0;    // Default value

            EditorGUILayout.Space();    
            EditorGUILayout.LabelField("Size Curve:");
            EditorGUILayout.PropertyField(sizeCurve, GUIContent.none, GUILayout.Height(Screen.height * 0.1f));
        EditorGUILayout.EndVertical();
    }

    /// <summary>
    /// Inits the styles.
    /// </summary>
    private void InitStyles()
    {
        titleStyle = new GUIStyle();
        titleStyle.alignment = TextAnchor.MiddleCenter;
        titleStyle.fontSize = 15;
        titleStyle.fontStyle = FontStyle.Bold;
    }
}
