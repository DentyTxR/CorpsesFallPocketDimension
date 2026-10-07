using Exiled.API.Interfaces;
using System.ComponentModel;

namespace SCP106RagdollFallExiled
{
    public class Config : IConfig
    {
        public bool IsEnabled { get; set; } = true;
        public bool Debug { get; set; }
    }
}