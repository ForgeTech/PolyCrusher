using System;
using UnityEngine;
using UnityEngine.UI;

public class TestSubMenuAction : SubMenuAction
{
    [Header("Settings")]
    [SerializeField]
    private int buttonCount = 4;

    protected override void GenerateComponents(AbstractMenuManager parent)
    {
        // TODO
        ActionPerformed += parent.DestroyMenuManager;
    }
}