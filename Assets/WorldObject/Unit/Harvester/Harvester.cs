using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RTS;

public class Harvester : Unit {
    public float collectionRate, depositRate;
    public Building resourceStore;

    private bool harvesting = false, emptying = false;
    private float currentHarvest = 0.0f;
    private float currentDeposit = 0.0f;
    private WorldObject collectionTarget;
    protected ResourceType harvestType;

    protected override void Start() {
        base.Start();
        harvestType = ResourceType.Unknown;
    }

    public override void SetCreator(Building creator) {
        base.SetCreator(creator);
        resourceStore = creator;
    }

    protected override void Update() {
        base.Update();
        if (!rotating && !moving) {
            if (harvesting || emptying) {
                if (harvesting) {
                    Collect();
                    if (IsFull(harvestType) || collectionTarget.IsEmpty(harvestType)) {
                        faction.UnClaim(collectionTarget, this);
                        collectionTarget = null;
                        harvesting = false;
                        emptying = true;
                        StartMove(resourceStore.transform.position, resourceStore.gameObject);
                    }
                }
                else {
                    Deposit();
                    if (IsEmpty(harvestType)) {
                        emptying = false;
                    }
                }
            }
            else {
                StartHarvest(FindClosestValidResource());
            }
        }
    }

    private void StartHarvest(WorldObject target) {
        if (!target)
            return;

        if (target.IsEmpty(harvestType)) {
            faction.UnClaim(target, this);
            collectionTarget = null;
            harvesting = emptying = false;
        }
        else if (faction.Claim(target, this)) {
            collectionTarget = target;
            StartMove(target.transform.position, target.gameObject);
            currentHarvest = 0.0f;
            harvesting = true;
            emptying = false;
        }
    }

    protected virtual WorldObject FindClosestValidResource() {
        List<WorldObject> resourcePoints = new List<WorldObject>(FindObjectsOfType<Resource>())
            .FindAll(r => !faction.isClaimed(r) && !r.IsEmpty(harvestType));
        return WorkManager.FindNearestWorldObjectInListToPosition(new List<WorldObject>(resourcePoints), this.transform.position);
    }

    private void Collect() {
        currentHarvest += collectionRate * Time.deltaTime;
        int harvest = Mathf.FloorToInt(currentHarvest);
        if (harvest >= 1) {
            if (SpaceRemaining(harvestType) < harvest) {
                harvest = SpaceRemaining(harvestType);
            }
            harvest = collectionTarget.Harvest(harvestType, harvest);
            AddResource(harvestType, harvest);
            currentHarvest -= harvest;
        }
    }

    private void Deposit() {
        currentDeposit += depositRate * Time.deltaTime;
        int deposit = Mathf.FloorToInt(currentDeposit);
        if (deposit >= 1) {
            if (deposit > getResource(harvestType))
                deposit = getResource(harvestType);
            deposit = resourceStore.Place(harvestType, deposit);
            currentDeposit -= deposit;
            RemoveResource(harvestType, deposit);
        }
    }

    protected override void DrawSelectionBox(Rect selectBox) {
        base.DrawSelectionBox(selectBox);
        if (harvestType == ResourceType.Unknown)
            return;
        float percentFull = (float) getResource(harvestType) / (float) getResourceLimit(harvestType);
        float maxHeight = selectBox.height - 4;
        float height = maxHeight * percentFull;
        float leftPos = selectBox.x + selectBox.width - 7;
        float topPos = selectBox.y + 2 + (maxHeight - height);
        float width = 5;
        Texture2D resourceBar = ResourceManager.GetResourceHealthBar(harvestType);
        if (resourceBar)
            GUI.DrawTexture(new Rect(leftPos, topPos, width, height), resourceBar);
    }
}
