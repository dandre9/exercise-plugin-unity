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
    using System;

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
        DirectionsResponse directionsResponse;
        private int _counter;
        GameObject _directionsGO;
        private bool _recalculateNext, adjustZoom;
        const int MAX_COORDS_NUMBER = 25;
        Mesh mesh;

        double[,] coordsList;

        protected virtual void Awake()
        {
            if (_map == null)
            {
                _map = FindObjectOfType<AbstractMap>();
            }
            _directions = MapboxAccess.Instance.Directions;
        }

        public void Start()
        {
            foreach (var modifier in MeshModifiers)
            {
                modifier.Initialize();
            }

            // Query();
        }

        protected virtual void OnDestroy()
        {
            // _map.OnInitialized -= Query;
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

            directionsResponse = response;

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
            mesh = _directionsGO.AddComponent<MeshFilter>().mesh;
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

            _directionsGO.transform.SetParent(_map.transform);

            if (adjustZoom)
            {
                adjustZoom = false;
                AdjustMapZoom();
            }


            return _directionsGO;
        }

        void AdjustMapZoom()
        {
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
                HandleDirectionsResponse(directionsResponse);
            }
        }

        public void UpdateMap()
        {
            HandleDirectionsResponse(directionsResponse);
        }

        public void DrawRoute()
        {
            adjustZoom = true;
            _map.SetZoom(22f);
            _map.UpdateMap();
            coordsList = ExerciseService.GetRouteCoords();
            int indexToCompare = 0, indexOffset = 1;
            List<int> indexes = new List<int>();

            for (int i = 0; i < coordsList.Length / 3; i++)
            {
                if (i == 0)
                    continue;

                double nDistance = GetDistance(coordsList[i - 1, 0], coordsList[i, 0],
                    coordsList[i - 1, 1], coordsList[i, 1],
                    coordsList[i - 1, 2], coordsList[i, 2]);

                if (nDistance <= 100)
                {
                    double distance = GetDistance(coordsList[indexToCompare, 0], coordsList[i, 0],
                    coordsList[indexToCompare, 1], coordsList[i, 1],
                    coordsList[indexToCompare, 2], coordsList[i, 2]);

                    if (distance >= 20)
                    {
                        indexes.Add(i);
                        indexToCompare = i;
                    }
                }
            }

            if (indexes.Count > MAX_COORDS_NUMBER)
            {
                indexOffset = (int)Math.Ceiling((double)indexes.Count / MAX_COORDS_NUMBER);
            }

            double[,] newCoords = new double[indexes.Count / indexOffset, 2];

            for (int j = 0; j < indexes.Count / indexOffset; j++)
            {
                if (j == (indexes.Count / indexOffset) - 1)
                {
                    newCoords[j, 0] = coordsList[indexes[indexes.Count - 1], 0];
                    newCoords[j, 1] = coordsList[indexes[indexes.Count - 1], 1];
                }
                else
                {
                    newCoords[j, 0] = coordsList[indexes[j * indexOffset], 0];
                    newCoords[j, 1] = coordsList[indexes[j * indexOffset], 1];
                }
            }

            coordsList = newCoords;

            Query();
        }

        public static double GetDistance(double lat1, double lat2, double lon1,
                                         double lon2, double el1, double el2)
        {
            if (lat1 == 0)
                return 0;

            int R = 6371; // Radius of the earth

            double latDistance = (Math.PI / 180) * (lat2 - lat1);
            double lonDistance = (Math.PI / 180) * (lon2 - lon1);
            double a = Math.Sin(latDistance / 2) * Math.Sin(latDistance / 2)
                    + Math.Cos((Math.PI / 180) * (lat1)) * Math.Cos((Math.PI / 180) * (lat2))
                    * Math.Sin(lonDistance / 2) * Math.Sin(lonDistance / 2);
            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            double distance = R * c * 1000; // convert to meters

            distance = Math.Pow(distance, 2);

            return Math.Sqrt(distance);
        }
    }
}
