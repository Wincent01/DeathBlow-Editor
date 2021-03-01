using System;
using System.Collections.Generic;
using System.Linq;
using InfectedRose.Database.Concepts;
using InfectedRose.Database.Generic;
using UnityEngine;

namespace DeathBlow.Components.Game
{
    [Serializable]
    public struct ObjectSkill
    {
        [SerializeField]
        public int _skillId;

        [SerializeField]
        public int _castOnType;

        [SerializeField]
        public int _aiCombatWeight;
    }
    
    [GameComponent(ComponentId.SkillComponent)]
    public class SkillComponent : GameComponent
    {
        [SerializeField]
        public List<ObjectSkill> _skills;
        
        public override void OnLoad()
        {
            base.OnLoad();
            
            _skills = new List<ObjectSkill>();

            var skills = WorkspaceControl.Database["ObjectSkills"].Where(s => s.Key == Lot);

            foreach (var skill in skills)
            {
                _skills.Add(new ObjectSkill
                {
                    _skillId = skill.Value<int>("skillID"),
                    _castOnType = skill.Value<int>("castOnType"),
                    _aiCombatWeight = skill.Value<int>("AICombatWeight")
                });
            }
        }

        public override void Save(LwoObject lwo)
        {
            base.Save(lwo);

            var skillsTable = WorkspaceControl.Database["ObjectSkills"];
            
            var skills = skillsTable.Where(s => s.Key == Lot);

            foreach (var skill in skills.ToArray())
            {
                skillsTable.Remove(skill);
            }

            foreach (var skill in _skills)
            {
                var entry = skillsTable.Create(Lot);

                entry["skillID"].Value = skill._skillId;
                entry["castOnType"].Value = skill._castOnType;
                entry["AICombatWeight"].Value = skill._aiCombatWeight;
            }
        }
    }
}