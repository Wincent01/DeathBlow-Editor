using System;
using DeathBlow.Components.Editors;
 
using UnityEditor;
using UnityEngine;

namespace DeathBlow.Components.Game
{
    [GameComponent(ComponentId.BouncerComponent)]
    public class BouncerComponent : GameComponent
    {
        private Transform CachedDestinationReference { get; set; }
        
        public override void OnDetailGizmos(ObjectDetails details)
        {
            base.OnDetailGizmos(details);

            var destinationReference = CachedDestinationReference;

            if (destinationReference == null)
            {
                return;
            }

            var end = destinationReference.transform.position;
            var start = transform.position;
            
            var partA = start;

            Gizmos.color = Color.blue;
            
            for (var t = 0.0f; t <= 1.1f; t += 0.1f)
            {
                var partB = Utilities.Parabola(start, end, Math.Abs(start.y - end.y), t);
                
                Gizmos.DrawLine(partA, partB);

                partA = partB;
            }
            
            var color = Color.red;

            if (Physics.Raycast(end, Vector3.down, 1))
            {
                color = Color.green;
            }
            
            color.a = 0.5f;

            Gizmos.color = color;
            
            Gizmos.DrawSphere(end, 1);
        }

        public override void OnDetailGUI(ObjectDetails details)
        {
            base.OnDetailGUI(details);

            var destinationReference = details.ReferenceSelector<Transform>("destination", "Bouncer Destination");
            
            details.SimpleDataSelector(ObjectDataType.Float32, "bouncer_speed", "Bouncer Speed");
            
            details.SimpleDataSelector(ObjectDataType.Boolean, "bouncer_uses_high_arc", "Bouncer Uses High Arc");
            
            details.SimpleDataSelector(ObjectDataType.Boolean, "lock_controls", "Lock Controls");

            details.SimpleDataSelector(ObjectDataType.Boolean, "stickLanding", "Stick Landing");

            CachedDestinationReference = destinationReference;

            if (destinationReference != null)
            {
                var destination = Utilities.ToGameSpace(destinationReference.position);
                
                details.SetEntry("bouncer_destination", ObjectDataType.UTF16, $"{destination.x}\x1F{destination.y}\x1F{destination.z}");
            }
            else
            {
                destinationReference = new GameObject("Destination").transform;

                destinationReference.parent = details.transform;
                
                details.SetReference("destination", destinationReference);
            }
        }
    }
}