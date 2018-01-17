using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RTS;

public class Builder : Unit {
    private bool building = false;
    private Building currentProject;
    public float buildRate;
    private float currentBuild = 0.0f;
    private Dictionary<ResourceType, int> resourcesSpent;

    public float gatherRate;
    private float currentGather = 0.0f;
    private Building gatherTarget;


    protected override void Awake() {
        base.Awake();
        resourcesSpent = WorkManager.InitResourceList();
    }

    protected override void Update() {
        base.Update();
        if (!rotating && !moving) {
            if (currentProject) {
                PerformConstruction();
            }
            else {
                Idle();
            }
        }
    }

    protected override void DrawSelectionBox(Rect selectBox) {
        base.DrawSelectionBox(selectBox);
        //TODO: Use the resource colors? Use the Harvester version of this but draw one bar for each resource.
        float percentFull = PercentInventoryFull();
        float maxHeight = selectBox.height - 4;
        float height = maxHeight * percentFull;
        float leftPos = selectBox.x + selectBox.width - 7;
        float topPos = selectBox.y + 2 + (maxHeight - height);
        float width = 5;
        Texture2D resourceBar = ResourceManager.ConstructionTexture;
        if (resourceBar)
            GUI.DrawTexture(new Rect(leftPos, topPos, width, height), resourceBar);
    }

    public void PerformConstruction() {
        if (gatherTarget) {
            Gather();
            if (EnoughResources()) {
                StartBuilding();
            }
        }
        else if (building) {
            Build();
            if (DoneBuilding()) {
                CompleteBuilding();
            }
            else if (IsEmpty()) {
                building = false;
            }
        }
        else {
            if (EnoughResources()) {
                StartBuilding();
            }
            else {
                Building potentialTarget = FindClosestValidResource();
                if (potentialTarget) {
                    if (faction.Claim(potentialTarget, this)) {
                        gatherTarget = potentialTarget;
                        building = false;
                        currentGather = 0.0f;
                        StartMove(gatherTarget.transform.position, gatherTarget.gameObject);
                    }
                }
                else if (!IsEmpty()) {
                    StartBuilding();
                }
            }
        }
    }

    private void Gather() {
        currentGather += gatherRate * Time.deltaTime;
        int gather = Mathf.FloorToInt(currentGather);
        if (gather >= 1) {
            foreach (ResourceType type in currentProject.getCost().Keys) {
                if (AmountToGather(type) > 0 && !IsFull(type) && gatherTarget.getResource(type) > 0) {
                    if (SpaceRemaining(type) < gather) {
                        gather = SpaceRemaining(type);
                    }
                    gather = gatherTarget.Harvest(type, gather);
                    AddResource(type, gather);
                    currentGather -= gather;
                    return;
                }
            }
            //Something went wrong.
            Debug.LogError(this + " could not pick up a resource from " + gatherTarget);
        }
    }

    private Building FindClosestValidResource() {
        List<Building> gatherBuildings = new List<Building>(faction.GetComponentsInChildren<Building>())
            .FindAll(b => b.IsMemberOf(faction) && !faction.isClaimed(b) && HasDesiredResources(b));
        gatherBuildings.Sort(BuilderSourceSorter);
        if (gatherBuildings.Count == 0) {
            Debug.Log(this + " is waiting for a building with resources.");
            return null;
        }
        Building best = gatherBuildings[0];

        Debug.Log(this + " going to " + best);
        return best;
    }

    private bool HasDesiredResources(Building b) {
        Dictionary<ResourceType, int> cost = currentProject.getCost();
        foreach (ResourceType type in cost.Keys) {
            if (AmountToGather(type) > 0 && !IsFull(type) && b.getResource(type) > 0) {
                return true;
            }
        }
        return false;
    }

    private int AmountToBuild(ResourceType type) {
        return currentProject.getCost()[type] - resourcesSpent[type];
    }

    private int AmountToGather(ResourceType type) {
        return currentProject.getCost()[type] - getResource(type) - resourcesSpent[type];
    }

    private int BuilderSourceSorter(WorldObject a, WorldObject b) {
        int compareResult = GetDemandFulfillment(b).CompareTo(GetDemandFulfillment(a));
        return compareResult;
    }

    private float GetDemandFulfillment(WorldObject wo) {
        Dictionary<ResourceType, int> cost = currentProject.getCost();
        int totalDemand = 0;
        int totalFulfillment = 0;
        foreach (ResourceType type in cost.Keys) {
            int demand = AmountToGather(type);
            totalDemand += demand;
            totalFulfillment += Mathf.Min(wo.getResource(type), demand);
        }
        return (float) totalFulfillment / (float) totalDemand;
    }

    private void StartBuilding() {
        building = true;
        faction.UnClaim(gatherTarget, this);
        gatherTarget = null;
        currentBuild = 0.0f;
        StartMove(currentProject.transform.position, currentProject.gameObject);
    }

    private void Build() {
        currentBuild += buildRate * Time.deltaTime;
        int build = Mathf.FloorToInt(currentBuild);
        if (build >= 1) {
            foreach (ResourceType type in currentProject.getCost().Keys) {
                if (AmountToBuild(type) > 0 && !IsEmpty(type)) {
                    if (AmountToBuild(type) < build) {
                        build = SpaceRemaining(type);
                    }
                    build = Harvest(type, build);
                    resourcesSpent[type] += build;
                    currentBuild -= build;
                    Debug.Log(this + " has built " + CurrentProgress() + " of " + currentProject);
                    currentProject.SetConstructionProgress(CurrentProgress());
                    return;
                }
            }
            //Something went wrong.
            Debug.LogError(this + " could not use any resource to build " + currentProject);
        }
    }

    private void CompleteBuilding() {
        currentProject.FinishConstruction();
        faction.UnClaim(currentProject, this);
        faction.UnClaim(gatherTarget, this);
        currentProject = gatherTarget = null;
        currentBuild = currentGather = 0.0f;
        building = false;
        resourcesSpent = WorkManager.InitResourceList();
    }

    private void Idle() {
        //TODO: Go back home?
        Destroy(this.gameObject);
    }

    private bool EnoughResources() {
        foreach (ResourceType type in currentProject.getCost().Keys) {
            if (AmountToGather(type) > 0 && !IsFull(type))
                return false;
        }
        return true;
    }

    private bool DoneBuilding() {
        Dictionary<ResourceType, int> cost = currentProject.getCost();
        foreach (ResourceType type in cost.Keys) {
            if (resourcesSpent[type] < cost[type])
                return false;
        }
        return true;
    }

    private float CurrentProgress() {
        Dictionary<ResourceType, int> cost = currentProject.getCost();
        int totalDemand = 0;
        int totalFulfillment = 0;
        foreach (ResourceType type in cost.Keys) {
            int demand = cost[type];
            totalDemand += demand;
            totalFulfillment += Mathf.Min(resourcesSpent[type], demand);
        }
        return (float) totalFulfillment / (float) totalDemand;
    }

    private float PercentInventoryFull() {
        Dictionary<ResourceType, int> cost = currentProject.getCost();
        int totalDemand = 0;
        int totalFulfillment = 0;
        foreach (ResourceType type in cost.Keys) {
            int demand = Mathf.Min(AmountToBuild(type), getResourceLimit(type));
            totalDemand += demand;
            totalFulfillment += Mathf.Min(getResource(type), demand);
        }
        return (float) totalFulfillment / (float) totalDemand;
    }

    public void SetProject(Building project) {
        currentProject = project;
        StartMove(currentProject.transform.position, currentProject.gameObject);
    }
}
