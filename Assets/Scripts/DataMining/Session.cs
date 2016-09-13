using UnityEngine;
using System.Collections;
using MongoDB.Driver;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Net.NetworkInformation;
using System.Net;

public class Session
{
    public Session(GameMode gameMode)
    {
        macAddress = getMAC();
        version = DataCollector.instance.buildVersion;
        inEditor = Application.isEditor;
        string tempName = BaseSteamManager.Instance.GetSteamName();
        string tempId = BaseSteamManager.Instance.GetSteamID();
        if (tempName.Length != 0) {
            steamName = tempName;
        }

        if (tempId.Length != 0) {
            steamId = tempId;
        }

        switch (gameMode)
        {
            case GameMode.NormalMode: this.mode = "normal"; break;
            case GameMode.YOLOMode: this.mode = "yolo"; break;
            default: this.mode = gameMode.ToString(); break;
        }

        players = PlayerManager.PlayerCountInGameSession;
        time = (int)(Time.time * 1000);
    }

    //public ObjectId _id { get; set; }
    [BsonIgnore]
    public string _id { get; set; }
    public string macAddress { get; set; }
    public string steamId { get; set; }
    public string steamName { get; set; }
    public string version { get; set; }
    public bool inEditor { get; set; }
    public string mode { get; set; }
    [BsonIgnoreIfNull]
    public int players { get; set; }

    //public DateTime Timestamp { get; set; }

    /// <summary>
    /// start time of session
    /// </summary>
    [BsonIgnore]
    public int time { get; set; }


    /// <summary>
    /// retrieves mac address (get network interfaces)
    /// </summary>
    private static string getMAC()
    {
        //IPGlobalProperties computerProperties = IPGlobalProperties.GetIPGlobalProperties();
        NetworkInterface[] nics = NetworkInterface.GetAllNetworkInterfaces();
        string info = "";

        // only save address of first adapter
        for (int n = 0; n < 1; n++)
        {
            if (n > 0)
            {
                info += "\n";
            }


            PhysicalAddress address = nics[n].GetPhysicalAddress();
            byte[] bytes = address.GetAddressBytes();
            string mac = null;
            for (int i = 0; i < bytes.Length; i++)
            {
                mac = string.Concat(mac + (string.Format("{0}", bytes[i].ToString("X2"))));
                if (i != bytes.Length - 1)
                {
                    mac = string.Concat(mac + "-");
                }
            }

            info += mac;
        }
        return info;
    }

}

