﻿using BepInEx;
using R2API;
using R2API.Utils;
using System.Reflection;
using UnityEngine;
using BepInEx.Configuration;
using Path = System.IO.Path;
using TILER2;
using static TILER2.MiscUtil;

namespace K1454.SupplyDrop
{
    [BepInPlugin(ModGuid, ModName, ModVer)]
    [BepInDependency(R2API.R2API.PluginGUID, R2API.R2API.PluginVersion)]
    [BepInDependency(TILER2Plugin.ModGuid, TILER2Plugin.ModVer)]
    [R2APISubmoduleDependency(nameof(ItemAPI), nameof(BuffAPI), nameof(LanguageAPI), nameof(ResourcesAPI),
                              nameof(PrefabAPI), nameof(SoundAPI), nameof(OrbAPI), nameof(EffectAPI), nameof(DirectorAPI), nameof(ProjectileAPI), nameof(ArtifactAPI), nameof(RecalculateStatsAPI))]
    public class SupplyDropPlugin : BaseUnityPlugin
    {
        public const string ModVer = "1.5.0";
        public const string ModName = "Supply Drop";
        public const string ModGuid = "com.K1454.SupplyDrop";

        internal static FilingDictionary<CatalogBoilerplate> masterItemList = new FilingDictionary<CatalogBoilerplate>();

        internal static BepInEx.Logging.ManualLogSource _logger;


        public static AssetBundle MainAssets;
        private static ConfigFile ConfigFile;

        private void Awake()
        {
            _logger = Logger;

            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("SupplyDrop.supplydrop_assets"))
            {
                MainAssets = AssetBundle.LoadFromStream(stream);
            }
            ConfigFile = new ConfigFile(Path.Combine(Paths.ConfigPath, ModGuid + ".cfg"), true);

            masterItemList = T2Module.InitAll<CatalogBoilerplate>(new T2Module.ModInfo
            {
                displayName = "Supply Drop",
                longIdentifier = "SUPPLYDROP",
                shortIdentifier = "SUPPDRP",
                mainConfigFile = ConfigFile
            });

            using (var bankStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("SupplyDrop.SupplyDropSounds.bnk"))
            {
                var bytes = new byte[bankStream.Length];
                bankStream.Read(bytes, 0, bytes.Length);
                SoundAPI.SoundBanks.Add(bytes);
            }

            T2Module.SetupAll_PluginAwake(masterItemList);
            T2Module.SetupAll_PluginStart(masterItemList);
        }
        private void Start()
        {
            
            CatalogBoilerplate.ConsoleDump(Logger, masterItemList);
        }
    }
}
