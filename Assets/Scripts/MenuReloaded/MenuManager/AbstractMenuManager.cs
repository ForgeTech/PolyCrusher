using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using InControl;

public enum MenuSelection
{
    HorizontalSelection = 0,
    VerticalSelection = 1
}

public abstract class AbstractMenuManager : MonoBehaviour
{
    #region Inspector fields
    [Header("Selection Settings")]
    [SerializeField]
    protected int startIndex;

    [SerializeField]
    private MenuSelection menuSelection = MenuSelection.VerticalSelection;

    [Header("Timing Settings")]
    [Tooltip("The time which is waited before the button action is triggered.")]
    [SerializeField]
    private float buttonPressedWaitTime = 0.2f;

    [SerializeField]
    private float stickMovedWaitTime = 0.3f;

    [Header("Transitions")]
    [SerializeField]
    public TransitionEnum[] transitions;

    [SerializeField]
    public ElementPressedEnum[] pressedHandler;
    #endregion

    protected SelectorInterface selector;
    protected MenuInputHandler menuInputHandler;

    protected bool acceptButtonInput = true;
    protected bool acceptStickInput = true;
    // Is used for sub menus -> Sub menus set this member of its parent to false when the sub menu is created.
    protected bool isInputActive = true;

    // Map for all interactive components
    protected readonly Dictionary<int, GameObject> components = new Dictionary<int, GameObject>();

    #region Delegates and Events
    public delegate void SelectedEventHandler(AbstractMenuManager triggerManager, GameObject selectedComponent);
    public event SelectedEventHandler ComponentSelected;

    public delegate void NavigationHandler();
    public event NavigationHandler NavigationNext;
    public event NavigationHandler NavigationPrevious;

    public delegate void PlayerActionControlChangedHandler(PlayerControlActions playerAction);
    public event PlayerActionControlChangedHandler PlayerActionChanged;
    #endregion

    #region Properties
    public Dictionary<int, GameObject> MenuComponents
    {
        get { return this.components; }
    }

    public SelectorInterface Selector { get { return this.selector; } }
    #endregion

    protected virtual void Start ()
    {
        InitializeMenuManager();
	}
	
	protected virtual void Update ()
    {
        if (isInputActive)
        {
            if (acceptStickInput)
                HandleNavigation();

            if (acceptButtonInput)
            {
                HandleSelection();
                HandleBackSelection();
            }
        }
    }

    public virtual void InitializeMenuManager()
    {
        InitializeDictionary();
        InitializeSelector();
        SetPlayerControlActions(PlayerControlActions.CreateWithGamePadBindings());
    }

    public void SetPlayerControlActions(PlayerControlActions action)
    {
        menuInputHandler = new DefaultMenuInputHandler(action);
        OnPlayerActionChanged(action);
    }

    public void SwitchNavigationActivationState()
    {
        acceptStickInput = !acceptStickInput;
    }

    protected virtual void InitializeSelector()
    {
        TransitionHandlerInterface[] pickedTransitions = MenuReloadedUtil.MapTransitionEnumToHandler(transitions);
        ElementPressedHandler[] pickedPressedHandler = MenuReloadedUtil.MapElementPressedEnumToHandler(pressedHandler);

        selector = new Selector(startIndex, components, pickedTransitions, pickedPressedHandler);
    }

    protected virtual void HandleSelection()
    {
        GameObject g;
        menuInputHandler.HandleSelectInput(() => {
            try {
                components.TryGetValue(selector.Current, out g);
            }
            catch (KeyNotFoundException e) {
                throw e;
            }
            StartCoroutine(WaitBeforeTriggerAction(g));
        });
    }

    protected virtual void HandleBackSelection()
    {
        GameObject g;
        menuInputHandler.HandleBackInput(() => {
            // TODO Call back action
        });
    }

    private void PerformActionOnSelectedElement(GameObject selectedElement)
    {
        selectedElement.GetComponent<ActionHandlerInterface>().PerformAction(this);
        OnComponentSelected(selectedElement);
    }

    protected virtual void HandleNavigation()
    {
        // Action callbacks
        Action next = () => {
            selector.Next();
            OnNextSelection();
            StartCoroutine(StickInputCooldown());

        };
        Action previous = () => {
            selector.Previous();
            OnPreviousSelection();
            StartCoroutine(StickInputCooldown());
        };

        if (menuSelection == MenuSelection.HorizontalSelection)
            menuInputHandler.HandleHorizontalInput(previous, next);
        else if (menuSelection == MenuSelection.VerticalSelection)
            menuInputHandler.HandleVerticalInput(previous, next);

    }

    /// <summary>
    /// Searches for all child objects and adds them to the dictionary. Uses the SelectionID of the NavigationInformation component as key for the dictionary
    /// </summary>
    protected virtual void InitializeDictionary()
    {
        foreach (Transform child in transform)
        {
            NavigationInformation ni = child.GetComponent<NavigationInformation>();
            if (ni != null)
                components.Add(ni.SelectionID, child.gameObject);
            else
                Debug.LogError("NavigationInformation component is missing!");
        }
    }

    #region IEnumerator methods
    protected IEnumerator StickInputCooldown()
    {
        acceptStickInput = false;
        yield return new WaitForSeconds(stickMovedWaitTime);
        acceptStickInput = true;
    }

    protected IEnumerator WaitBeforeTriggerAction(GameObject selectedGameObject)
    {
        acceptButtonInput = false;
        selector.HandleSelectedElement();
        yield return new WaitForSeconds(buttonPressedWaitTime);
        PerformActionOnSelectedElement(selectedGameObject);
        acceptButtonInput = true;
    }
    #endregion

    #region Abstract Methods
    public abstract void DestroyMenuManager();
    #endregion

    #region Event Methods
    protected void OnComponentSelected(GameObject selectedComponent)
    {
        if (ComponentSelected != null)
            ComponentSelected(this, selectedComponent);
    }

    protected void OnNextSelection()
    {
        if (NavigationNext != null)
            NavigationNext();
    }

    protected void OnPreviousSelection()
    {
        if (NavigationPrevious != null)
            NavigationPrevious();
    }

    protected void OnPlayerActionChanged(PlayerControlActions playerAction)
    {
        if (PlayerActionChanged != null)
            PlayerActionChanged(playerAction);
    }
    #endregion
}