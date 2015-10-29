using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System;


public class ConfigScript : MonoBehaviour {

    private static string path;
    private static Dictionary<string, Dictionary<string, string>> IniDictionary = new Dictionary<string, Dictionary<string, string>>();
    private static bool Initialized = false;
    public static string selectedLanguage;



    /// <summary>
    /// Sections list
    /// </summary>
    public enum Sections
    {
        Section01,
    }
    /// <summary>
    /// Keys list
    /// </summary>
    public enum Keys
    {
        Key01,
        Key02,
        Key03,
    }

    private int[] width;
    private int[] height;

  
    void Awake()
    {
        path = Path.Combine(Application.persistentDataPath, "IniFile.ini");
        width = new int[4]{1280, 1366, 1600, 1920 };
        height = new int[4] { 720, 768, 900, 1080 };

        if (!File.Exists(path))
        {
            WriteIni();
        }


       


        if (IniReadValue("graphics", "quality") != null)
        {
            string desiredQuality = IniReadValue("graphics", "quality");
            if (desiredQuality != QualitySettings.GetQualityLevel().ToString())
            {
                for (int i = 0; i < 7; i++)
                {
                    if (desiredQuality == i.ToString())
                    {
                        QualitySettings.SetQualityLevel(i);
                    }
                }
            }
        }
        else
        {
            QualitySettings.SetQualityLevel(6);
            IniWriteValue("graphics", "quality", 6.ToString());
        }


        if (IniReadValue("graphics", "resolution") != null)
        {
            string desiredRes = IniReadValue("graphics", "resolution");
            if (desiredRes != Screen.currentResolution.width.ToString())
            {
                for (int i = 0; i < width.Length; i++)
                {
                    if (desiredRes == width[i].ToString())
                    {
                        Screen.SetResolution(width[i], height[i], false);
                        Canvas.ForceUpdateCanvases();
                    }
                }
            }
        }
        else
        {
            bool changed = false;
            for (int i = 0; i < width.Length; i++)
            {
                if (Screen.currentResolution.width == width[i])
                {
                    Screen.SetResolution(Screen.currentResolution.width, Screen.currentResolution.height, true);
                    IniWriteValue("graphics", "resolution", Screen.currentResolution.width.ToString());
                    changed = true;
                }
            }

            if (!changed)
            {
                Screen.SetResolution(1280, 720, true);
                IniWriteValue("graphics", "resolution", 1280.ToString());
            }
        }


        if (IniReadValue("graphics", "fullscreen") != null)
        {
            string desiredFS = IniReadValue("graphics", "fullscreen");
            if (desiredFS != Screen.fullScreen.ToString())
            {
                for (int i = 0; i < 2; i++)
                {
                    if (desiredFS == i.ToString())
                    {
                        if (i == 0)
                        {
                            Screen.fullScreen = false;
                        }
                        else
                        {
                            Screen.fullScreen = true;
                        }
                    }
                }
            }
        }
        else
        {
            Screen.fullScreen = true;
            IniWriteValue("graphics", "fullscreen", 1.ToString());
        }


        if (IniReadValue("graphics", "antialiasing") != null)
        {
            string desiredQuality = IniReadValue("graphics", "antialiasing").ToString();
            if (desiredQuality != QualitySettings.antiAliasing.ToString())
            {
                for (int i = 0; i < 4; i++)
                {
                    if (desiredQuality == (i * 2).ToString())
                    {
                        QualitySettings.antiAliasing = i * 2;
                    }
                }
            }
        }
        else
        {
            QualitySettings.antiAliasing = 2;
            IniWriteValue("graphics", "antialiasing", 2.ToString());
        }


        if (IniReadValue("graphics", "vsync") != null)
        {
            string desiredVsync = IniReadValue("graphics", "vsync").ToString();
            if (desiredVsync != QualitySettings.vSyncCount.ToString())
            {
                for (int i = 0; i < 2; i++)
                {
                    if (desiredVsync == i.ToString())
                    {
                        QualitySettings.vSyncCount = i;
                    }
                }
            }
        }
        else
        {
            QualitySettings.vSyncCount = 0;
            IniWriteValue("graphics", "vsync", 0.ToString());
        }



        if (IniReadValue("language", "active") != null)
        {
            selectedLanguage = IniReadValue("language", "active");

        }
        else
        {
            IniWriteValue("language", "active", "English");
            selectedLanguage = "English";
        }


        Debug.Log("Config loaded");

    }


