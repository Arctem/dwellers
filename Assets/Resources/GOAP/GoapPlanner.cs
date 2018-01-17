using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoapPlanner {
    /**
     * The plan starts at the goal and plans backwards.
     */
    public static Queue<GoapAction> Plan(GameObject agent, HashSet<GoapAction> availableActions, Dictionary<string, object> worldState, Dictionary<string, object> goal) {
        foreach (GoapAction a in availableActions) {
            a.DoReset();
        }

        HashSet<GoapAction> usableActions = new HashSet<GoapAction>();
        foreach (GoapAction a in availableActions) {
            if (a.CheckProceduralPrecondition(agent)) {
                usableActions.Add(a);
            }
        }

        List<Node> leaves = new List<Node>();

        Node start = new Node(null, 0, goal, null);
        bool success = BuildGraph(start, leaves, usableActions, worldState);

        if (!success) {
            Debug.Log(agent + " could not plan in its current state.");
            return null;
        }

        Node cheapest = null;
        foreach (Node leaf in leaves) {
            if (cheapest == null) {
                cheapest = leaf;
            }
            else if (leaf.runningCost < cheapest.runningCost) {
                cheapest = leaf;
            }
        }

        Queue<GoapAction> plan = new Queue<GoapAction>();
        Node n = cheapest;
        while (n != null) {
            plan.Enqueue(n.action);
            n = n.parent;
        }

        return plan;
    }

    private static bool BuildGraph(Node parent, List<Node> leaves, HashSet<GoapAction> usableActions, Dictionary<string, object> worldState) {
        bool foundPath = false;

        foreach (GoapAction action in usableActions) {
            // Check if our current state is a valid outcome of this action.
            if (SubsetOfState(action.Effects, parent.state)) {
                // Create the state we would be in before this action.
                Dictionary<string, object> currentState = RewindAction(parent.state, action);
                Node node = new Node(parent, parent.runningCost + action.Cost, currentState, action);

                // If we've reached a subset of the worldState, we're done!
                if (SubsetOfState(currentState, worldState)) {
                    leaves.Add(node);
                    foundPath = true;
                }
                else {
                    // If we haven't, recurse deeper. Remove the last used action to prevent repeated actions.
                    HashSet<GoapAction> subset = ActionSubset(usableActions, action);
                    foundPath = foundPath || BuildGraph(node, leaves, subset, worldState);
                }
            }
        }

        return foundPath;
    }

    private static HashSet<GoapAction> ActionSubset(HashSet<GoapAction> actions, GoapAction toRemove) {
        HashSet<GoapAction> subset = new HashSet<GoapAction>(actions);
        subset.Remove(toRemove);
        return subset;
    }

    /**
     * Returns true if every key/value in test is in state.
     */
    private static bool SubsetOfState(Dictionary<string, object> test, Dictionary<string, object> state) {
        foreach (string key in test.Keys) {
            if (!state.ContainsKey(key) || test[key] != state[key]) {
                return false;
            }
        }

        return true;
    }

    /**
     * Computes the state before the action. This consists of two steps: deleting the effects of the action and then applying its preconditions.
     * Assumes that state is a valid outcome of action (action.Effects is a subset of state).
     */
    private static Dictionary<string, object> RewindAction(Dictionary<string, object> state, GoapAction action) {
        Dictionary<string, object> newState = new Dictionary<string, object>(state);
        foreach (string key in action.Effects.Keys) {
            newState.Remove(key);
        }
        foreach (string key in action.Preconditions.Keys) {
            newState[key] = action.Preconditions[key];
        }

        return newState;
    }

    /**
     * Returns a merge of state and change, where the values of change take precedence.
     */
    private static Dictionary<string, object> ApplyStateChange(Dictionary<string, object> state, Dictionary<string, object> change) {
        Dictionary<string, object> newState = new Dictionary<string, object>(state);
        foreach (string key in change.Keys) {
            newState[key] = change[key];
        }

        return newState;
    }

    private class Node {
        public Node parent;
        public float runningCost;
        public Dictionary<string, object> state;
        public GoapAction action;

        public Node(Node parent, float runningCost, Dictionary<string, object> state, GoapAction action) {
            this.parent = parent;
            this.runningCost = runningCost;
            this.state = state;
            this.action = action;
        }
    }
}
