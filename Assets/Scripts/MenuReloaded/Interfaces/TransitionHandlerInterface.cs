using UnityEngine;
using System.Collections;

public interface TransitionHandlerInterface
{
    void OnFocus(GameObject gameobject);
    void OnDefocus(GameObject gameobject);
}