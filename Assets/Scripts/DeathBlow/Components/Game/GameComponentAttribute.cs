using System;
using InfectedRose.Database.Concepts;

namespace DeathBlow.Components.Game
{
    [AttributeUsage(AttributeTargets.Class)]
    public class GameComponentAttribute : Attribute
    {
        public ComponentId ComponentId { get; set; }
        
        public string Table { get; set; }

        public GameComponentAttribute(ComponentId componentId, string table = "")
        {
            ComponentId = componentId;

            Table = table;
        }
    }
}