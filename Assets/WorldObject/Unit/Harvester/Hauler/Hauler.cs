using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RTS;

public class Hauler : Harvester {
    protected override WorldObject FindClosestValidResource() {
        List<WorldObject> gatherBuildings = new List<WorldObject>(faction.GetComponentsInChildren<Building>())
            .FindAll(b => b.IsMemberOf(faction) && !faction.isClaimed(b));
        gatherBuildings.Sort(HaulerSorter);
        WorldObject closest = gatherBuildings[0];

        if(!closest.IsEmpty(ResourceType.Iron)) {
            harvestType = ResourceType.Iron;
        } else if (!closest.IsEmpty(ResourceType.Wood)) {
            harvestType = ResourceType.Wood;
        } else {
            return null;
        }
        Debug.Log("Going to " + closest);
        return closest;
    }

    private static int HaulerSorter(WorldObject a, WorldObject b) {
        return b.GetFullness().CompareTo(a.GetFullness());
    }
}
