using System;
using System.Collections.Generic;
using System.Linq;
using InfectedRose.Builder.Behaviors;
using UnityEditor;
using UnityEngine;

namespace DeathBlow.Components.Behaviors
{
    [Serializable]
    public class BehaviorParameter
    {
        [SerializeField] private string _name;

        [SerializeField] private float _value;

        [SerializeField] private BehaviorDetails _action;

        public string Name
        {
            get => _name;
            set => _name = value;
        }

        public float Value
        {
            get => _value;
            set => _value = value;
        }

        public BehaviorDetails Action
        {
            get => _action;
            set => _action = value;
        }
    }

    [CustomEditor(typeof(BehaviorDetails))]
    public class BehaviorDetailsEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
        }
    }
    
    public class BehaviorDetails : MonoBehaviour
    {
        [SerializeField] private Template _template;

        [SerializeField] private int _effectId;

        [SerializeField] private string _effectHandle;
        
        [SerializeField] private List<BehaviorParameter> _parameters = new List<BehaviorParameter>();

        public List<BehaviorParameter> Parameters
        {
            get => _parameters;
            set => _parameters = value;
        }

        public Template Template
        {
            get => _template;
            set => _template = value;
        }

        public int EffectId
        {
            get => _effectId;
            set => _effectId = value;
        }

        public string EffectHandle
        {
            get => _effectHandle;
            set => _effectHandle = value;
        }

        public void OnDrawGizmos()
        {
            Gizmos.color = Color.green;

            foreach (var parameter in _parameters.Where(parameter => parameter.Action != null))
            {
                Gizmos.DrawLine(transform.position, parameter.Action.transform.position);
            }
        }
    }
}