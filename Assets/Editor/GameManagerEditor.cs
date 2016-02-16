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

    /// <summary>
    /// Is called every time the inspected object is selected.
    /// </summary>
    private void OnEnable()
    {
        gameManager = (GameManager)target;
        serializedGameManager = new SerializedObject(gameManager);
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
        DrawRessourceSetting();
        EditorGUILayout.Space();
        DrawEnemyCountSettings();
        EditorGUILayout.Space();
        DrawWaveIncreaseSettings();
        EditorGUILayout.Space();
        DrawUtilities();
    }

    /// <summary>
    /// Draws the game mode information.
    /// </summary>
    public void DrawGameMode()
    {
        // Serialized propertie
        SerializedProperty serializedGameMode = serializedGameManager.FindProperty("gameMode");

        // Draw GUI
        EditorGUILayout.LabelField("Game Mode", EditorStyles.boldLabel);

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
        EditorGUILayout.LabelField("Spawn Information", EditorStyles.boldLabel);

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
        EditorGUILayout.LabelField("Ressource Settings", EditorStyles.boldLabel);

        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.PropertyField(enemyRessourcePool);
        EditorGUILayout.PropertyField(currentEnemyRessourceValue);
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
        EditorGUILayout.LabelField("Enemy Count Settings", EditorStyles.boldLabel);

        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.PropertyField(maxEnemyActiveCount);
        EditorGUILayout.PropertyField(currentEnemyCount);
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
        EditorGUILayout.LabelField("Wave Increase Settings", EditorStyles.boldLabel);

        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.PropertyField(enemyRessourceIncreaseFactor);
        EditorGUILayout.PropertyField(enemyCountIncreaseFactor);
        EditorGUILayout.PropertyField(timeBetweeenWaveDecreaseFactor);
        EditorGUILayout.PropertyField(enemyHealthIncreaseFactor);
        EditorGUILayout.PropertyField(enemyDamageIncreaseFactor);
        EditorGUILayout.EndVertical();
    }

    /// <summary>
    /// Draws utilities.
    /// </summary>
    public void DrawUtilities()
    {
        EditorGUILayout.LabelField("Utilities", EditorStyles.boldLabel);

        EditorGUILayout.BeginVertical("box");

        EditorGUILayout.BeginHorizontal();

        EditorGUILayout.BeginVertical();
        EditorGUILayout.LabelField("Kills enemies with 'Enemy' tag.", EditorStyles.label);
        if (GUILayout.Button("Kill Enemies", GUILayout.Height(Screen.height * 0.03f)))
        {
            gameManager.KillAllEnemies();
        }
        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginVertical();
        EditorGUILayout.LabelField("Resets all resources.", EditorStyles.label);
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