    private static bool FirstRead()
    {
        if (File.Exists(path))
        {
            using (StreamReader sr = new StreamReader(path))
            {
                string line;
                string theSection = "";
                string theKey = "";
                string theValue = "";
                while (!string.IsNullOrEmpty(line = sr.ReadLine()))
                {
                    line.Trim();
                    if (line.StartsWith("[") && line.EndsWith("]"))
                    {
                        theSection = line.Substring(1, line.Length - 2);
                    }
                    else
                    {
                        string[] ln = line.Split(new char[] { '=' });
                        theKey = ln[0].Trim();
                        theValue = ln[1].Trim();
                    }
                    if (theSection == "" || theKey == "" || theValue == "")
                        continue;
                    PopulateIni(theSection, theKey, theValue);
                }
            }
        }
        return true;
    }

    private static void PopulateIni(string _Section, string _Key, string _Value)
    {
        if (IniDictionary.Keys.Contains(_Section))
        {
            if (IniDictionary[_Section].Keys.Contains(_Key))
                IniDictionary[_Section][_Key] = _Value;
            else
                IniDictionary[_Section].Add(_Key, _Value);
        }
        else
        {
            Dictionary<string, string> neuVal = new Dictionary<string, string>();
            neuVal.Add(_Key.ToString(), _Value);
            IniDictionary.Add(_Section.ToString(), neuVal);
        }
    }
    /// <summary>
    /// Write data to INI file. Section and Key no in enum.
    /// </summary>
    /// <param name="_Section"></param>
    /// <param name="_Key"></param>
    /// <param name="_Value"></param>
    public static void IniWriteValue(string _Section, string _Key, string _Value)
    {
        if (!Initialized)
            FirstRead();
        PopulateIni(_Section, _Key, _Value);
        //write ini
        WriteIni();
    }
    /// <summary>
    /// Write data to INI file. Section and Key bound by enum
    /// </summary>
    /// <param name="_Section"></param>
    /// <param name="_Key"></param>
    /// <param name="_Value"></param>
    public static void IniWriteValue(Sections _Section, Keys _Key, string _Value)
    {
        IniWriteValue(_Section.ToString(), _Key.ToString(), _Value);
    }

    private static void WriteIni()
    {
        using (StreamWriter sw = new StreamWriter(path))
        {
            foreach (KeyValuePair<string, Dictionary<string, string>> sezioni in IniDictionary)
            {
                sw.WriteLine("[" + sezioni.Key.ToString() + "]");
                foreach (KeyValuePair<string, string> chiave in sezioni.Value)
                {
                    // value must be in one line
                    string vale = chiave.Value.ToString();
                    vale = vale.Replace(Environment.NewLine, " ");
                    vale = vale.Replace("\r\n", " ");
                    sw.WriteLine(chiave.Key.ToString() + " = " + vale);
                }
            }
        }
    }
    /// <summary>
    /// Read data from INI file. Section and Key bound by enum
    /// </summary>
    /// <param name="_Section"></param>
    /// <param name="_Key"></param>
    /// <returns></returns>
    public static string IniReadValue(Sections _Section, Keys _Key)
    {
        if (!Initialized)
            FirstRead();
        return IniReadValue(_Section.ToString(), _Key.ToString());
    }
    /// <summary>
    /// Read data from INI file. Section and Key no in enum.
    /// </summary>
    /// <param name="_Section"></param>
    /// <param name="_Key"></param>
    /// <returns></returns>
    public static string IniReadValue(string _Section, string _Key)
    {
        if (!Initialized)
            FirstRead();
        if (IniDictionary.ContainsKey(_Section))
            if (IniDictionary[_Section].ContainsKey(_Key))
                return IniDictionary[_Section][_Key];
        return null;
    }


    public static void LanguageChange(string language)
    {
        selectedLanguage = language;
        IniWriteValue("language", "active", language);

    }
}
