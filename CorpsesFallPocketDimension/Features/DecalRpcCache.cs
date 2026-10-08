using Decals;
using InventorySystem.Items.Autosync;
using InventorySystem.Items.Firearms.Modules;
using RelativePositioning;
using System.Collections;
using System.Reflection;
using UnityEngine;

namespace CorpsesFallPocketDimension.Features
{
    //credit to dr.dinspit on discord for this entire code
    //& github.com/icedchai/DarkAndGrittyGunplay
    internal static class DecalRpcCache
    {
        private static bool _resolved;
        private static ModularAutosyncItem? _template;
        private static Enum? _impactDecalRpc;

        public static bool TryResolve(out ModularAutosyncItem template, out Enum impactDecalRpc)
        {
            if (!_resolved)
                ResolveOnce();
            template = _template!;
            impactDecalRpc = _impactDecalRpc!;
            return _template != null && _impactDecalRpc != null;
        }

        private static void ResolveOnce()
        {
            _resolved = true;
            try
            {
                Type t = typeof(ModularAutosyncItem);
                object? templatesVal = null;
                foreach (string name in new[] { "AllTemplates", "_allTemplates" })
                {
                    PropertyInfo? prop = t.GetProperty(name, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                    if (prop != null)
                    {
                        templatesVal = prop.GetValue(null);
                        break;
                    }
                    FieldInfo? field = t.GetField(name, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                    if (field != null)
                    {
                        templatesVal = field.GetValue(null);
                        break;
                    }
                }

                if (templatesVal is ModularAutosyncItem[] arr && arr.Length > 0)
                    _template = arr[0];
                else if (templatesVal is IEnumerable en)
                {
                    foreach (object? o in en)
                    {
                        if (o is ModularAutosyncItem m)
                        {
                            _template = m;
                            break;
                        }
                    }
                }

                Type? rpcType = typeof(ImpactEffectsModule).GetNestedType("RpcType", BindingFlags.Public | BindingFlags.NonPublic);
                if (rpcType != null && rpcType.IsEnum)
                    _impactDecalRpc = (Enum)Enum.Parse(rpcType, "ImpactDecal");
            }
            catch
            {
                _template = null;
            }
        }

        public static void PlaceBlood(Vector3 position, Vector3 direction)
        {
            if (direction == Vector3.zero)
                direction = Vector3.down;

            try
            {
                Vector3 normalizedDirection = direction.normalized;
                Vector3 hitPosition = position + normalizedDirection * 0.1f;
                Vector3 startPosition = position - normalizedDirection * 0.5f;
                SpawnDecal(hitPosition, startPosition, DecalPoolType.Blood);
            }
            catch (Exception ex)
            {
            }
        }

        public static void SpawnDecal(Vector3 position, Vector3 startPosition, DecalPoolType type = DecalPoolType.Blood)
        {
            if (!DecalRpcCache.TryResolve(out ModularAutosyncItem autoItem, out Enum rpcSubheader))
                return;

            RelativePosition hitPoint = new RelativePosition(position);
            RelativePosition startRaycastPoint = new RelativePosition(startPosition);

            for (byte b = 0; b < autoItem.AllSubcomponents.Length; b++)
            {
                if (autoItem.AllSubcomponents[b] is ImpactEffectsModule)
                {
                    using (new AutosyncRpc(autoItem.ItemId, out var writer))
                    {
                        writer.WriteByte(b);
                        writer.WriteSubheader(rpcSubheader);
                        writer.WriteByte((byte)type);
                        writer.WriteRelativePosition(hitPoint);
                        writer.WriteRelativePosition(startRaycastPoint);
                        return;
                    }
                }
            }
        }
    }
}