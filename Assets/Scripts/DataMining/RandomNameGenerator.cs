using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

class RandomNameGenerator
{
    static List<string> names = new List<string>();
    static Random rnd = new Random();

    public static string Generate()
    {
        // if names have not been loaded, load them
        if(names.Count <= 0)
        {
            StreamReader theReader = new StreamReader("Assets/Scripts/DataMining/names.txt", Encoding.Default);
            string line = theReader.ReadLine();

            using (theReader)
            {
                if (line != null)
                {
                    do
                    {
                        line = theReader.ReadLine();

                        if(line != "")
                        {
                            names.Add(line);
                        }

                        line = theReader.ReadLine();
                    }
                    while (line != null);
                }

                theReader.Close();
            }
        }
        
        int location = rnd.Next(names.Count - 1);

        return names[location].ToUpper();
    }
}

