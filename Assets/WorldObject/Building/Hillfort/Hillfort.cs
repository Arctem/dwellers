using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RTS;

public class Hillfort : Building {
    protected override void Start() {
        base.Start();
        actions = new string[] { "Hillfort", "Lumberyard" };
    }

    public override int Place(ResourceType type, int amount) {
        faction.AddResource(type, amount);
        return amount;
    }

    public override void PerformAction(string actionToPerform) {
        base.PerformAction(actionToPerform);
        CreateBuilding(actionToPerform);
    }

    private void CreateBuilding(string buildingName) {
        Vector3 buildPoint = new Vector3(transform.position.x, transform.position.y, transform.position.z + 10);
        if (player)
            player.CreateBuilding(buildingName, buildPoint, this, playingArea);
    }

    private void MakeWorker() {
        Builder builder = faction.AddUnit("Builder", spawnPoint, transform.rotation, this).GetComponent<Builder>();
        builder.SetProject(null);
    }
}
