 
using UnityEngine;

namespace DeathBlow.Components.Game
{
    [GameComponent(ComponentId.DestructibleComponent, "DestructibleComponent")]
    public class DestructibleComponent : GameComponent
    {
        [SerializeField] [LoadField("faction")]
        public int _faction;
        
        [SerializeField] [LoadField("factionList")]
        public string _factionList;
        
        [SerializeField] [LoadField("life")]
        public int _life;
        
        [SerializeField] [LoadField("imagination")]
        public int _imagination;
        
        [SerializeField] [LoadField("LootMatrixIndex")]
        public int _LootMatrixIndex;
        
        [SerializeField] [LoadField("CurrencyIndex")]
        public int _CurrencyIndex;

        [SerializeField] [LoadField("level")]
        public int _level;
        
        [SerializeField] [LoadField("armor")]
        public float _armor;
        
        [SerializeField] [LoadField("death_behavior")]
        public int _deathBehavior;
        
        [SerializeField] [LoadField("isnpc")]
        public bool _isnpc;
        
        [SerializeField] [LoadField("attack_priority")]
        public int _attackPriority;
        
        [SerializeField] [LoadField("isSmashable")]
        public bool _isSmashable;
        
        [SerializeField] [LoadField("difficultyLevel")]
        public int _difficultyLevel;
    }
}