using System.IO;
using DeathBlow.Components.Editors;
using InfectedRose.Database.Concepts;
using InfectedRose.Nif;
using RakDotNet.IO;
using UnityEditor;
using UnityEngine;

namespace DeathBlow.Components.Game
{
    [CustomEditor(typeof(RenderComponent))]
    public class RenderComponentEditor : GameComponentEditor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            var renderGameObjectProperty = serializedObject.FindProperty("_renderGameObject");

            if (renderGameObjectProperty.objectReferenceValue != null)
            {
                EditorGUILayout.PropertyField(renderGameObjectProperty);
            }

            AssetProperty("Render Asset", "_renderAsset", "");
            
            AssetProperty("Icon Asset", "_iconAsset", "ui/ingame");
        }
    }
    
    [GameComponent(ComponentId.RenderComponent, "RenderComponent")]
    public class RenderComponent : GameComponent
    {
        [SerializeField, HideInInspector] [LoadField("render_asset")]
        public string _renderAsset;

        [SerializeField, HideInInspector]
        public GameObject _renderGameObject;
        
        [SerializeField, HideInInspector] [LoadField("icon_asset")]
        public string _iconAsset;
        
        [SerializeField] [LoadField("IconID")]
        public int _iconId;
        
        [SerializeField] [LoadField("shader_id")]
        public int _shaderId;

        [SerializeField] [LoadField("effect1")]
        public int _effect1;
        
        [SerializeField] [LoadField("effect2")]
        public int _effect2;
        
        [SerializeField] [LoadField("effect3")]
        public int _effect3;
        
        [SerializeField] [LoadField("effect4")]
        public int _effect4;
        
        [SerializeField] [LoadField("effect5")]
        public int _effect5;

        [SerializeField] [LoadField("effect6")]
        public int _effect6;

        [SerializeField] [LoadField("animationGroupIDs")]
        public string _animationGroupIds;

        [SerializeField] [LoadField("fade")]
        public bool _fade;

        [SerializeField] [LoadField("usedropshadow")]
        public bool _useDropShadow;
        
        [SerializeField] [LoadField("fadeInTime")]
        public float _fadeInTime;
        
        [SerializeField] [LoadField("maxShadowDistance")]
        public float _maxShadowDistance;

        [SerializeField] [LoadField("ignoreCameraCollision")]
        public bool _ignoreCameraCollision;

        [SerializeField] [LoadField("renderComponentLOD1")]
        public int _renderComponentLOD1;
        
        [SerializeField] [LoadField("renderComponentLOD2")]
        public int _renderComponentLOD2;

        [SerializeField] [LoadField("gradualSnap")]
        public bool _gradualSnap;

        [SerializeField] [LoadField("animationFlag")]
        public int _animationFlag;

        [SerializeField] [LoadField("AudioMetaEventSet")]
        public string _audioMetaEventSet;

        [SerializeField] [LoadField("billboardHeight")]
        public float _billboardHeight;

        [SerializeField] [LoadField("chatBubbleOffset")]
        public float _chatBubbleOffset;

        [SerializeField] [LoadField("staticBillboard")]
        public bool _staticBillboard;

        [SerializeField] [LoadField("LXFMLFolder")]
        public string _lxfmlFolder;

        [SerializeField] [LoadField("attachIndicatorsToNode")]
        public bool _attachIndicatorsToNode;
        
        public override void OnLoad()
        {
            if (_renderGameObject != null)
            {
                DestroyImmediate(_renderGameObject);
            }
            
            if (_renderAsset != null)
            {
                _renderAsset = _renderAsset.Replace("\\\\", "\\").Replace('\\', '/');

                if (Path.GetExtension(_renderAsset) != ".nif")
                {
                    return;
                }
                
                using var steam = Utilities.OpenAssetStreamRead(_renderAsset.ToLower());
                
                if (steam == null) throw new FileNotFoundException($"{_renderAsset}");
                
                using var reader = new BitReader(steam);
                
                var nif = new NiFile();
                nif.Deserialize(reader);
                nif.ReadBlocks(reader);
                
                var constructor = new ModelConstructor();

                constructor.File = nif;
                constructor.Name = Path.GetFileNameWithoutExtension(_renderAsset);
                constructor.CreatePrefab = true;
                constructor.PrefabPath = Path.Combine(
                                WorkspaceControl.CurrentWorkspace.AssetModelsPath,
                                constructor.Name
                );
                
                var root = constructor.Construct();

                root.transform.parent = transform;

                _renderGameObject = root;
            }
        }
    }
}