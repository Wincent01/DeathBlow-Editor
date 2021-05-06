using InfectedRose.Database.Concepts;
using InfectedRose.Luz;
using UnityEditor;
using UnityEngine;

namespace DeathBlow.Components.Game
{
    [GameComponent(ComponentId.MovingPlatformComponent)]
    public class MovingPlatformComponent : GameComponent
    {   
        private Transform CachedDestinationReference { get; set; }

        public override void OnDetailGizmos(ObjectDetails details)
        {
            base.OnDetailGizmos(details);

            if (CachedDestinationReference == null)
            {
                return;
            }

            var parts = CachedDestinationReference.GetComponentsInChildren<WaypointDetails>();

            Transform start = null;
            
            var color = Color.blue;
            color.a = 0.5f;
            
            foreach (var part in parts)
            {
                if (start == null)
                {
                    start = part.transform;
                    
                    continue;
                }

                Gizmos.color = color;

                var position = start.position;
                
                Gizmos.DrawSphere(position, 1.0f);
                
                Gizmos.color = Color.cyan;

                Gizmos.DrawLine(position, part.transform.position);

                start = part.transform;
            }

            if (start != null)
            {
                Gizmos.color = color;

                Gizmos.DrawSphere(start.position, 1.0f);
            }
        }

        public override void OnDetailGUI(ObjectDetails details)
        {
            base.OnDetailGUI(details);
            
            var destinationReference = details.ReferenceSelector<Transform>("platform_path", "Platform Path");

            CachedDestinationReference = destinationReference;
            
            if (destinationReference != null)
            {
                var pathDetails = destinationReference.gameObject.AddOrGetComponent<PathDetails>();

                details.SetEntry("CheckPrecondition", ObjectDataType.UTF16, "");
                
                details.SetEntry("friction", ObjectDataType.Float32, "1.5");
                
                details.SetEntry("interaction_distance", ObjectDataType.Float32, "16");

                details.SetEntry("create_physics", ObjectDataType.Boolean, "1");
                
                details.SetEntry("add_to_navmesh", ObjectDataType.Boolean, "1");

                details.SetEntry("attached_path", ObjectDataType.UTF16, pathDetails.PathName);
                
                details.SimpleDataSelector(ObjectDataType.Int32, "attached_path_start", "Path Start");

                details.SetEntry("platformIsMover", ObjectDataType.Boolean, "1");
                
                details.SetEntry("platformIsRotater", ObjectDataType.Boolean, "0");

                details.SetEntry("platformIsSimpleMover", ObjectDataType.Boolean, "0");

                details.SetEntry("platformNoUpdateSync", ObjectDataType.Boolean, "0");

                details.SetEntry("allowPosSnap", ObjectDataType.Boolean, "1");
                
                details.SetEntry("dbonly", ObjectDataType.Boolean, "1");
                
                details.SetEntry("bounding_radius_override", ObjectDataType.Boolean, "0");
                
                details.SetEntry("maxLerpDist", ObjectDataType.Int32, "4");
                
                details.SimpleDataSelector(ObjectDataType.Boolean, "startPathingOnLoad", "Start Pathing On Load");
                
                details.SimpleDataSelector(ObjectDataType.UTF8, "platformSoundStart", "Start Sound");

                details.SimpleDataSelector(ObjectDataType.UTF8, "platformSoundStop", "Stop Sound");

                details.SimpleDataSelector(ObjectDataType.UTF8, "platformSoundTravel", "Travel Sound");
            }
            else
            {
                destinationReference = new GameObject("Platform Path").transform;

                destinationReference.parent = details.transform;

                destinationReference.position = details.transform.position;

                var pathDetails = destinationReference.gameObject.AddOrGetComponent<PathDetails>();

                pathDetails.PathName = GUID.Generate().ToString();

                pathDetails.Type = PathType.MovingPlatform;
                
                details.SetReference("platform_path", destinationReference);
            }
        }
    }
}