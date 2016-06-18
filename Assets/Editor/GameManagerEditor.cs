using UnityEngine;
using System.Collections;
using UnityEditor;

[CanEditMultipleObjects]
[CustomEditor(typeof(GameManager))]
public class GameManagerEditor : Editor
{
    // Reference to the game manager.
    private GameManager gameManager;

    // Serialized version of the game manager.
    private SerializedObject serializedGameManager;

    // Styles
    private GUIStyle titleStyle;

    /// <summary>
    /// Is called every time the inspected object is selected.
    /// </summary>
    private void OnEnable()
    {
        gameManager = (GameManager)target;
        serializedGameManager = new SerializedObject(gameManager);

        InitStyles();
    }

    /// <summary>
    /// Draw the GUI.
    /// </summary>
    public override void OnInspectorGUI()
    {
        //DrawDefaultInspector();

        DrawGameMode();
        EditorGUILayout.Space();
        DrawSpawnInformation();
        EditorGUILayout.Space();

        EditorGUIUtility.labelWidth = Screen.width * 0.65f;
        DrawRessourceSetting();
        EditorGUILayout.Space();
        DrawEnemyCountSettings();
        EditorGUILayout.Space();
        DrawWaveIncreaseSettings();
        EditorGUILayout.Space();
        DrawSpecialWaveProperties();
        EditorGUILayout.Space();
        EditorGUIUtility.labelWidth = 0;
        DrawUtilities();

        if (GUI.changed)
            EditorUtility.SetDirty(gameManager);

        serializedGameManager.ApplyModifiedProperties();
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

    /// <summary>
    /// Draws the game mode information.
    /// </summary>
    public void DrawGameMode()
    {
        // Serialized propertie
        SerializedProperty serializedGameMode = serializedGameManager.FindProperty("gameMode");

        // Draw GUI
        EditorGUILayout.LabelField("Game Mode", titleStyle);

        EditorGUILayout.BeginHorizontal("box");

        EditorGUILayout.BeginVertical();
        EditorGUILayout.LabelField("Choose the game mode for this specific level.", EditorStyles.label);
        EditorGUILayout.Space();

        EditorGUILayout.PropertyField(serializedGameMode);
        EditorGUILayout.EndVertical();

        EditorGUILayout.EndHorizontal();
    }

    /// <summary>
    /// Draws the spawn information.
    /// </summary>
    private void DrawSpawnInformation()
    {
        SerializedProperty spawnInfo = serializedGameManager.FindProperty("spawnInfo");
        SerializedProperty bossSpawnInfo = serializedGameManager.FindProperty("bossSpawnInfo");

        //Draw GUI
        EditorGUILayout.LabelField("Spawn Information", titleStyle);

        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.PropertyField(spawnInfo, true);     //True --> Include children
        EditorGUILayout.PropertyField(bossSpawnInfo, true);
        
        EditorGUILayout.EndVertical();
    }

    /// <summary>
    /// Draws the ressource settings.
    /// </summary>
    private void DrawRessourceSetting()
    {
        SerializedProperty enemyRessourcePool = serializedGameManager.FindProperty("enemyRessourcePool");
        SerializedProperty currentEnemyRessourceValue = serializedGameManager.FindProperty("currentEnemyRessourceValue");

        //Draw GUI
        EditorGUILayout.LabelField("Ressource Settings", titleStyle);

        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.PropertyField(enemyRessourcePool, new GUIContent("Ressource Pool"));
        EditorGUILayout.PropertyField(currentEnemyRessourceValue, new GUIContent("Current Ressources"));
        EditorGUILayout.EndVertical();
    }

    /// <summary>
    /// Draws enemy count settings.
    /// </summary>
    public void DrawEnemyCountSettings()
    {
        SerializedProperty maxEnemyActiveCount = serializedGameManager.FindProperty("maxEnemyActiveCount");
        SerializedProperty currentEnemyCount = serializedGameManager.FindProperty("currentEnemyCount");
        SerializedProperty timeBetweenWave = serializedGameManager.FindProperty("timeBetweenWave");

        //Draw GUI
        EditorGUILayout.LabelField("Enemy Count Settings", titleStyle);

        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.PropertyField(maxEnemyActiveCount, new GUIContent("Max Enemies"));
        EditorGUILayout.PropertyField(currentEnemyCount, new GUIContent("Enemy Count"));
        EditorGUILayout.PropertyField(timeBetweenWave);
        EditorGUILayout.EndVertical();
    }

    /// <summary>
    /// Draws the wave increase settings.
    /// </summary>
    public void DrawWaveIncreaseSettings()
    {
        SerializedProperty enemyRessourceIncreaseFactor = serializedGameManager.FindProperty("enemyRessourceIncreaseFactor");
        SerializedProperty enemyCountIncreaseFactor = serializedGameManager.FindProperty("enemyCountIncreaseFactor");
        SerializedProperty timeBetweeenWaveDecreaseFactor = serializedGameManager.FindProperty("timeBetweeenWaveDecreaseFactor");
        SerializedProperty enemyHealthIncreaseFactor = serializedGameManager.FindProperty("enemyHealthIncreaseFactor");
        SerializedProperty enemyDamageIncreaseFactor = serializedGameManager.FindProperty("enemyDamageIncreaseFactor");

        //Draw GUI
        EditorGUILayout.LabelField("Wave Increase Settings", titleStyle);

        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.PropertyField(enemyRessourceIncreaseFactor, new GUIContent("Res. Increase Factor"));
        EditorGUILayout.PropertyField(enemyCountIncreaseFactor, new GUIContent("Enemy Count Increase"));
        EditorGUILayout.PropertyField(timeBetweeenWaveDecreaseFactor);
        EditorGUILayout.PropertyField(enemyHealthIncreaseFactor);
        EditorGUILayout.PropertyField(enemyDamageIncreaseFactor);
        EditorGUILayout.EndVertical();
    }

    public void DrawSpecialWaveProperties()
    {
        SerializedProperty specialWaveEnabled = serializedGameManager.FindProperty("specialWaveModeEnabled");
        SerializedProperty specialWaveProbability = serializedGameManager.FindProperty("specialWaveProbablity");

        EditorGUILayout.LabelField("Special Wave Settings", titleStyle);

        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.PropertyField(specialWaveEnabled, new GUIContent("Special Wave Mode Enabled"));
        EditorGUILayout.PropertyField(specialWaveProbability, new GUIContent("Ocurrance probablity"));
        EditorGUILayout.EndVertical();
    }

    /// <summary>
    /// Draws utilities.
    /// </summary>
    public void DrawUtilities()
    {
        EditorGUILayout.LabelField("Utilities", titleStyle);

        EditorGUILayout.BeginVertical("box");

        EditorGUILayout.BeginHorizontal();

        EditorGUILayout.BeginVertical();
        if (GUILayout.Button("Kill Enemies", GUILayout.Height(Screen.height * 0.03f)))
        {
            gameManager.KillAllEnemies();
        }
        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginVertical();
        if (GUILayout.Button("Reset Resources", GUILayout.Height(Screen.height * 0.03f)))
        {
            gameManager.CurrentEnemyRessourceValue = 0;
        }
        EditorGUILayout.EndVertical();

        

        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Initiates the start of the next wave.", EditorStyles.label);
        if (GUILayout.Button("Next Wave!", GUILayout.Height(Screen.height * 0.05f)))
        {
            gameManager.CurrentEnemyRessourceValue = 0;
            gameManager.KillAllEnemies();
        }

        EditorGUILayout.EndVertical();
    }
}
