using System;
using UnityEngine;
using UnityEngine.UI;

public class TestSubMenuAction : SubMenuAction
{
    [SerializeField]
    private AbstractActionHandler[] buttons;

    protected override void GenerateComponents(AbstractMenuManager parent)
    {
        AddChildrenToContainer(parent);
    }

    private void AddChildrenToContainer(AbstractMenuManager parent)
    {
        int i = 0;
        foreach (AbstractActionHandler action in buttons)
        {
            GameObject instantiatedAction = Instantiate(action.gameObject);
            AbstractActionHandler actionComponent = instantiatedAction.GetComponent<AbstractActionHandler>();
            NavigationInformation navigationInformation = instantiatedAction.GetComponent<NavigationInformation>();
            navigationInformation.SelectionID = i++;

            instantiatedAction.transform.SetParent(parent.gameObject.transform, false);

            // Listen to action performed so the parent submenu destroys automatically
            actionComponent.ActionPerformed += parent.DestroyMenuManager;
        }
    }
}