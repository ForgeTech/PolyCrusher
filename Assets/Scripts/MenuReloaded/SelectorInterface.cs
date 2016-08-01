using UnityEngine;
using System.Collections;

public interface SelectorInterface {


    int Current
    {
        get;
    }
       
    void Next();
    void Previous();

}
