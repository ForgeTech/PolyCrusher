using System;
using UnityEngine;

class RandomNameGenerator
{
    static string[] names;
    static readonly System.Random rnd = new System.Random();
    private static readonly char[] seperator = new char[] { '\r', '\n' };

    public static string Generate()
    {
        try
        {
            if (names == null)
            {
                TextAsset textAsset = Resources.Load<TextAsset>("names");
                if (textAsset != null)
                    names = textAsset.text.Split(seperator, StringSplitOptions.RemoveEmptyEntries);
            }
        }
        catch {
            return "NONAME";
        }

        int location = rnd.Next(names.Length - 1);
        return names[location].ToUpper();
    }
}