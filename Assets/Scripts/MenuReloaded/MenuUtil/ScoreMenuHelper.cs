using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

public class ScoreMenuHelper : MonoBehaviour
{
    #region Inspector variables
    [Header("Needed Inputs")]
    [SerializeField]
    private RectTransform scoreContainer;

    [SerializeField]
    private Text scoreText;

    [SerializeField]
    private Text playerCountLevelName;

    [SerializeField]
    private Text eventGameName;

    [Header("Tweening options")]
    [SerializeField]
    private LeanTweenType easeType = LeanTweenType.easeOutSine;

    [SerializeField]
    private float timeBeforeNextPhase = 0.5f;

    [SerializeField]
    private string hexColor = "#292929FF";

    [Header("Phase 1")]
    [SerializeField]
    private float startUpScaleTweenTime = 0.2f;

    [Header("Phase 2")]
    [SerializeField]
    private float textAddWaitTime = 0.01f;

    [SerializeField]
    private float nextLineWaitTime = 0.4f;

    [SerializeField]
    private float scaleTime = 0.4f;

    [SerializeField]
    private float upScale = 1.4f;

    [Header("Phase 3")]
    [SerializeField]
    private float totalScoreTime = 0.5f;

    [SerializeField]
    private float scoreTextUpScale = 1.1f;

    [Header("Audio")]
    [SerializeField]
    private AudioSource countClickSound;

    [SerializeField]
    private AudioSource countFinishedSound;

    [SerializeField]
    private AudioSource scoreSound;
    #endregion

    #region Internal members
    private readonly Dictionary<HighscoreType, ScoreData> highscoreEntries = new Dictionary<HighscoreType, ScoreData>();
    private string originalScoreText;

    private WaitForSeconds waitForPhase01;
    private WaitForSeconds waitForTextAdd;
    private WaitForSeconds waitForNextLine;
    private WaitForSeconds waitForNextPhase;
    #endregion

    private void Start ()
    {
        VerifyInputs();
        Initialize();
        StartHighscoreAnimationPhases();
        EnableMenuMusic();
    }

    private void StartHighscoreAnimationPhases()
    {
        // Init scores with 0
        foreach (var highscoreEntry in highscoreEntries)
            highscoreEntry.Value.scoreText.text = string.Format(highscoreEntry.Value.originalScoreText, 0);

        // Initiate the phases
        StartCoroutine(StartAnimationPhase01());
    }

    #region Phase01
    /// <summary>
    /// Fades container in and shows wave number.
    /// </summary>
    private IEnumerator StartAnimationPhase01()
    {
        LeanTween.scale(scoreText.rectTransform, Vector3.one, startUpScaleTweenTime).setEase(easeType);
        LeanTween.scale(scoreContainer, Vector3.one, startUpScaleTweenTime).setEase(easeType);

        scoreText.text = CreateHighscoreString("?", "?", "?");
        yield return waitForPhase01;

        yield return waitForNextPhase;
        StartCoroutine(StartAnimationPhase02());
    }
    #endregion

    #region Phase02
    /// <summary>
    /// Show the multiplication values of each highscore category.
    /// </summary>
    private IEnumerator StartAnimationPhase02()
    {
        ScoreContainer score = DataCollector.instance.getScoreContainer();
        Queue<ScoreData> animationWorkQueue = new Queue<ScoreData>();

        RegisterAnimationQueue(animationWorkQueue, score);

        // Count up each high score category
        foreach (var highscoreEntry in highscoreEntries)
        {
            countFinishedSound.Play();
            if (highscoreEntry.Key != HighscoreType.WaveCount)
            {
                ScoreData currentScore = animationWorkQueue.Dequeue();
                for (int i = 0; i < currentScore.scoreMultiplier; i++)
                {
                    countClickSound.Play();
                    highscoreEntry.Value.scoreText.text = string.Format(currentScore.originalScoreText, i + 1);
                    yield return waitForTextAdd;
                }
                countClickSound.Stop();
            }
            else // Special case for the wave score multiplier -> Floating point
            {
                ScoreData currentScore = animationWorkQueue.Dequeue();
                highscoreEntry.Value.scoreText.text = string.Format(currentScore.originalScoreText, currentScore.scoreMultiplier);
                yield return waitForTextAdd;
            }
            DoScaleTween(highscoreEntry.Value.scoreText.rectTransform, this.scaleTime, upScale);
            yield return waitForNextLine;
        }

        yield return waitForNextPhase;
        StartCoroutine(StartAnimationPhase03());
    }

