using InfectedRose.Database.Concepts;
using UnityEngine;

namespace DeathBlow.Components.Game
{
    [GameComponent(ComponentId.BaseCombatAIComponent, "BaseCombatAIComponent")]
    public class BaseCombatAIComponent : GameComponent
    {
        [SerializeField] [LoadField("behaviorType")]
        public int _behaviorType;
        
        [SerializeField] [LoadField("combatRoundLength")]
        public float _combatRoundLength;
        
        [SerializeField] [LoadField("combatRole")]
        public int _combatRole;
        
        [SerializeField] [LoadField("minRoundLength")]
        public float _minRoundLength;
        
        [SerializeField] [LoadField("maxRoundLength")]
        public float _maxRoundLength;
        
        [SerializeField] [LoadField("tetherSpeed")]
        public float _tetherSpeed;
        
        [SerializeField] [LoadField("pursuitSpeed")]
        public float _pursuitSpeed;
        
        [SerializeField] [LoadField("combatStartDelay")]
        public float _combatStartDelay;
        
        [SerializeField] [LoadField("softTetherRadius")]
        public float _softTetherRadius;
        
        [SerializeField] [LoadField("hardTetherRadius")]
        public float _hardTetherRadius;
        
        [SerializeField] [LoadField("spawnTimer")]
        public float _spawnTimer;
        
        [SerializeField] [LoadField("tetherEffectID")]
        public int _tetherEffectID;
        
        [SerializeField] [LoadField("ignoreMediator")]
        public bool _ignoreMediator;
        
        [SerializeField] [LoadField("aggroRadius")]
        public float _aggroRadius;
        
        [SerializeField] [LoadField("ignoreStatReset")]
        public bool _ignoreStatReset;
        
        [SerializeField] [LoadField("ignoreParent")]
        public bool _ignoreParent;
    }
}