﻿using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class MenuManager : MonoBehaviour
{
    readonly Dictionary<int, GameObject> components = new Dictionary<int, GameObject>();

    [SerializeField]
    private int startIndex;

    private SelectorInterface selector;
    private InputInterface input;

    private bool acceptInput = true;

    private MenuManager parent = null;

    // Is used for sub menus -> Sub menus set this member of its parent to false when the sub menu is created.
    private bool isInputActive = true;

    void Start ()
    {
        InitializeDictionary();
        selector = new Selector(startIndex, components, new DefaultColorTransition());
        input = new TestInput();
    }
	
	void Update ()
    {
        if (acceptInput && isInputActive)
        {
            HandleNavigation();
            HandleSelection();
        }
    }

    void HandleSelection()
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
        }
    }

    void HandleNavigation()
    {
        if (input.GetHorizontal("P1_") > 0.5f)
        {
            selector.Next();
            StartCoroutine(InputCooldown(0.2f));
        } else if (input.GetHorizontal("P1_") < -0.5f)
        {
            selector.Previous();
            StartCoroutine(InputCooldown(0.2f));
        }
    }

    private IEnumerator InputCooldown(float time)
    {
        acceptInput = false;
        yield return new WaitForSeconds(time);
        acceptInput = true;
    }

    /// <summary>
    /// Searches for all child objects and adds them to the dictionary. Uses the SelectionID of the NavigationInformation component as key for the dictionary
    /// </summary>
    void InitializeDictionary()
    {        
        foreach(Transform child in transform)
        {
            NavigationInformation ni = child.GetComponent<NavigationInformation>();
            if (ni != null)
            {
                components.Add(ni.SelectionID, child.gameObject);
            }
            else
            {
                Debug.LogError("NavigationInformation component is missing!");
            }        
        }
    }
}