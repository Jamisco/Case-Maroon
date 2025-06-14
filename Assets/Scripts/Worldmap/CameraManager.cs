using CaseMaroon.WorldMap;
using Cinemachine;
using GridMapMaker;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace CaseMaroon.Systems
{
    public class CameraManager : MonoBehaviour
    {
        // make camera zoom in/out to match map size.
        // camera should always maintain map bounds

        private Camera mainCamera;

        private Worldmap worldMap;
        private CinemachineVirtualCamera virtualCamera;
        private CinemachineConfiner2D cameraConfiner;
        
        public float dragSpeed = 2f;        // Speed of dragging

        [Range(0f, 10f)]
        public float scrollSpeed = 2;     // Speed of zooming
        public float minZoom = 5f;          // Minimum zoom level (for orthographic size)
        public float maxZoom = 20f;         // Maximum zoom level (for orthographic size)

        private Vector3 dragOrigin;

        private void Awake()
        {
            Init();
            worldMap.OnWorldGenerated += WorldMap_OnWorldGenerated;
        }

        private void WorldMap_OnWorldGenerated(Worldmap map)
        {
            SetCamSettings();
        }

        private void Start()
        {
            SetCamSettings();
        }

        private void Update()
        {
            HandleDrag();
            HandleZoom();
        }

        private void Init()
        {
            mainCamera = GetComponent<Camera>();
            worldMap = FindAnyObjectByType<Worldmap>();

            virtualCamera = GetComponent<CinemachineVirtualCamera>();

            if (virtualCamera == null)
            {
                Debug.LogError("Virtual Camera not Founds");
                return;
            }
            
            cameraConfiner = GetComponent<CinemachineConfiner2D>();

            if (virtualCamera == null)
            {
                Debug.LogError("Virtual Camera not Founds");
                return;
            }

        }

        private void SetCamSettings()
        {
            cameraConfiner.m_BoundingShape2D = worldMap.polygonCollider;
            cameraConfiner.InvalidateCache();

            // now we must create a polygon and box collider of gridmanager

            // bopx collider will be limit bounds
            // polygon collider will be the precise bounds of the map
        }

        void HandleDrag()
        {
            if (Input.GetMouseButtonDown(0)) // Left mouse button pressed
            {
                dragOrigin = Input.mousePosition;
                return;
            }

            Bounds mapBounds = worldMap.gridManager.LocalBounds;
            Bounds camBounds = GetCameraBounds();
            bool atEdge = IsAtConfinerEdge();

            // this if block is to prevent the virtual camera position from going pass the bounds of the confiner. 
            // If we dont have this, when dragging, even though the camera will confine to the bounds, the virtual camera will still be able to go pass the bounds.
            if (virtualCamera.transform.position != Camera.main.transform.position)
            {
                transform.position = Camera.main.transform.position;
            }

            if (Input.GetMouseButton(0)) // Holding the left mouse button
            {
                Vector3 difference = Camera.main.ScreenToWorldPoint(dragOrigin) - Camera.main.ScreenToWorldPoint(Input.mousePosition);
                difference.z = 0; // Keep the z-axis steady (for 2D)

                transform.position += difference; // Move the camera
                dragOrigin = Input.mousePosition; // Update drag origin
            }
        }

        // Method to handle zoom with scroll wheel
        void HandleZoom()
        {
            float scroll = Input.GetAxis("Mouse ScrollWheel"); // Get scroll input

            if (scroll != 0 )
            {
                Vector2 mousePos = Input.mousePosition; // Get mouse position

                Vector3 worldPoint = Camera.main.ScreenToWorldPoint(mousePos);

                if (!worldMap.WithinWorldBounds(worldPoint))
                {
                    return;
                }

                virtualCamera.m_Lens.OrthographicSize = Mathf.Clamp(Camera.main.orthographicSize - scroll * scrollSpeed, minZoom, maxZoom);

                // everything the zoom changes, the confiner has to be invalidated. Invalidation essentially calculates the bounds of the virtucal camera given the current ortho size
                cameraConfiner.InvalidateCache();
            }
        }

        Bounds GetCameraBounds()
        {
            Camera cam = Camera.main;

            float vertExtent = cam.orthographicSize;
            float horzExtent = vertExtent * cam.aspect;

            Vector3 camPos = cam.transform.position;

            return new Bounds(camPos, new Vector3(horzExtent * 2, vertExtent * 2, 0f));
        }

        private bool IsAtConfinerEdge()
        {
            if (cameraConfiner == null || virtualCamera == null) return false;

            // Get the camera's current position
            Vector3 cameraPosition = virtualCamera.transform.position;

            // Get the confiner's bounding shape
            Collider2D confinerBounds = cameraConfiner.m_BoundingShape2D;

            if (confinerBounds != null)
            {
                // Check if the camera is at the edge of the confiner
                Vector2 closestPoint = confinerBounds.ClosestPoint(cameraPosition);
                float distance = Vector2.Distance(cameraPosition, closestPoint);

                // If the distance is very small, the camera is at the edge
                return Mathf.Approximately(distance, 0f);
            }

            return false;
        }

    }
}
