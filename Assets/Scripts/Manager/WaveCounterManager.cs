using UnityEngine;
using System.Collections;

/// <summary>
/// Manages the animation of the wave counter.
/// This script implements a singleton.
/// </summary>
public class WaveCounterManager : MonoBehaviour 
{
    // Reference to the instance
    private WaveCounterManager waveCounterManagerInstance;

    // Reference to the wave number.
    [SerializeField]
    protected UnityEngine.UI.Text waveNumber;

    // Reference to the wave round font.
    [SerializeField]
    protected UnityEngine.UI.Text waveRoundText;

    // Reference to the permanent wave number.
    [SerializeField]
    protected UnityEngine.UI.Text waveTextPermanent;

    // Reference to the boss text message.
    [SerializeField]
    protected UnityEngine.UI.Text bossText;

    // Reference to the special wave text message.
    [SerializeField]
    protected UnityEngine.UI.Text specialWaveText;

    // Reference to the play time label.
    [SerializeField]
    protected UnityEngine.UI.Text playTime;

    // Reference to the permanent scoreNumber;
    [SerializeField]
    protected UnityEngine.UI.Text scoreTextPermanent;

    //[SerializeField]
    //[Range(0f, 1f)]
    protected float scoreDamp = 0.9f;
    private int displayScore = 0;
    private float nextActionTime = 0f;
    private float sampleRate = 32;

    public WaveCounterManager WaveCounterManagerInstance
    {
        get 
        {
            if (waveCounterManagerInstance == null)
                waveCounterManagerInstance = GameObject.FindObjectOfType<WaveCounterManager>();

            return this.waveCounterManagerInstance;
        }
    }

    private void Awake()
    {
        // Only trigger wave animation in normal mode
        //if(GameManager.GameManagerInstance.CurrentGameMode == GameMode.NormalMode)
            GameManager.WaveStarted += TriggerWaveCounterAnimation;

        if (waveTextPermanent != null)
            waveTextPermanent.text = "";
    }

    private void Update()
    {
        if(GameManager.GameManagerInstance.CurrentGameMode == GameMode.YOLOMode)
            FillTimeLabel();

        // calculate display score
        if (Time.time > nextActionTime)
        {
            displayScore += (int)Mathf.Ceil((DataCollector.instance.intermediateScore - displayScore) * (1f - scoreDamp));
            nextActionTime = (float)(Time.time + (1.0 / sampleRate) - (Time.time - nextActionTime));

            if(scoreTextPermanent != null)
            {
                scoreTextPermanent.text = displayScore.ToString("N0");
            }
        }

    }

    /// <summary>
    /// Fills the time label if available.
    /// </summary>
    protected void FillTimeLabel()
    {
        if (playTime != null)
        {
            TimeUtil time = PlayerManager.PlayTime;
            playTime.text = string.Format("{0:00}:{1:00}:{2:00}", time.Minute, time.Second, time.Milliseconds);
        }
    }

    /// <summary>
    /// Triggers the wave counter animation.
    /// </summary>
    protected void TriggerWaveCounterAnimation()
    {
        if (waveNumber != null && waveRoundText != null)
        {
            Animator number = waveNumber.GetComponent<Animator>();
            Animator roundText = waveRoundText.GetComponent<Animator>();
            Animator boss = bossText.GetComponent<Animator>();
            Animator special = specialWaveText.GetComponent<Animator>();

            waveNumber.text = GameManager.GameManagerInstance.Wave.ToString();

            if (waveTextPermanent != null)
            {
                waveTextPermanent.text = GameManager.GameManagerInstance.Wave.ToString();
            }

            if (boss != null && GameManager.GameManagerInstance.IsBossWave)
                boss.SetTrigger("WaveStarted");
            else if (GameManager.GameManagerInstance.IsCurrentlySpecialWave)
                special.SetTrigger("WaveStarted");
            else
                number.SetTrigger("WaveStarted");

            roundText.SetTrigger("WaveStarted");
        }
    }
}
