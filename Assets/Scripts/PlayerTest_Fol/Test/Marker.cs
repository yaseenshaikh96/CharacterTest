using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Marker : MonoBehaviour
{
    [SerializeField] private GameObject VectorGO;
    void Start()
    {
        MarkerList.vectorGO = VectorGO;
    }
}

public class MarkerList
{
    public static GameObject vectorGO;
    public List<Marker> mMarkers;
    public List<MarkerVector> mMarkerVectors;
    public GameObject mParentGO;

    public MarkerList()
    {
        mMarkers = new List<Marker>();
        mMarkerVectors = new List<MarkerVector>();
        mParentGO = new UnityEngine.GameObject("Markers");
    }
    public void MakeMarker(string name, Vector3 position, Color? color = null)
    {
        if (color == null)
            color = Color.white;

        GameObject sphere = UnityEngine.GameObject.CreatePrimitive(UnityEngine.PrimitiveType.Sphere);
        sphere.transform.localScale = Vector3.one * 0.2f;
        sphere.GetComponent<Renderer>().material.color = (Color)color;
        sphere.transform.parent = mParentGO.transform;
        sphere.gameObject.name = "Marker " + name;

        Marker marker = new Marker(name, (Vector3)position, (Color)color, sphere);
        mMarkers.Add(marker);
    }

    public void MakeMarkerVector(string name, Vector3 position, Vector3 lookAt, Color? color = null)
    {
        if (color == null)
            color = Color.white;

        GameObject markerVectorParent = UnityEngine.GameObject.Instantiate(vectorGO);
        markerVectorParent.transform.position = position;

        markerVectorParent.GetComponentInChildren<Renderer>().material.color = (Color)color;
        markerVectorParent.transform.parent = mParentGO.transform;
        markerVectorParent.gameObject.name = "Marker " + name;

        markerVectorParent.transform.LookAt(lookAt + position, Vector3.up);

        MarkerVector markerVectorObj = new MarkerVector(name, position, lookAt, (Color)color, markerVectorParent);

        mMarkerVectors.Add(markerVectorObj);
    }

    public void UpdateVector(string name, Vector3 position, Vector3 lookAt)
    {
        foreach(var v in mMarkerVectors)
        {
            if(v.mName == name)
            {
                v.mGameObject.transform.position = position;
                v.mGameObject.transform.LookAt(position+lookAt);
            }
        }
    }

    public void UpdateMarker(string name, Vector3 position)
    {
        foreach (var m in mMarkers)
        {
            if (m.mName == name)
            {
                m.mGameObject.transform.position = position;
            }
        }
    }

    public void Delete(string name)
    {
        foreach (var m in mMarkers)
        {
            if (m.mName == name)
            {
                UnityEngine.GameObject.Destroy(m.mGameObject);
                mMarkers.Remove(m);
                return;
            }
        }
    }
    public void DeleteAll()
    {
        foreach (var m in mMarkers)
        {
            UnityEngine.GameObject.Destroy(m.mGameObject);
        }
        mMarkers.Clear();
    }

    public class Marker
    {
        public Vector3 mPosition;
        public Color mColor;
        public GameObject mGameObject;
        public string mName;

        public Marker(string name, Vector3 position, Color color, GameObject gameObject)
        {
            mName = name;
            mPosition = position;
            mColor = color;
            mGameObject = gameObject;
        }
    }
    public class MarkerVector : Marker
    {
        Vector3 mDir;
        public MarkerVector(string name, Vector3 position, Vector3 dir, Color color, GameObject gameObject)
            : base(name, position, color, gameObject)
        {
            mDir = dir;
        }
    }
}

