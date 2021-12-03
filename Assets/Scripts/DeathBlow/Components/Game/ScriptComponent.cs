using DeathBlow.Components.Editors;
 
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
            
            AssetProperty("Server Script", "_serverScript", settings: AssetPropertySettings.Edit);
            AssetProperty("Client Script", "_clientScript", settings: AssetPropertySettings.Edit);
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

        public override void OnDetailGUI(ObjectDetails details)
        {
            base.OnDetailGUI(details);

            var entry = details.GetEntry("custom_script_server");

            var value = entry == null ? "" : entry.Value;

            value = Utilities.AssetProperty("Custom Script Server", value, settings: AssetPropertySettings.Edit);

            if (entry == null && !string.IsNullOrWhiteSpace(value))
            {
                entry = details.SetEntry("custom_script_server", ObjectDataType.UTF16, value, true);
            }
            else if (entry != null)
            {
                entry.Value = value;
            }
        }
    }
}