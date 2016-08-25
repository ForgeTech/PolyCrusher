using System.Collections;
using System.Collections.Generic;
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

        scoreText.text = CreateHighscoreString("42", "?", "?");
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

        // Test values -> Remove
        //score.setWave(21);
        //score.addPolysTriggered(13);
        //score.addPolyKills(44);
        //score.addCutKills(46);
        //score.addBossKills(3);
        //score.addPowerupsCollected(25);
        //score.addPlayerRevials(12);

        RegisterAnimationQueue(animationWorkQueue, score);

        // Count up each high score category
        foreach (var highscoreEntry in highscoreEntries)
        {
            ScoreData currentScore = animationWorkQueue.Dequeue();
            for (int i = 0; i < currentScore.scoreMultiplier; i++)
            {
                highscoreEntry.Value.scoreText.text = string.Format(currentScore.originalScoreText, i + 1);
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
        AddAnimationEntry(animationWorkQueue, HighscoreType.WaveCount, (int)score.getWave(), score.getWaveScore());
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

    private void AddAnimationEntry(Queue<ScoreData> animationQueue, HighscoreType highscoreType, int scoreMultiplier, int scoreValue)
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
        int onlineRank = 122;

        RegisterAnimationQueue(animationWorkQueue, score);

        // Set each score values to the calculated score
        foreach (var highscoreEntry in highscoreEntries)
        {
            ScoreData currentScore = animationWorkQueue.Dequeue();
            highscoreEntry.Value.scoreText.text = string.Format("{0:0,0}", currentScore.scoreValue);
            DoScaleTween(highscoreEntry.Value.scoreText.rectTransform, scaleTime, upScale);
            yield return waitForNextLine;
        }

        // Set score
        scoreText.text = CreateHighscoreString(score.getWave(), score.getScoreSum(), 0);
        DoScaleTween(scoreText.rectTransform, scaleTime, scoreTextUpScale);
        yield return new WaitForSeconds(totalScoreTime);

        // Set online rank
        scoreText.text = CreateHighscoreString(score.getWave(), score.getScoreSum(), onlineRank);
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
    }

    private void Initialize()
    {
        // Resize to 0
        scoreContainer.localScale = Vector3.zero;
        scoreText.rectTransform.localScale = Vector3.zero;

        InitializeWaitForSeconds();

        originalScoreText = scoreText.text.ToString();

        foreach (Transform child in scoreContainer.transform)
        {
            Text textItem = child.GetComponent<Text>();
            Text scoreNumberText = child.GetChild(0).GetComponent<Text>();
            HighscoreIdentifier scoreType = child.GetComponent<HighscoreIdentifier>();

            highscoreEntries.Add(scoreType.ScoreType, new ScoreData(textItem, scoreNumberText));
        }
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

        public int scoreMultiplier = 0;
        public int scoreValue = 0;
        public readonly string originalScoreText;

        public ScoreData(Text highScoreEntry, Text scoreText)
        {
            this.highScoreEntry = highScoreEntry;
            this.scoreText = scoreText;
            originalScoreText = scoreText.text.ToString();
        }
    }
}