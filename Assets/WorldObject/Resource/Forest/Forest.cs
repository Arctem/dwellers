using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RTS;

public class Forest : Resource {

    private int numTrees;

    protected override void Start() {
        base.Start();
        numTrees = GetComponentsInChildren<Tree>().Length;
    }

    protected override void Update() {
        base.Update();
        float percentLeft = (float) getResource(ResourceType.Wood) / (float) getResourceLimit(ResourceType.Wood);
        if (percentLeft < 0)
            percentLeft = 0;
        int numTreesToShow = Mathf.CeilToInt(percentLeft * numTrees);
        Tree[] trees = GetComponentsInChildren<Tree>();
        if (numTreesToShow >= 0 && numTreesToShow < trees.Length) {
            for (int i = numTreesToShow; i < trees.Length; i++) {
                foreach (Renderer r in trees[i].GetComponentsInChildren<Renderer>()) {
                    r.enabled = false;
                }
            }
            CalculateBounds();
        }
    }
}
