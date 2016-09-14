﻿using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.IO;

public class VirtualControllerManager : MonoBehaviour {

    // CONNECTION PORTS
    private enum PORTS : int {
        PING = 11001,
        PING_SEND = 11002,
        REGISTER = 11000
    }
    private int UDP_PORT = 11100;

    // VERSIONS
    private enum VERSION : int {
        SUPPORTED_PING = 1,
        SUPPORTED_REGISTER = 1
    }

    // CONNECTION COMMANDS
    private enum COMMAND : byte {
        REGISTER_SUCCESS = 1,
        INVALID_VERSION = 255,
        NO_PORT_AVALAIBLE = 254
    }

    // LIST OF VIRTUAL CONTROLLERS 
    private List<VirtualController> virtualControllers = new List<VirtualController>();

    // CONNECTION PROPERTIES
    private Socket registerSocket;
    private UdpClient pingSocket;

    // MARK: UNITY-MONOBEHAVIOUR STANDARD METHODS

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
            }
        });
    }

    void OnApplicationQuit()
    {
        Debug.Log("Application is Quitting, all Sockets will be closed");
        for (int i = 0; i < virtualControllers.Count; i++)
        {
            virtualControllers[i].Disconnect();
            virtualControllers.Remove(virtualControllers[i]);
        } 
        registerSocket.Close();
        pingSocket.Close();
    }

    // MARK: PUBLIC METHODS

    void HandlePingConnection(IPEndPoint endPoint)
    {
        // TODO: Get Game Name -> 
        string gameName = "Windows";

        byte[] gameNameData = UTF8Encoding.UTF8.GetBytes(gameName);
        // TODO: Think about handling what if length is longer than a UInt16?
        byte[] gameNameLengthData = BitConverter.GetBytes(Convert.ToUInt16(gameNameData.Length));

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
        VirtualController virtualController = new VirtualController(UDP_PORT);
        virtualControllers.Add(virtualController);

        // Connect to Andi
        if (AddNewVirtualController(virtualController))
        {
            byte[] portData = BitConverter.GetBytes(Convert.ToUInt16(UDP_PORT));

            MemoryStream memoryStream = new MemoryStream();
            memoryStream.WriteByte((byte)COMMAND.REGISTER_SUCCESS);
            memoryStream.Write(portData, 0, portData.Length);

            handler.Send(memoryStream.ToArray());

            memoryStream.Close();
            UDP_PORT++;
        }
        else
        {
            handler.Send(new byte[] { (byte)COMMAND.NO_PORT_AVALAIBLE});
        }
    }

    // MARK: PRIVATE METHODS

    private bool CheckVersion(Socket handler, int version)
    {
        byte[] versionBuffer = new byte[1];
        handler.Receive(versionBuffer);
        return (versionBuffer[0] == version);
    }

    // TODO: Only sample code remove this later!
    private bool AddNewVirtualController(VirtualController virtualController)
    {
        virtualController.ConnectVirtualControllerToGame(null);

        return true;
    }

}