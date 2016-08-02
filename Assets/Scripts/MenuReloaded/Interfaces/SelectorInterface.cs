using UnityEngine;
using System.Collections;

interface SelectorInterface
{
    int Current { get; }
       
    void Next();
    void Previous();
}