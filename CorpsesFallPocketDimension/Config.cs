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

        [Description("Whether or not to apply the custom sinkhole animation items that drop.")]
        public bool ApplyCustomSinkholeToItems { get; set; } = true;

        [Description("Whether or not to apply the custom sinkhole animation corpses that drop.")]
        public bool ApplyCustomSinkholeToCorpses { get; set; } = true;

        [Description("Delay in seconds for when to play the sinkhole animation when the game picks a drop location.")]
        public float SinkholeDelay { get; set; } = 1f;
    }

#elif LABAPI

    public class Config
    {
        public bool Debug { get; set; } = false;

        [Description("The chance for a corpse to be dropped instead of an item. THIS IS A FLOAT, 0.5 MEANS 50%, 0.1 MEANS 10%, 1 MEANS 100%")]
        public float ChanceToDropCorpse { get; set; } = 0.5f;

        [Description("Whether or not to apply the custom sinkhole animation items that drop.")]
        public bool ApplyCustomSinkholeToItems { get; set; } = true;

        [Description("Whether or not to apply the custom sinkhole animation corpses that drop.")]
        public bool ApplyCustomSinkholeToCorpses { get; set; } = true;

        [Description("Delay in seconds for when to play the sinkhole animation when the game picks a drop location.")]
        public float SinkholeDelay { get; set; } = 1f;
    }

#endif
}