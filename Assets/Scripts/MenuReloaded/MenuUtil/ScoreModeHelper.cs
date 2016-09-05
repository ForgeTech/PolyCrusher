using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class ScoreModeHelper : MonoBehaviour
{
    [SerializeField]
    [TextArea]
    private string yoloText = "";

	private void Awake ()
    {
        CheckGameMode();
	}

    private void CheckGameMode()
    {
        ScoreContainer score = DataCollector.instance.getScoreContainer();
        if (score.getGameMode() == GameMode.YOLOMode)
        {
            Text currentText = GetComponent<Text>();
            currentText.text = yoloText;
        }
    }
}