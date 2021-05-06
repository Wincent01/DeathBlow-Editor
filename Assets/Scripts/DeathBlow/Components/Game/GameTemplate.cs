using InfectedRose.Database.Concepts;
using UnityEditor;
using UnityEngine;

namespace DeathBlow.Components.Game
{
    [CustomEditor(typeof(GameTemplate))]
    public class GameTemplateEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            if (!WorkspaceControl.Ok)
            {
                GUILayout.Label("Workspace not loaded.");
                
                return;
            }
            
            var gameTemplate = (GameTemplate) serializedObject.targetObject;
            
            if (GUILayout.Button("Generate ID"))
            {
                gameTemplate.Lot = WorkspaceControl.Database["Objects"].ClaimKey(100000);
            }
            
            base.OnInspectorGUI();

            if (GUILayout.Button("Save"))
            {
                gameTemplate.Save();
            }
        }
    }

    public class GameTemplate : MonoBehaviour
    {
        [SerializeField] private int _lot = -1;

        public int Lot
        {
            get => _lot;
            set => _lot = value;
        }

        public void Save()
        {
            if (!WorkspaceControl.Ok) return;
            
            var database = WorkspaceControl.Database;

            var lwo = database.LoadObject(_lot) ?? database.NewObject(_lot);

            _lot = lwo.Lot;
            
            lwo.Clear();

            foreach (var component in gameObject.GetComponents(typeof(GameComponent)))
            {
                if (!(component is GameComponent gameComponent)) continue;
                
                Debug.Log($"GameComponent: {component.GetType()}");
                
                gameComponent.Save(lwo);
            }
        }
    }
}