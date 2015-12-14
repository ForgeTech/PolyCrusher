using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class LanguageManager : MonoBehaviour {

    private Text[] sceneTextObjects;
    private string[] menuObjectNames;
    private int sceneIndex;
    private string prefabName;

	// Use this for initialization
	void Start () {
        sceneTextObjects = GameObject.FindObjectsOfType<Text>();
        sceneIndex = Application.loadedLevel;
        menuObjectNames = new string[] { "LevelSelectionObject(Clone)", "CharacterSelectionObject(Clone)", "MainMenuObject(Clone)", "OptionMenuObject(Clone)" };

        if (sceneIndex == 0)
        {
            GameObject go;

            for (int i = 0; i < menuObjectNames.Length; i++)
            {
                go = GameObject.Find(menuObjectNames[i]);

                if (go != null)
                {
                    if (go.name != prefabName)
                    {
                        prefabName = go.name;

                        UpdateTextList();
                    }
                    break;
                }
            }
        }
        else
        {
            GameObject go = GameObject.Find("YOLO_Label");
            if (go != null)
            {
                sceneTextObjects = new Text[3];
                sceneTextObjects[0] = GameObject.Find("WaveRoundText").GetComponent<Text>();
                sceneTextObjects[1] = GameObject.Find("BossText").GetComponent<Text>();
                sceneTextObjects[2] = go.GetComponent<Text>();
            }
            else
            {
                sceneTextObjects = new Text[2];
                sceneTextObjects[0] = GameObject.Find("WaveRoundText").GetComponent<Text>();
                sceneTextObjects[1] = GameObject.Find("BossText").GetComponent<Text>();
            }     
        }
    }
	
	// Update is called once per frame
	void Update () {
        
        
        UpdateTextList();
        UpdateLanguage();
	}

    void UpdateTextList()
    {
        //Debug.Log("new text objects loaded");
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
