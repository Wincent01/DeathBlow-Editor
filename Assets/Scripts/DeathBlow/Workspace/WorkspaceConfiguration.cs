using UnityEngine;

namespace DeathBlow.Workspace
{
    [CreateAssetMenu(fileName = "Workspace Configuration", menuName = "Death Blow/New Workspace", order = 1)]
    public class WorkspaceConfiguration : ScriptableObject
    { 
        [SerializeField]
        private string _workspaceName = "Default Workspace";
        
        [SerializeField]
        private string _workingRoot;

        [SerializeField]
        private string _database;

        [SerializeField]
        private Material _vertexMaterial;

        [SerializeField]
        private Material _normalMaterial;

        [SerializeField]
        private string _assetPath;
        
        [SerializeField]
        private string _assetModelsPath;
        
        [SerializeField]
        private string _assetObjectsPath;
        
        public string WorkspaceName
        {
            get => _workspaceName;
            set => _workspaceName = value;
        }

        public string WorkingRoot
        {
            get => _workingRoot;
            set => _workingRoot = value;
        }

        public string Database
        {
            get => _database;
            set => _database = value;
        }

        public Material VertexMaterial
        {
            get => _vertexMaterial;
            set => _vertexMaterial = value;
        }

        public Material NormalMaterial
        {
            get => _normalMaterial;
            set => _normalMaterial = value;
        }

        public string AssetPath
        {
            get => _assetPath;
            set => _assetPath = value;
        }

        public string AssetModelsPath
        {
            get => _assetModelsPath;
            set => _assetModelsPath = value;
        }

        public string AssetObjectsPath
        {
            get => _assetObjectsPath;
            set => _assetObjectsPath = value;
        }
    }
}