using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RTS;

public class Resource : WorldObject {
    public bool infinite;

    public int startWood, startIron;
    protected override void Start() {
        base.Start();
        AddStartResources();
    }

    private void AddStartResources() {
        AddResource(ResourceType.Wood, startWood);
        AddResource(ResourceType.Iron, startIron);
    }


    public override int Harvest(ResourceType type, int amount) {
        if (infinite && getResourceLimit(type) > 0) {
            return amount;
        }
        else
            return base.Harvest(type, amount);
    }

    public override bool IsEmpty(ResourceType type) {
        return !infinite && base.IsEmpty(type);
    }
}
