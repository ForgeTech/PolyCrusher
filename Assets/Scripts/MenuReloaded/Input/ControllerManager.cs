using UnityEngine;
using System.Collections.Generic;
using InControl;


public enum ControllerStateChange
{
    Connected, Disconnected, Quit
}

public delegate void ControllerStateChangetHandler(ControllerStateChange stateChange);
public delegate void MidSessionControllerConnectHandler(VirtualController virtualController);

public class ControllerManager : MonoBehaviour, VirtualControllerHandler
{
    #region variables
    private SmartphoneController smartphoneController;
    private KeyboardController keyboardController;

    private Dictionary<int, SmartphoneController> smartphoneControllers;
    private List<int> pendingSmartPhoneControllers;

    private int currentSmartPhoneController = 0;
    private int maxSmartphoneConroller = 4;

    public static event ControllerStateChangetHandler ControllerStateChanged;
    public static event MidSessionControllerConnectHandler MidSessionControllerConnect;
    #endregion

    #region methods

    #region initialization
    void Start()
    {
        smartphoneControllers = new Dictionary<int, SmartphoneController>(4);
        pendingSmartPhoneControllers = new List<int>();
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
        pendingSmartPhoneControllers.Add(virtualController.controllerID);
        OnControllerStateChanged(ControllerStateChange.Disconnected);
    }
    #endregion

    #region event methods
    private void OnControllerStateChanged(ControllerStateChange stateChange)
    {
        if (ControllerStateChanged != null)
        {
            ControllerStateChanged(stateChange);
        }
    }

    private void OnMidSessionControllerConnect(VirtualController virtualController)
    {
        if (MidSessionControllerConnect != null)
        {
            MidSessionControllerConnect(virtualController);
        }
    }
    #endregion

    #region managing smartphone controllers
    public bool AddNewVirtualController(VirtualController virtualController)
    {
        if (pendingSmartPhoneControllers.Count > 0)
        {
            virtualController.ConnectVirtualControllerToGame(this);
            smartphoneController = smartphoneControllers[pendingSmartPhoneControllers[0]];

            smartphoneControllers.Remove(pendingSmartPhoneControllers[0]);
            pendingSmartPhoneControllers.RemoveAt(0);

            smartphoneControllers.Add(virtualController.controllerID, smartphoneController);
            OnControllerStateChanged(ControllerStateChange.Connected);
            
            return true;
        }
        else if(currentSmartPhoneController < maxSmartphoneConroller)
        {
            currentSmartPhoneController++;
            virtualController.ConnectVirtualControllerToGame(this);
            smartphoneController = new SmartphoneController();
            InputManager.AttachDevice(smartphoneController);
            smartphoneControllers.Add(virtualController.controllerID, smartphoneController);
            OnControllerStateChanged(ControllerStateChange.Connected);
            if(PlayerManager.PlayTime.TotalTime >= 0.0f)
            {
                OnMidSessionControllerConnect(virtualController);
            }

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
