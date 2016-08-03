using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public abstract class AbstractMenuManager : MonoBehaviour
{
    const float INPUT_WAIT_TIME = 0.1f;

    #region Inspector fields
    [SerializeField]
    private int startIndex;
    #endregion

    protected SelectorInterface selector;
    protected InputInterface input;

    protected bool acceptInput = true;
    // Is used for sub menus -> Sub menus set this member of its parent to false when the sub menu is created.
    protected bool isInputActive = true;

    // Map for all interactive components
    protected readonly Dictionary<int, GameObject> components = new Dictionary<int, GameObject>();

    #region Delegates and Events
    public delegate void SelectedEventHandler(AbstractMenuManager triggerManager, GameObject selectedComponent);
    public event SelectedEventHandler ComponentSelected;
    #endregion

    protected virtual void Start ()
    {
        InitializeMenuManager();
	}
	
	protected virtual void Update ()
    {
        if (acceptInput && isInputActive)
        {
            HandleNavigation();
            HandleSelection();
        }
    }

    public void InitializeMenuManager()
    {
        InitializeDictionary();
        selector = new Selector(startIndex, components, new TransitionHandlerInterface[] { new DefaultColorTransition(), new DefaultScaleTransition() });
        input = new TestInput();
    }

    protected virtual void HandleSelection()
    {
        GameObject g;
        if (input.GetButtonDown("P1_Ability"))
        {
            try
            {
                components.TryGetValue(selector.Current, out g);
            }
            catch (KeyNotFoundException e)
            {
                throw e;
            }
            g.GetComponent<ActionHandlerInterface>().PerformAction(this);
            OnComponentSelected(g);
        }
    }

    protected virtual void HandleNavigation()
    {
        if (input.GetHorizontal("P1_") > 0.5f)
        {
            selector.Next();
            StartCoroutine(InputCooldown());
        }
        else if (input.GetHorizontal("P1_") < -0.5f)
        {
            selector.Previous();
            StartCoroutine(InputCooldown());
        }
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

    protected IEnumerator InputCooldown()
    {
        acceptInput = false;
        yield return new WaitForSeconds(INPUT_WAIT_TIME);
        acceptInput = true;
    }

    #region Abstract Methods
    public abstract void DestroyMenuManager();
    #endregion

    #region Event Methods
    protected void OnComponentSelected(GameObject selectedComponent)
    {
        if (ComponentSelected != null)
            ComponentSelected(this, selectedComponent);
    }
    #endregion
}
