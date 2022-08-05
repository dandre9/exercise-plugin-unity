namespace Mapbox.Unity.MeshGeneration.Factories
{
    using UnityEngine;
    using Mapbox.Directions;
    using System.Collections.Generic;
    using System.Linq;
    using Mapbox.Unity.Map;
    using Data;
    using Modifiers;
    using Mapbox.Utils;
    using Mapbox.Unity.Utilities;
    using System.Collections;
    using UnityEngine.Rendering;

    public class DirectionsFactory : MonoBehaviour
    {
        [SerializeField]
        AbstractMap _map;

        [SerializeField]
        MeshModifier[] MeshModifiers;
        [SerializeField]
        Material _material;
        private List<double> _cachedWaypoints;

        [SerializeField]
        [Range(1, 10)]
        private float UpdateFrequency = 2;
        [SerializeField] int routeSize = 300;



        private Directions _directions;
        private int _counter;

        GameObject _directionsGO;
        private bool _recalculateNext;

        [SerializeField]
        double[,] coordsList = {
            {-19.95738130156992, -43.94111616290653},
            {-19.958316484184348, -43.9422968125221},
            {-19.957544388165246, -43.941809429254434},
            {-19.95572241763445, -43.94032987969168},
            {-19.95506871650044, -43.93988647313167},
            {-19.956298872173495, -43.939181781447864},
            {-19.94034828789822, -43.933101531767896}
            // {-16.748643829507063, -43.85449843520393},
            // {-19.92492475363458, -43.907891340074535},
            // {-19.93070855300031, -43.98324712904912},
            // {-19.930537221858607, -44.01786897465524},
            // {-19.93296901857714, -43.93769443316437},
            // {-19.931325391876737, -43.937289621176824},
            // {-19.93106704371101, -43.9382722850997},
            // {-19.932718929070365, -43.938802482464396},
            // {-19.93296901857714, -43.93769443316437},
            // {-19.931325391876737, -43.937289621176824},
            // {-19.93106704371101, -43.9382722850997},
            // {-19.932718929070365, -43.938802482464396},
            // {-19.93296901857714, -43.93769443316437},
            // {-19.931325391876737, -43.937289621176824},
            // {-19.93106704371101, -43.9382722850997},
            // {-19.932718929070365, -43.938802482464396},
            // {-19.93296901857714, -43.93769443316437},
            // {-19.931325391876737, -43.937289621176824},
            // {-19.93106704371101, -43.9382722850997},
            // {-19.932718929070365, -43.938802482464396},
            // {-19.93296901857714, -43.93769443316437},
            // {-19.931325391876737, -43.937289621176824},
            // {-19.93106704371101, -43.9382722850997},
            // {-19.932718929070365, -43.938802482464396},
            // {-19.93296901857714, -43.93769443316437},
            // {-19.931325391876737, -43.937289621176824},
            // {-19.93106704371101, -43.9382722850997},
            // {-19.932718929070365, -43.938802482464396},
            // {-19.93189951045794, -43.93800302294052}
        };

        protected virtual void Awake()
        {
            if (_map == null)
            {
                _map = FindObjectOfType<AbstractMap>();
            }
            _directions = MapboxAccess.Instance.Directions;
            _map.OnInitialized += Query;
            // _map.OnUpdated += Query;
        }

        public void Start()
        {
            foreach (var modifier in MeshModifiers)
            {
                modifier.Initialize();
            }

            // _map.SetCenterLatitudeLongitude(new Vector2d(-19.9494027, -43.9377527));
            // _map.UpdateMap();

            Query();
        }

        protected virtual void OnDestroy()
        {
            _map.OnInitialized -= Query;
            // _map.OnUpdated -= Query;
        }

        void Query()
        {
            var count = coordsList.Length / 2;
            var wp = new Vector2d[count];
            for (int i = 0; i < count; i++)
            {
                wp[i].x = coordsList[i, 0];
                wp[i].y = coordsList[i, 1];
                // Debug.Log($"mapScale({_map.WorldRelativeScale}): wp[{i}] = {wp[i]}");
            }
            var _directionResource = new DirectionResource(wp, RoutingProfile.Walking);
            _directionResource.Steps = false;
            _directions.Query(_directionResource, HandleDirectionsResponse);
        }

        void HandleDirectionsResponse(DirectionsResponse response)
        {
            if (response == null || null == response.Routes || response.Routes.Count < 1)
            {
                return;
            }

            var meshData = new MeshData();
            var dat = new List<Vector3>();
            foreach (var point in response.Routes[0].Geometry)
            {
                dat.Add(Conversions.GeoToWorldPosition(point.x, point.y, _map.CenterMercator, _map.WorldRelativeScale).ToVector3xz());
            }

            var feat = new VectorFeatureUnity();
            feat.Points.Add(dat);

            foreach (MeshModifier mod in MeshModifiers.Where(x => x.Active))
            {
                mod.Run(feat, meshData, _map.WorldRelativeScale);
            }

            CreateGameObject(meshData);
        }

        GameObject CreateGameObject(MeshData data)
        {
            if (_directionsGO != null)
            {
                _directionsGO.Destroy();
            }
            _directionsGO = new GameObject("direction waypoint " + " entity");
            var mesh = _directionsGO.AddComponent<MeshFilter>().mesh;
            mesh.subMeshCount = data.Triangles.Count;

            mesh.SetVertices(data.Vertices);
            _counter = data.Triangles.Count;
            for (int i = 0; i < _counter; i++)
            {
                var triangle = data.Triangles[i];
                mesh.SetTriangles(triangle, i);
            }

            _counter = data.UV.Count;
            for (int i = 0; i < _counter; i++)
            {
                var uv = data.UV[i];
                mesh.SetUVs(i, uv);
            }

            mesh.RecalculateNormals();
            _directionsGO.AddComponent<MeshRenderer>().material = _material;

            Transform meshCenter = transform;
            meshCenter.position = mesh.bounds.center;
            _map.SetCenterLatitudeLongitude(meshCenter.GetGeoPosition(_map.CenterMercator, _map.WorldRelativeScale));
            _map.UpdateMap();

            if (mesh.bounds.size.x >= routeSize || mesh.bounds.size.z >= routeSize)
            {
                int zoomToRemove = 0;
                float routeW = mesh.bounds.size.x, routeH = mesh.bounds.size.z;

                while (routeW >= routeSize || routeH >= routeSize)
                {
                    zoomToRemove++;
                    routeW /= 2;
                    routeH /= 2;
                }

                _map.SetZoom(_map.Zoom - zoomToRemove);
                _map.UpdateMap();
                Query();
            }

            _directionsGO.transform.SetParent(_map.transform);

            return _directionsGO;
        }

        public void UpdateMap()
        {
            Query();
        }
    }
}
