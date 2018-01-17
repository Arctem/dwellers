using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RTS;

public class Lumberjack : Harvester {

    // Use this for initialization
    protected override void Start() {
        base.Start();
        harvestType = ResourceType.Wood;
    }

    // Update is called once per frame
    protected override void Update() {
        base.Update();
    }
}
