using UnityEngine;
using CaseMaroon.WorldMap;
using Cinemachine;
using System;

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
        }

        private void Start()
        {
            Worldmap.Instance.OnWorldGenerated += OnWorldGenerated;
        }

        private void OnWorldGenerated(Worldmap map)
        {
            worldMap = map;
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
                Debug.LogError("Virtual Camera not Found");
                return;
            }
            
            cameraConfiner = GetComponent<CinemachineConfiner2D>();

            if (virtualCamera == null)
            {
                Debug.LogError("Virtual Camera not Found");
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

        public bool withinMap = false;
        void HandleDrag()
        {
            if (Input.GetMouseButtonDown(0)) // Left mouse button pressed
            {
                dragOrigin = Input.mousePosition;
                return;
            }

            Bounds mapBounds = worldMap.gridManager.LocalBounds;
            Bounds camBounds = GetCameraBounds();
            withinMap = WithinMapBounds();

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

                float currentZoom = virtualCamera.m_Lens.OrthographicSize;

                maxZoom = CalculateMaxOrthoSize();

                float newZoom = Mathf.Clamp(currentZoom - scroll * scrollSpeed, minZoom, maxZoom);

                virtualCamera.m_Lens.OrthographicSize = newZoom;

                // cache has to be invalidate whenever zoom is changed. This is because, confiner works my limiting the box in which camera can move.
                // as this zoom changes, this box, also changes, and when you invalidateCache, you essentially recalculate said box.
                // if cache is not invalidated, you will be unable to drag the mouse around the map accurately.
                cameraConfiner.InvalidateCache();
            }
        }

        float CalculateMaxOrthoSize()
        {
            Bounds worldBounds = worldMap.gridManager.LocalBounds;
            float aspect = virtualCamera.m_Lens.Aspect;

            float maxHeight = worldBounds.extents.y;
            float maxWidth = worldBounds.extents.x / aspect;
            return Mathf.Min(maxHeight, maxWidth);
        }

        private bool WithinMapBounds()
        {
            Bounds mapBounds = worldMap.gridManager.LocalBounds;

            Bounds camBounds = GetCameraBounds();

            // Check if the camera bounds are fully inside the confiner bounds
            if (mapBounds.Contains(camBounds.min) && mapBounds.Contains(camBounds.max))
            {
                return true; // Camera is touching or outside the confiner edge
            }

            return false;
        }

        Bounds GetCameraBounds()
        {
            float height = 2f * virtualCamera.m_Lens.OrthographicSize;
            float width = height * virtualCamera.m_Lens.Aspect;

            Vector3 center = virtualCamera.transform.position;
            center.z = 0f; // Force Z position to 0

            return new Bounds(center, new Vector3(width, height, 0));
        }

    }
}
