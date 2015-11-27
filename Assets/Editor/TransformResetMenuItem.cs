using UnityEngine;
using System.Collections;
using UnityEditor;

public class TransformResetMenuItem
{
    [MenuItem("CONTEXT/Transform/ResetValues")]
    private static void ResetValues(MenuCommand command)
    {
        Transform t = command.context as Transform;

        t.position = Vector3.zero;
        t.localScale = new Vector3(1, 1, 1);
        t.rotation = Quaternion.identity;
    }
}
