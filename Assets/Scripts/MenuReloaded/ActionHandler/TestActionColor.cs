using UnityEngine;
using UnityEngine.UI;
using System;

public class TestActionColor : AbstractActionHandler
{
    public override void PerformAction<T>(T t)
    {
        gameObject.GetComponentInChildren<Text>().text = "it works!";
        OnActionPerformed();
    }
}
