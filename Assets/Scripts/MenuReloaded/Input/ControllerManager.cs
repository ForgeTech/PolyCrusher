using UnityEngine;
using System.Collections.Generic;
using InControl;
using System;

public class ControllerManager : MonoBehaviour, VirtualControllerHandler
{

    #region variables
    private SmartphoneController smartphoneController;
    private KeyboardController keyboardController;

    private Dictionary<VirtualController, SmartphoneController> smartphoneControllers;

    private int currentSmartPhoneController = 0;
    private int maxSmartphoneConroller = 4;
    #endregion

    #region methods
    void Start()
    {
        smartphoneControllers = new Dictionary<VirtualController, SmartphoneController>(4);
        keyboardController = new KeyboardController();
        InputManager.AttachDevice(keyboardController);
    }

    void OnDestroy()
    {
        InputManager.DetachDevice(keyboardController);
        foreach (KeyValuePair<VirtualController, SmartphoneController> entry in smartphoneControllers)
        {
            InputManager.DetachDevice(entry.Value);
        }
    }

    #region controller updates
    public void VirtualControllerMoves(VirtualController virtualController, Vector2 movement)
    {
        smartphoneControllers.TryGetValue(virtualController, out smartphoneController);
        if (smartphoneController != null)
        {
            smartphoneController.SetLeftAnalogStick = movement; 
        }
    }

    public void VirtualControllerShoots(VirtualController virtualController, Vector2 shoot)
    {
        smartphoneControllers.TryGetValue(virtualController, out smartphoneController);
        if (smartphoneController != null)
        {
            smartphoneController.SetRightAnalogStick = shoot;
        }
    }

    public void VirtualControllerSendsSpecialAttack(VirtualController virtualController)
    {
        smartphoneControllers.TryGetValue(virtualController, out smartphoneController);
        if (smartphoneController != null)
        {
            smartphoneController.SetAbilityPressed = true;
        }
    }

    public void VirtualControllerSendsBackButton(VirtualController virtualController)
    {
        smartphoneControllers.TryGetValue(virtualController, out smartphoneController);
        if (smartphoneController != null)
        {
            smartphoneController.SetBackPressed = true;
        }
    }

    public void VirtualControllerSendsPauseButton(VirtualController virtualController)
    {
        smartphoneControllers.TryGetValue(virtualController, out smartphoneController);
        if (smartphoneController != null)
        {
            smartphoneController.SetPausePressed = true;
        }
    }

    public void VirtualControllerQuitsTheGame(VirtualController virtualController)
    {
        RemoveVirtualController(virtualController);
    }

    public void VirtualControllerIsNotResponsing(VirtualController virtualController)
    {
        Debug.Log("Controller is not responding!");
    }
    #endregion

    #region managing smartphone controllers
    public bool AddNewVirtualController(VirtualController virtualController)
    {
        if(currentSmartPhoneController < maxSmartphoneConroller)
        {
            currentSmartPhoneController++;
            smartphoneController = new SmartphoneController(virtualController);
            smartphoneControllers.Add(virtualController, smartphoneController);
        }
        return false;
    }

    private void RemoveVirtualController(VirtualController virtualController)
    {
        smartphoneControllers.TryGetValue(virtualController, out smartphoneController);
        if (smartphoneController != null)
        {
            InputManager.DetachDevice(smartphoneController);
            currentSmartPhoneController--;
        }
    }
    #endregion

    #endregion
}