    private void RegisterAnimationQueue(Queue<ScoreData> animationWorkQueue, ScoreContainer score)
    {
        if(score.getGameMode() == GameMode.NormalMode)
            AddAnimationEntry(animationWorkQueue, HighscoreType.WaveCount, score.getWaveMultiplier(), score.getWaveScore());
        else
            AddAnimationEntry(animationWorkQueue, HighscoreType.WaveCount, score.getYoloScore(), score.getYoloScore());     // Only show wave score not the multiplier -> In YOLO-Mode there should be no wave multiplier

        AddAnimationEntry(animationWorkQueue, HighscoreType.PolygonTriggered, score.getPolysTriggered(), score.getPolysTriggeredScore());
        AddAnimationEntry(animationWorkQueue, HighscoreType.PolyKill, score.getPolyKills(), score.getPolyKillsScore());
        AddAnimationEntry(animationWorkQueue, HighscoreType.LinecutKill, score.getCutKills(), score.getCutKillsScore());
        AddAnimationEntry(animationWorkQueue, HighscoreType.BossKill, score.getBossKills(), score.getBossKillsScore());
        AddAnimationEntry(animationWorkQueue, HighscoreType.PowerUpsGathered, score.getPowerupsCollected(), score.getPowerupsCollectedScore());
        AddAnimationEntry(animationWorkQueue, HighscoreType.RevivalExpenses, score.getPlayerRevivals(), score.getPlayerRevivalsScore());
    }

    private void DoScaleTween(RectTransform rect, float scaleTime, float upScale)
    {
        LeanTween.scale(rect, Vector3.one * upScale, scaleTime * 0.5f).setEase(easeType)
                .setOnComplete(() => {
                    LeanTween.scale(rect, Vector3.one, scaleTime * 0.5f).setEase(easeType);
                });
    }

    private void AddAnimationEntry(Queue<ScoreData> animationQueue, HighscoreType highscoreType, float scoreMultiplier, int scoreValue)
    {
        highscoreEntries[highscoreType].scoreMultiplier = scoreMultiplier;
        highscoreEntries[highscoreType].scoreValue = scoreValue;
        animationQueue.Enqueue(highscoreEntries[highscoreType]);
    }
    #endregion

    #region Phase03
    /// <summary>
    /// Shows the calculated values, the total score and the rank.
    /// </summary>
    /// <returns></returns>
    private IEnumerator StartAnimationPhase03()
    {
        ScoreContainer score = DataCollector.instance.getScoreContainer();
        Queue<ScoreData> animationWorkQueue = new Queue<ScoreData>();

        object onlineRank = null;
        if (BaseSteamManager.Instance.GetRank() == 0)
            onlineRank = "?";
        else
            onlineRank = BaseSteamManager.Instance.GetRank();

        RegisterAnimationQueue(animationWorkQueue, score);

        // Set each score values to the calculated score
        foreach (var highscoreEntry in highscoreEntries)
        {
            ScoreData currentScore = animationWorkQueue.Dequeue();
            highscoreEntry.Value.scoreText.text = string.Format("{0:0,0}", currentScore.scoreValue);
            DoScaleTween(highscoreEntry.Value.scoreText.rectTransform, scaleTime, upScale);
            countFinishedSound.Play();
            yield return waitForNextLine;
        }

        // Set score
        StringBuilder timeString = null;
        scoreSound.Play();
        if (score.getGameMode() == GameMode.NormalMode)
            scoreText.text = CreateHighscoreString((int)score.getWave(), score.getScoreSum(), 0);
        else
        {
            TimeUtil time = TimeUtil.MillisToTime(score.getYoloScore());
            timeString = new StringBuilder(string.Format("{0:00}", time.Minute))
                .Append(":")
                .Append(string.Format("{0:00}", time.Second))
                .Append(":")
                .Append(string.Format("{0:000}", time.Milliseconds));
            scoreText.text = CreateHighscoreString(timeString.ToString(), score.getScoreSum(), 0);
        }

        DoScaleTween(scoreText.rectTransform, scaleTime, scoreTextUpScale);
        yield return new WaitForSeconds(totalScoreTime);

        // Set online rank
        scoreSound.Play();
        if (score.getGameMode() == GameMode.NormalMode)
            scoreText.text = CreateHighscoreString((int)score.getWave(), score.getScoreSum(), onlineRank);
        else
            scoreText.text = CreateHighscoreString(timeString.ToString(), score.getScoreSum(), onlineRank);

        DoScaleTween(scoreText.rectTransform, scaleTime, scoreTextUpScale);
    }
    #endregion

