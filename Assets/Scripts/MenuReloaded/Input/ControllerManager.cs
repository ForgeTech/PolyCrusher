using UnityEngine;
using System.Collections.Generic;
using InControl;


public enum ControllerStateChange
{
    Connected, Disconnected, Quit
}

public delegate void ControllerStateChangetHandler(ControllerStateChange stateChange);

public class ControllerManager : MonoBehaviour, VirtualControllerHandler
{
    #region variables
    private SmartphoneController smartphoneController;
    private KeyboardController keyboardController;

    private Dictionary<int, SmartphoneController> smartphoneControllers;

    private int currentSmartPhoneController = 0;
    private int maxSmartphoneConroller = 4;

    public static event ControllerStateChangetHandler ControllerStateChanged;
    #endregion

    #region methods

    #region initialization
    void Start()
    {
        smartphoneControllers = new Dictionary<int, SmartphoneController>(4);
        keyboardController = new KeyboardController();

        InputManager.AttachDevice(keyboardController);
    }
    #endregion

    #region reset
    void OnDestroy()
    {
        InputManager.DetachDevice(keyboardController);
        foreach (KeyValuePair<int, SmartphoneController> entry in smartphoneControllers)
        {
            InputManager.DetachDevice(entry.Value);
        }
    }
    #endregion

    #region controller updates
    public void VirtualControllerMoves(VirtualController virtualController, Vector2 movement)
    {
        smartphoneControllers.TryGetValue(virtualController.controllerID, out smartphoneController);
        if (smartphoneController != null)
        {
            smartphoneController.SetLeftAnalogStick = movement; 
        }
    }

    public void VirtualControllerShoots(VirtualController virtualController, Vector2 shoot)
    {
        smartphoneControllers.TryGetValue(virtualController.controllerID, out smartphoneController);
        if (smartphoneController != null)
        {
            smartphoneController.SetRightAnalogStick = shoot;
        }
    }

    public void VirtualControllerSendsSpecialAttack(VirtualController virtualController)
    {
        smartphoneControllers.TryGetValue(virtualController.controllerID, out smartphoneController);
        if (smartphoneController != null)
        {
            smartphoneController.SetAbilityPressed = true;
        }
    }

    public void VirtualControllerSendsBackButton(VirtualController virtualController)
    {
        smartphoneControllers.TryGetValue(virtualController.controllerID, out smartphoneController);
        if (smartphoneController != null)
        {
            smartphoneController.SetBackPressed = true;
        }
    }

    public void VirtualControllerSendsPauseButton(VirtualController virtualController)
    {
        smartphoneControllers.TryGetValue(virtualController.controllerID, out smartphoneController);
        if (smartphoneController != null)
        {
            smartphoneController.SetPausePressed = true;
        }
    }

    public void VirtualControllerQuitsTheGame(VirtualController virtualController)
    {
        RemoveVirtualController(virtualController);
        OnControllerStateChanged(ControllerStateChange.Quit);

    }

    public void VirtualControllerIsNotResponsing(VirtualController virtualController)
    {
        OnControllerStateChanged(ControllerStateChange.Disconnected);
    }
    #endregion

    #region event method
    private void OnControllerStateChanged(ControllerStateChange stateChange)
    {
        if (ControllerStateChanged != null)
        {
            ControllerStateChanged(stateChange);
        }
    }
    #endregion

    #region managing smartphone controllers
    public bool AddNewVirtualController(VirtualController virtualController)
    {
        if(currentSmartPhoneController < maxSmartphoneConroller)
        {
            currentSmartPhoneController++;
            virtualController.ConnectVirtualControllerToGame(this);
            smartphoneController = new SmartphoneController(virtualController);
            InputManager.AttachDevice(smartphoneController);
            smartphoneControllers.Add(virtualController.controllerID, smartphoneController);
            OnControllerStateChanged(ControllerStateChange.Connected);
            return true;
        }
        return false;
    }

    private void RemoveVirtualController(VirtualController virtualController)
    {
        smartphoneControllers.TryGetValue(virtualController.controllerID, out smartphoneController);
        if (smartphoneController != null)
        {
            InputManager.DetachDevice(smartphoneController);
            currentSmartPhoneController--;
        }
    }
    #endregion

    #endregion
}
