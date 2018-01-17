using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RTS;

public class WorldObject : MonoBehaviour {

    public string objectName;
    public Texture2D buildImage;
    public int costWood, costIron, hitPoints, maxHitPoints;

    protected Player player;
    protected Faction faction;
    protected string[] actions = { };
    protected bool currentlySelected = false;

    protected Bounds selectionBounds;
    protected Rect playingArea = new Rect(0.0f, 0.0f, 0.0f, 0.0f);

    public int startWoodLimit, startIronLimit;
    private Dictionary<ResourceType, int> resources, resourceLimits, resourceCosts;

    protected GUIStyle healthStyle = new GUIStyle();
    protected float healthPercentage = 1.0f;

    private List<Material> oldMaterials = new List<Material>();

    private HashSet<WorldObject> collidingWith = new HashSet<WorldObject>();

    protected virtual void Awake() {
        selectionBounds = ResourceManager.InvalidBounds;
        CalculateBounds();

        resources = WorkManager.InitResourceList();
        resourceLimits = WorkManager.InitResourceList();
        resourceCosts = WorkManager.InitResourceList();
        AddStartResourceLimits();
        SetResourceCosts();
    }

    private void AddStartResourceLimits() {
        IncrementResourceLimit(ResourceType.Wood, startWoodLimit);
        IncrementResourceLimit(ResourceType.Iron, startIronLimit);
    }

    private void SetResourceCosts() {
        resourceCosts[ResourceType.Wood] = costWood;
        resourceCosts[ResourceType.Iron] = costIron;
    }

    public void IncrementResourceLimit(ResourceType type, int amount) {
        resourceLimits[type] += amount;
    }

    protected void AddResource(ResourceType type, int amount) {
        resources[type] += amount;
    }

    protected void RemoveResource(ResourceType type, int amount) {
        resources[type] -= amount;
    }

    public virtual int Place(ResourceType type, int amount) {
        if (resources[type] + amount > resourceLimits[type]) {
            amount = resourceLimits[type] - resources[type];
        }
        resources[type] += amount;
        return amount;
    }

    public virtual int Harvest(ResourceType type, int amount) {
        if (amount > resources[type]) {
            amount = resources[type];
        }
        int preharvest = resources[type];
        resources[type] -= amount;
        return amount;
    }

    public virtual bool IsEmpty() {
        foreach(int value in resources.Values) {
            if (value > 0)
                return false;
        }
        return true;
    }

    public virtual bool IsEmpty(ResourceType type) {
        return resources[type] <= 0;
    }

    public virtual bool IsFull() {
        foreach (ResourceType type in resources.Keys) {
            if (resources[type] < resourceLimits[type])
                return false;
        }
        return true;
    }

    public virtual bool IsFull(ResourceType type) {
        return resources[type] >= resourceLimits[type];
    }

    public virtual bool CanHold(ResourceType type, int amount) {
        return resources[type] + amount > resourceLimits[type];
    }

    public virtual int SpaceRemaining(ResourceType type) {
        return resourceLimits[type] - resources[type];
    }

    public virtual int getResource(ResourceType type) {
        return resources[type];
    }

    public int getResourceLimit(ResourceType type) {
        return resourceLimits[type];
    }

    public Dictionary<ResourceType, int> getCost() {
        return resourceCosts;
    }

    protected virtual void Start() {
        player = transform.root.GetComponentInChildren<Player>();
    }

    protected virtual void Update() {

    }

    protected virtual void OnGUI() {
        if (currentlySelected)
            DrawSelection();
    }

    private void DrawSelection() {
        GUI.skin = ResourceManager.SelectBoxSkin;
        Rect selectBox = WorkManager.CalculateSelectionBox(selectionBounds, playingArea);
        //Draw the selection box around the currently selected object, within the bounds of the playing area
        GUI.BeginGroup(playingArea);
        DrawSelectionBox(selectBox);
        GUI.EndGroup();
    }

