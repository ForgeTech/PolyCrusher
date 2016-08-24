using UnityEngine;
using System.Collections;
using System;

public class ResumeGameAction : AbstractActionHandler {
    public override void PerformAction<T>(T triggerInstance)
    {

        GameObject.FindObjectOfType<PauseMenuManager>().ResumeGame();


        OnActionPerformed();
    }
}
