﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RTS;

public class UserInput : MonoBehaviour {

    private Player player;
    // Use this for initialization
    void Start() {
        player = transform.root.GetComponent<Player>();
    }

    // Update is called once per frame
    void Update() {
        if (player.human) {
            MoveCamera();
            RotateCamera();
            MouseActivity();
        }
    }

    private void MoveCamera() {
        float xpos = Input.mousePosition.x;
        float ypos = Input.mousePosition.y;
        Vector3 movement = new Vector3(0, 0, 0);

        //horizontal camera movement
        if (xpos >= 0 && xpos < ResourceManager.ScrollWidth) {
            movement.x -= ResourceManager.ScrollSpeed;
        }
        else if (xpos <= Screen.width && xpos > Screen.width - ResourceManager.ScrollWidth) {
            movement.x += ResourceManager.ScrollSpeed;
        }

        //vertical camera movement
        if (ypos >= 0 && ypos < ResourceManager.ScrollWidth) {
            movement.z -= ResourceManager.ScrollSpeed;
        }
        else if (ypos <= Screen.height && ypos > Screen.height - ResourceManager.ScrollWidth) {
            movement.z += ResourceManager.ScrollSpeed;
        }

        movement = Camera.main.transform.TransformDirection(movement);
        movement.y = 0;

        //away from ground movement
        movement.y -= ResourceManager.ScrollSpeed * Input.GetAxis("Mouse ScrollWheel");

        //calculate desired camera position based on received input
        Vector3 origin = Camera.main.transform.position;
        Vector3 destination = origin;
        destination.x += movement.x;
        destination.y += movement.y;
        destination.z += movement.z;

        //limit away from ground movement to be between a minimum and maximum distance
        if (destination.y > ResourceManager.MaxCameraHeight) {
            destination.y = ResourceManager.MaxCameraHeight;
        }
        else if (destination.y < ResourceManager.MinCameraHeight) {
            destination.y = ResourceManager.MinCameraHeight;
        }

        //if a change in position is detected perform the necessary update
        if (destination != origin) {
            Camera.main.transform.position = Vector3.MoveTowards(origin, destination, Time.deltaTime * ResourceManager.ScrollSpeed);
        }
    }

    private void RotateCamera() {
        Vector3 origin = Camera.main.transform.eulerAngles;
        Vector3 destination = origin;

        //detect rotation amount if ALT is being held and the Right mouse button is down
        if ((Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt)) && Input.GetMouseButton(1)) {
            destination.x -= Input.GetAxis("Mouse Y") * ResourceManager.RotateAmount;
            destination.y += Input.GetAxis("Mouse X") * ResourceManager.RotateAmount;
        }

        //if a change in position is detected perform the necessary update
        if (destination != origin) {
            Camera.main.transform.eulerAngles = Vector3.MoveTowards(origin, destination, Time.deltaTime * ResourceManager.RotateSpeed);
        }
    }

    private void MouseActivity() {
        if (Input.GetMouseButtonDown(0))
            LeftMouseClick();
        else if (Input.GetMouseButtonDown(1))
            RightMouseClick();
        MouseHover();
    }

    private void LeftMouseClick() {
        if (player.hud.MouseInBounds()) {
            if (player.IsFindingBuildingLocation()) {
                if (player.CanPlaceBuilding())
                    player.StartConstruction(player.hud.GetPlayingArea());
                return;
            }

            GameObject hitObject = WorkManager.FindHitObject(Input.mousePosition);
            Vector3 hitPoint = WorkManager.FindHitPoint(Input.mousePosition);
            if (hitObject && hitPoint != ResourceManager.InvalidPosition) {
                if (player.SelectedObject &&
                    player.SelectedObject.MouseClick(hitObject, hitPoint, player))
                    return;
                else if (hitObject.name != "Ground") {
                    WorldObject worldObject = hitObject.GetComponent<WorldObject>();
                    if (worldObject == null)
                        worldObject = hitObject.GetComponentInParent<WorldObject>();
                    if (worldObject) {
                        if (player.SelectedObject)
                            player.SelectedObject.SetSelection(false);
                        player.SelectedObject = worldObject;
                        worldObject.SetSelection(true, player.hud.GetPlayingArea());
                    }
                }
            }
        }
    }

    private void RightMouseClick() {
        if (player.hud.MouseInBounds() && !Input.GetKey(KeyCode.LeftAlt) && player.SelectedObject) {
            if (player.IsFindingBuildingLocation()) {
                player.CancelBuildingPlacement();
            }
            else {
                player.SelectedObject.SetSelection(false, player.hud.GetPlayingArea());
                player.SelectedObject = null;
            }
        }
    }

    private void MouseHover() {
        if (player.hud.MouseInBounds()) {
            if (player.IsFindingBuildingLocation()) {
                player.FindBuildingLocation();
            }
            else {
                GameObject hoverObject = WorkManager.FindHitObject(Input.mousePosition);
                if (hoverObject) {
                    if (player.SelectedObject)
                        player.SelectedObject.SetHoverState(hoverObject);
                    else if (hoverObject.name != "Ground") {
                        Player owner = hoverObject.transform.root.GetComponent<Player>();
                        if (owner) {
                            Unit unit = hoverObject.transform.parent.GetComponent<Unit>();
                            Building building = hoverObject.transform.parent.GetComponent<Building>();
                            /*if (owner.username == player.username && (unit || building))
                                player.hud.SetCursorState(CursorState.Select);*/
                        }
                    }
                }
            }
        }
    }
}