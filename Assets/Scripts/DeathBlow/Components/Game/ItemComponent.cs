using InfectedRose.Database.Concepts;
using UnityEngine;

namespace DeathBlow.Components.Game
{
    [GameComponent(ComponentId.ItemComponent, "ItemComponent")]
    public class ItemComponent : GameComponent
    {
        [SerializeField, LoadField("equipLocation")]
        public string _equipLocation;
        
        [SerializeField, LoadField("baseValue")]
        public int _baseValue;
        
        [SerializeField, LoadField("isKitPiece")]
        public bool _isKitPiece;
        
        [SerializeField, LoadField("rarity")]
        public int _rarity;
        
        [SerializeField, LoadField("itemType")]
        public int _itemType;
        
        [SerializeField, LoadField("itemInfo")]
        public long _itemInfo;
        
        [SerializeField, LoadField("inLootTable")]
        public bool _inLootTable;
        
        [SerializeField, LoadField("inVendor")]
        public bool _inVendor;
        
        [SerializeField, LoadField("isUnique")]
        public bool _isUnique;
        
        [SerializeField, LoadField("isBOP")]
        public bool _isBOP;
        
        [SerializeField, LoadField("isBOE")]
        public bool _isBOE;
        
        [SerializeField, LoadField("reqFlagID")]
        public int _reqFlagID;

        [SerializeField, LoadField("reqSpecialtyID")]
        public int _reqSpecialtyID;
    }
}