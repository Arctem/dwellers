using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RTS;

public class Building : WorldObject {

    public string unitType;
    public string constructionUnitType;
    public float spawnTime;
    public int spawnLimit;
    private int currentUnits;
    private float currentSpawnProgress = 0.0f;
    private bool constructed = true;
    protected Vector3 spawnPoint;

    private float constructionProgress = 1.0f;
    protected GUIStyle buildingStyle = new GUIStyle();

    protected override void Awake() {
        base.Awake();
        float spawnX = selectionBounds.center.x + transform.forward.x * selectionBounds.extents.x + transform.forward.x * 10;
        float spawnZ = selectionBounds.center.z + transform.forward.z + selectionBounds.extents.z + transform.forward.z * 10;
        spawnPoint = new Vector3(spawnX, 0.0f, spawnZ);
    }

    protected override void Start() {
        base.Start();
        faction = transform.parent.parent.GetComponent<Faction>();
    }

    protected override void Update() {
        base.Update();
        if (constructed)
            ProcessBuildQueue();
    }

    protected override void OnGUI() {
        base.OnGUI();
        if (!constructed) {
            DrawBuildProgress();
        }
    }

    private void DrawBuildProgress() {
        GUI.skin = ResourceManager.SelectBoxSkin;
        Rect selectBox = WorkManager.CalculateSelectionBox(selectionBounds, playingArea);
        //Draw the selection box around the currently selected object, within the bounds of the main draw area
        GUI.BeginGroup(playingArea);
        DrawBuildingBar(selectBox, "Building ...");
        GUI.EndGroup();
    }

    protected void DrawBuildingBar(Rect selectBox, string label) {
        buildingStyle.padding.top = -30;
        buildingStyle.normal.background = ResourceManager.ConstructionTexture;
        GUI.Label(new Rect(selectBox.x, selectBox.y - 7, selectBox.width * constructionProgress, 5), label, buildingStyle);
    }

    protected void ProcessBuildQueue() {
        if (currentUnits < spawnLimit) {
            currentSpawnProgress += Time.deltaTime * ResourceManager.BuildSpeed;
            if (currentSpawnProgress > spawnTime) {
                if (faction) {
                    faction.AddUnit(unitType, spawnPoint, transform.rotation, this);
                    currentUnits++;
                }
                currentSpawnProgress -= spawnTime;
            }
        }
    }

    public void DispatchBuilder(Building newBuilding) {
        Builder builder = faction.AddUnit(constructionUnitType,
            spawnPoint, transform.rotation, this)
            .GetComponent<Builder>();
        builder.SetProject(newBuilding);
    }

    public void StartConstruction() {
        constructionProgress = 0.0f;
        constructed = false;
    }

    public void SetConstructionProgress(float progress) {
        constructionProgress = progress;
    }

    public void FinishConstruction() {
        constructionProgress = 1.0f;
        constructed = true;
    }

    public float getSpawnPercentage() {
        return currentSpawnProgress / spawnTime;
    }
}
