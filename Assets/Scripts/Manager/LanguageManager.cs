using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class LanguageManager : MonoBehaviour {

    public Text[] sceneTextObjects;
    private string[] menuObjectNames;
    private int sceneIndex;
    private string prefabName;

	// Use this for initialization
	void Start () {
        sceneTextObjects = GameObject.FindObjectsOfType<Text>();
        sceneIndex = Application.loadedLevel;
        menuObjectNames = new string[] { "LevelSelectionObject(Clone)", "CharacterSelectionObject(Clone)", "MainMenuObject(Clone)", "OptionMenuObject(Clone)" };
    }
	
	// Update is called once per frame
	void Update () {
        Debug.Log("SceneIndex: " + sceneIndex);
        if (sceneIndex == 0)
        {
            GameObject go;

            for(int i = 0; i < menuObjectNames.Length; i++)
            {
                go = GameObject.Find(menuObjectNames[i]);

                if (go != null)
                {
                    if (go.name != prefabName)
                    {
                        prefabName = go.name;
                        Debug.Log(prefabName);
                        UpdateTextList();
                    }
                    break;
                }
            }         
        }
        UpdateLanguage();
	}

    void UpdateTextList()
    {
        Debug.Log("new text objects loaded");
        sceneTextObjects = GameObject.FindObjectsOfType<Text>();
        //UpdateLanguage();
    }



    void UpdateLanguage()
    {
        for(int i = 0; i < sceneTextObjects.Length; i++)
        {
            if (sceneTextObjects[i] != null)
            {
                string replacementText = LanguageFileReader.GetLanguageObject(sceneTextObjects[i].gameObject.name);
                if (replacementText != null)
                {
                    sceneTextObjects[i].text = replacementText;
                }
            }           
        }
    }
}
