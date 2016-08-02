using UnityEngine;
using System.Collections;
using System;

public class TestAction : MonoBehaviour, ActionHandlerInterface
{
    public void PerformAction<T>(T triggerInstance)
    {
        Application.LoadLevel(9);
    }
}
