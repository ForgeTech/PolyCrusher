using UnityEngine;

public enum HighscoreType
{
    WaveCount = 0,
    PolygonTriggered = 1,
    PolyKill = 2,
    LinecutKill = 3,
    BossKill = 4,
    PowerUpsGathered = 5,
    RevivalExpenses = 6
}

public class HighscoreIdentifier : MonoBehaviour
{
    [Header("Set High Score Type!")]
    [SerializeField]
    private HighscoreType highscoreType = HighscoreType.WaveCount;

    public HighscoreType ScoreType
    {
        get { return this.highscoreType; }
    }
}