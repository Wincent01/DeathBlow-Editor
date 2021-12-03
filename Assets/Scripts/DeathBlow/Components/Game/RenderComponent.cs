using System;
using System.IO;
using DeathBlow.Components.Editors;
 
using InfectedRose.Nif;
using RakDotNet.IO;
using UnityEditor;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

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
                
                var renderAsset = _renderAsset;

                FileStream stream = null;
                string directory = null;

                if (Path.GetExtension(renderAsset) == ".kfm")
                {
                    foreach (var file in Directory.GetFiles(
                                    WorkspaceControl.CurrentWorkspace.WorkingRoot,
                                    Path.GetFileNameWithoutExtension(renderAsset) + ".nif*",
                                    SearchOption.AllDirectories)
                    )
                    {
                        renderAsset = file;

                        directory = Path.GetDirectoryName(file);
                        
                        Debug.Log($"Found NIF from KFM: {file}");

                        stream = File.OpenRead(renderAsset);
                        
                        break;
                    }
                }
                else if (Path.GetExtension(renderAsset) == ".nif")
                {   
                    directory = Path.GetDirectoryName(renderAsset.ToLower());

                    stream = Utilities.OpenAssetStreamRead(renderAsset.ToLower());
                }
                else
                {
                    Debug.LogError($"Unknown render file {renderAsset} format");
                    
                    return;
                }

                if (stream == null)
                {
                    Debug.LogError($"Failed to find render asset: {renderAsset}");
                    
                    return;
                }
                
                using var reader = new BitReader(stream);
                GameObject root = null;
                
                try
                {
                    var nif = new NiFile();
                    nif.Deserialize(reader);
                    nif.ReadBlocks(reader);
                    
                    var constructor = new ModelConstructor();
                    
                    constructor.NifPath = directory;
                    constructor.File = nif;
                    constructor.Name = Path.GetFileNameWithoutExtension(renderAsset);
                    constructor.CreatePrefab = true;
                    constructor.PrefabPath = Path.Combine(
                                    WorkspaceControl.CurrentWorkspace.AssetModelsPath,
                                    constructor.Name
                    );
                    
                    root = constructor.Construct();

                    root.transform.parent = transform;

                    root.transform.localPosition = Vector3.zero;

                    _renderGameObject = root;
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }
                finally
                {
                    if (root != null)
                    {
                        DestroyImmediate(root);
                    }
                }
                
                stream.Dispose();
            }
        }
    }
}