using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.IO;

public class LanguageFileReader : MonoBehaviour {

    public TextAsset GameAsset;   

    static List<Dictionary<string, string>> languages = new List<Dictionary<string, string>>();
    static Dictionary<string, string> obj;
    static Dictionary<string, string> currentLanguage;
    static XmlNode selectedNode;
    public static string selectedLanguage;

    void Start()
    {
        //selectedLanguage = ConfigScript.IniReadValue("language", "active");
        Debug.Log("selected Language: " + selectedLanguage);
        GetLanguages();
        Debug.Log("List count " + languages.Count);
    }

    public void GetLanguages()
    {
        XmlDocument xmlDoc = new XmlDocument(); // xmlDoc is the new xml document.
        xmlDoc.LoadXml(GameAsset.text); // load the file.
        XmlNodeList languagesList = xmlDoc.GetElementsByTagName("language"); // array of the language nodes.

        foreach (XmlNode languageObject in languagesList)
        {
            XmlNodeList languageContent = languageObject.ChildNodes;
            obj = new Dictionary<string, string>(); // Create a object(Dictionary) to colect the both nodes inside the level node and then put into levels[] array.

            foreach (XmlNode languageItems in languageContent) // levels itens nodes.
            {
                if (languageItems.Name == "name")
                {
                    obj.Add("name", languageItems.InnerText); // put this in the dictionary.
                }               

                if (languageItems.Name == "object")
                {
                    obj.Add(languageItems.Attributes["name"].Value, languageItems.InnerText);                    
                }                
            }
            languages.Add(obj); // add whole obj dictionary in the languages[].
        }

        XmlNodeList selected = xmlDoc.GetElementsByTagName("selected");
        selectedNode = selected[0];
        selectedLanguage = selected[0].InnerText;
        Debug.Log("XML_Language: "+selectedLanguage); 

        ExtractCurrentLanguage();
    }


    private static void ExtractCurrentLanguage()
    {
        foreach(Dictionary<string, string> dict in languages)
        {
            //Debug.Log("dict");
            if(dict["name"] == selectedLanguage)
            {
                currentLanguage = dict;
                //Debug.Log("dict: " + currentLanguage.Count);
            }
        }
    }


    public static string GetLanguageObject(string key)
    {
        if (currentLanguage.ContainsKey(key))
        {
            return currentLanguage[key];
        }
        return null;
        
    }

   
    public static void ChangeLanguage(string language)
    {
        selectedLanguage = language;
        selectedNode.InnerText = language;
        ExtractCurrentLanguage();
    }
}
