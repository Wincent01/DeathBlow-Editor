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

    [Header("Debug")]
    [SerializeField] private string _sourceZoneFile;

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

        if (File.Exists(_sourceZoneFile))
        {
            using var sourceStream = File.OpenRead(_sourceZoneFile);
            using var sourceReader = new BitReader(sourceStream);

            var source = new LuzFile();
            source.Deserialize(sourceReader);

            luz.UnknownByte = source.UnknownByte;
            luz.Transitions = source.Transitions;

            Debug.Log($"Unknown Byte: {luz.UnknownByte}");
        }

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
            luzScene.SceneId = sceneId;
            luzScene.LayerId = s.SceneLayer;
            luzScene.SceneName = s.name;
            luzScene.UnknownByteArray = new byte[3] { 0, 0, 0 };

            sceneId++;

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

            lvl.LvlVersion = 0x48;
            lvl.LevelSkyConfig = new LevelSkyConfig();

            lvl.LevelSkyConfig.Skybox = scene.SkyBox;

            /*lvl.LevelSkyConfig.UnknownFloatArray0 = new float[25]
            {
                10, 0.41960784792900085f, 0.6196078658103943f, 0.7490196228027344f, 1, 1, 1, 1, 1, 1, 0, -2500, 1500, 150, 175, 3100, 100, 1000, 350, 150, 225, 3100, 100, 1000, 350
            };*/
            var lightRotation = scene.Light != null ? scene.Light.transform.rotation : new Quaternion(0.41960784792900085f, 0.6196078658103943f, 0.7490196228027344f, 1);
            var unknownRotation0 = scene._unknownRotation0 != null ? scene._unknownRotation0.rotation : new Quaternion(1, 1, 1, 1);
            var unknownRotation1 = scene._unknownRotation1 != null ? scene._unknownRotation1.rotation : new Quaternion(1, -0.39152511954307556f, -0.5821850299835205f, -0.7125792503356934f);

            lvl.LevelSkyConfig.UnknownFloatArray0 = new float[25]
            {
                scene.Luminosity, // Luminosity?
                lightRotation.x, lightRotation.y, lightRotation.z, lightRotation.w, // Quaternion
                unknownRotation0.x, unknownRotation0.y, unknownRotation0.z, unknownRotation0.w, // Quaternion
                unknownRotation1.x, unknownRotation1.y, unknownRotation1.z, unknownRotation1.w, // Quaternion
                scene.FogStart, // Fog start
                scene.FogEnd, // Fog end
                scene._unknownFogSetting0, scene._unknownFogSetting1, scene._unknownFogSetting2, scene._unknownFogSetting3,
                scene._fogStart1,
                scene._fogEnd1,
                scene._unknownFogSetting0_1, scene._unknownFogSetting1_1, scene._unknownFogSetting2_1, scene._unknownFogSetting3_1
            };
            var vector0 = scene._unknownVector0;
            var vector1 = scene._unknownVector1;
            lvl.LevelSkyConfig.UnknownFloatArray1 = new float[3] { vector0.x, vector0.y, vector0.z }; //{ 1, 0.98f, 0.85f };
            lvl.LevelSkyConfig.UnknownFloatArray2 = new float[3] { vector1.x, vector1.y, vector1.z }; //{ 0.78f, 0.89f, 1 };
            lvl.LevelSkyConfig.UnknownSectionData = Enumerable.Repeat((byte) 10, 388).ToArray();
            lvl.LevelSkyConfig.Identifiers = new IdStruct[11]
            {
                new IdStruct(0, 0, 0),
                new IdStruct(1, 100, 150),
                new IdStruct(2, 150, 200),
                new IdStruct(3, 200, 250),
                new IdStruct(4, 250, 300),
                new IdStruct(5, 40, 40),
                new IdStruct(6, 40, 40),
                new IdStruct(7, 400, 600),
                new IdStruct(8, 60, 100),
                new IdStruct(9, 50, 100),
                new IdStruct(10, 300, 400)
            };

            if (File.Exists(scene._sourceLevelFile))
            {
                using var sourceStream = File.OpenRead(scene._sourceLevelFile);
                using var sourceReader = new BitReader(sourceStream);

                var source = new LvlFile();
                source.Deserialize(sourceReader);

                lvl.LevelSkyConfig.Skybox = source.LevelSkyConfig.Skybox;
                lvl.LevelSkyConfig.UnknownFloatArray0 = source.LevelSkyConfig.UnknownFloatArray0;
                lvl.LevelSkyConfig.UnknownFloatArray1 = source.LevelSkyConfig.UnknownFloatArray1;
                lvl.LevelSkyConfig.UnknownFloatArray2 = source.LevelSkyConfig.UnknownFloatArray2;
                lvl.LevelSkyConfig.UnknownSectionData = source.LevelSkyConfig.UnknownSectionData;
                lvl.LevelSkyConfig.Identifiers = source.LevelSkyConfig.Identifiers;
                lvl.LevelEnvironmentConfig = source.LevelEnvironmentConfig;
            }

            lvl.LevelObjects = new LevelObjects(lvl.LvlVersion);

            var objects = scene.GetComponentsInChildren<ObjectDetails>().Where(o => !o.IsSpawned).ToArray();

            var objectId = 3696899ul;

            var scale = gameObject.transform.localScale;
            scale.z *= -1;
            gameObject.transform.localScale = scale;

            lvl.LevelObjects.Templates = objects.Select(o => {
                var levelObject = new LevelObjectTemplate(lvl.LvlVersion);

                var objectScale = o.transform.localScale;
                objectScale.z *= -1;
                o.transform.localScale = objectScale;

                levelObject.Lot = o.Lot;
                levelObject.ObjectId = objectId++;
                var position = Utilities.ToNative(o.transform.position);
                //position.Z *= -1;
                //position.X += 3.2f / 4;
                levelObject.Position = position;
                levelObject.Rotation = Utilities.ToNative(o.transform.rotation);
                levelObject.Scale = o.transform.localScale.x;
                levelObject.LegoInfo = o.GetDataDictionary();
                
                objectScale = o.transform.localScale;
                objectScale.z *= -1;
                o.transform.localScale = objectScale;

                return levelObject;
            }).ToArray();

            scale = gameObject.transform.localScale;
            scale.z *= -1;
            gameObject.transform.localScale = scale;
            
            var normalizedLvlName = scene.SceneName.ToLower().Replace(" ", "_");

            using var lvlStream = File.Create(Path.Combine(resultPath, $"{nameNormalized}_{normalizedLvlName}.lvl"));
            using var lvlWriter = new BitWriter(lvlStream);

            lvl.Serialize(lvlWriter);
        }

        using var luzStream = File.Create(Path.Combine(resultPath, $"{nameNormalized}.luz"));
        using var luzWriter = new BitWriter(luzStream);

        luz.Serialize(luzWriter);

        if (!WorkspaceControl.Ok)
        {
            Debug.LogError("Workspace not loaded, could not insert zone into database.");

            return;
        }

        var zoneTable = WorkspaceControl.Database["ZoneTable"];

        var zoneEntry = new ZoneTableTable(zoneTable.FirstOrDefault(z => z.Key == (int) _zoneId) ?? zoneTable.Create(_zoneId));

        zoneEntry.zoneName = Path.Combine("death_blow/", nameNormalized + "/", $"{nameNormalized}.luz");
        zoneEntry.ghostdistance_min = _ghostDistanceMin;
        zoneEntry.ghostdistance = _ghostDistanceMax;
        zoneEntry.population_soft_cap = _populationSoftCap;
        zoneEntry.population_hard_cap = _populationHardCap;
        zoneEntry.fZoneWeight = 1.0f;
        zoneEntry.widthInChunks = terrain.Source.Weight;
        zoneEntry.heightInChunks = terrain.Source.Height;
        zoneEntry.petsAllowed = true;
        zoneEntry.mountsAllowed = true;
        zoneEntry.PlayerLoseCoinsOnDeath = true;
        zoneEntry.disableSaveLoc = true;
        zoneEntry.scriptID = -1;

        Debug.Log("Finished export.");
    }
}
