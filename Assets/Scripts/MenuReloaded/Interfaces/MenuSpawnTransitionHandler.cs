using System.Collections.Generic;
using UnityEngine;

public enum SpawnTransitionEnum
{
    ScaleTransition = 0
}

public interface MenuSpawnTransitionHandler
{
    void HandleMenuSpawnTransition(Dictionary<int, GameObject> components, SelectorInterface selecor);
}