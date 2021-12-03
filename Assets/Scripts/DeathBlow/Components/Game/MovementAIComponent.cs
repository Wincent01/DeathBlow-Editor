 
using UnityEngine;

namespace DeathBlow.Components.Game
{
    [GameComponent(ComponentId.MovementAIComponent, "MovementAIComponent")]
    public class MovementAIComponent : GameComponent
    {
        [SerializeField] [LoadField("MovementType")]
        public string _MovementType;
        
        [SerializeField] [LoadField("WanderChance")]
        public float _WanderChance;
        
        [SerializeField] [LoadField("WanderDelayMin")]
        public float _WanderDelayMin;
        
        [SerializeField] [LoadField("WanderDelayMax")]
        public float _WanderDelayMax;
        
        [SerializeField] [LoadField("WanderSpeed")]
        public float _WanderSpeed;
        
        [SerializeField] [LoadField("WanderRadius")]
        public float _WanderRadius;
        
        [SerializeField] [LoadField("attachedPath")]
        public string _attachedPath;
    }
}