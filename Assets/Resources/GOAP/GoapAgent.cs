using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoapAgent : MonoBehaviour {

    private FSM stateMachine;
    private FSM.FSMState idleState;
    private FSM.FSMState moveToState;
    private FSM.FSMState performActionState;

    private HashSet<GoapAction> availableActions;
    private Queue<GoapAction> currentPlan;

    private IGoap agent;

    private GoapPlanner planner;

    void Start() {
        stateMachine = new FSM();
        availableActions = new HashSet<GoapAction>();
        currentPlan = new Queue<GoapAction>();
        planner = new GoapPlanner();
        FindAgent();
        CreateIdleState();
        CreateMoveToState();
        CreatePerformActionState();
        stateMachine.PushState(idleState);
        LoadActions();
    }

    void Update() {
        stateMachine.Update(this.gameObject);
    }

    public void AddAction(GoapAction a) {
        availableActions.Add(a);
    }

    public void RemoveAction(GoapAction action) {
        availableActions.Remove(action);
    }

    public GoapAction GetAction(Type action) {
        foreach (GoapAction g in availableActions) {
            if (g.GetType().Equals(action)) {
                return g;
            }
        }

        return null;
    }

    private bool HasActionPlan() {
        return currentPlan.Count > 0;
    }

    private void CreateIdleState() {
        idleState = (fsm, gameObj) => {
            // GOAP Planning

            Dictionary<string, object> worldState = agent.WorldState();
            Dictionary<string, object> goal = agent.CreateGoalState();

            Queue<GoapAction> plan = GoapPlanner.Plan(gameObject, availableActions, worldState, goal);

            if (plan != null) {
                currentPlan = plan;
                agent.PlanFound(goal, plan);

                fsm.PopState();
                fsm.PushState(performActionState);
            }
            else {
                Debug.Log("<color=orange>Failed Plan:</color>" + goal);
                agent.PlanFailed(goal);
                fsm.PopState();
                fsm.PushState(idleState);
            }
        };
    }

    private void CreateMoveToState() {
        moveToState = (fsm, gameObj) => {
            GoapAction action = currentPlan.Peek();
            if (action.RequiresInRange() && action.target == null) {
                Debug.Log("<color=red>Fatal error:</color> Action requires a target but has none. Planning failed. You did not assign the target in your Action.CheckProceduralPrecondition()");
                fsm.PopState(); // move
                fsm.PopState(); // perform
                fsm.PushState(idleState);
                return;
            }

            if (agent.MoveAgent(action)) {
                fsm.PopState();
            }
        };
    }

    private void CreatePerformActionState() {
        performActionState = (fsm, gameObj) => {
            if (!HasActionPlan()) {
                Debug.Log("<color=red>Done actions</color>");
                fsm.PopState();
                fsm.PushState(idleState);
                agent.ActionsFinished();
                return;
            }

            GoapAction action = currentPlan.Peek();
            if (action.IsDone()) {
                currentPlan.Dequeue();
            }

            if (HasActionPlan()) {
                action = currentPlan.Peek();
                bool inRange = !action.RequiresInRange() || action.IsInRange();
                if (inRange) {
                    bool success = action.Perform(gameObj);

                    if (!success) {
                        fsm.PopState();
                        fsm.PushState(idleState);
                        agent.PlanAborted(action);
                    }
                }
                else {
                    fsm.PushState(moveToState);
                }
            }
            else {
                fsm.PopState();
                fsm.PushState(idleState);
                agent.ActionsFinished();
            }
        };
    }

    private void FindAgent() {
        foreach (Component comp in gameObject.GetComponents<Component>()) {
            if (typeof(IGoap).IsAssignableFrom(comp.GetType())) {
                agent = (IGoap) comp;
                return;
            }
        }
    }

    private void LoadActions() {
        GoapAction[] actions = gameObject.GetComponents<GoapAction>();
        foreach (GoapAction a in actions) {
            availableActions.Add(a);
        }
    }
}
