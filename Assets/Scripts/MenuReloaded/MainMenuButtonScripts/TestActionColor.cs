using UnityEngine;
using UnityEngine.UI;
using System;

public class TestActionColor : MonoBehaviour, ActionHandlerInterface
{
    public void PerformAction()
    {

        gameObject.GetComponentInChildren<Text>().text = "it works!";
    }
}
