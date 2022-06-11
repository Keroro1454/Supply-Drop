using BepInEx;
using R2API;
using R2API.Utils;
using System.Reflection;
using UnityEngine;
using BepInEx.Configuration;
using Path = System.IO.Path;
using System.Collections.Generic;

namespace K1454.SupplyDrop
{
    [BepInPlugin(ModGuid, ModName, ModVer)]
    [BepInDependency(R2API.R2API.PluginGUID, R2API.R2API.PluginVersion)]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.EveryoneNeedSameModVersion)]
    [R2APISubmoduleDependency(nameof(ItemAPI), nameof(LanguageAPI), nameof(PrefabAPI), nameof(RecalculateStatsAPI), nameof(DirectorAPI), nameof(DeployableAPI), nameof(DamageAPI), nameof(SoundAPI), nameof(OrbAPI))]
    public class SupplyDropPlugin : BaseUnityPlugin
    {
        public const string ModVer = "1.4.10";
        public const string ModName = "Supply Drop";
        public const string ModGuid = "com.K1454.SupplyDrop";

        internal static BepInEx.Logging.ManualLogSource ModLogger;

        public static AssetBundle MainAssets;
        private static ConfigFile ConfigFile;

        public static Dictionary<string, string> ShaderLookup = new Dictionary<string, string>()
        {
            {"fake ror/hopoo games/deferred/hgstandard", "shaders/deferred/hgstandard"},
            {"fake ror/hopoo games/fx/hgcloud intersection remap", "shaders/fx/hgintersectioncloudremap" },
            {"fake ror/hopoo games/fx/hgcloud remap", "shaders/fx/hgcloudremap" },
            {"fake ror/hopoo games/fx/hgdistortion", "shaders/fx/hgdistortion" },
            {"fake ror/hopoo games/deferred/hgsnow topped", "shaders/deferred/hgsnowtopped" },
            {"fake ror/hopoo games/fx/hgsolid parallax", "shaders/fx/hgsolidparallax" }
        };

        private void Awake()
        {
            ModLogger = Logger;

            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("SupplyDrop.supplydrop_assets"))
            {
                MainAssets = AssetBundle.LoadFromStream(stream);
            }


            ConfigFile = new ConfigFile(Path.Combine(Paths.ConfigPath, ModGuid + ".cfg"), true);

            //using (var bankStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("SupplyDrop.SupplyDropSounds.bnk"))
            //{
            //    var bytes = new byte[bankStream.Length];
            //    bankStream.Read(bytes, 0, bytes.Length);
            //    SoundAPI.SoundBanks.Add(bytes);
            //}
        }
    }
}
