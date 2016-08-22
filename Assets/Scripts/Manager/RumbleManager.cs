using UnityEngine;
using System.Collections;
using InControl;
using System.Collections.Generic;


public enum RumbleType
{
    ChargerSpecial, ChargerSpecialFriends, Timesphere, LowHealth, BasicRumbleExtraShort, BasicRumbleShort, BasicRumbleLong, BasicRumble, PlayerDeath, PlayerRespawn, FatmanSpecial, FatmanSpecialNearby, PolygonExplosion, MusicBeat, NumberOfTypes
}

public class RumbleManager : MonoBehaviour
{


    [SerializeField]
    private static RumbleManager instance;

    private static bool rumbleEnabled = true;
    
    private static WaitForSeconds[] waitTimes;

    private static WaitForSeconds longWait = new WaitForSeconds(9.5f);


    private Dictionary<RumbleType, string> rumbleMethodNames;

    public static RumbleManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<RumbleManager>();
            }
            return instance;
        }
    }

    public bool RumbleEnabled
    {
        get
        {
            return rumbleEnabled;
        }
        set
        {
            rumbleEnabled = value;
        }
    }

    #region Methods

    void Awake()
    {
        instance = this;
        waitTimes = new WaitForSeconds[15];
        
        FillDictionary();
        FillWaitTimes();
    }


    private void FillDictionary()
    {
        rumbleMethodNames = new Dictionary<RumbleType, string>();
        RumbleType tmp;
        for (int i = 0; i < (int)RumbleType.NumberOfTypes; i++)
        {
            tmp = (RumbleType)i;
            rumbleMethodNames.Add(tmp, tmp.ToString());
        }
    }

    private void FillWaitTimes()
    {
        for(int i = 0; i < waitTimes.Length; i++)
        {
            waitTimes[i] = new WaitForSeconds(0.1f* i +0.1f);
        }
    }

    public void Rumble(InputDevice inputDevice, RumbleType rumbleType)
    {
        if (rumbleEnabled)
        {
            StartCoroutine(rumbleMethodNames[rumbleType], inputDevice);
        }
    }



    #endregion

    #region Rumble Coroutines

    private IEnumerator ChargerSpecial(InputDevice inputDevice)
    {
        inputDevice.Vibrate(0.15f, 0.4f);
        yield return waitTimes[2];

        inputDevice.Vibrate(0.5f, 1.0f);
        yield return waitTimes[3];

        inputDevice.StopVibration();
    
    }


    private IEnumerator ChargerSpecialFriends(InputDevice inputDevice)
    {
        inputDevice.Vibrate(0.0f, 0.25f);
        yield return waitTimes[2];

        inputDevice.Vibrate(0.25f, 0.7f);
        yield return waitTimes[3];

        inputDevice.StopVibration();
    }


    private IEnumerator FatmanSpecial(InputDevice inputDevice)
    {
        yield return waitTimes[1];

        for (int i = 0; i < 5; i++)
        {
            inputDevice.Vibrate(0.6f, 0.6f);
            yield return waitTimes[0];

            if (i < 4)
            {
                inputDevice.Vibrate(0.0f, 0.0f);
                yield return waitTimes[0];
            }
        }

        yield return waitTimes[2];

        inputDevice.StopVibration();
    }

    private IEnumerator FatmanSpecialNearby(InputDevice inputDevice)
    {
        yield return waitTimes[1];

        for (int i = 0; i < 5; i++)
        {
            inputDevice.Vibrate(0.4f, 0.4f);
            yield return waitTimes[1];

            if (i < 4)
            {
                inputDevice.Vibrate(0.0f, 0.0f);
                yield return waitTimes[0];
            }
        }

        yield return waitTimes[2];

        inputDevice.StopVibration();
    }


    private IEnumerator Timesphere(InputDevice inputDevice)
    {
       
        inputDevice.Vibrate(0.2f, 0.0f);
        yield return waitTimes[7];

        inputDevice.Vibrate(0.1f, 0.0f);
        yield return waitTimes[3];

        inputDevice.StopVibration();
    }

    private IEnumerator LowHealth(InputDevice inputDevice)
    {      
        for(int i = 0; i < 3; i++)
        {
            inputDevice.Vibrate(0.25f, 0.0f);
            yield return waitTimes[3];

            inputDevice.Vibrate(0.15f, 0.0f);
            yield return waitTimes[1];
        }
        

        inputDevice.StopVibration();
    }

    private IEnumerator BasicRumbleExtraShort(InputDevice inputDevice)
    {
        inputDevice.Vibrate(0.0f, 0.5f);
        yield return waitTimes[1];

        inputDevice.Vibrate(0.0f, 0.2f);
        yield return waitTimes[1];

        inputDevice.StopVibration();
    }


    private IEnumerator BasicRumbleShort(InputDevice inputDevice)
    {
        inputDevice.Vibrate(0.1f, 0.5f);
        yield return waitTimes[4];

        inputDevice.Vibrate(0.0f, 0.2f);
        yield return waitTimes[1];

        inputDevice.StopVibration();
    }

    private IEnumerator BasicRumbleLong(InputDevice inputDevice)
    {
        inputDevice.Vibrate(0.6f, 0.8f);
        yield return waitTimes[6];

        inputDevice.Vibrate(0.2f, 0.5f);
        yield return waitTimes[1];

        inputDevice.StopVibration();
    }


    private IEnumerator PolygonExplosion(InputDevice inputDevice)
    {
        inputDevice.Vibrate(0.8f, 0.8f);
        yield return waitTimes[8];

        inputDevice.Vibrate(0.4f, 0.4f);
        yield return waitTimes[6];

        inputDevice.Vibrate(0.2f, 0.2f);
        yield return waitTimes[1];

        inputDevice.StopVibration();
    }
  

    #endregion

}
