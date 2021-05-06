using DeathBlow;
using InfectedRose.Luz;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(WaypointDetails))]
public class WaypointDetailsEditor : Editor
{
    public override void OnInspectorGUI()
    {
        if (GUILayout.Button("New Waypoint"))
        {
            var details = (WaypointDetails) serializedObject.targetObject;

            var waypoints = details.transform.parent.GetComponentsInChildren<WaypointDetails>();

            var waypoint = new GameObject($"Waypoint - {waypoints.Length}");

            waypoint.transform.position = details.transform.position;

            var waypointDetails = waypoint.AddOrGetComponent<WaypointDetails>();

            Selection.activeGameObject = waypoint;

            waypoint.transform.parent = details.transform.parent;
        }
        
        base.OnInspectorGUI();
    }
}

public class WaypointDetails : MonoBehaviour
{
    [SerializeField] private float _speed;

    [SerializeField] private float _wait;

    [SerializeField] private bool _lockPlayer;

    public float Speed
    {
        get => _speed;
        set => _speed = value;
    }

    public float Wait
    {
        get => _wait;
        set => _wait = value;
    }

    public bool LockPlayer
    {
        get => _lockPlayer;
        set => _lockPlayer = value;
    }
}