    private string CreateHighscoreString(object wave, object score, object onlineRank)
    {
        return string.Format(originalScoreText, wave, score, onlineRank);
    }

    private void VerifyInputs()
    {
        if (scoreContainer == null)
            Debug.LogError("ScoreContainer is not set!");

        if (scoreText == null)
            Debug.LogError("ScoreText is not set!");

        if (playerCountLevelName == null)
            Debug.LogError("PlayerCountLevelName is not set!");
    }

    private void Initialize()
    {
        // Resize to 0
        scoreContainer.localScale = Vector3.zero;
        scoreText.rectTransform.localScale = Vector3.zero;

        InitializeWaitForSeconds();

        // Event game name  (Only active if this is an event build)
        ScoreContainer scoreData = DataCollector.instance.getScoreContainer();
        if (DataCollector.instance.eventBuild)
        {
            eventGameName.gameObject.SetActive(true);
            eventGameName.text = string.Format(eventGameName.text, scoreData.getGameName());
        }

        // Init highscore entries
        originalScoreText = scoreText.text.ToString();
        foreach (Transform child in scoreContainer.transform)
        {
            Text textItem = child.GetComponent<Text>();
            Text scoreNumberText = child.GetChild(0).GetComponent<Text>();
            HighscoreIdentifier scoreType = child.GetComponent<HighscoreIdentifier>();

            highscoreEntries.Add(scoreType.ScoreType, new ScoreData(textItem, scoreNumberText));
        }

        // Init player count and levelname
        string splittedLevelName = Regex.Replace(scoreData.getLevelName(), "([a-z](?=[A-Z])|[A-Z](?=[A-Z][a-z]))", "$1 ");
        
        playerCountLevelName.text = string.Format(playerCountLevelName.text,
            scoreData.getPlayerCount(),
            scoreData.getLevelName() == "" ? "?" : splittedLevelName.ToUpper());
    }

    private void InitializeWaitForSeconds()
    {
        waitForPhase01 = new WaitForSeconds(startUpScaleTweenTime);
        waitForTextAdd = new WaitForSeconds(textAddWaitTime);
        waitForNextLine = new WaitForSeconds(nextLineWaitTime);
        waitForNextPhase = new WaitForSeconds(timeBeforeNextPhase);
    }

    private class ScoreData
    {
        public readonly Text highScoreEntry;
        public readonly Text scoreText;

        public float scoreMultiplier = 0;
        public int scoreValue = 0;
        public readonly string originalScoreText;

        public ScoreData(Text highScoreEntry, Text scoreText)
        {
            this.highScoreEntry = highScoreEntry;
            this.scoreText = scoreText;
            originalScoreText = scoreText.text.ToString();
        }
    }

    #region menuMusicActivation
    private void EnableMenuMusic()
    {
        GameObject globalScripts = GameObject.FindGameObjectWithTag("GlobalScripts");
        if (globalScripts != null)
        {
            AudioSource menuMusic = globalScripts.GetComponent<AudioSource>();
            if (menuMusic != null)
            {
                menuMusic.Stop();
                menuMusic.volume = 1.0f;
                menuMusic.time = 0.0f;
                menuMusic.Play();
            }
        }
        else
        {
            Debug.Log("No global scripts game object found!");
        }
    }
    #endregion
}