using System;
using UnityEngine;
using UnityEngine.UI;

public abstract class SubMenuAction : AbstractActionHandler
{
    public override void PerformAction<T>(T manager)
    {
        MenuManager parentManager = manager.GetComponent<MenuManager>();

        if (parentManager != null)
        {
            // Create a SubMenu GameObject
            GameObject subMenu = new GameObject("SubMenuManager");
            subMenu.transform.parent = transform;

            // Add the SubMenuManager component to the GameObject and initialize after the component generation!
            SubMenuManager subMenuManager = subMenu.AddComponent<SubMenuManager>();
            GenerateComponents(subMenuManager);
            subMenuManager.InitializeMenuManager();
            subMenuManager.RegisterParent(parentManager);

            OnActionPerformed();
        }
    }

    protected abstract void GenerateComponents(AbstractMenuManager parent);
}