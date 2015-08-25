using UnityEngine;
using System.Collections;

/// <summary>
/// Handles the behaviour of the wave ressource bar.
/// </summary>
public class WaveRessrouceBarInformation : MonoBehaviour 
{
    [Tooltip("The reference to the ressource bar.")]
    [SerializeField]
    protected UnityEngine.UI.Image ressourceBar;

	
	// Update is called once per frame
	void Update () 
    {
        CalculateWaveInformation();
	}

    /// <summary>
    /// Calculates an filles the ressource bar with the wave information.
    /// </summary>
    protected void CalculateWaveInformation()
    {
        if (ressourceBar != null)
        {
            // Calculate the normalized ressource value (Range from 0 to 1)
            float ressourceValue = (float) GameManager.GameManagerInstance.AccumulatedRessourceValue / (float) GameManager.gameManagerInstance.EnemyRessourcePool;

            // Set the fill amount.
            ressourceBar.fillAmount = ressourceValue;
        }
    }
}
