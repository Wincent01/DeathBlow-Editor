using UnityEngine;
using System.Collections;
using UnityEditor;
using DeathBlow;
using InfectedRose.Luz;
using InfectedRose.Lvl;
using InfectedRose.Terrain;
using System.Linq;
using InfectedRose.Terrain.Editing;
using System.Threading.Tasks;
using DeathBlow.Components;
using System.IO;
using RakDotNet.IO;
using InfectedRose.Database.Concepts.Tables;

[CustomEditor(typeof(ZoneDetails))]
public class ZoneDetailsEditor : Editor
{
    public override void OnInspectorGUI()
    {
        if (GUILayout.Button("New Scene"))
        {
            var zoneInstance = ((ZoneDetails)serializedObject.targetObject).gameObject;

            var sceneInstance = new GameObject($"New Scene");

            sceneInstance.transform.parent = zoneInstance.transform;

            var sceneDetails = sceneInstance.AddOrGetComponent<SceneDetails>();

            sceneDetails.SceneName = "New Scene";

            sceneDetails.SceneLayer = 0;

            sceneDetails.SkyBox = "";
        }

        base.OnInspectorGUI();

        if (GUILayout.Button("Export"))
        {
            ((ZoneDetails)serializedObject.targetObject).Export();
        }
    }
}


public class ZoneDetails : MonoBehaviour
{
    [SerializeField] private string _zoneName;

    [SerializeField] private uint _zoneId;

    [SerializeField] private Transform _spawnPoint;

    [SerializeField] private float _ghostDistanceMin;

    [SerializeField] private float _ghostDistanceMax;

    [SerializeField] private int _populationSoftCap;

    [SerializeField] private int _populationHardCap;

    public string ZoneName { get => _zoneName; set => _zoneName = value; }

    public uint ZoneId { get => _zoneId; set => _zoneId = value; }

    public Transform SpawnPoint { get => _spawnPoint; set => _spawnPoint = value; }

    public void Export()
    {
        Debug.Log("Started export.");

        var luz = new LuzFile();

        luz.SpawnPoint = Utilities.ToNative(SpawnPoint.position);
        luz.SpawnRotation = Utilities.ToNative(SpawnPoint.rotation);

        luz.Version = 0x26;
        luz.WorldId = _zoneId;

        var nameNormalized = ZoneName.ToLower().Replace(" ", "_");

        var resultPath = Path.Combine(WorkspaceControl.CurrentWorkspace.WorkingRoot, "maps/", "death_blow/", nameNormalized + "/");

        Debug.Log(resultPath);

        Directory.CreateDirectory(resultPath);

        var scenes = GetComponentsInChildren<SceneDetails>();

        var sceneId = 0u;
        luz.Scenes = scenes.Select(s => {
            var luzScene = new LuzScene();

            var normalizedLvlName = s.SceneName.ToLower().Replace(" ", "_");

            luzScene.FileName = $"{nameNormalized}_{normalizedLvlName}.lvl";
            luzScene.SceneId = sceneId++;
            luzScene.LayerId = s.SceneLayer;
            luzScene.SceneName = s.name;

            return luzScene;
        }).ToArray();

        var terrainFile = $"{nameNormalized}.raw";

        var terrainDetails = GetComponentInChildren<TerrainDetails>();

        var terrain = terrainDetails.Export();

        terrain.SaveAsync(Path.Combine(resultPath, terrainFile)).Wait();

        luz.TerrainDescription = "Terrain";
        luz.TerrainFileName = terrainFile;
        luz.TerrainFile = terrainFile;

        foreach (var scene in scenes)
        {
            var lvl = new LvlFile();

            lvl.LevelInfo = new LevelInfo();

            lvl.LvlVersion = 0x26;
            lvl.LevelSkyConfig = new LevelSkyConfig();
            lvl.LevelSkyConfig.Skybox[0] = scene.SkyBox;

            lvl.LevelObjects = new LevelObjects(lvl.LvlVersion);

            var objects = scene.GetComponentsInChildren<ObjectDetails>();

            var objectId = 3696899ul;

            lvl.LevelObjects.Templates = objects.Select(o => {
                var levelObject = new LevelObjectTemplate(lvl.LvlVersion);

                levelObject.Lot = o.Lot;
                levelObject.ObjectId = objectId++;
                levelObject.Position = Utilities.ToNative(o.transform.position);
                levelObject.Rotation = Utilities.ToNative(o.transform.rotation);
                levelObject.Scale = o.transform.localScale.x;
                levelObject.LegoInfo = o.GetDataDictionary();

                return levelObject;
            }).ToArray();

            var normalizedLvlName = scene.SceneName.ToLower().Replace(" ", "_");

            using var lvlStream = File.Create(Path.Combine(resultPath, $"{nameNormalized}_{normalizedLvlName}.lvl"));
            using var lvlWriter = new BitWriter(lvlStream);

            lvl.Serialize(lvlWriter);
        }

        using var luzStream = File.Create(Path.Combine(resultPath, $"{nameNormalized}.luz"));
        using var luzWriter = new BitWriter(luzStream);

        luz.Serialize(luzWriter);

        var zoneTable = WorkspaceControl.Database["ZoneTable"];

        var zoneEntry = new ZoneTableTable(zoneTable.FirstOrDefault(z => z.Key == _zoneId) ?? zoneTable.Create(_zoneId));

        zoneEntry.zoneName = Path.Combine("death_blow/", nameNormalized + "/", $"{nameNormalized}.luz");
        zoneEntry.ghostdistance_min = _ghostDistanceMin;
        zoneEntry.ghostdistance = _ghostDistanceMax;
        zoneEntry.population_soft_cap = _populationSoftCap;
        zoneEntry.population_hard_cap = _populationHardCap;

        Debug.Log("Finished export.");
    }
}
