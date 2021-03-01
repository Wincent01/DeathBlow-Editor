using System;

namespace DeathBlow.Components.Game
{
    [AttributeUsage(AttributeTargets.Field)]
    public class LoadFieldAttribute : Attribute
    {
        public string Name { get; set; }

        public LoadFieldAttribute(string name)
        {
            Name = name;
        }
    }
}