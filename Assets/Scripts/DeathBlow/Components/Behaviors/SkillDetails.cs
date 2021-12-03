using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace DeathBlow.Components.Behaviors
{
    [CustomEditor(typeof(SkillDetails))]
    public class SkillDetailsEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            if (!WorkspaceControl.Ok)
            {
                GUILayout.Label("Workspace not loaded.");
                
                return;
            }
            
            var skillDetails = (SkillDetails) serializedObject.targetObject;
            
            if (GUILayout.Button("Generate ID"))
            {
                skillDetails.SkillId = WorkspaceControl.Database["SkillBehavior"].ClaimKey(1000);
            }
            
            base.OnInspectorGUI();

            if (skillDetails.Start != null && GUILayout.Button("Save"))
            {
                skillDetails.Save();
            }
        }
    }
    
    public class SkillDetails : MonoBehaviour
    {
        [SerializeField] private int _skillId;

        [SerializeField] private BehaviorDetails _start;

        [SerializeField] private int _imaginationCost;

        [SerializeField] private float _cooldown;

        [SerializeField] private int _skillIcon;

        [SerializeField] private int _cooldownGroup;

        public int SkillId
        {
            get => _skillId;
            set => _skillId = value;
        }

        public BehaviorDetails Start
        {
            get => _start;
            set => _start = value;
        }

        public int ImaginationCost
        {
            get => _imaginationCost;
            set => _imaginationCost = value;
        }

        public float Cooldown
        {
            get => _cooldown;
            set => _cooldown = value;
        }

        public int SkillIcon
        {
            get => _skillIcon;
            set => _skillIcon = value;
        }

        public int CooldownGroup
        {
            get => _cooldownGroup;
            set => _cooldownGroup = value;
        }

        public void Save()
        {
            /*
            var skillBehaviorTable = WorkspaceControl.Database["SkillBehavior"];
            var behaviorParameterTable = WorkspaceControl.Database["BehaviorParameter"];

            var skillBehavior = skillBehaviorTable.FirstOrDefault(
                            s => s.Key == _skillId
            );

            if (skillBehavior != null)
            {
                skillBehaviorTable.Remove(skillBehavior);
            }

            skillBehavior = skillBehaviorTable.Create(_skillId);
            
            var skillBehaviorContext = new SkillBehaviorTable(skillBehavior);

            skillBehaviorContext.cooldown = _cooldown;
            skillBehaviorContext.cooldowngroup = _cooldownGroup;
            skillBehaviorContext.imaginationcost = _imaginationCost;
            skillBehaviorContext.skillIcon = _skillIcon;
            
            var behaviorIdTable = new Dictionary<BehaviorDetails, int>();
            
            SaveIDs(behaviorIdTable, _start);

            skillBehaviorContext.behaviorID = behaviorIdTable[_start];

            foreach (var behaviorPair in behaviorIdTable)
            {
                var behavior = behaviorPair.Key;
                var behaviorId = behaviorPair.Value;

                foreach (var parameter in behavior.Parameters)
                {
                    var behaviorParameter = behaviorParameterTable.Create(behaviorId);
                    
                    var behaviorParameterContext = new BehaviorParameterTable(behaviorParameter);

                    behaviorParameterContext.parameterID = parameter.Name;

                    if (parameter.Action != null)
                    {
                        behaviorParameterContext.value = behaviorIdTable[parameter.Action];
                    }
                    else
                    {
                        behaviorParameterContext.value = parameter.Value;
                    }
                }
            }
            */
        }

        public void SaveIDs(Dictionary<BehaviorDetails, int> map, BehaviorDetails start)
        {
            /*
            if (map.ContainsKey(start))
            {
                return;
            }

            var behaviorTemplateTable = WorkspaceControl.Database["BehaviorTemplate"];

            var behaviorTemplate = behaviorTemplateTable.Create();

            map[start] = behaviorTemplate.Key;
            
            var behaviorTemplateContext = new BehaviorTemplateTable(behaviorTemplate);

            behaviorTemplateContext.effectID = start.EffectId;
            behaviorTemplateContext.effectHandle = start.EffectHandle;
            behaviorTemplateContext.templateID = (int) start.Template;

            foreach (var parameter in start.Parameters.Where(parameter => parameter.Action != null))
            {
                SaveIDs(map, parameter.Action);
            }
            */
        }
    }
}