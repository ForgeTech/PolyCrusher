using UnityEngine;
using System.Collections;

public class PolygonUtil : MonoBehaviour {

    #region variables
    private bool loadingSoundPlaying;
    private bool declineSoundPlaying;

    private RumbleManager rumbleManager;
    #endregion

    #region properties
    public RumbleManager RumbleManager
    {
        set { rumbleManager = value; }
    }
    #endregion

    #region methods

    #region sounds
    private IEnumerator SoundCoolDown()
    {
        yield return new WaitForSeconds(1.2f);
        loadingSoundPlaying = false;
    }

    private IEnumerator SoundDeclineCoolDown()
    {
        yield return new WaitForSeconds(1.2f);
        declineSoundPlaying = false;
    }
    #endregion

    #region rumbleMethods
    /// <summary>
    /// Setting the rumble according to the poly status 
    /// </summary>
    private void DistanceRumble(BasePlayer[] playerScripts, float polyLerpDistance)
    {
        if (rumbleManager != null)
        {
            float distance = Mathf.Clamp01(polyLerpDistance) / 2.0f;
            for (int i = 0; i < playerScripts.Length; i++)
            {
                if (playerScripts[i].InputDevice != null)
                {
                    playerScripts[i].InputDevice.Vibrate(0.0f, distance);
                }
            }
        }
    }

    /// <summary>
    /// stopping the rumble for all players
    /// </summary>
    private void StopRumble(BasePlayer[] playerScripts)
    {
        if (rumbleManager != null)
        {
            for (int i = 0; i < playerScripts.Length; i++)
            {
                if (playerScripts[i].InputDevice != null)
                {
                    playerScripts[i].InputDevice.StopVibration();
                }
            }
        }
    }

    /// <summary>
    /// explosion rumble
    /// </summary>
    private void ExplosionRumble(BasePlayer[] playerScripts)
    {
        if (rumbleManager != null)
        {
            for (int i = 0; i < playerScripts.Length; i++)
            {
                if (playerScripts[i].InputDevice != null)
                {
                    rumbleManager.Rumble(playerScripts[i].InputDevice, RumbleType.PolygonExplosion);
                }
            }
        }
    }

    #endregion

    #region polyLerpDistance (distance of the players to each other)
    public float CalculatePolygonLerpDistance(GameObject[] players, float maximumPolygonDistance)
    {
        float distance = 0.0f;
        bool firstSet = false;

        for (int i = 0; i < players.Length; i++)
        {
            for (int j = i + 1; j < players.Length; j++)
            {
                float distanceNew = Vector3.Distance(players[i].transform.position, players[j].transform.position);

                if (!firstSet)
                {
                    firstSet = true;
                    distance = distanceNew;
                }
                else if (distance >= distanceNew)
                {
                    distance = distanceNew;
                }
            }
        }
        return distance /= maximumPolygonDistance;
    }
    #endregion

    #region playerEnergyLevels
    public bool CheckPlayerEnergyLevels(BasePlayer[] playerScripts)
    {
        for(int i = 0; i < playerScripts.Length; i++)
        {
            if (playerScripts[i].Energy != playerScripts[i].MaxEnergy)
            {
                return false;
            }
        }
        return true;
    }
    #endregion

    #region playerRequiredDistanceCheck
    public bool CheckPlayerDistances(GameObject[] players, float maximumPolygonDistance)
    {
        for (int i = 0; i < players.Length; i++)
        {
            for (int j = i + 1; j < players.Length; j++)
            {
                if (i != j && Vector3.Distance(players[i].transform.position, players[j].transform.position) < maximumPolygonDistance)
                {
                    return false;
                }
            }
        }
        return true;
    }
    #endregion

    #region playerAnglesFromTheMiddle (used to determine the correct winding order)
    /// <summary>
    /// provides the current rotation angle for the players according to a certain middle point
    /// important for the correct polygon mesh construction
    /// </summary>
    public GameObject[] AllignPlayers(GameObject[] players, int donkey)
    {
        Vector3 middle = new Vector3();
        float[] angles = new float[players.Length];

        for (int i = 0; i < players.Length; i++)
        {
            if (i != donkey)
            {
                middle += players[i].transform.position;
            }
        }
        if (donkey != -1)
        {
            middle /= (players.Length - 1);
        }
        else
        {
            middle /= players.Length;
        }

        for (int i = 0; i < players.Length; i++)
        {
            Vector3 tmp = new Vector3();
            Vector3 tmp2 = new Vector3();
            Vector3 tmp3 = middle * 100.0f;
            tmp3 = new Vector3(Mathf.Round(tmp3.x), Mathf.Round(tmp3.y), Mathf.Round(tmp3.z));
            tmp3 /= 100.0f;
            tmp3.y += 5.0f;

            tmp = (players[i].transform.position - tmp3) * 100.0f;
            tmp = new Vector3(Mathf.Round(tmp.x), Mathf.Round(tmp.y), Mathf.Round(tmp.z));
            tmp /= 100.0f;

            tmp2 = (tmp3);

            float newAngle = Mathf.Rad2Deg * (Mathf.Atan2(tmp.x * tmp2.x, tmp.z * tmp2.z));
            angles[i] = newAngle;
        }

        for (int i = 0; i < players.Length; i++)
        {
            for (int j = 0; j < players.Length; j++)
            {
                if (i != j && angles[i] < angles[j])
                {
                    GameObject go = players[i];
                    players[i] = players[j];
                    players[j] = go;

                    float tmp = angles[i];
                    angles[i] = angles[j];
                    angles[j] = tmp;
                }
            }
        }
        return players;
    }
    #endregion

    #region playerInvincibility
    /// <summary>
    /// resets the health and energy values after using the polygon ability
    /// </summary>
    void RestorePlayerPowers(BasePlayer[] playerGameObjects)
    {
        for (int i = 0; i < playerGameObjects.Length; i++)
        {
            playerGameObjects[i].Health = 100;
            playerGameObjects[i].Energy = 50;
        }

        StartCoroutine("PlayerInvincibility");
    }
    #endregion

    #endregion
}
