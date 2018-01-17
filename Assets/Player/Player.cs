using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RTS;

public class Player : MonoBehaviour {
    public string username;
    public bool human;
    public HUD hud;
    public Faction redFaction, blueFaction;

    public Material notAllowedMaterial, allowedMaterial;
    private Building tmpBuilding;
    private string buildingName;
    private Building tmpCreator;
    private bool findingPlacement = false;

    public WorldObject SelectedObject { get; set; }

    void Start() {
        hud = GetComponentInChildren<HUD>();
    }

    void Update() {
        if (human) {
            hud.SetResourceValues(redFaction.GetResources(), redFaction.GetResourceLimits(),
                blueFaction.GetResources(), blueFaction.GetResourceLimits());
        }
        if (findingPlacement) {
            tmpBuilding.CalculateBounds();
            if (CanPlaceBuilding())
                tmpBuilding.SetTransparentMaterial(allowedMaterial, false);
            else
                tmpBuilding.SetTransparentMaterial(notAllowedMaterial, false);
        }
    }

    public void CreateBuilding(string buildingName, Vector3 buildPoint, Building creator, Rect playingArea) {
        ResetConstruction();

        this.buildingName = buildingName;
        GameObject newBuilding = (GameObject) Instantiate(ResourceManager.GetBuilding(buildingName), buildPoint, new Quaternion());
        tmpBuilding = newBuilding.GetComponent<Building>();
        if (tmpBuilding) {
            tmpCreator = creator;
            findingPlacement = true;
            tmpBuilding.SetTransparentMaterial(notAllowedMaterial, true);
            tmpBuilding.gameObject.AddComponent<Rigidbody>();
            tmpBuilding.gameObject.GetComponent<Rigidbody>().isKinematic = true;
            tmpBuilding.gameObject.GetComponent<Collider>().isTrigger = true;
            tmpBuilding.SetPlayingArea(playingArea);
        }
        else
            Destroy(newBuilding);
    }

    public void StartConstruction(Rect playingArea) {
        Building newBuilding = tmpCreator.GetFaction().AddBuilding(buildingName,
            tmpBuilding.transform.position,
            tmpBuilding.transform.rotation,
            tmpCreator).GetComponent<Building>();

        newBuilding.SetPlayingArea(playingArea);
        newBuilding.StartConstruction();
        tmpCreator.DispatchBuilder(newBuilding);

        ResetConstruction();
    }

    private void ResetConstruction() {
        if (tmpBuilding)
            Destroy(tmpBuilding.gameObject);
        findingPlacement = false;
        tmpBuilding = tmpCreator = null;
        buildingName = "";
    }

    public void CancelBuildingPlacement() {
        findingPlacement = false;
        Destroy(tmpBuilding.gameObject);
        tmpBuilding = null;
        tmpCreator = null;
    }

    public bool IsFindingBuildingLocation() {
        return findingPlacement;
    }

    public void FindBuildingLocation() {
        Vector3 newLocation = WorkManager.FindHitPoint(Input.mousePosition, LayerMask.GetMask("Terrain"));
        newLocation.y = 0;
        tmpBuilding.transform.position = newLocation;
    }

    public bool CanPlaceBuilding() {
        return !tmpBuilding.isColliding();
    }
}
