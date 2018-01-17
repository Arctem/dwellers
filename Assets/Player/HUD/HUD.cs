using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RTS;

public class HUD : MonoBehaviour {

    public Texture2D[] resources;
    public Texture2D[] resourceHealthBars;
    public Texture2D buttonHover, buttonClick;
    private Dictionary<ResourceType, Texture2D> resourceImages;

    public GUISkin resourceSkin, ordersSkin;
    private const int ORDERS_BAR_WIDTH = 512, ORDERS_BAR_HEIGHT = 128, RESOURCE_BAR_WIDTH = 128;
    private const int ICON_WIDTH = 32, ICON_HEIGHT = 32, TEXT_WIDTH = 128, TEXT_HEIGHT = 32;
    private const int SELECTION_NAME_HEIGHT = 15;
    private const int BUILD_IMAGE_WIDTH = 64, BUILD_IMAGE_HEIGHT = 64;
    private const int ACTIONS_PER_ROW = ORDERS_BAR_WIDTH / BUILD_IMAGE_WIDTH;
    private const int BUTTON_SPACING = 7;
    private const int SCROLL_BAR_WIDTH = 22;

    public GUISkin selectBoxSkin;
    public Texture2D healthy, damaged, critical, construction;

    private int buildAreaHeight = 96;

    private Dictionary<ResourceType, int> redResourceValues, redResourceLimits, blueResourceValues, blueResourceLimits;

    private WorldObject lastSelection;
    private float sliderValue;

    private Player player;

    // Use this for initialization
    void Start() {
        //buildAreaHeight = Screen.height - RESOURCE_BAR_HEIGHT - SELECTION_NAME_HEIGHT - 2 * BUTTON_SPACING;

        redResourceValues = new Dictionary<ResourceType, int>();
        redResourceLimits = new Dictionary<ResourceType, int>();
        blueResourceValues = new Dictionary<ResourceType, int>();
        blueResourceLimits = new Dictionary<ResourceType, int>();

        player = transform.root.GetComponent<Player>();
        ResourceManager.StoreSelectBoxItems(selectBoxSkin, healthy, damaged, critical, construction);

        resourceImages = new Dictionary<ResourceType, Texture2D>();
        for (int i = 0; i < resources.Length; i++) {
            switch (resources[i].name) {
                case "wood":
                    resourceImages.Add(ResourceType.Wood, resources[i]);
                    redResourceValues.Add(ResourceType.Wood, 0);
                    redResourceLimits.Add(ResourceType.Wood, 0);
                    blueResourceValues.Add(ResourceType.Wood, 0);
                    blueResourceLimits.Add(ResourceType.Wood, 0);
                    break;
                case "iron":
                    resourceImages.Add(ResourceType.Iron, resources[i]);
                    redResourceValues.Add(ResourceType.Iron, 0);
                    redResourceLimits.Add(ResourceType.Iron, 0);
                    blueResourceValues.Add(ResourceType.Iron, 0);
                    blueResourceLimits.Add(ResourceType.Iron, 0);
                    break;
                default:
                    break;
            }
        }

        Dictionary<ResourceType, Texture2D> resourceHealthBarTextures = new Dictionary<ResourceType, Texture2D>();
        for (int i = 0; i < resourceHealthBars.Length; i++) {
            switch (resourceHealthBars[i].name) {
                case "wood":
                    resourceHealthBarTextures.Add(ResourceType.Wood, resourceHealthBars[i]);
                    break;
                case "iron":
                    resourceHealthBarTextures.Add(ResourceType.Iron, resourceHealthBars[i]);
                    break;
                default:
                    break;
            }
        }
        ResourceManager.SetResourceHealthBarTextures(resourceHealthBarTextures);
    }

    void OnGUI() {
        if (player && player.human) {
            DrawOrdersBar();
            DrawResourceBar(true, redResourceValues, redResourceLimits);
            DrawResourceBar(false, blueResourceValues, blueResourceLimits);
        }
    }

    private void DrawOrdersBar() {
        GUI.skin = ordersSkin;
        GUI.BeginGroup(new Rect(Screen.width / 2 - ORDERS_BAR_WIDTH / 2, Screen.height - ORDERS_BAR_HEIGHT, ORDERS_BAR_WIDTH, ORDERS_BAR_HEIGHT));
        GUI.Box(new Rect(0, 0, ORDERS_BAR_WIDTH, ORDERS_BAR_HEIGHT), "");
        string selectionName = "";
        if (player.SelectedObject) {
            selectionName = player.SelectedObject.objectName;
            if (player.SelectedObject.IsOwnedBy(player)) {
                //reset slider value if the selected object has changed
                if (lastSelection && lastSelection != player.SelectedObject)
                    sliderValue = 0.0f;
                DrawActions(player.SelectedObject.GetActions());
                //store the current selection
                lastSelection = player.SelectedObject;
            }
        }
        if (!selectionName.Equals("")) {
            int topPos = buildAreaHeight + BUTTON_SPACING;
            GUI.Label(new Rect(0, topPos, ORDERS_BAR_WIDTH, SELECTION_NAME_HEIGHT), selectionName);
        }
        GUI.EndGroup();
    }

