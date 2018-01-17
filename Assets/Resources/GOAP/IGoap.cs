using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IGoap {
    // Current state of the agent and world.
    Dictionary<string, object> WorldState();

    // Request the agent determine their new goal.
    Dictionary<string, object> CreateGoalState();

    // Callback for if a plan could not be found for a goal.
    void PlanFailed(Dictionary<string, object> failedGoal);

    // Callback for if a plan could be found for a goal.
    void PlanFound(Dictionary<string, object> goal, Queue<GoapAction> plan);

    // Callback for completion of a plan.
    void ActionsFinished();

    // An action caused the plan to abort.
    void PlanAborted(GoapAction action);

    // Called during Update to request the agent move itself toward the action's target.
    // Returns bool if the agent is in range of the target.
    bool MoveAgent(GoapAction nextAction);
}
