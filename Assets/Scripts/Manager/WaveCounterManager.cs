﻿using UnityEngine;
using System.Collections;

/// <summary>
/// Manages the animation of the wave counter.
/// This script implements a singleton.
/// </summary>
public class WaveCounterManager : MonoBehaviour 
{
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

    // prefab for fading score number
    [SerializeField]
    protected GameObject scorePopup;

    // canvas to spawn popup on
    [SerializeField]
    protected GameObject canvas;

    //[SerializeField]
    //[Range(0f, 1f)]
    protected float scoreDamp = 0.9f;
    private int displayScore = 0;
    private float nextActionTime = 0f;
    private float sampleRate = 32;


    // Reference to the instance
    private static WaveCounterManager _instance;
    public static WaveCounterManager instance
    {
        get 
        {
            if (_instance == null)
                _instance = GameObject.FindObjectOfType<WaveCounterManager>();

            return _instance;
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

    private void Start()
    {
        if(canvas == null)
        {
            canvas = GameObject.FindGameObjectWithTag("IngameCanvas");
        }
        
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

    public void ScorePopup(int score)
    {
        if(GameManager.gameManagerInstance.CurrentGameMode == GameMode.NormalMode && scorePopup != null && canvas != null && score != 0)
        {
            //Debug.Log("[popup] " + score);

            GameObject popup = Instantiate(scorePopup);
            UnityEngine.UI.Text text = popup.GetComponent<UnityEngine.UI.Text>();

            float multiplikator = 1.2f;
            if (score >= 0)
            {
                multiplikator = 1 + score / 1000f;
            }
            float multiplikatorScale = 1 + (multiplikator - 1) * 0.1f;

            text.color = new Color(1f, 2.4f - multiplikator, 0);

            if (text != null)
            {
                if (score >= 0)
                    text.text = "+" + score.ToString("N0");
                else{
                    text.text = score.ToString();
                    text.color = Color.red;
                }
            }

            popup.transform.SetParent(canvas.transform, false);
            RectTransform rectTrans = popup.GetComponent<RectTransform>();
            LeanTween.moveY(rectTrans, rectTrans.position.y + 120, 3.5f).setEase(LeanTweenType.easeOutQuad);
            LeanTween.alphaText(rectTrans, 0, 3.5f).setEase(LeanTweenType.easeOutQuart).setOnComplete(() => { Destroy(popup); });

            LeanTween.scale(rectTrans, Vector3.one * multiplikator, 0.05f).setEase(LeanTweenType.easeInQuad).setOnComplete(()=> { LeanTween.scale(rectTrans, Vector3.one * multiplikatorScale, 0.1f).setEase(LeanTweenType.easeInQuad); });
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
