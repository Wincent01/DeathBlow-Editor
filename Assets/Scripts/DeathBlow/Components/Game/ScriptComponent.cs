using DeathBlow.Components.Editors;
using InfectedRose.Database.Concepts;
using UnityEditor;
using UnityEngine;

namespace DeathBlow.Components.Game
{
    [CustomEditor(typeof(ScriptComponent))]
    public class ScriptComponentEditor : GameComponentEditor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            
            AssetProperty("Server Script", "_serverScript");
            AssetProperty("Client Script", "_clientScript");
        }
    }
    
    [GameComponent(ComponentId.ScriptComponent, "ScriptComponent")]
    public class ScriptComponent : GameComponent
    {
        [SerializeField, HideInInspector] [LoadField("script_name")]
        public string _serverScript;

        [SerializeField, HideInInspector] [LoadField("client_script_name")]
        public string _clientScript;

        public override void OnLoad()
        {
            _serverScript = Utilities.HostPath(_serverScript);
            _clientScript = Utilities.HostPath(_clientScript);
        }
    }
}