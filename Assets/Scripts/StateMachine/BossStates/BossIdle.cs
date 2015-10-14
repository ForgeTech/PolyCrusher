using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class BossIdle : FSMState
{
    #region Private Members


    #endregion

    
    public BossIdle()
    {
        this.stateID = StateID.BossIdle;
    }

    /// <summary>
    /// Actions of the State.
    /// </summary>
    /// <param name="player">Player reference</param>
    /// <param name="npc">NPC reference</param>
    public override void Act(GameObject player, GameObject npc)
    {
        //Play animation.
    }

    /// <summary>
    /// Reason for a transition to another state.
    /// </summary>
    /// <param name="player">Player reference</param>
    /// <param name="npc">NPC reference</param>
    public override void Reason(GameObject player, GameObject npc)
    {
        DecideNextState(player, npc);
    }

    /// <summary>
    /// Set entering conditions.
    /// </summary>
    public override void DoBeforeEntering()
    {
        base.DoBeforeEntering();
    }

    /// <summary>
    /// Set leaving conditions.
    /// </summary>
    public override void DoBeforeLeaving()
    {
        base.DoBeforeLeaving();
    }

    /// <summary>
    /// Decide which State should be chosen.
    /// </summary>
    private void DecideNextState(GameObject player, GameObject npc)
    {
        MonoBehaviour m = npc.GetComponent<MonoBehaviour>();

        if (m is BossEnemy)
        {
            BossEnemy e = (BossEnemy) m;

            // Only make a transition if the npc has an target player.
            if (e.TargetPlayer != null)
            {
                ChooseState(e);
            }            
        }
    }

    /// <summary>
    /// Calculates the probability for the State change based on the boss enemy values and changes to this state.
    /// </summary>
    /// <param name="e">Boss enemy</param>
    private void ChooseState(BossEnemy e)
    {
        // Total probability of all enabled Phases.
        float totalProb = 0f;

        //Random Value between 0 and 1.
        float randomValue = 0f;

        // Arrays for the total probability calculation.
        bool[] phaseEnabled = new bool[] { e.MeleePhase.phaseEnabled, e.RangedPhase.phaseEnabled, e.SpecialPhase.phaseEnabled };
        float[] probabilities = new float[] { e.MeleePhase.phaseProbability, e.RangedPhase.phaseProbability, e.SpecialPhase.phaseProbability };

        // Index of the probability based calculation.
        int indexFoundElement = -1;

        // Key Value list
        List<KeyValuePair<int, float>> indexProbability = new List<KeyValuePair<int, float>>();
        
        for (int i = 0; i < phaseEnabled.Length; i++)
        {
            if (!phaseEnabled[i])
                probabilities[i] = 0f;
            else
                totalProb += probabilities[i];

            // Fill List.
            indexProbability.Add(new KeyValuePair<int, float>(i, probabilities[i]));
        }

        // Sort List descending
        indexProbability.Sort((first, next) =>
        {
            return next.Value.CompareTo(first.Value);
        });
        
        // Map the range of the Random value to the total probability.
        randomValue = UnityEngine.Random.value * totalProb;

        // Search for the list element based on its probability.
        for (int i = 0; i < indexProbability.Count; i++)
        {
            if (randomValue < indexProbability[i].Value)    // Element found.
            {
                Debug.Log("Index: " + indexProbability[i].Key + ", Value: " + indexProbability[i].Value);
                //Save the found index.
                indexFoundElement = indexProbability[i].Key;
            }
            else
            {
                randomValue -= indexProbability[i].Value;
            }
        }

        // Switch state based on the calculatet probability.
        if (indexFoundElement == 0)
        {
            // Switch to attack state.
        }
        else if (indexFoundElement == 1)
        {
            // Switch to ranged state.
        }
        else if (indexFoundElement == 2)
        {
            // Switch to special state.
        }
    }
}
