using UnityEngine;
using System.Collections;
using System;

public class SubMenuManager : AbstractMenuManager
{
    // In ms
    [Header("Sub Menu Settings")]
    [SerializeField]
    protected float fadeOutTweenTime = 0.2f;

    protected override void Start()
    {
        // Do nothing
        // Initialize has to be called manually.
    }

    // Reference to the parent menu
    protected MenuManager parent = null;

    public override void DestroyMenuManager()
    {
        RectTransform rect = GetComponent<RectTransform>();
        LeanTween.scale(rect, Vector3.zero, fadeOutTweenTime).setEase(LeanTweenType.easeOutCubic);

        // DeRegister parent
        parent.ActivateInput();

        // Destroy children
        foreach (var pair in components)
            Destroy(pair.Value, fadeOutTweenTime);

        // Destroy manager
        Destroy(this.gameObject, fadeOutTweenTime);
    }

    public void RegisterParent(MenuManager parent)
    {
        this.parent = parent;
        this.parent.DeactivateInput();
    }
}