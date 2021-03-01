using System;
using System.IO;
using System.Reflection;
using InfectedRose.Nif;
using RakDotNet.IO;
using UnityEngine;

namespace DeathBlow.Components
{
    public class NiPropertyComponent : MonoBehaviour
    {
        [HideInInspector]
        public byte[] data;

        [HideInInspector]
        public string type;
        
        public bool Editing { get; set; }

        public string Notice { get; set; }
        
        public NiProperty Temporary { get; set; }
        
        public void SetProperty(NiProperty property)
        {
            if (property == null)
            {
                return;
            }
            
            type = property.GetType().FullName;

            using var stream = new MemoryStream();
            using var writer = new BitWriter(stream);

            property.Serialize(writer);

            data = stream.ToArray();
        }

        public NiProperty GetProperty()
        {
            var t = Type.GetType(type);

            if (t == null)
            {
                return null;
            }

            var instance = (NiProperty) Activator.CreateInstance(t);
            
            using var stream = new MemoryStream(data);
            using var reader = new BitReader(stream);

            instance.Deserialize(reader);

            return instance;
        }
    }
}