﻿using UnityEngine;
using System.Collections.Generic;
using System.Xml;
using System;

public class LanguageFileReader : MonoBehaviour {

 
    private TextAsset GameAsset;
    static List<Dictionary<string, string>> languages = new List<Dictionary<string, string>>();
    static Dictionary<string, string> obj;
    static Dictionary<string, string> currentLanguage;
   
    static XmlDocument xmlDoc;
    public static string selectedLanguage;

    private string defaultLanguage = "English";

    void Awake()
    {
        selectedLanguage = "English";
        GameAsset = Resources.Load("Language/languages") as TextAsset;
        xmlDoc = new XmlDocument();
        xmlDoc.LoadXml(GameAsset.text);           
       
        GetLanguages();        
    }

    public void GetLanguages()
    {
        xmlDoc = new XmlDocument(); // xmlDoc is the new xml document.
        xmlDoc.LoadXml(GameAsset.text); // load the file.
        XmlNodeList languagesList = xmlDoc.GetElementsByTagName("language"); // array of the language nodes.

        foreach (XmlNode languageObject in languagesList)
        {
            XmlNodeList languageContent = languageObject.ChildNodes;
            obj = new Dictionary<string, string>(); 

            foreach (XmlNode languageItems in languageContent) 
            {
                if (languageItems.Name == "name")
                {
                    obj.Add("name", languageItems.InnerText); 
                }               

                if (languageItems.Name == "object")
                {
                    obj.Add(languageItems.Attributes["name"].Value, languageItems.InnerText);                    
                }                
            }
            languages.Add(obj); // add whole obj dictionary in the languages[].
        }

       

        string temp = PlayerPrefs.GetString("SelectedLanguage");
        if (!String.IsNullOrEmpty(temp))
        {
            selectedLanguage = temp;          
        }
        else
        {
            PlayerPrefs.SetString("SelectedLanguage", defaultLanguage);
            selectedLanguage = defaultLanguage;
        }

        ExtractCurrentLanguage();

        
    }


    private static void ExtractCurrentLanguage()
    {
        foreach(Dictionary<string, string> dict in languages)
        {           
            if(dict["name"] == selectedLanguage)
            {
                currentLanguage = dict;                
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
        PlayerPrefs.SetString("SelectedLanguage", language);
        ExtractCurrentLanguage();
    }


}
