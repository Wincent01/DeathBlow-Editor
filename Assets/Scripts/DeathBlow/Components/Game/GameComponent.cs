using System.Reflection;
 
using UnityEngine;

namespace DeathBlow.Components.Game
{
    [RequireComponent(typeof(GameTemplate))]
    public class GameComponent : MonoBehaviour
    {
        public int Lot => GetComponent<GameTemplate>().Lot;
        
        public virtual void OnLoad()
        {
        }

        public virtual void Save()
        {
            /*
            var componentAttribute = GetType().GetCustomAttribute<GameComponentAttribute>();
            
            if (componentAttribute == null) return;

            var lwoComponent = lwo.Add(componentAttribute.ComponentId, componentAttribute.Table);
            
            if (lwoComponent.Row == null) return;
            
            foreach (var field in GetType().GetFields())
            {
                var attribute = field.GetCustomAttribute<LoadFieldAttribute>();
                
                if (attribute == null) continue;

                lwoComponent.Row[attribute.Name].Value = field.GetValue(this);
            }
            */
        }

        public virtual void OnDetailGizmos(ObjectDetails details)
        {
            
        }
        
        public virtual void OnDetailGUI(ObjectDetails details)
        {

        }

        public T GetOrAddComponent<T>() where T : Component
        {
            var component = GetComponent<T>();

            if (component == null) component = gameObject.AddComponent<T>();

            return component;
        }
    }
}