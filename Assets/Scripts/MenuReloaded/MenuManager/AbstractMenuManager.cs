using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public enum MenuSelection
{
    HorizontalSelection = 0,
    VerticalSelection = 1,
    SubSelection = 2
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
    private float buttonPressedWaitTime = 0.3f;

    [SerializeField]
    private float stickMovedWaitTime = 0.3f;

    [SerializeField]
    protected float menuSpawnTweenTime = 0.2f;

    [Header("Transitions")]
    [SerializeField]
    public TransitionEnum[] transitions;

    [SerializeField]
    public ElementPressedEnum[] pressedHandlerEnum;

    [SerializeField]
    public SpawnTransitionEnum spawnHandlerEnum;

    [Header("Sub selector transitions")]
    [Tooltip("The transition of the sub selector when the main selector changes, not the overall sub selector transitions!")]
    public TransitionEnum[] subSelectorTransition;
    #endregion

    #region Internal Fields
    protected SelectorInterface selector;
    protected MenuInputHandler menuInputHandler;
    protected ActionHandlerInterface backAction;

    protected bool acceptButtonInput = true;
    protected bool acceptStickInputInternal = true;
    protected bool acceptStickInputExternal = true;
    // Is used for sub menus -> Sub menus set this member of its parent to false when the sub menu is created.
    protected bool isInputActive = false;

    // Map for all interactive components
    protected readonly Dictionary<int, GameObject> components = new Dictionary<int, GameObject>();

    protected MenuSpawnTransitionHandler spawnHandler;

    protected WaitForSeconds buttonPressedWait;
    protected WaitForSeconds stickMovedWait;
    protected WaitForSeconds menuSpawnWait;

    private Action navigationNextAction;
    private Action navigationPreviousAction;
    private Action subNavigationNextAction;
    private Action subNavigationPreviousAction;
    #endregion

    #region Delegates and Events
    public delegate void SelectedEventHandler(AbstractMenuManager triggerManager, GameObject selectedComponent);
    public event SelectedEventHandler ComponentSelected;

    public delegate void NavigationHandler();
    public event NavigationHandler NavigationNext;
    public event NavigationHandler NavigationPrevious;

    public event NavigationHandler SubNavigationNext;
    public event NavigationHandler SubNavigationPrevious;

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
        InitializePlayerControlActions();
        StartCoroutine(TriggerMenuSpawnTween(() => {
            spawnHandler.HandleMenuSpawnTransition(MenuComponents, Selector);
        }));
    }

    protected virtual void Update ()
    {
        if (isInputActive)
        {
            if (acceptStickInputInternal && acceptStickInputExternal)
            {
                if (menuSelection != MenuSelection.SubSelection)
                    HandleNavigation();
                else
                    HandleSubNavigation();
            }

            if (acceptButtonInput)
            {
                HandleSelection();
                HandleBackSelection();
            }
        }
    }

    public void SetMenuInputActive(bool isInputActive)
    {
        this.isInputActive = isInputActive;
    }

    public virtual void InitializeMenuManager()
    {
       // InitializeCoroutineHelper();
        InitializeActions();
        InitializeDictionary();
        InitializeSpawnHandler();
        InitializeSelector();
        InitializeBackAction();
    }

    protected void InitializeActions()
    {
        // Next & Previous Action callbacks
        navigationNextAction = () => {
            selector.Next();
            OnNextSelection();
            StartCoroutine(StickInputCooldown());
        };
        navigationPreviousAction = () => {
            selector.Previous();
            OnPreviousSelection();
            StartCoroutine(StickInputCooldown());
        };

        // Sub navigation actions
        subNavigationNextAction = () => {
            ((SelectorWithSubSelector)selector).SubSelectorNext();
            OnSubNavigationNext();
            StartCoroutine(StickInputCooldown());
        };
        subNavigationPreviousAction = () => {
            ((SelectorWithSubSelector)selector).SubSelectorPrevious();
            OnSubNavigationPrevious();
            StartCoroutine(StickInputCooldown());
        };
    }

    protected virtual void InitializePlayerControlActions()
    {
        SetPlayerControlActions(PlayerControlActions.CreateWithGamePadBindings());
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

    protected virtual void InitializeSpawnHandler()
    {
        this.spawnHandler = MenuReloadedUtil.SpawnTransitionEnumToHandler(spawnHandlerEnum, menuSpawnTweenTime, LeanTweenType.easeOutSine);
    }

    protected virtual void InitializeBackAction()
    {
        backAction = GetComponent<ActionHandlerInterface>();
        if (backAction == null)
        {
            Debug.Log("<color=#ff0000ff><b>No Action Handler for the back action was found at the MenuManage GameObject."
                + " Using NoOp Action instead.</b></color>");
            
            backAction = new NoOpAction();
        }
    }

    protected virtual void InitializeSelector()
    {
        TransitionHandlerInterface[] pickedTransitions = MenuReloadedUtil.MapTransitionEnumToHandler(transitions);
        ElementPressedHandler[] pickedPressedHandler = MenuReloadedUtil.MapElementPressedEnumToHandler(pressedHandlerEnum);

        if (menuSelection != MenuSelection.SubSelection)
            selector = new Selector(startIndex, components, pickedTransitions, pickedPressedHandler, true);
        else
            selector = new SelectorWithSubSelector(startIndex, components, pickedTransitions, pickedPressedHandler, false, MenuReloadedUtil.MapTransitionEnumToHandler(subSelectorTransition));
    }

    public void SetPlayerControlActions(PlayerControlActions action)
    {
        if (menuInputHandler != null)
            menuInputHandler.DestroyPlayerAction();

        menuInputHandler = new DefaultMenuInputHandler(action);
        OnPlayerActionChanged(action);
    }

    public void SwitchNavigationActivationState()
    {
        acceptStickInputExternal = !acceptStickInputExternal;
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
        menuInputHandler.HandleBackInput(() => {
            backAction.PerformAction<MonoBehaviour>(this);
        });
    }

    private void PerformActionOnSelectedElement(GameObject selectedElement)
    {
        selectedElement.GetComponent<ActionHandlerInterface>().PerformAction(this);
        OnComponentSelected(selectedElement);
    }

    protected virtual void HandleSubNavigation()
    {
        menuInputHandler.HandleVerticalInput(navigationPreviousAction, navigationNextAction);
        menuInputHandler.HandleHorizontalInput(subNavigationPreviousAction, subNavigationNextAction);
    }

    protected virtual void HandleNavigation()
    {
        if (menuSelection == MenuSelection.HorizontalSelection)
            menuInputHandler.HandleHorizontalInput(navigationPreviousAction, navigationNextAction);
        else if (menuSelection == MenuSelection.VerticalSelection)
            menuInputHandler.HandleVerticalInput(navigationPreviousAction, navigationNextAction);
    }

    #region IEnumerator methods
    protected IEnumerator StickInputCooldown()
    {
        acceptStickInputInternal = false;
        acceptButtonInput = false;  // Also deactivate buttons so during the tween nothing can be pressed
        //yield return stickMovedWait;
        yield return StartCoroutine(CoroutineUtil.WaitForRealSeconds(stickMovedWaitTime));
        acceptButtonInput = true;
        acceptStickInputInternal = true;
    }

    protected IEnumerator WaitBeforeTriggerAction(GameObject selectedGameObject)
    {
        acceptButtonInput = false;
        selector.HandleSelectedElement();
        //yield return buttonPressedWait;
        yield return StartCoroutine(CoroutineUtil.WaitForRealSeconds(buttonPressedWaitTime));
        PerformActionOnSelectedElement(selectedGameObject);
        acceptButtonInput = true;
    }

    /// <summary>
    /// Handles the menu spawn tween.
    /// The input is disabled during this spawn tween.
    /// </summary>
    protected IEnumerator TriggerMenuSpawnTween(Action spawnTweenAction)
    {
        spawnTweenAction();
        SetMenuInputActive(false);
        //yield return menuSpawnWait;
        yield return StartCoroutine(CoroutineUtil.WaitForRealSeconds(menuSpawnTweenTime));
        SetMenuInputActive(true);
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

    protected void OnSubNavigationNext()
    {
        if (SubNavigationNext != null)
            SubNavigationNext();
    }

    protected void OnSubNavigationPrevious()
    {
        if (SubNavigationPrevious != null)
            SubNavigationPrevious();
    }
    #endregion
}