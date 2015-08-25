using UnityEngine;
using System.Collections;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

/// <summary>
/// Object which should be filled with relevant game data
/// an event must have one of the listed enum types
/// </summary>
public class Event{
    public enum TYPE : byte
    {
        gameStart = 0,
        death = 1,
        kill = 2,
        powerup = 3,
        join = 4,
        disconnect = 5,
        ability = 6,
        superAbility = 7,
        waveUp = 8,
        sessionEnd = 9
    }

    #region Chain Methods

    public Event(TYPE type)
    {
        this.type = type;
    }

    public Event addPos(Transform t)
    {
        pos = new float[2];
        pos[0] = t.position.x;
        pos[1] = t.position.z;
        return this;
    }

    public Event addCharacter(string c)
    {
        this.character = c;
        return this;
    }


    public Event addEnemy(string e)
    {
        this.enemy = e;
        return this;
    }

    public Event addWave()
    {
        this.wave = getWave();

        return this;
    }

    public static float getWave()
    {
        float wave = 0;
        if (GameManager.gameManagerInstance != null)
        {
            wave= GameManager.gameManagerInstance.Wave;
            wave += (float)GameManager.GameManagerInstance.AccumulatedRessourceValue / (float)GameManager.gameManagerInstance.EnemyRessourcePool;
        }
        return wave;
    }

    public Event addLevel()
    {
        this.level = System.IO.Path.GetFileName(Application.loadedLevelName);
        return this;
    }

    public Event addPlayerCount()
    {
        this.playerCount = PlayerManager.PlayerCount;
        return this;
    }

    /// <summary>
    /// should be "phone" or "gamepad"
    /// </summary>
    public Event addDevice(string device){
        this.device = device;
        return this;
    }

    public Event addPowerup(string powerup)
    {
        this.powerup = powerup;
        return this;
    }

    /// <summary>
    /// Sets a name for this game (needed for highscore)
    /// </summary>
    public Event addGameName(string name)
    {
        this.name = name;
        return this;
    }

    /// <summary>
    /// Sets a contact email for this game (needed for highscore)
    /// </summary>
    public Event addEmail(string email)
    {
        this.email = email;
        return this;
    }

    #endregion

    /// <summary>
    /// Should be called when all parameters of the event are set
    /// </summary>
    public void send()
    {
        if (DataCollector.instance != null && DataCollector.instance.enabled == true)
        {
            DataCollector.instance.addEvent(this);
        }
    }

    public override string ToString()
    {
        return "Event (type: " + this.type.ToString() + ")";
    }

    #region Class Members
    //public ObjectId session_id { get; set; }
    public string session_id { get; set; }
    public TYPE type { get; set; }
    public int time { get; set; }    // milliseconds since session start

    [BsonIgnoreIfNull]
    public string character { get; set; }
    [BsonIgnoreIfNull]
    protected string enemy { get; set; }
    [BsonIgnoreIfNull]
    protected float? wave { get; set; }
    [BsonIgnoreIfNull]
    protected string level { get; set; }
    [BsonIgnoreIfNull]
    protected float[] pos { get; set; }
    [BsonIgnoreIfNull]
    protected int? playerCount { get; set; }    // ? to make variable nullable
    [BsonIgnoreIfNull]
    protected string device { get; set; }
    [BsonIgnoreIfNull]
    protected string powerup { get; set; }
    [BsonIgnoreIfNull]
    protected string name { get; set; }
    [BsonIgnoreIfNull]
    protected string email { get; set; }

    #endregion
}

/*

////// EXAMPLE EVENT ////////

new Event(Event.TYPE.ability).addPos(this.transform).addWave().addLevel().addCharacter(this.playerName).addPlayerCount().send();

*/
