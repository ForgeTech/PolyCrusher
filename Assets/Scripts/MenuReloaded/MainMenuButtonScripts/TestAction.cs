using UnityEngine;
using System.Collections;
using System;

public class TestAction : MonoBehaviour, ActionHandlerInterface
{
    public void PerformAction()
    {
        Application.LoadLevel(9);
    }
}
