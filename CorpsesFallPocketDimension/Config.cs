#if EXILED

using Exiled.API.Interfaces;

#endif

using System.ComponentModel;

namespace CorpsesFallPocketDimension
{
#if EXILED
    public class Config : IConfig
    {
        public bool IsEnabled { get; set; } = true;
        public bool Debug { get; set; }

        [Description("The chance for a corpse to be dropped instead of an item. THIS IS A FLOAT, 0.5 MEANS 50%, 0.1 MEANS 10%, 1 MEANS 100%")]
        public float ChanceToDropCorpse { get; set; } = 0.5f;
    }
#elif LABAPI

    public class Config
    {
        [Description("The chance for a corpse to be dropped instead of an item. THIS IS A FLOAT, 0.5 MEANS 50%, 0.1 MEANS 10%, 1 MEANS 100%")]
        public float ChanceToDropCorpse { get; set; } = 0.5f;
    }

#endif
}