using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FSM {
    private Stack<FSMState> stateStack = new Stack<FSMState>();

    public delegate void FSMState(FSM fsm, GameObject gameObject);

    public void Update(GameObject gameObject) {
        if (stateStack.Peek() != null) {
            stateStack.Peek().Invoke(this, gameObject);
        }
    }

    public void PushState(FSMState state) {
        stateStack.Push(state);
    }

    public FSMState PopState() {
        return stateStack.Pop();
    }
}

public interface FSMState {
    void Update(FSM fsm, GameObject gameObject);
}
