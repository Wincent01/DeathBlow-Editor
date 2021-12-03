using DeathBlow.Components.Editors;
 
using UnityEditor;
using UnityEngine;

namespace DeathBlow.Components.Game
{
    [CustomEditor(typeof(PhysicsComponent))]
    public class PhysicsComponentEditor : GameComponentEditor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            
            AssetProperty("Physics Asset", "_physicsAsset", "physics/");
            AssetProperty("Boundary Asset", "_boundaryAsset", "physics/");
            AssetProperty("Gravity Volume Asset", "_gravityVolumeAsset", "physics/");
        }
    }
    
    [GameComponent(ComponentId.PhysicsComponent, "PhysicsComponent")]
    public class PhysicsComponent : GameComponent
    {
        [SerializeField] [LoadField("static")]
        public float _static;

        [SerializeField, HideInInspector] [LoadField("physics_asset")]
        public string _physicsAsset;

        [SerializeField] [LoadField("jump")]
        public float _jump;

        [SerializeField] [LoadField("doublejump")]
        public float _doubleJump;

        [SerializeField] [LoadField("speed")]
        public float _speed;

        [SerializeField] [LoadField("rotSpeed")]
        public float _rotSpeed;

        [SerializeField] [LoadField("playerHeight")]
        public float _playerHeight;

        [SerializeField] [LoadField("playerRadius")]
        public float _playerRadius;

        [SerializeField] [LoadField("pcShapeType")]
        public int _pcShapeType;

        [SerializeField] [LoadField("collisionGroup")]
        public int _collisionGroup;

        [SerializeField] [LoadField("airSpeed")]
        public float _airSpeed;

        [SerializeField, HideInInspector] [LoadField("boundaryAsset")]
        public string _boundaryAsset;

        [SerializeField] [LoadField("jumpAirSpeed")]
        public float _jumpAirSpeed;

        [SerializeField] [LoadField("friction")]
        public float _friction;

        [SerializeField, HideInInspector] [LoadField("gravityVolumeAsset")]
        public string _gravityVolumeAsset;

        public override void OnLoad()
        {
            _physicsAsset = Utilities.HostPath(_physicsAsset);
            _boundaryAsset = Utilities.HostPath(_boundaryAsset);
            _gravityVolumeAsset = Utilities.HostPath(_gravityVolumeAsset);
        }
    }
}