using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Put the labes for the transition here.
/// </summary>
public enum Transition
{
    NullTransition = 0,
    SawPlayer = 1,
    InPlayerAttackRange = 2,
    LostPlayerAttackRange = 3,
    Walking = 4,
    ReachedDestination = 5,
}

/// <summary>
/// Place the labels for the States here.
/// </summary>
public enum StateID
{
    NullStateID = 0,
    Idle = 1,
    FollowPlayer = 2,
    AttackPlayer = 3,
    WalkTowardsTarget = 4,
    ShootPlayer = 5,
}

/// <summary>
/// Represents a State of the Finite State Machine.
/// A state is represented by a Dictionary pair (Transition - State)
/// which determines which state the FSM should change when a Transition
/// is activated.
/// Reason(): Is used to determine which transition should be fired.
/// Act(): The actual logic. Perform the action of the NPC, etc..
/// </summary>
public abstract class FSMState
{
    // The dictionary for the transition and the state
    protected Dictionary<Transition, StateID> map = new Dictionary<Transition, StateID>();
    
    // The current StateID
    protected StateID stateID;

    /// <summary>
    /// Gets the current stateID.
    /// </summary>
    public StateID ID
    {
        get { return this.stateID; }
    }

    /// <summary>
    /// Adds a transition with the corresponding state.
    /// </summary>
    /// <param name="transition">Transition</param>
    /// <param name="id">State</param>
    public void AddTransition(Transition transition, StateID id)
    {
        // Check if the params are invalid, if valid --> add Transition to the map.
        if (transition == Transition.NullTransition)
            Debug.LogError("FSMState: NullTransition!");
        else if (id == StateID.NullStateID)
            Debug.LogError("FSMState: NullStateID!");
        else if (map.ContainsKey(transition))
            Debug.LogError("FSMState: State " + stateID.ToString() + " already has transition " + transition.ToString() + "!");
        else
            map.Add(transition, id);
    }

    /// <summary>
    /// Deletes a pair "transition-state" from the dictionary.
    /// </summary>
    /// <param name="transition"></param>
    public void DeleteTransition(Transition transition)
    {
        if (transition == Transition.NullTransition)
            Debug.LogError("FSMState: NullTransition!");
        else if (map.ContainsKey(transition))
        {
            map.Remove(transition);
        }
        else
            Debug.LogError("FSMState: Transition " + transition.ToString() + " was not on the state's transition list.");
    }

    /// <summary>
    /// Returns the new state the FSM should be if this state receives a transition.
    /// </summary>
    /// <param name="transition">Transition</param>
    /// <returns></returns>
    public StateID GetOutputState(Transition transition)
    {
        if (map.ContainsKey(transition))
            return map[transition];
        else
            return StateID.NullStateID;
    }

    /// <summary>
    /// Use this method to set up State condidtions before entering it.
    /// This method is called automatically bei the FSMSystem.
    /// </summary>
    public virtual void DoBeforeEntering() { }

    /// <summary>
    /// Use this method to make necessary changes (like reseting variables) before the 
    /// FSMSystem changes to another State. This method is called automatically.
    /// </summary>
    public virtual void DoBeforeLeaving() { }

    /// <summary>
    /// This method decides if the current state should transition to the
    /// new state.
    /// </summary>
    /// <param name="player">Reference to the player.</param>
    /// <param name="npc">Reference to the NPC which is controlled by this script.</param>
    public abstract void Reason(GameObject player, GameObject npc);

    /// <summary>
    /// This method controls the behaviour of the NPC.
    /// Every action of the NPC should be placed here.
    /// </summary>
    /// <param name="player">Reference to the player.</param>
    /// <param name="npc">Reference to the NPC that is controlled by this class.</param>
    public abstract void Act(GameObject player, GameObject npc);
}