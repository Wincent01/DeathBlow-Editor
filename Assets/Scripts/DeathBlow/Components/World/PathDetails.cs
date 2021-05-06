using DeathBlow;
using InfectedRose.Luz;
using UnityEditor;
using UnityEngine;


[CustomEditor(typeof(PathDetails))]
public class PathDetailsEditor : Editor
{
    public override void OnInspectorGUI()
    {
        if (GUILayout.Button("New Waypoint"))
        {
            var details = (PathDetails) serializedObject.targetObject;

            var waypoints = details.GetComponentsInChildren<WaypointDetails>();

            var waypoint = new GameObject($"Waypoint - {waypoints.Length}");

            waypoint.transform.position = details.transform.position;

            var waypointDetails = waypoint.AddOrGetComponent<WaypointDetails>();

            Selection.activeGameObject = waypoint;

            waypoint.transform.parent = details.transform;
        }
        
        base.OnInspectorGUI();
    }
}

public class PathDetails : MonoBehaviour
{
    [SerializeField] private string _pathName;

    [SerializeField] private PathType _type;

    [SerializeField] private PathBehavior _behavior;

    public string PathName
    {
        get => _pathName;
        set => _pathName = value;
    }

    public PathType Type
    {
        get => _type;
        set => _type = value;
    }

    public PathBehavior Behavior
    {
        get => _behavior;
        set => _behavior = value;
    }
}