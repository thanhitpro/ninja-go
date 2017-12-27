using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mapbox.Unity.Utilities;
using Mapbox.Utils;
using Mapbox.Unity.Map;
using Mapbox.Directions;
using Mapbox.Unity;
using Mapbox.Unity.MeshGeneration.Data;
using Mapbox.Unity.MeshGeneration.Modifiers;
using System.Linq;

public class GenerateEnemies : MonoBehaviour
{

    public static Vector2d _PlayerPosition = Vector2d.zero;
    private float ratio = 0.005f;
    public int numOfEnemies;
    private bool generated = false;
    public GameObject botPrefab;

    [SerializeField]
    MeshModifier[] MeshModifiers;

    [SerializeField]
    Material _material;

    private List<GameObject> bots = new List<GameObject>();
    public static AbstractMap _map;
    private List<Directions> botsdirection = new List<Directions>();
    private List<Vector2d> botsPos2d = new List<Vector2d>();
    private Vector2d curPlayerPos = Vector2d.zero;
    private Vector2d curBotPos = Vector2d.zero;
    private Directions curBotDir;
    private GameObject curBot;
    private int indexHandle = 0;
    private int indexQuery = 0;

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (!generated)
        {
            Vector2d test = generate();
            if (test.x != 0 && test.y != 0)
            {
                generated = true;
            }
            else
            {
                return;
            }

            for (int i = 0; i < numOfEnemies; i++)
            {
                Debug.Log("Bot " + i + ": ");
                GameObject boti = Instantiate(botPrefab);
                boti.transform.position = Conversions.GeoToWorldPosition(test,
                                                                 _map.CenterMercator,
                                                                 _map.WorldRelativeScale).ToVector3xz();
                bots.Add(boti);

                botsPos2d.Add(test);
                curPlayerPos = _PlayerPosition;
                curBot = boti;

                Directions _directions = MapboxAccess.Instance.Directions;
                curBotDir = _directions;
                botsdirection.Add(_directions);
                Query();
                test = generate();
                indexQuery++;
            }


        }

    }

    void OnDestroy()
    {
        _map.OnInitialized -= Query;
    }

    void Query()
    {
        Debug.Log("Run here");
        _map.OnInitialized -= Query;
        var wp = new Vector2d[2];
        wp[0] = curPlayerPos;
        wp[1] = botsPos2d[indexQuery];
        var _directionResource = new DirectionResource(wp, RoutingProfile.Walking);
        _directionResource.Steps = true;
        curBotDir.Query(_directionResource, HandleDirectionsResponse);
    }

    void HandleDirectionsResponse(DirectionsResponse response)
    {
        if (null == response.Routes || response.Routes.Count < 1)
        {
            return;
        }
        Debug.Log("Run here out");

        var meshData = new MeshData();
        var dat = new List<Vector3>();
        foreach (var point in response.Routes[0].Geometry)
        {
            dat.Add(Conversions.GeoToWorldPosition(point.x, point.y, _map.CenterMercator, _map.WorldRelativeScale).ToVector3xz());
        }

        Debug.Log("Current Bot's Position = " + bots[indexHandle].transform.position.x + ", " + bots[indexHandle].transform.position.y);
        bots[indexHandle].transform.position = dat[dat.Count-1];
        Debug.Log("After update Bot's Position = " + bots[indexHandle].transform.position.x + ", " + bots[indexHandle].transform.position.y);


        var feat = new VectorFeatureUnity();
        feat.Points.Add(dat);

        foreach (MeshModifier mod in MeshModifiers.Where(x => x.Active))
        {
            mod.Run(feat, meshData, _map.WorldRelativeScale);
        }

        CreateGameObject(meshData);
        indexHandle++;
    }

    GameObject CreateGameObject(MeshData data)
    {
        var go = new GameObject("direction waypoint " + " entity");
        var mesh = go.AddComponent<MeshFilter>().mesh;
        mesh.subMeshCount = data.Triangles.Count;

        mesh.SetVertices(data.Vertices);
        for (int i = 0; i < data.Triangles.Count; i++)
        {
            var triangle = data.Triangles[i];
            mesh.SetTriangles(triangle, i);
        }

        for (int i = 0; i < data.UV.Count; i++)
        {
            var uv = data.UV[i];
            mesh.SetUVs(i, uv);
        }

        mesh.RecalculateNormals();
        go.AddComponent<MeshRenderer>().material = _material;
        return go;
    }

    Vector2d generate()
    {
        if (_PlayerPosition.x == 0 && _PlayerPosition.y == 0)
        {
            return Vector2d.zero;
        }
        float longitudeValue = (float)_PlayerPosition.x;
        float lattitudeValue = (float)_PlayerPosition.y;

        float maxLongitude = longitudeValue + ratio;
        float minLongitude = longitudeValue - ratio;

        float maxLattitude = lattitudeValue + ratio;
        float minLattitude = lattitudeValue - ratio;

        return new Vector2d((double)Random.Range(minLongitude, maxLongitude), (double)Random.Range(minLattitude, maxLattitude));
    }
}
