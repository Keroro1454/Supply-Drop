﻿using BepInEx;
using R2API;
using R2API.Utils;
using System.Reflection;
using UnityEngine;
using BepInEx.Configuration;
using Path = System.IO.Path;
using TILER2;
using static TILER2.MiscUtil;
using RoR2;

namespace K1454.SupplyDrop
{
    [BepInPlugin(ModGuid, ModName, ModVer)]
    [BepInDependency(R2API.R2API.PluginGUID, R2API.R2API.PluginVersion)]
    [BepInDependency(TILER2Plugin.ModGuid, TILER2Plugin.ModVer)]
    [R2APISubmoduleDependency(nameof(ItemAPI), nameof(LanguageAPI), nameof(ResourcesAPI), nameof(PlayerAPI), nameof(PrefabAPI))]
    public class SupplyDropPlugin : BaseUnityPlugin
    {
        public const string ModVer = "1.2.2";
        public const string ModName = "Supply Drop";
        public const string ModGuid = "com.K1454.SupplyDrop";

        internal static FilingDictionary<CatalogBoilerplate> masterItemList = new FilingDictionary<CatalogBoilerplate>();

        internal static BepInEx.Logging.ManualLogSource _logger;
        private static ConfigFile ConfigFile;

        private void Awake()
        {
            _logger = Logger;

            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("SupplyDrop.supplydrop_assets"))
            {
                var bundle = AssetBundle.LoadFromStream(stream);
                var provider = new AssetBundleResourcesProvider("@SupplyDrop", bundle);
                ResourcesAPI.AddProvider(provider);
            }
            ConfigFile = new ConfigFile(Path.Combine(Paths.ConfigPath, ModGuid + ".cfg"), true);

            masterItemList = T2Module.InitAll<CatalogBoilerplate>(new T2Module.ModInfo
            {
                displayName = "Supply Drop",
                longIdentifier = "SUPPLYDROP",
                shortIdentifier = "SUPPDRP",
                mainConfigFile = ConfigFile
            });

            T2Module.SetupAll_PluginAwake(masterItemList);
        }
        private void Start()
        {
            T2Module.SetupAll_PluginStart(masterItemList);
            CatalogBoilerplate.ConsoleDump(Logger, masterItemList);
        }
    }
}
