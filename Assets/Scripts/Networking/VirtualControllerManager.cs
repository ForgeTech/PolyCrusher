﻿using UnityEngine;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.IO;

public class VirtualControllerManager : MonoBehaviour {

    // CONTROLLER MANAGER REFERENCE
    [SerializeField]
    private ControllerManager controllerManager = null;

    // CONNECTION PORTS
    private enum PORTS : int {
        PING = 11001,
        PING_SEND = 11002,
        REGISTER = 11000,
        CLOSE_GAME = 11003
    }
    private int UDP_PORT = 11100;
    private int CONTROLLER_ID = 100;

    // VERSIONS
    private enum VERSION : int {
        SUPPORTED_PING = 1,
        SUPPORTED_REGISTER = 1
    }

    // CONNECTION COMMANDS
    private enum COMMAND : byte {
        REGISTER_SUCCESS = 1,

        INVALID_VERSION = 255,
        NO_PORT_AVALAIBLE = 254,
        GAME_CLOSED = 0
    }

    // LIST OF VIRTUAL CONTROLLERS 
    private List<VirtualController> virtualControllers = new List<VirtualController>();

    // CONNECTION PROPERTIES
    private Socket registerSocket;
    private UdpClient pingSocket;

    private Socket connectionSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
    private List<IPEndPoint> virtualControllerEndPoints = new List<IPEndPoint>();

    // UNITY-MONOBEHAVIOUR STANDARD METHODS

	void Start ()
    {
        registerSocket = SocketHelper.CreateTCPServer((int)PORTS.REGISTER, (int)VERSION.SUPPORTED_REGISTER, (handler) => {
            if (SocketHelper.CheckVersion(handler, (int)VERSION.SUPPORTED_REGISTER))
            {
                HandleRegisterConnection(handler);
            }
        });

        pingSocket = SocketHelper.CreateUDPServer((int)PORTS.PING, (endPoint, receivedBytes) => {
            if (SocketHelper.CheckVersion(ref receivedBytes, (int)VERSION.SUPPORTED_PING))
            {
                HandlePingConnection(endPoint);
                AddVirtualControllerIP(endPoint);
            }
        });
    }

    void OnApplicationQuit()
    {
        Send((byte)COMMAND.GAME_CLOSED);

        for (int i = 0; i < virtualControllers.Count; i++)
        {
            virtualControllers[i].Disconnect();
            virtualControllers.Remove(virtualControllers[i]);
        } 
        registerSocket.Close();
        pingSocket.Close();
        connectionSocket.Close();
        virtualControllerEndPoints.Clear();
    }

    // PUBLIC METHODS

    void HandlePingConnection(IPEndPoint endPoint)
    {
        string gameName = "Noisy";
        UnityThreading.Task future = UnityThreadHelper.Dispatcher.Dispatch(() =>
        {
            gameName = BaseSteamManager.Instance.GetSteamName();
        });

        // Wait max. 3 seconds for the task to end
        future.WaitForSeconds(3f);
        gameName = gameName.ToUpper(); 

        byte[] gameNameData = UTF8Encoding.UTF8.GetBytes(gameName);
        byte[] gameNameLengthData;
        try {
            gameNameLengthData = BitConverter.GetBytes(Convert.ToUInt16(gameNameData.Length));
        } catch(OverflowException e) {
            gameName = gameName.Substring(0,8);
            gameNameData = UTF8Encoding.UTF8.GetBytes(gameName);
            gameNameLengthData = BitConverter.GetBytes(Convert.ToUInt16(gameNameData.Length));
        }

        MemoryStream memoryStream = new MemoryStream();
        memoryStream.Write(gameNameLengthData, 0, gameNameLengthData.Length);
        memoryStream.Write(gameNameData, 0, gameNameData.Length);

        // Sending back with UPD Game Name
        Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        socket.SendTo(memoryStream.ToArray(), new IPEndPoint(endPoint.Address, (int)PORTS.PING_SEND));
        socket.Close();
        
        memoryStream.Close();
    }

    void HandleRegisterConnection(Socket handler)
    {
        VirtualController virtualController = new VirtualController(UDP_PORT, CONTROLLER_ID);
        virtualControllers.Add(virtualController);

        if (controllerManager!= null && controllerManager.AddNewVirtualController(virtualController))
        {
            byte[] portData = BitConverter.GetBytes(Convert.ToUInt16(UDP_PORT));

            MemoryStream memoryStream = new MemoryStream();
            memoryStream.WriteByte((byte)COMMAND.REGISTER_SUCCESS);
            memoryStream.Write(portData, 0, portData.Length);

            handler.Send(memoryStream.ToArray());

            memoryStream.Close();
            UDP_PORT++;
            CONTROLLER_ID++;
        }
        else
        {
            handler.Send(new byte[] { (byte)COMMAND.NO_PORT_AVALAIBLE});
        }
    }

    // PRIVATE METHODS

    private bool CheckVersion(Socket handler, int version)
    {
        byte[] versionBuffer = new byte[1];
        handler.Receive(versionBuffer);
        return (versionBuffer[0] == version);
    }

    private void Send(byte command) {
        Send(new byte[] { command });
    } 

    private void Send(byte[] data) {
        foreach (IPEndPoint endPoint in virtualControllerEndPoints)
        {
            connectionSocket.SendTo(data, endPoint);
        }    
    }

    private void AddVirtualControllerIP(IPEndPoint endPoint){
        IPEndPoint controller = new IPEndPoint(endPoint.Address, (int)PORTS.CLOSE_GAME);
        if(!virtualControllerEndPoints.Contains(controller)){
            virtualControllerEndPoints.Add(controller);
        }
    }
}