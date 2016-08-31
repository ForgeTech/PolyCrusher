using UnityEngine;

[RequireComponent(typeof(BasePlayer))]
public class HalfGodLikeAchievementChecker : MonoBehaviour
{
    private BasePlayer player;
    private int damageTaken = 0;

    private void Start()
    {
        player = GetComponent<BasePlayer>();
        player.DamageTaken += InrementDamage;
        GameManager.WaveEnded += CheckAchievementStatus;
    }

    private void CheckAchievementStatus()
    {
        if (GameManager.gameManagerInstance.Wave == 10 && damageTaken == 0)
            BaseSteamManager.Instance.LogAchievementData(AchievementID.ACH_NO_DAMAGE_UNTIL_W10);
    }

    private void InrementDamage(int damageTaken)
    {
        this.damageTaken += damageTaken;
    }
}