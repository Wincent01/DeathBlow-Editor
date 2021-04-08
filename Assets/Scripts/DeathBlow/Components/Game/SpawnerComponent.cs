using DeathBlow.Components.Editors;
using InfectedRose.Database.Concepts;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace DeathBlow.Components.Game
{
    [GameComponent(ComponentId.SpawnerComponent)]
    public class SpawnerComponent : GameComponent
    {
        public void OnDrawGizmos()
        {
            var details = GetComponentInParent<ObjectDetails>();

            if (details == null)
            {
                return;
            }

            var cameraPosition = SceneView.currentDrawingSceneView.camera.transform.position;

            if (Vector3.Distance(cameraPosition, transform.position) > 40.5f) return;

            var entry = details.GetEntry("spawntemplate");

            Utilities.GizmosDrawString($"Spawner [{(entry == null ? "<select>" : entry.Value)}]", transform.position);
        }

        public override void OnDetailGUI(ObjectDetails details)
        {
            base.OnDetailGUI(details);

            var entry = details.GetEntry("spawntemplate");

            var value = entry == null ? "" : entry.Value;

            if (details.SpawnerTemplate != null)
            {
                value = details.SpawnerTemplate.GetComponent<ObjectDetails>().Lot.ToString();
            }
            else
            {
                value = EditorGUILayout.TextField("Spawn Template", value);
            }

            if (entry == null && !string.IsNullOrWhiteSpace(value))
            {
                entry = details.CreateEntry("spawntemplate", ObjectDataType.Int32, value, false);
            }
            else if (entry != null)
            {
                entry.Value = value;
            }

            entry = details.GetEntry("respawn");

            value = entry == null ? "" : entry.Value;

            value = EditorGUILayout.TextField("Respawn (seconds)", value);

            if (entry == null && !string.IsNullOrWhiteSpace(value))
            {
                entry = details.CreateEntry("respawn", ObjectDataType.Float32, value, false);
            }
            else if (entry != null)
            {
                entry.Value = value;
            }
        }
    }
}