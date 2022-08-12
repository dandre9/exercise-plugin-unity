namespace Mapbox.Examples
{
    using UnityEngine;
    using UnityEngine.EventSystems;
    using Mapbox.Unity.Map;
    using Mapbox.Unity.Utilities;
    using Mapbox.Unity.MeshGeneration.Factories;
    using System;
    using Mapbox.Utils;

    public class CameraMovement : MonoBehaviour
    {
        [SerializeField] AbstractMap _mapManager;
        [SerializeField] DirectionsFactory directionsFactory;
        [SerializeField] float _panSpeed = 20f;
        [SerializeField] float _zoomSpeed = 50f;
        [SerializeField] Camera _referenceCamera;

        Quaternion _originalRotation;
        Vector3 _origin;
        Vector3 _delta;
        bool _shouldDrag, moved;
        private Vector3 _mousePosition;
        private Vector3 _mousePositionPrevious;
        private bool _isInitialized = false;
        private Plane _groundPlane = new Plane(Vector3.up, 0);
        private bool _dragStartedOnUI = false;

        void Awake()
        {
            if (null == _referenceCamera)
            {
                _referenceCamera = GetComponent<Camera>();
                if (null == _referenceCamera) { Debug.LogErrorFormat("{0}: reference camera not set", this.GetType().Name); }
            }
            _mapManager.OnInitialized += () =>
            {
                _isInitialized = true;
            };
        }

        public void Update()
        {
            if (Input.GetMouseButtonDown(0) && EventSystem.current.IsPointerOverGameObject())
            {
                _dragStartedOnUI = true;
            }

            if (Input.GetMouseButtonUp(0))
            {
                _dragStartedOnUI = false;
            }
        }

        private void LateUpdate()
        {
            if (!_isInitialized) { return; }

            if (!_dragStartedOnUI)
            {
                if (Input.touchSupported && Input.touchCount > 0)
                {
                    HandleTouch();
                }
                else
                {
                    HandleMouseAndKeyBoard();
                }
            }
        }

        void HandleMouseAndKeyBoard()
        {
            // zoom
            float scrollDelta = 0.0f;
            scrollDelta = Input.GetAxis("Mouse ScrollWheel");
            ZoomMapUsingTouchOrMouse(scrollDelta);

            //pan keyboard
            float xMove = Input.GetAxis("Horizontal");
            float zMove = Input.GetAxis("Vertical");

            PanMapUsingKeyBoard(xMove, zMove);

            //pan mouse
            PanMapUsingTouchOrMouse();
        }

        void HandleTouch()
        {
            float zoomFactor = 0.0f;
            //pinch to zoom.
            switch (Input.touchCount)
            {
                case 1:
                    {
                        PanMapUsingTouchOrMouse();
                    }
                    break;
                case 2:
                    {
                        // Store both touches.
                        Touch touchZero = Input.GetTouch(0);
                        Touch touchOne = Input.GetTouch(1);

                        // Find the position in the previous frame of each touch.
                        Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
                        Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;

                        // Find the magnitude of the vector (the distance) between the touches in each frame.
                        float prevTouchDeltaMag = (touchZeroPrevPos - touchOnePrevPos).magnitude;
                        float touchDeltaMag = (touchZero.position - touchOne.position).magnitude;

                        // Find the difference in the distances between each frame.
                        zoomFactor = 0.01f * (touchDeltaMag - prevTouchDeltaMag);
                    }
                    ZoomMapUsingTouchOrMouse(zoomFactor);
                    break;
                default:
                    break;
            }
        }

        void ZoomMapUsingTouchOrMouse(float y)
        {
            if (Mathf.Abs(y) > .1f)
            {
                float zoomFactor = y > 0 ? 0.2f : -0.2f;

                if (_mapManager.Zoom == 20 && zoomFactor >= 0 || _mapManager.Zoom == 4 && zoomFactor <= 0)
                    zoomFactor = 0;

                if (moved)
                {
                    moved = false;
                    _mapManager.SetZoom(Convert.ToInt32(_mapManager.Zoom));
                    directionsFactory.UpdateMap();
                }

                float newZoom = (float)((decimal)_mapManager.Zoom + (decimal)zoomFactor);
                _mapManager.SetZoom(newZoom);
                _mapManager.UpdateMap();

                if (newZoom % 1 == 0)
                    directionsFactory.UpdateMap();
            }
        }

        void PanMapUsingKeyBoard(float xMove, float zMove)
        {
            if (Math.Abs(xMove) > 0.0f || Math.Abs(zMove) > 0.0f)
            {
                // Get the number of degrees in a tile at the current zoom level.
                // Divide it by the tile width in pixels ( 256 in our case)
                // to get degrees represented by each pixel.
                // Keyboard offset is in pixels, therefore multiply the factor with the offset to move the center.
                float factor = _panSpeed * (Conversions.GetTileScaleInDegrees((float)_mapManager.CenterLatitudeLongitude.x, _mapManager.AbsoluteZoom));

                var latitudeLongitude = new Vector2d(_mapManager.CenterLatitudeLongitude.x + zMove * factor * 2.0f, _mapManager.CenterLatitudeLongitude.y + xMove * factor * 4.0f);

                _mapManager.UpdateMap(latitudeLongitude, _mapManager.Zoom);
            }
        }

        void PanMapUsingTouchOrMouse()
        {
            UseMeterConversion();
        }

        void UseMeterConversion()
        {
            if (Input.GetMouseButtonUp(1))
            {
                var mousePosScreen = Input.mousePosition;
                //assign distance of camera to ground plane to z, otherwise ScreenToWorldPoint() will always return the position of the camera
                //http://answers.unity3d.com/answers/599100/view.html
                mousePosScreen.z = _referenceCamera.transform.localPosition.y;
                var pos = _referenceCamera.ScreenToWorldPoint(mousePosScreen);

                var latlongDelta = _mapManager.WorldToGeoPosition(pos);
                Debug.Log("Latitude: " + latlongDelta.x + " Longitude: " + latlongDelta.y);
            }

            if (Input.GetMouseButton(0) && !EventSystem.current.IsPointerOverGameObject())
            {
                var mousePosScreen = Input.mousePosition;
                //assign distance of camera to ground plane to z, otherwise ScreenToWorldPoint() will always return the position of the camera
                //http://answers.unity3d.com/answers/599100/view.html
                mousePosScreen.z = _referenceCamera.transform.localPosition.y;
                _mousePosition = _referenceCamera.ScreenToWorldPoint(mousePosScreen);

                if (_shouldDrag == false)
                {
                    // moved = true;
                    _shouldDrag = true;
                    _origin = _referenceCamera.ScreenToWorldPoint(mousePosScreen);
                }
            }
            else
            {
                _shouldDrag = false;
            }

            if (_shouldDrag == true)
            {
                if (!moved)
                {
                    _mapManager.SetZoom(Convert.ToInt32(_mapManager.Zoom));
                    _mapManager.UpdateMap();
                    moved = true;
                }

                var changeFromPreviousPosition = _mousePositionPrevious - _mousePosition;
                if (Mathf.Abs(changeFromPreviousPosition.x) > 0.0f || Mathf.Abs(changeFromPreviousPosition.y) > 0.0f)
                {
                    _mousePositionPrevious = _mousePosition;
                    var offset = _origin - _mousePosition;

                    if (Mathf.Abs(offset.x) > 0.0f || Mathf.Abs(offset.z) > 0.0f)
                    {
                        if (null != _mapManager)
                        {
                            float factor = _panSpeed * Conversions.GetTileScaleInMeters((float)0, _mapManager.AbsoluteZoom) / _mapManager.UnityTileSize;
                            var latlongDelta = Conversions.MetersToLatLon(new Vector2d(offset.x * factor, offset.z * factor));
                            var newLatLong = _mapManager.CenterLatitudeLongitude + latlongDelta;

                            _mapManager.UpdateMap(newLatLong, _mapManager.Zoom);
                            directionsFactory.UpdateMap();
                        }
                    }
                    _origin = _mousePosition;
                }
                else
                {
                    if (EventSystem.current.IsPointerOverGameObject())
                    {
                        return;
                    }
                    _mousePositionPrevious = _mousePosition;
                    _origin = _mousePosition;
                }
            }
        }
    }
}