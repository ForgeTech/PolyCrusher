using UnityEngine;
using System;
using System.Text;
using System.Collections;
using System.Threading;
using System.Net;
using System.Net.Sockets;

// MARK: INTERFACE FOR VIRTUAL CONTROLLER
public interface VirtualControllerHandler
{
    void VirtualControllerMoves(VirtualController virtualController, Vector2 movement);

    void VirtualControllerShoots(VirtualController virtualController, Vector2 shoot);

    void VirtualControllerSendsSpecialAttack(VirtualController virtualController);

    void VirtualControllerSendsBackButton(VirtualController virtualController);

    void VirtualControllerSendsPauseButton(VirtualController virtualController);

    void VirtualControllerQuitsTheGame(VirtualController virtualController);

    void VirtualControllerIsNotResponsing(VirtualController virtualController);

    bool AddNewVirtualController(VirtualController virtualController);
}

public class VirtualController
{
    // ID FOR SMARTPHONE MANAGER
    public int controllerID;

    // MARK: PRIVATE PROPERTIES

    // GAME COMMANDS
    private enum COMMANDS : byte {
        MOVE = 1,
        SHOOT = 2,
        SPECIAL_ATTACK = 3,
        BACK_BUTTON = 4,
        PAUSE_BUTTON = 5,
        QUITTED_GAME = 6
    }
    // HANDLER INTERFACE PROPERTY
    private VirtualControllerHandler virtualControllerHandler;
    
    // CONNECTION PROPERTIES
    private int port;
    private UdpClient listener;

    // CONNECTION ALIVE VALUES
    private bool isAlive = true;
    private int ALIVE_INTERVAL = 2000;
    
    private const float ONE_PERCENT = 0.07f;

    // CONSTRUCTOR
    public VirtualController(int port, int id)
    {
        this.port = port;
        this.controllerID = id;
    }

    // PUBLIC METHODS

    public void ConnectVirtualControllerToGame(VirtualControllerHandler virtualControllerHandler) {
        this.virtualControllerHandler = virtualControllerHandler;

        UnityThreadHelper.Dispatcher.Dispatch(() => {
            listener = SocketHelper.CreateUDPServer(port, (endPoint, receivedBytes) =>
            {
                Debug.Log("UDP Socket listens!!");
                HandleGameCommand(receivedBytes);
            });
            UnityThreadHelper.CreateThread(() =>
            {
                IsVirtualControllerAlive();
            });
        });
    }

    public void Disconnect()
    {
        listener.Close();
    }

    // PRIVATE METHODS

    private void HandleGameCommand(byte[] receivedBytes)
    {
        if (receivedBytes.Length >= 1)
        {
            byte gameCommand = receivedBytes[0];

            switch (gameCommand)
            {
                case (byte)COMMANDS.MOVE:
                    Debug.Log("Movement Data received!");
                    Vector2 move = CalculateVectorValues(new byte[]{receivedBytes[1], receivedBytes[2]});
                    virtualControllerHandler.VirtualControllerMoves(this, move);
                    break;
                case (byte)COMMANDS.SHOOT:
                    Debug.Log("Shoot Data received!");
                    Vector2 shoot = CalculateVectorValues(new byte[]{receivedBytes[1], receivedBytes[2]});
                    virtualControllerHandler.VirtualControllerShoots(this, shoot);
                    break;
                case (byte)COMMANDS.SPECIAL_ATTACK:
                    Debug.Log("SpecialAttack-Command received!");
                    virtualControllerHandler.VirtualControllerSendsSpecialAttack(this);
                    break;
                case (byte)COMMANDS.BACK_BUTTON:
                    Debug.Log("BackButton-Command received!");
                    virtualControllerHandler.VirtualControllerSendsBackButton(this);
                    break;
                case (byte)COMMANDS.PAUSE_BUTTON:
                     Debug.Log("Pause-Command received!");
                    virtualControllerHandler.VirtualControllerSendsPauseButton(this);
                    break;
                case (byte)COMMANDS.QUITTED_GAME:
                    Debug.Log("Controller quitted game!");
                    virtualControllerHandler.VirtualControllerQuitsTheGame(this);
                    break;
                default:
                    break;
            }

            isAlive = true;
        }
        else
        {
            Debug.Log("Controller on Port: " + port + " sended invalid data");
        }
    }


    private void IsVirtualControllerAlive(){
        try {
            Thread.Sleep(ALIVE_INTERVAL);
        } catch(ThreadAbortException e) {
            Thread.ResetAbort();
        }

        if(!isAlive){
            virtualControllerHandler.VirtualControllerIsNotResponsing(this);
            Debug.Log("VC is not alive!");
        } else {
            Debug.Log("Is Alive");
            isAlive = false;
        }
    }

    private Vector2 CalculateVectorValues(byte[] receivedBytes){
        Vector2 pos = new Vector2((sbyte)receivedBytes[0], (sbyte)receivedBytes[1]);
        pos.x = (pos.x/ONE_PERCENT)/100;
        pos.y = (pos.y/ONE_PERCENT)/100;
        return pos;
    }

}