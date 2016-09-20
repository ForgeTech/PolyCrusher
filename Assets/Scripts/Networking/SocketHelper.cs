using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;

public static class SocketHelper
{
    private const int LISTEN_BUFFER = 100;

    public static Socket CreateTCPServer(int port, int version, Action<Socket> connected)
    {
        Socket listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        UnityThreadHelper.CreateThread(() => {
            try
            {
                listener.Bind(new IPEndPoint(IPAddress.Any, port));
                listener.Listen(LISTEN_BUFFER);

                // Start listening for connections.
                while (true)
                {
                    UnityThreadHelper.Dispatcher.Dispatch(() => {
                        Debug.Log("Waiting for a TCP connection...");
                    });
                    // Thread sleeps while waiting for an incoming connection
                    Socket handler = listener.Accept();
                    UnityThreadHelper.Dispatcher.Dispatch(() => {
                        Debug.Log("TCP data received");
                    });
                    // Handle data in Closure      
                    connected(handler);

                    handler.Shutdown(SocketShutdown.Both);
                    handler.Close();
                }
            }
            catch (Exception e)
            {
                // TODO: Error Handling
                UnityThreadHelper.Dispatcher.Dispatch(() => {
					Debug.Log(e);
				});
            }
            finally
            {
                UnityThreadHelper.Dispatcher.Dispatch(() => {
					Debug.Log("Socket closed!");
				});
                listener.Close();
            }
        });

        return listener;
    }

    public static UdpClient CreateUDPServer(int port, Action<IPEndPoint, byte[]> messageReceived)
    {
        // TODO Exception handling

        UdpClient listener = new UdpClient(port);
        IPEndPoint groupEndPoint = new IPEndPoint(IPAddress.Any, port);

        UnityThreadHelper.CreateThread(() =>
        {
            try
            {
                while (true)
                {
                    UnityThreadHelper.Dispatcher.Dispatch(() => {
                        Debug.Log("Waiting for a UDP connection...");
                    });
                    byte[] receivedBytes = listener.Receive(ref groupEndPoint);
                    UnityThreadHelper.Dispatcher.Dispatch(() => {
                        Debug.Log("UDP data received");
                    });
                    // TODO Add validation on ip with groupEndPoint. Optional
                    messageReceived(groupEndPoint, receivedBytes);
                }
            }
            catch (Exception e)
            {
                UnityThreadHelper.Dispatcher.Dispatch(() => {
					Debug.Log(e);
				});
                // TODO: Error Handling
            }
            finally
            {
                 UnityThreadHelper.Dispatcher.Dispatch(() => {
                        Debug.Log("Listener closed.");
                });
                listener.Close();
            }
        });

        return listener;
    }

    // TODO: Error handling socket will not be closed if he waites for a command
    public static void SendTCPCommand(IPAddress address, int port, byte[] data, Action<Socket> response)
    {
        // TODO: Use Async Task maybe
        UnityThreadHelper.CreateThread(() =>
        {
            // TODO Exception handling
            UnityThreadHelper.Dispatcher.Dispatch(() => {
                Debug.Log("lets send: " + address + " PORT: " + port);
            });

            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Connect(new IPEndPoint(address, port));

            socket.Send(data);
            response(socket);

            socket.Close();
            return;

        });
    }

    public static bool CheckVersion(Socket handler, int version)
    {
        byte[] versionBuffer = new byte[1];
        handler.Receive(versionBuffer);
        return CheckVersion(ref versionBuffer, version);
    }

    public static bool CheckVersion(ref byte[] message, int version)
    {
        if (message.Length < 1)
        {
            return false;
        }

        byte versionByte = message[0];

        byte[] newMessage = new byte[message.Length - 1];
        Array.Copy(message, 1, newMessage, 0, message.Length - 1);
        message = newMessage;

        return (versionByte == version);
    }

}
