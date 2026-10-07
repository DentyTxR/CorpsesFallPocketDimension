using Exiled.API.Interfaces;
using System.ComponentModel;

namespace CorpsesFallPocketDimension
{
    public class Config : IConfig
    {
        public bool IsEnabled { get; set; } = true;
        public bool Debug { get; set; }

        [Description("The chance for a corpse to be dropped instead of an item. THIS IS A FLOAT, 0.5 MEANS 50%, 0.1 MEANS 10%, 1 MEANS 100%")]
        public float ChanceToDropCorpse { get; set; } = 0.5f;
    }
}