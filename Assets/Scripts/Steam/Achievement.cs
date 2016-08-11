using UnityEngine;
using System.Collections;

public class Achievement
{
    public string name;
    public string desc;
    public bool achieved;
    
    /// <param name="name">The "Display Name" that will be shown to players in game and on the Steam Community.</param>
    /// <param name="desc">The "Description" that will be shown to players in game and on the Steam Community.</param>
    public Achievement(string name, string desc)
    {
        this.name = name;
        this.desc = desc;
        this.achieved = false;
    }
}

public enum AchievementID : int
    {
        ACH_PLAY_42_GAMES,
        ACH_PLAY_21_GAMES,
        ACH_CURRENT_HIGHSCORE,
        ACH_PLAY_ALL_CHARACTERS,
        ACH_PLAY_WITH_FOUR,
        ACH_KILL_1000_ASSES,
        ACH_PLAY_ALONE,
        ACH_GET_ALL_POWERUPS,
        ACH_CUT_100_ENEMIES,
        ACH_CREDITS_VIEWED,
        ACH_PICK_SPACETIME_MANGO,
        ACH_A_MILLION_SHOTS,
        ACH_KILL_TWO_ENEMIES,
        ACH_DIED_IN_W1,
        ACH_STARTED_GAME_ONCE,
        ACH_STARTED_GAME_THRICE,
        ACH_STARTED_GAME_TEN_TIMES,
        ACH_SMARTPHONE_JOIN,
        ACH_REACH_W10,
        ACH_REACH_W20,
        ACH_REACH_W30,
        ACH_KILL_B055_WITH_CHICKEN,
        ACH_KILL_B055_WITH_CUTTING,
        ACH_KILL_40_ENEMIES_WITH_POLY,
        ACH_SMART_ENOUGH_FOR_THE_MENU,
        ACH_SURVIVE_YOLO_5_MINUTES,
        ACH_LAST_MAN_STANDING,
        ACH_DIED_IN_TRAP,
        ACH_NO_DAMAGE_UNTIL_W10,
};