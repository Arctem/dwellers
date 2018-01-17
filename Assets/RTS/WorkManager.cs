using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RTS {
    public static class WorkManager {
        public static Dictionary<ResourceType, int> InitResourceList() {
            Dictionary<ResourceType, int> list = new Dictionary<ResourceType, int>();
            list.Add(ResourceType.Wood, 0);
            list.Add(ResourceType.Iron, 0);
            return list;
        }

        public static Rect CalculateSelectionBox(Bounds selectionBounds, Rect playingArea) {
            //shorthand for the coordinates of the centre of the selection bounds
            float cx = selectionBounds.center.x;
            float cy = selectionBounds.center.y;
            float cz = selectionBounds.center.z;
            //shorthand for the coordinates of the extents of the selection bounds
            float ex = selectionBounds.extents.x;
            float ey = selectionBounds.extents.y;
            float ez = selectionBounds.extents.z;

            //Determine the screen coordinates for the corners of the selection bounds
            List<Vector3> corners = new List<Vector3>();
            corners.Add(Camera.main.WorldToScreenPoint(new Vector3(cx + ex, cy + ey, cz + ez)));
            corners.Add(Camera.main.WorldToScreenPoint(new Vector3(cx + ex, cy + ey, cz - ez)));
            corners.Add(Camera.main.WorldToScreenPoint(new Vector3(cx + ex, cy - ey, cz + ez)));
            corners.Add(Camera.main.WorldToScreenPoint(new Vector3(cx - ex, cy + ey, cz + ez)));
            corners.Add(Camera.main.WorldToScreenPoint(new Vector3(cx + ex, cy - ey, cz - ez)));
            corners.Add(Camera.main.WorldToScreenPoint(new Vector3(cx - ex, cy - ey, cz + ez)));
            corners.Add(Camera.main.WorldToScreenPoint(new Vector3(cx - ex, cy + ey, cz - ez)));
            corners.Add(Camera.main.WorldToScreenPoint(new Vector3(cx - ex, cy - ey, cz - ez)));

            //Determine the bounds on screen for the selection bounds
            Bounds screenBounds = new Bounds(corners[0], Vector3.zero);
            for (int i = 1; i < corners.Count; i++) {
                screenBounds.Encapsulate(corners[i]);
            }

            //Screen coordinates start in the bottom left corner, rather than the top left corner
            //this correction is needed to make sure the selection box is drawn in the correct place
            float selectBoxTop = playingArea.height - (screenBounds.center.y + screenBounds.extents.y) + (Screen.height - playingArea.yMax);
            float selectBoxLeft = screenBounds.center.x - screenBounds.extents.x - playingArea.xMin;
            float selectBoxWidth = 2 * screenBounds.extents.x;
            float selectBoxHeight = 2 * screenBounds.extents.y;

            return new Rect(selectBoxLeft, selectBoxTop, selectBoxWidth, selectBoxHeight);
        }

        public static WorldObject FindNearestWorldObjectInListToPosition(List<WorldObject> objects, Vector3 position) {
            if (objects == null || objects.Count == 0)
                return null;
            WorldObject nearestObject = objects[0];
            float distanceToNearestObject = Vector3.Distance(position, nearestObject.transform.position);
            for (int i = 1; i < objects.Count; i++) {
                float distanceToObject = Vector3.Distance(position, objects[i].transform.position);
                if (distanceToObject < distanceToNearestObject) {
                    distanceToNearestObject = distanceToObject;
                    nearestObject = objects[i];
                }
            }
            return nearestObject;
        }

        public static Vector3 FindHitPoint(Vector3 origin) {
            Ray ray = Camera.main.ScreenPointToRay(origin);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
                return hit.point;
            return ResourceManager.InvalidPosition;
        }

        public static Vector3 FindHitPoint(Vector3 origin, LayerMask layerMask) {
            Ray ray = Camera.main.ScreenPointToRay(origin);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask.value)) {
                return hit.point;
            }
            return ResourceManager.InvalidPosition;
        }

        public static GameObject FindHitObject(Vector3 origin) {
            Ray ray = Camera.main.ScreenPointToRay(origin);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
                return hit.collider.gameObject;
            return null;
        }
    }
}