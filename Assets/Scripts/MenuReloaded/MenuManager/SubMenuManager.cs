using UnityEngine;
using System.Collections;
using System;

public class SubMenuManager : AbstractMenuManager
{
    protected override void Start()
    {
        // TODO: Do nothing
        // Initialize has to be called manually.
    }

    // Reference to the parent menu
    protected MenuManager parent = null;

    public override void DestroyMenuManager()
    {
        // DeRegister parent
        parent.DeRegisterSubMenu();

        // Destroy children
        foreach (var pair in components)
            Destroy(pair.Value);

        // Destroy manager
        Destroy(this.gameObject);
    }

    public void RegisterParent(MenuManager parent)
    {
        this.parent = parent;
        this.parent.RegisterSubMenu();
    }
}