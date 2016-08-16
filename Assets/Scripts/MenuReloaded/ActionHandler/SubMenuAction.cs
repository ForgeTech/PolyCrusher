using System;
using UnityEngine;
using UnityEngine.UI;

public abstract class SubMenuAction : AbstractActionHandler
{
    [Header("Transitions")]
    [SerializeField]
    private TransitionEnum[] transitions;

    [SerializeField]
    private ElementPressedEnum[] pressedHandler;

    [Header("Sub-Menu UI Elements")]
    [SerializeField]
    protected RectTransform container;

    public override void PerformAction<T>(T manager)
    {
        MenuManager parentManager = manager.GetComponent<MenuManager>();

        if (parentManager != null)
        {
            // Instantiate a SubMenu GameObject
            GameObject subMenu = Instantiate(container.gameObject);
            subMenu.transform.SetParent(transform, false);

            // Add the SubMenuManager component to the GameObject and initialize after the component generation!
            SubMenuManager subMenuManager = subMenu.AddComponent<SubMenuManager>();

            // TODO: Find a cleaner way to assign these 2 handler!
            subMenuManager.transitions = transitions;
            subMenuManager.pressedHandlerEnum = pressedHandler;

            GenerateComponents(subMenuManager);
            subMenuManager.InitializeMenuManager();
            subMenuManager.RegisterParent(parentManager);

            OnActionPerformed();
        }
    }

    protected abstract void GenerateComponents(AbstractMenuManager parent);
}