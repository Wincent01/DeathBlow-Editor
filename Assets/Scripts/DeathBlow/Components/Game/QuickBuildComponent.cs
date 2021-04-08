using DeathBlow.Components.Editors;
using InfectedRose.Database.Concepts;
using UnityEditor;
using UnityEngine;

namespace DeathBlow.Components.Game
{
    [GameComponent(ComponentId.QuickBuildComponent, "RebuildComponent")]
    public class QuickBuildComponent : GameComponent
    {
        [SerializeField]
        [LoadField("activityID")]
        public int _ActivityID;

        [SerializeField]
        [LoadField("complete_time")]
        public float _CompleteTime;

        [SerializeField]
        [LoadField("interruptible")]
        public bool _Interruptible;

        [SerializeField]
        [LoadField("reset_time")]
        public float _ResetTime;

        [SerializeField]
        [LoadField("self_activator")]
        public bool _SelfActivator;

        [SerializeField]
        [LoadField("take_imagination")]
        public int _TakeImagination;

        [SerializeField]
        [LoadField("post_imagination_cost")]
        public int _PostImaginationCost;

        [SerializeField]
        [LoadField("time_before_smash")]
        public float _TimeBeforeSmash;

        [SerializeField]
        [LoadField("custom_modules")]
        public string _CustomModules;
    }
}