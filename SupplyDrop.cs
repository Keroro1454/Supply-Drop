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
        public const string ModVer = "1.0.0";
        public const string ModName = "Supply Drop";
        public const string ModGuid = "com.K1454.SupplyDrop";

        internal static FilingDictionary<ItemBoilerplate> masterItemList = new FilingDictionary<ItemBoilerplate>();

        internal static BepInEx.Logging.ManualLogSource _logger;
        private static ConfigFile ConfigFile;

        private void Awake()
        {
            _logger = Logger;

            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("SupplyDrop.supplydrop"))
            {
                var bundle = AssetBundle.LoadFromStream(stream);
                var provider = new AssetBundleResourcesProvider("@SupplyDrop", bundle);
                ResourcesAPI.AddProvider(provider);
            }
            ConfigFile = new ConfigFile(Path.Combine(Paths.ConfigPath, ModGuid + ".cfg"), true);

            masterItemList = ItemBoilerplate.InitAll("SupplyDrop");

            foreach (ItemBoilerplate x in masterItemList)
            {
                x.SetupConfig(ConfigFile);
            }

            int longestName = 0;
            foreach (ItemBoilerplate x in masterItemList)
            {
                x.SetupAttributes("SUPPLYDROP", "SUPPDRP");
                if (x.itemCodeName.Length > longestName) longestName = x.itemCodeName.Length;
            }

            Logger.LogMessage("Index dump follows (pairs of name / index):");
            foreach (ItemBoilerplate x in masterItemList)
            {
                if (x is Equipment eqp)
                    Logger.LogMessage("Equipment SUPPDRP" + x.itemCodeName.PadRight(longestName) + "/" + ((int)eqp.regIndex).ToString());
                else if (x is Item item)
                    Logger.LogMessage("     Item SUPPDRP" + x.itemCodeName.PadRight(longestName) + " / " + ((int)item.regIndex).ToString());
                else if (x is Artifact afct)
                    Logger.LogMessage(" Artifact SUPPDRP" + x.itemCodeName.PadRight(longestName) + " / " + ((int)afct.regIndex).ToString());
                else
                    Logger.LogMessage("    Other SUPPDRP" + x.itemCodeName.PadRight(longestName) + " / N/A");
            }

            foreach (ItemBoilerplate x in masterItemList)
            {
                x.SetupBehavior();
            }
        }

        public static CharacterModel.RendererInfo[] ItemDisplaySetup(GameObject obj)
        {
            MeshRenderer[] meshes = obj.GetComponentsInChildren<MeshRenderer>(); //We find the array of renderers for our meshes in the model (GameObject obj) and put them in an array.
            CharacterModel.RendererInfo[] renderInfos = new CharacterModel.RendererInfo[meshes.Length]; //We create an array for the render infos of the size of our mesh renderer array.

            for (int i = 0; i < meshes.Length; i++)
            {
                renderInfos[i] = new CharacterModel.RendererInfo //For each spot in our renderInfos array, we create a new render info for it.
                {
                    defaultMaterial = meshes[i].material, //we retrieve the material that the mesh uses.
                    renderer = meshes[i], //we retrieve the MeshRenderer.
                    defaultShadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On, //We allow our mesh to cast shadows or not.
                    ignoreOverlays = false //We allow the mesh to be affected by overlays like OnFire or PredatoryInstinctsCritOverlay.
                };
            }


            return renderInfos;
        }
    }
}
