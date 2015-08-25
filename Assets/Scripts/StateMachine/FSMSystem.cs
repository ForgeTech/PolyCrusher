using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Represents the actual Finite State machine.
/// It contains a list with all states of the NPC and implements methods
/// for adding and deleting states.
/// </summary>
public class FSMSystem
{
    // List of states.
    protected List<FSMState> states;

    // The current state ID.
    protected StateID currentStateID;
    
    /// <summary>
    /// Gets the current state id.
    /// </summary>
    public StateID CurrentStateID
    {
        get { return this.currentStateID; }
    }

    // Current state.
    protected FSMState currentState;

    /// <summary>
    /// Gets the current state.
    /// </summary>
    public FSMState CurrentState
    {
        get { return this.currentState; }
    }

    /// <summary>
    /// Initializes the FSMSystem.
    /// </summary>
    public FSMSystem()
    {
        states = new List<FSMState>();
    }

    /// <summary>
    /// Adds a new State into the FSM if it isn't already inside.
    /// The first state is also the initial state.
    /// </summary>
    /// <param name="state">State which will be added.</param>
    public void AddState(FSMState state)
    {
        if (state == null)
            Debug.LogError("FSMSystem: Null reference is not allowed!");
        else if (states.Count == 0) // Set initial state if it is the first state.
        {
            states.Add(state);
            currentState = state;
            currentStateID = state.ID;
        }
        else 
        {
            bool added = false;

            foreach (FSMState s in states)
            {
                if (s.ID == state.ID)
                {
                    added = true;
                    Debug.LogError("FSMSystem: State " + state.ID.ToString() + " has already been added.");
                }
            }

            if (!added)
                states.Add(state);
        }
    }

    /// <summary>
    /// Deletes a state if it exists.
    /// </summary>
    /// <param name="id">State id</param>
    public void DeleteState(StateID id)
    {
        if (id == StateID.NullStateID)
            Debug.LogError("FSMSystem: NullStateID not allowed!");
        else
        {
            //Search for the state.
            foreach (FSMState state in states)
            {
                if (state.ID == id)
                {
                    states.Remove(state);
                    return;         // :(
                }
            }
            Debug.LogError("FSMState: State with the id " + id.ToString() + " was not found.");
        }
    }

    /// <summary>
    /// Tries to change the state of the FSM based on the current state 
    /// and the given transition.
    /// If the current state does not have a target state for the passed transition,
    /// no transition will be performed.
    /// </summary>
    /// <param name="transition">Transition</param>
    public void PerformTransition(Transition transition)
    {
        // StateID of the desired transition.
        StateID id = currentState.GetOutputState(transition);

        if (transition == Transition.NullTransition)
            Debug.LogError("FSMState: NullTransition is not allowed.");
        else if (id == StateID.NullStateID)
            Debug.LogError("FSMState: State " + currentStateID.ToString() + " does not have a target state for transition " + transition.ToString());
        else
        {
            // Change current state
            currentStateID = id;

            foreach (FSMState state in states)
            {
                if (state.ID == currentStateID)
                {
                    // Call processing before leaving.
                    currentState.DoBeforeLeaving();

                    //Change current state.
                    currentState = state;

                    //Call proceccing after entering.
                    currentState.DoBeforeEntering();
                    break;
                }
            }
        }
    }
}