    private void DrawActions(string[] actions) {
        GUIStyle buttons = new GUIStyle();
        buttons.hover.background = buttonHover;
        buttons.active.background = buttonClick;
        GUI.skin.button = buttons;
        int numActions = actions.Length;
        //define the area to draw the actions inside
        GUI.BeginGroup(new Rect(0, 0, ORDERS_BAR_WIDTH, buildAreaHeight));
        //draw scroll bar for the list of actions if need be
        if (numActions >= MaxNumRows(buildAreaHeight))
            DrawSlider(buildAreaHeight, numActions / (float) ACTIONS_PER_ROW);
        //display possible actions as buttons and handle the button click for each
        for (int i = 0; i < numActions; i++) {
            int column = i % ACTIONS_PER_ROW;
            int row = i / ACTIONS_PER_ROW;
            Rect pos = GetButtonPos(row, column);
            Texture2D action = ResourceManager.GetBuildImage(actions[i]);
            if (action) {
                //create the button and handle the click of that button
                if (GUI.Button(pos, action)) {
                    if (player.SelectedObject)
                        player.SelectedObject.PerformAction(actions[i]);
                }
            }
        }
        GUI.EndGroup();
    }

    private int MaxNumRows(int areaHeight) {
        return areaHeight / BUILD_IMAGE_HEIGHT;
    }

    private Rect GetButtonPos(int row, int column) {
        int left = SCROLL_BAR_WIDTH + column * BUILD_IMAGE_WIDTH;
        float top = row * BUILD_IMAGE_HEIGHT - sliderValue * BUILD_IMAGE_HEIGHT;
        return new Rect(left, top, BUILD_IMAGE_WIDTH, BUILD_IMAGE_HEIGHT);
    }

    private void DrawSlider(int groupHeight, float numRows) {
        //slider goes from 0 to the number of rows that do not fit on screen
        sliderValue = GUI.VerticalSlider(GetScrollPos(groupHeight), sliderValue, 0.0f, numRows - MaxNumRows(groupHeight));
    }

    private Rect GetScrollPos(int groupHeight) {
        return new Rect(BUTTON_SPACING, BUTTON_SPACING, SCROLL_BAR_WIDTH, groupHeight - 2 * BUTTON_SPACING);
    }

    private void DrawResourceBar(bool leftAligned, Dictionary<ResourceType, int> resourceValues,
        Dictionary<ResourceType, int> resourceLimits) {
        GUI.skin = resourceSkin;
        GUI.BeginGroup(new Rect(leftAligned ? 0 : Screen.width - RESOURCE_BAR_WIDTH, 0, RESOURCE_BAR_WIDTH, Screen.height));
        GUI.Box(new Rect(0, 0, RESOURCE_BAR_WIDTH, Screen.height), "");

        // Draw resource amounts
        int topPos = 4, iconLeft = 4, textLeft = 20;
        DrawResourceIcon(ResourceType.Wood, iconLeft, textLeft, topPos, resourceValues, resourceLimits);
        topPos += TEXT_HEIGHT;
        DrawResourceIcon(ResourceType.Iron, iconLeft, textLeft, topPos, resourceValues, resourceLimits);

        GUI.EndGroup();
    }

    private void DrawResourceIcon(ResourceType type, int iconLeft, int textLeft, int topPos, Dictionary<ResourceType, int> resourceValues,
        Dictionary<ResourceType, int> resourceLimits) {
        Texture2D icon = resourceImages[type];
        string text = resourceValues[type].ToString() + "/" + resourceLimits[type].ToString();
        GUI.DrawTexture(new Rect(iconLeft, topPos, ICON_WIDTH, ICON_HEIGHT), icon);
        GUI.Label(new Rect(textLeft, topPos, TEXT_WIDTH, TEXT_HEIGHT), text);
    }

    public bool MouseInBounds() {
        //Screen coordinates start in the lower-left corner of the screen
        //not the top-left of the screen like the drawing coordinates do
        Vector3 mousePos = Input.mousePosition;
        bool insideResources = mousePos.x <= RESOURCE_BAR_WIDTH || mousePos.x >= Screen.width - RESOURCE_BAR_WIDTH;
        bool insideOrders = mousePos.y < ORDERS_BAR_HEIGHT && Mathf.Abs(mousePos.x - (Screen.width / 2)) < ORDERS_BAR_WIDTH / 2;
        return !insideResources && !insideOrders;
    }

    public Rect GetPlayingArea() {
        return new Rect(RESOURCE_BAR_WIDTH, 0, Screen.width - 2 * RESOURCE_BAR_WIDTH, Screen.height - ORDERS_BAR_HEIGHT);
    }

    public void SetResourceValues(Dictionary<ResourceType, int> redResourceValues,
        Dictionary<ResourceType, int> redResourceLimits,
        Dictionary<ResourceType, int> blueResourceValues,
        Dictionary<ResourceType, int> blueResourceLimits) {
        this.redResourceValues = redResourceValues;
        this.redResourceLimits = redResourceLimits;
        this.blueResourceValues = blueResourceValues;
        this.blueResourceLimits = blueResourceLimits;
    }
}
