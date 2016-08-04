using UnityEngine;

public abstract class AbstractActionHandler : MonoBehaviour, ActionHandlerInterface
{
    public event ActionPerformedEventHandler ActionPerformed;

    public virtual void OnActionPerformed()
    {
        if (ActionPerformed != null)
            ActionPerformed();
    }

    public abstract void PerformAction<T>(T triggerInstance) where T : MonoBehaviour;
}