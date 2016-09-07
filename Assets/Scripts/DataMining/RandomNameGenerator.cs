using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

class RandomNameGenerator
{
    static readonly List<string> names = new List<string>();
    static readonly Random rnd = new Random();

    public static string Generate()
    {
        // if names have not been loaded, load them
        if(names.Count <= 0)
        {
            try { 
                StreamReader theReader = new StreamReader("Assets/Resources/names.txt", Encoding.Default);
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
            }catch(Exception e)
            {
                names.Add("NONAME");
            }
        }
        
        int location = rnd.Next(names.Count - 1);

        return names[location].ToUpper();
    }
}