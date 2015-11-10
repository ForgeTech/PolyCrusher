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

    public WaveCounterManager WaveCounterManagerInstance
    {
        get 
        {
            if (waveCounterManagerInstance == null)
                waveCounterManagerInstance = GameObject.FindObjectOfType<WaveCounterManager>();

            return this.waveCounterManagerInstance;
        }
    }

    void Awake()
    {
        GameManager.WaveStarted += TriggerWaveCounterAnimation;

        if (waveTextPermanent != null)
        {
            waveTextPermanent.text = "";
        }
    }

	// Use this for initialization
	void Start () 
    {
	    
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

            waveNumber.text = GameManager.GameManagerInstance.Wave.ToString();

            if (waveTextPermanent != null)
            {
                waveTextPermanent.text = GameManager.GameManagerInstance.Wave.ToString();
            }

            if (GameManager.GameManagerInstance.IsBossWave)
                boss.SetTrigger("WaveStarted");
            else
                number.SetTrigger("WaveStarted");

            roundText.SetTrigger("WaveStarted");
            
        }
    }
}
