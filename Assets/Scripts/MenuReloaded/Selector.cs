using UnityEngine;
using System.Collections.Generic;
using System;

public class Selector : SelectorInterface {


    private int current;

    private int minValue;
    private int maxValue;

    readonly Dictionary<int, GameObject> components;


    public Selector(int startIndex, Dictionary<int, GameObject> components )
    {
        current = startIndex;
        this.components = components;
        MinMax();
    }

    private void MinMax()
    {       
        foreach(var pair in components)
        {
            maxValue = Math.Max(maxValue, pair.Key);
            minValue = Math.Min(minValue, pair.Key);
        }
    }


    private bool CheckIndex(int index)
    {
        GameObject go;
        try{
            return components.TryGetValue(index, out go);
        }
        catch(KeyNotFoundException e)
        {
            throw e;
        }
    }


    public int Current
    {
        get
        {
            return current;
        }
    }

    public void Next()
    {
        if (!CheckIndex(++current))
        {
            current = minValue;
        }
    }

    public void Previous()
    {
        if (!CheckIndex(--current))
        {
            current = maxValue;
        }
    }

}
