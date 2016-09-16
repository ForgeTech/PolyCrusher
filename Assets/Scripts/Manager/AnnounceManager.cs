using UnityEngine;
using System.Collections;

public class AnnounceManager : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField]
    [Tooltip("Determines if the next wave announce is played or not.")]
    protected bool playNextWaveSound = true;

    // Reference to the announcer voice.
    protected GameObject announcerVoice;

    // Audio clip references
    protected MultipleAudioclips randomStart;
    protected MultipleAudioclips nextWave;
    protected MultipleAudioclips trap;
    protected MultipleAudioclips bossWave;
    protected MultipleAudioclips playerDeath;
    protected MultipleAudioclips polyExecute;
    protected MultipleAudioclips polyFail;
    protected MultipleAudioclips specialWave;

    // Use this for initialization
    void Start ()
    {
        // Init audio
        InitializeAudioSource();

        // Random start sound
        AnnounceRandomStart();

        // Register events
        BasePlayer.PlayerDied += AnnouncePlayerDeath;
        Trap.TrapTriggered += AnnounceTrap;
        GameManager.WaveStarted += AnnounceNextWave;
        BossEnemy.BossSpawned += AnnounceBossWave;
        PolygonEnemyDetection.PolygonEnemyDeaths += AnnouncePolyFail;
	}

    /// <summary>
    /// Plays a random start sound.
    /// </summary>
    protected void AnnounceRandomStart()
    {
        if (randomStart != null)
            randomStart.PlayRandomClip();
    }

    /// <summary>
    /// Announce the next wave.
    /// </summary>
    protected void AnnounceNextWave()
    {
        if (nextWave != null)
        {
            if (GameManager.gameManagerInstance.Wave > 1)
            {
                if (!GameManager.GameManagerInstance.IsBossWave && !GameManager.gameManagerInstance.IsCurrentlySpecialWave && playNextWaveSound)
                    nextWave.PlayRandomClip();
                else if (GameManager.gameManagerInstance.IsCurrentlySpecialWave && playNextWaveSound)
                    specialWave.PlayRandomClip();
            }
        }
    }

    /// <summary>
    /// Announce a trap death.
    /// </summary>
    protected void AnnounceTrap(Trap t)
    {
        if (trap != null)
            trap.PlayRandomClip();
    }

    /// <summary>
    /// Announces the boss wave.
    /// </summary>
    protected void AnnounceBossWave(BossEnemy boss)
    {
        if (bossWave != null)
            bossWave.PlayRandomClip();
    }

    /// <summary>
    /// Announces the player death.
    /// </summary>
    protected void AnnouncePlayerDeath()
    {
        if (playerDeath != null)
            playerDeath.PlayRandomClip();
    }

    /// <summary>
    /// Announces the poly execute.
    /// </summary>
    protected void AnnouncePolyExecute()
    {
        if (polyExecute != null)
            polyExecute.PlayRandomClip();
    }

    /// <summary>
    /// Announces the poly fail.
    /// </summary>
    protected void AnnouncePolyFail(int count)
    {
        if (polyFail != null && count == 0)
            polyFail.PlayRandomClip();
    }

    /// <summary>
    /// Initializes the sound.
    /// </summary>
    protected virtual void InitializeAudioSource()
    {
        int childCount = transform.childCount;

        if (childCount == 0 || childCount > 1)
            Debug.LogError("AnnounceManager: Check child object!");
        else
        {
            // Get child object with the audio source
            announcerVoice = transform.GetChild(0).gameObject;

            if (announcerVoice != null)
            {
                MultipleAudioclips[] clips = announcerVoice.GetComponents<MultipleAudioclips>();

                for (int i = 0; i < clips.Length; i++)
                {
                    if (clips[i].AudioCategory == "RandomStart")
                        randomStart = clips[i];
                    if (clips[i].AudioCategory == "NextWave")
                        nextWave = clips[i];
                    if (clips[i].AudioCategory == "SpecialWave")
                        specialWave = clips[i];
                    if (clips[i].AudioCategory == "Trap")
                        trap = clips[i];
                    if (clips[i].AudioCategory == "BossWave")
                        bossWave = clips[i];
                    if (clips[i].AudioCategory == "PlayerDeath")
                        playerDeath = clips[i];
                    if (clips[i].AudioCategory == "PolyExecute")
                        polyExecute = clips[i];
                    if (clips[i].AudioCategory == "PolyFail")
                        polyFail = clips[i];
                }
            }
        }
    }
}