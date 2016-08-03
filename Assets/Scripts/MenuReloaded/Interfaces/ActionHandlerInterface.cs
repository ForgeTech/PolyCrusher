using UnityEngine;
using System.Collections;

public delegate void ActionPerformedEventHandler();

public enum ActionEnum
{
    LevelSelection = 0,
    TestAction = 99
}

public interface ActionHandlerInterface
{
    void PerformAction<T>(T triggerInstance) where T : MonoBehaviour;

    event ActionPerformedEventHandler ActionPerformed;
    void OnActionPerformed();
}
