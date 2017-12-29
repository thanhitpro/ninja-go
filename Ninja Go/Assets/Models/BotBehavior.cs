using Mapbox.Unity.MeshGeneration.Data;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BotBehavior : MonoBehaviour
{

    //private VectorFeatureUnity vectorFeatureUnity;

    private VectorFeatureUnity vectorFeatureUnity;

    public VectorFeatureUnity VectorFeatureUnity
    {
        get { return vectorFeatureUnity; }
        set { vectorFeatureUnity = value; }
    }

    private bool readyToMove = false;

    public bool ReadyToMove
    {
        get { return readyToMove; }
        set { readyToMove = value; }
    }

    private List<Vector3> movePoints;

    public List<Vector3> MovePoints
    {
        get { return movePoints; }
        set { movePoints = value; }
    }

    private int indexMove = 0;

    [SerializeField]
    float _positionFollowFactor;

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (readyToMove)
        {
            if (indexMove < (movePoints.Count - 1))
            {
                Vector3 nextPosition = movePoints[indexMove];
                transform.position = Vector3.Lerp(transform.position, nextPosition, Time.deltaTime * _positionFollowFactor);
                if (Vector3.Distance(transform.position, nextPosition) <= 1)
                {
                    indexMove++;
                }
            }
        }
    }
}
