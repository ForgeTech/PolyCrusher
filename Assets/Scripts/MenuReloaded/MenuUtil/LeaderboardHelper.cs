﻿using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;

[RequireComponent(typeof(AbstractMenuManager))]
public class LeaderboardHelper : MonoBehaviour
{
    #region Inspector
    [SerializeField]
    private GameObject dataRow;

    [SerializeField]
    private RectTransform highscoreContainer;

    [SerializeField]
    private Color playerHighlightColor;

    [Header("Arrows")]
    [SerializeField]
    private Image leftArrow;
    [SerializeField]
    private Image rightArrow;

    [SerializeField]
    private Image leftArrowApply;
    [SerializeField]
    private Image rightArrowApply;

    [Header("Tween settings")]
    [SerializeField]
    private LeanTweenType easeType = LeanTweenType.easeOutSine;

    [SerializeField]
    private float tweenTime = 0.2f;
    #endregion

    #region Internal members
    private AbstractMenuManager menuManager;
    private SelectorWithSubSelector selector;
    private SubSelectionArrowHelper arrowTweenHelper;

    private const int leaderBoardCount = 10;
    List<LeaderboardEntry> leaderboardEntries = new List<LeaderboardEntry>(leaderBoardCount);
    #endregion

    private void Start()
    {
        Initialize();
        InitializeEntries();
    }

    private void Initialize()
    {
        menuManager = GetComponent<AbstractMenuManager>();
        selector = (SelectorWithSubSelector)menuManager.Selector;

        arrowTweenHelper = new SubSelectionArrowHelper(leftArrow, rightArrow, leftArrowApply,
            rightArrowApply, easeType, tweenTime, menuManager, selector);

        // Event registration
        menuManager.NavigationNext += arrowTweenHelper.RepositionArrows;
        menuManager.NavigationPrevious += arrowTweenHelper.RepositionArrows;
        menuManager.SubNavigationNext += arrowTweenHelper.RepositionArrows;
        menuManager.SubNavigationPrevious += arrowTweenHelper.RepositionArrows;

        menuManager.SubNavigationNext += arrowTweenHelper.DoRightArrowSizeTween;
        menuManager.SubNavigationPrevious += arrowTweenHelper.DoLeftArrowSizeTween;
    }

    private void InitializeEntries()
    {
        DeleteAllRows();
        for (int i = 0; i < leaderBoardCount; i++)
        {
            leaderboardEntries.Add(null);
            AddDataRow(-1, null, -1, -1);
        }
    }

    private GameObject AddDataRow(int rank, string name, int score, int wave)
    {
        GameObject row = Instantiate(dataRow);
        row.transform.SetParent(highscoreContainer.transform, false);

        foreach (Transform child in row.transform)
        {
            Text txt = child.GetComponent<Text>();
            string rankInfo = null;
            if (rank > 0 && child.name.Equals("Number"))
                rankInfo = rank.ToString();
            else if (name != null && child.name.Equals("GameName"))
                rankInfo = name;
            else if (score >= 0 && child.name.Equals("Score"))
                rankInfo = string.Format("{0:0,0}", score);
            else if (wave > 0 && child.name.Equals("Wave"))
                rankInfo = wave.ToString();

            txt.text = rankInfo == null ? "??" : rankInfo;
        }

        return row;
    }

    private void ShowLeaderboardEntries()
    {
        foreach (LeaderboardEntry entry in leaderboardEntries)
        {
            GameObject g = AddDataRow(entry.rank, entry.steamName, entry.score, entry.wave);
            ColorifyPlayerInHighscore(g);
        }
    }

    private void DeleteAllRows()
    {
        foreach (Transform child in highscoreContainer.transform)
            Destroy(child.gameObject);
    }

    public void SetLeaderboardEntries(List<LeaderboardEntry> steamEntries)
    {
        DeleteAllRows();
        this.leaderboardEntries.Clear();

        foreach (LeaderboardEntry entry in steamEntries)
            this.leaderboardEntries.Add(entry);
        
        ShowLeaderboardEntries();
    }

    private void ColorifyPlayerInHighscore(GameObject row)
    {
        string foundName = "";
        foreach (Transform child in row.transform) {
            if (child.name.Equals("GameName"))
                foundName = child.GetComponent<Text>().text;
        }

        if (foundName.Equals(BaseSteamManager.Instance.GetSteamName()))
        {
            foreach (Transform child in row.transform)
            {
                Text txt = child.GetComponent<Text>();
                txt.color = playerHighlightColor;
            }
        }
    }
}
