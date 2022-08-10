namespace Mapbox.Examples
{
    using UnityEngine;
    using UnityEngine.EventSystems;
    using Mapbox.Unity.Map;
    using Mapbox.Unity.Utilities;
    using Mapbox.Unity.MeshGeneration.Factories;

    public class CameraMovement : MonoBehaviour
    {
        [SerializeField]
        AbstractMap _map;
        [SerializeField] DirectionsFactory directionsFactory;

        [SerializeField]
        float _panSpeed = 20f;

        [SerializeField]
        float _zoomSpeed = 50f;

        [SerializeField]
        Camera _referenceCamera;

        Quaternion _originalRotation;
        Vector3 _origin;
        Vector3 _delta;
        bool _shouldDrag;

        void HandleTouch()
        {
            float zoomFactor = 0.0f;
            //pinch to zoom. 
            switch (Input.touchCount)
            {
                case 1:
                    {
                        HandleMouseAndKeyBoard();
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
                        zoomFactor = 0.05f * (touchDeltaMag - prevTouchDeltaMag);
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
                float zoomFactor = y > 0 ? 0.1f : -0.1f;

                if (_map.Zoom == 20 && zoomFactor >= 0 || _map.Zoom == 4 && zoomFactor <= 0)
                    zoomFactor = 0;

                float newZoom = (float)((decimal)_map.Zoom + (decimal)zoomFactor);
                _map.SetZoom(newZoom);
                _map.UpdateMap();

                if (newZoom % 1 == 0)
                    directionsFactory.UpdateMap();
            }
        }

        void HandleMouseAndKeyBoard()
        {
            if (Input.GetMouseButton(0) && !EventSystem.current.IsPointerOverGameObject())
            {
                var mousePosition = Input.mousePosition;
                mousePosition.z = _referenceCamera.transform.localPosition.y;
                _delta = _referenceCamera.ScreenToWorldPoint(mousePosition) - _referenceCamera.transform.localPosition;
                _delta.y = 0f;
                if (_shouldDrag == false)
                {
                    _shouldDrag = true;
                    _origin = _referenceCamera.ScreenToWorldPoint(mousePosition);
                }
            }
            else
            {
                _shouldDrag = false;
            }

            if (_shouldDrag == true)
            {
                var offset = _origin - _delta;
                offset.y = transform.localPosition.y;
                transform.localPosition = offset;
            }
            else
            {
                if (EventSystem.current.IsPointerOverGameObject())
                {
                    return;
                }

                var x = Input.GetAxis("Horizontal");
                var z = Input.GetAxis("Vertical");
                var y = Input.GetAxis("Mouse ScrollWheel");

                if (!(Mathf.Approximately(x, 0) && Mathf.Approximately(y, 0) && Mathf.Approximately(z, 0)))
                {
                    float zoomFactor = y > 0 ? 0.1f : -0.1f;

                    if (_map.Zoom == 20 && zoomFactor >= 0 || _map.Zoom == 4 && zoomFactor <= 0)
                        zoomFactor = 0;

                    float newZoom = (float)((decimal)_map.Zoom + (decimal)zoomFactor);
                    _map.SetZoom(newZoom);
                    _map.UpdateMap();

                    if (newZoom % 1 == 0)
                        directionsFactory.UpdateMap();
                }
            }
        }

        void Awake()
        {
            _originalRotation = Quaternion.Euler(0, transform.eulerAngles.y, 0);

            if (_referenceCamera == null)
            {
                _referenceCamera = GetComponent<Camera>();
                if (_referenceCamera == null)
                {
                    throw new System.Exception("You must have a reference camera assigned!");
                }
            }
        }

        void LateUpdate()
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
}