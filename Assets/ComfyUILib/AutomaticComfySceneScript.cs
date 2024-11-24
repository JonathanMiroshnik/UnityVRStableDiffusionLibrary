using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.PlayerLoop;
using static UnityEngine.Rendering.DebugUI;


[Serializable]
public class GameAction
{
    // Notice that the index that will be considered in any list of any Scene is only the first such occurence of the index in the list.
    // e.x. We want to activate index 11 in a list, but there are two items of index 11, only the first one is considered!
    public int index;

    // The length of time in seconds it will take to finish this game action(TimeToFinishAllVoiceLinesInUnityEvent + ExtraDelayBetweenEvents)
    public float TimeToFinishGameAction;

    // Name of the GameAction
    public string name;

    // Indicates that the GameAction should be started if it encountered
    public bool value;

    // The UnityEvent that is called in the GameAction
    public UnityEvent eventToCall;
}

public class AutomaticComfySceneScript : MonoBehaviour
{
    // Used to make a unique GameActions list in every scene.
    public List<GameAction> gameActions;

    // Used to hold onto GameActions that should occur but are on hold because previous ones have not finished
    private Queue<GameAction> gameActionsQueue;

    // Only one GameAction is activated at a time, the rest of the Queue waits
    private GameAction activatedGameAction;


    public int IndexToStart = -1;
    private void Start()
    {
        // Turns the List of GameActions into a Queue in accordance to its initial ordering
        gameActionsQueue = new Queue<GameAction>();
        foreach (GameAction action in gameActions)
        {
            gameActionsQueue.Enqueue(action);
        }

        StartCoroutine(StartGameAction());
    }


    // NOTICE: If a single action calls two different GameActions, they will both be put on hold, EVEN IF the point of
    // the single action is to be done twice(once like this, once like that) it will still put them both on hold, waiting
    // to be started. To fix this, just create a big enough delay between these two.
    private void Update()
    {
        if (activatedGameAction != null)
        {
            // Run the time of the currently active Game Action
            activatedGameAction.TimeToFinishGameAction -= Time.deltaTime;
            if (activatedGameAction.TimeToFinishGameAction < 0)
            {                
                activatedGameAction = null;
            }
        }
        else
        {            
            if (gameActionsQueue.Count <= 0) return;
            // If there is no currently active GameAction, check if the next one in the Queue is ready to be activate
            if (!gameActionsQueue.Peek().value) return;

            // Start the next GameAction
            activatedGameAction = gameActionsQueue.Dequeue();
            activatedGameAction.eventToCall?.Invoke();
        }         
    }

    /// <summary>
    /// Does the given index's action at the beginning of the game. If there is no need for it, set IndexToStart = -1
    /// </summary>
    IEnumerator StartGameAction()
    {
        while (GameManager.getInstance() == null)
        {
            yield return new WaitForSeconds(2);
        }

        if (IndexToStart < 0) yield break;
        if (IndexToStart >= gameActions.Count) yield break;

        DoGameAction(IndexToStart);

        yield break;
    }

    /// <summary>
    /// Called from various scripts in the game. Called by functions that symbolize the end of a part of the linear narrative.
    /// </summary>
    /// <param name="curInd">Index of the action to invoke</param>
    public void DoGameAction(int curInd)
    {
        if (curInd >= gameActions.Count) return;
        if (gameActions[curInd].value) return;
        if (activatedGameAction != null)
        {
            if (activatedGameAction.index == curInd) return;
        }

        // Changing this infomation in the List, will also change in the Queue
        gameActions[curInd].value = true;
    }
}

