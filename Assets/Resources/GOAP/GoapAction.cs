using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class GoapAction : MonoBehaviour {

    private Dictionary<string, object> preconditions, effects;

    public WorldObject target;
    private bool inRange = false;

    private float cost;

    public GoapAction() {
        preconditions = new Dictionary<string, object>();
        effects = new Dictionary<string, object>();
    }

    public void DoReset() {
        inRange = false;
        target = null;
        Reset();
    }

    public abstract void Reset();
    public abstract bool IsDone();
    public abstract bool CheckProceduralPrecondition(GameObject agent);
    public abstract bool Perform(GameObject agent);
    public abstract bool RequiresInRange();

    /**
	 * Are we in range of the target?
	 * The MoveTo state will set this and it gets reset each time this action is performed.
	 */
    public bool IsInRange() {
        return inRange;
    }

    public void SetInRange(bool inRange) {
        this.inRange = inRange;
    }

    public void AddPrecondition(string key, object value) {
        preconditions.Add(key, value);
    }

    public void RemovePrecondition(string key) {
        preconditions.Remove(key);
    }

    public void AddEffect(string key, object value) {
        effects.Add(key, value);
    }

    public void RemoveEffect(string key) {
        effects.Remove(key);
    }

    public float Cost {
        get {
            return cost;
        }
    }

    public Dictionary<string, object> Preconditions {
        get {
            return preconditions;
        }
    }

    public Dictionary<string, object> Effects {
        get {
            return effects;
        }
    }
}