    protected virtual void DrawSelectionBox(Rect selectBox) {
        GUI.Box(selectBox, "");
        CalculateCurrentHealth();
        if (maxHitPoints > 0)
            GUI.Label(new Rect(selectBox.x, selectBox.y - 7, selectBox.width * healthPercentage, 5), "", healthStyle);
    }


    public virtual void PerformAction(string actionToPerform) {
        //it is up to children with specific actions to determine what to do with each of those actions
    }

    public void SetSelection(bool selected) {
        currentlySelected = selected;
    }

    public void SetSelection(bool selected, Rect playingArea) {
        currentlySelected = selected;
        if (selected)
            this.playingArea = playingArea;
    }

    public string[] GetActions() {
        return actions;
    }

    public virtual bool MouseClick(GameObject hitObject, Vector3 hitPoint, Player controller) {
        //let this be overriden by any children
        return false;
    }

    private void ChangeSelection(WorldObject worldObject, Player controller) {
        //this should be called by the following line, but there is an outside chance it will not
        SetSelection(false, playingArea);
        if (controller.SelectedObject)
            controller.SelectedObject.SetSelection(false, playingArea);
        controller.SelectedObject = worldObject;
        worldObject.SetSelection(true, controller.hud.GetPlayingArea());
    }

    public void CalculateBounds() {
        selectionBounds = new Bounds(transform.position, Vector3.zero);
        foreach (Renderer r in GetComponentsInChildren<Renderer>()) {
            selectionBounds.Encapsulate(r.bounds);
        }
    }

    public Bounds GetSelectionBounds() {
        return selectionBounds;
    }

    public virtual void SetHoverState(GameObject hoverObject) {
        //only handle input if owned by a human player and currently selected
        if (player && player.human && currentlySelected) {
            /*if (hoverObject.name != "Ground")
                player.hud.SetCursorState(CursorState.Select);*/
        }
    }

    public bool IsOwnedBy(Player owner) {
        return player && player.Equals(owner);
    }

    public bool IsMemberOf(Faction faction) {
        return this.faction && this.faction.Equals(faction);
    }

    public Faction GetFaction() {
        return this.faction;
    }

    public float GetFullness() {
        int totalLimit = 0;
        foreach (int limit in resourceLimits.Keys) {
            totalLimit += limit;
        }
        int totalStored = 0;
        foreach (int stored in resources.Values) {
            totalStored += stored;
        }
        return (float) totalStored / (float) totalLimit;
    }

    protected virtual void CalculateCurrentHealth() {
        healthPercentage = (float) hitPoints / (float) maxHitPoints;
        if (healthPercentage > 0.65f)
            healthStyle.normal.background = ResourceManager.HealthyTexture;
        else if (healthPercentage > 0.35f)
            healthStyle.normal.background = ResourceManager.DamagedTexture;
        else
            healthStyle.normal.background = ResourceManager.CriticalTexture;
    }

    public void SetColliders(bool enabled) {
        Collider[] colliders = GetComponentsInChildren<Collider>();
        foreach (Collider collider in colliders)
            collider.enabled = enabled;
    }

    public void SetTransparentMaterial(Material material, bool storeExistingMaterial) {
        if (storeExistingMaterial)
            oldMaterials.Clear();
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers) {
            if (storeExistingMaterial)
                oldMaterials.Add(renderer.material);
            renderer.material = material;
        }
    }

    public void RestoreMaterials() {
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        if (oldMaterials.Count == renderers.Length) {
            for (int i = 0; i < renderers.Length; i++) {
                renderers[i].material = oldMaterials[i];
            }
        }
    }

    public void SetPlayingArea(Rect playingArea) {
        this.playingArea = playingArea;
    }

    //Handling Collisions
    protected void OnTriggerEnter(Collider other) {
        collidingWith.Add(other.gameObject.GetComponent<WorldObject>());
    }

    protected void OnTriggerExit(Collider other) {
        collidingWith.Remove(other.gameObject.GetComponent<WorldObject>());
    }

    public bool isColliding() {
        return collidingWith.Count > 0;
    }
}
