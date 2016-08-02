using UnityEngine;
using UnityEngine.UI;
using System;

public class TestActionColor : MonoBehaviour, ActionHandlerInterface
{
    public void PerformAction<T>(T t)
    {
        gameObject.GetComponentInChildren<Text>().text = "it works!";
    }
}
