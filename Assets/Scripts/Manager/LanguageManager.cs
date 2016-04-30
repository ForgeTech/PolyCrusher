using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class LanguageManager : MonoBehaviour {

    [SerializeField]
    private static LanguageManager instance;

    private Text[] sceneTextObjects;
    private string replacementText;

    public static LanguageManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<LanguageManager>();
            }
            return instance;
        }
    }


    void Awake()
    {
        instance = this;
    }

    void Start () {
        UpdateTextList();        
        UpdateLanguage();
    }

    public void UpdateTextList()
    {      
        sceneTextObjects = FindObjectsOfType<Text>();       
    }

    public void UpdateLanguage()
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
