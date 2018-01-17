using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RTS;

public class Faction : MonoBehaviour {
    public int startWood, startWoodLimit, startIron, startIronLimit;
    private Dictionary<ResourceType, int> resources, resourceLimits;

    private Dictionary<WorldObject, WorldObject> claims;

    private void Awake() {
        resources = WorkManager.InitResourceList();
        resourceLimits = WorkManager.InitResourceList();
        AddStartResourceLimits();
        AddStartResources();

        claims = new Dictionary<WorldObject, WorldObject>();
    }

    private void AddStartResourceLimits() {
        IncrementResourceLimit(ResourceType.Wood, startWoodLimit);
        IncrementResourceLimit(ResourceType.Iron, startIronLimit);
    }

    private void AddStartResources() {
        AddResource(ResourceType.Wood, startWood);
        AddResource(ResourceType.Iron, startIron);
    }

    public void AddResource(ResourceType type, int amount) {
        resources[type] += amount;
    }

    public void IncrementResourceLimit(ResourceType type, int amount) {
        resourceLimits[type] += amount;
    }

    public GameObject AddUnit(string unitName, Vector3 spawnPoint, Quaternion rotation, Building creator) {
        Debug.Log("add " + unitName + " to " + this);
        Units units = GetComponentInChildren<Units>();
        GameObject newUnit = (GameObject) Instantiate(ResourceManager.GetUnit(unitName),
            spawnPoint, rotation);
        newUnit.transform.parent = units.transform;

        Unit unitObject = newUnit.GetComponent<Unit>();
        if (unitObject) {
            unitObject.SetCreator(creator);
        }

        return newUnit;
    }

    public GameObject AddBuilding(string buildingName, Vector3 spawnPoint, Quaternion rotation, Building creator) {
        Debug.Log("add " + buildingName + " to " + this);

        Buildings buildings = GetComponentInChildren<Buildings>();
        GameObject newBuilding = (GameObject) Instantiate(ResourceManager.GetBuilding(buildingName),
            spawnPoint, rotation);
        newBuilding.transform.parent = buildings.transform;

        Building buildingObject = newBuilding.GetComponent<Building>();
        return newBuilding;
    }

    public bool Claim(WorldObject claimee, WorldObject claimer) {
        if(claimee == null || claimer == null) {
            return false;
        }
        if (claims.ContainsKey(claimee)) {
            return false;
        }
        claims[claimee] = claimer;
        return true;
    }

    public bool UnClaim(WorldObject claimee, WorldObject claimer) {
        if (claimee == null || claimer == null) {
            return false;
        }

        if (claims.ContainsKey(claimee) && claims[claimee] == claimer) {
            claims.Remove(claimee);
            return true;
        }
        return false;
    }

    public bool isClaimed(WorldObject claimee) {
        return claims.ContainsKey(claimee);
    }

    public Dictionary<ResourceType, int> GetResources() {
        return resources;
    }

    public Dictionary<ResourceType, int> GetResourceLimits() {
        return resourceLimits;
    }
}
