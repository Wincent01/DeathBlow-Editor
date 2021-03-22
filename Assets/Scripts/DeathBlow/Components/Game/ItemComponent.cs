using InfectedRose.Database.Concepts;
using UnityEngine;

namespace DeathBlow.Components.Game
{
    [GameComponent(ComponentId.ItemComponent, "ItemComponent")]
    public class ItemComponent : GameComponent
    {
        [SerializeField] [LoadField("id")] public int _id;

        [SerializeField] [LoadField("equipLocation")]
        public string _equipLocation;

        [SerializeField] [LoadField("baseValue")]
        public int _baseValue;

        [SerializeField] [LoadField("isKitPiece")]
        public bool _isKitPiece;

        [SerializeField] [LoadField("rarity")] public int _rarity;

        [SerializeField] [LoadField("itemType")]
        public int _itemType;

        [SerializeField] [LoadField("itemInfo")]
        public long _itemInfo;

        [SerializeField] [LoadField("inLootTable")]
        public bool _inLootTable;

        [SerializeField] [LoadField("inVendor")]
        public bool _inVendor;

        [SerializeField] [LoadField("isUnique")]
        public bool _isUnique;

        [SerializeField] [LoadField("isBOP")] public bool _isBOP;
        [SerializeField] [LoadField("isBOE")] public bool _isBOE;

        [SerializeField] [LoadField("reqFlagID")]
        public int _reqFlagID;

        [SerializeField] [LoadField("reqSpecialtyID")]
        public int _reqSpecialtyID;

        [SerializeField] [LoadField("reqSpecRank")]
        public int _reqSpecRank;

        [SerializeField] [LoadField("reqAchievementID")]
        public int _reqAchievementID;

        [SerializeField] [LoadField("stackSize")]
        public int _stackSize;

        [SerializeField] [LoadField("color1")] public int _color1;
        [SerializeField] [LoadField("decal")] public int _decal;

        [SerializeField] [LoadField("offsetGroupID")]
        public int _offsetGroupID;

        [SerializeField] [LoadField("buildTypes")]
        public int _buildTypes;

        [SerializeField] [LoadField("reqPrecondition")]
        public string _reqPrecondition;

        [SerializeField] [LoadField("animationFlag")]
        public int _animationFlag;

        [SerializeField] [LoadField("equipEffects")]
        public int _equipEffects;

        [SerializeField] [LoadField("readyForQA")]
        public bool _readyForQA;

        [SerializeField] [LoadField("itemRating")]
        public int _itemRating;

        [SerializeField] [LoadField("isTwoHanded")]
        public bool _isTwoHanded;

        [SerializeField] [LoadField("minNumRequired")]
        public int _minNumRequired;

        [SerializeField] [LoadField("delResIndex")]
        public int _delResIndex;

        [SerializeField] [LoadField("currencyLOT")]
        public int _currencyLOT;

        [SerializeField] [LoadField("altCurrencyCost")]
        public int _altCurrencyCost;

        [SerializeField] [LoadField("subItems")]
        public string _subItems;

        [SerializeField] [LoadField("audioEventUse")]
        public string _audioEventUse;

        [SerializeField] [LoadField("noEquipAnimation")]
        public bool _noEquipAnimation;

        [SerializeField] [LoadField("commendationLOT")]
        public int _commendationLOT;

        [SerializeField] [LoadField("commendationCost")]
        public int _commendationCost;

        [SerializeField] [LoadField("audioEquipMetaEventSet")]
        public string _audioEquipMetaEventSet;

        [SerializeField] [LoadField("currencyCosts")]
        public string _currencyCosts;

        [SerializeField] [LoadField("ingredientInfo")]
        public string _ingredientInfo;

        [SerializeField] [LoadField("locStatus")]
        public int _locStatus;

        [SerializeField] [LoadField("forgeType")]
        public int _forgeType;

        [SerializeField] [LoadField("SellMultiplier")]
        public float _SellMultiplier;
    }
}