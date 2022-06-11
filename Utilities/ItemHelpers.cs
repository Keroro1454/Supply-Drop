using System.Collections.Generic;
using UnityEngine;
using RoR2;
using static R2API.RecalculateStatsAPI;

namespace SupplyDrop.Utils
{
    internal class ItemHelpers
    {
        /// <summary>
        /// A helper that will set up the RendererInfos of a GameObject that you pass in.
        /// <para>This allows it to go invisible when your character is not visible, as well as letting overlays affect it.</para>
        /// </summary>
        /// <param name="obj">The GameObject/Prefab that you wish to set up RendererInfos for.</param>
        /// <param name="debugmode">Do we attempt to attach a material shader controller instance to meshes in this?</param>
        /// <returns>Returns an array full of RendererInfos for GameObject.</returns>
        public static CharacterModel.RendererInfo[] ItemDisplaySetup(GameObject obj)
        {

            List<Renderer> AllRenderers = new List<Renderer>();

            var meshRenderers = obj.GetComponentsInChildren<MeshRenderer>();
            if (meshRenderers.Length > 0) { AllRenderers.AddRange(meshRenderers); }

            var skinnedMeshRenderers = obj.GetComponentsInChildren<SkinnedMeshRenderer>();
            if (skinnedMeshRenderers.Length > 0) { AllRenderers.AddRange(skinnedMeshRenderers); }

            CharacterModel.RendererInfo[] renderInfos = new CharacterModel.RendererInfo[AllRenderers.Count];

            for (int i = 0; i < AllRenderers.Count; i++)
            {
                renderInfos[i] = new CharacterModel.RendererInfo
                {
                    defaultMaterial = AllRenderers[i] is SkinnedMeshRenderer ? AllRenderers[i].sharedMaterial : AllRenderers[i].material,
                    renderer = AllRenderers[i],
                    defaultShadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On,
                    ignoreOverlays = false //We allow the mesh to be affected by overlays like OnFire or PredatoryInstinctsCritOverlay.
                };
            }

            return renderInfos;
        }

        public static void AddMaxShieldHelper(CharacterBody sender, StatHookEventArgs args, int inventoryCount, float baseStackHPPercent, float addStackHPPercent)
        {
            //if (inventoryCount > 0) //Keroro's preferred behavior.
            //{
            //    if (sender.inventory.GetItemCount(ItemIndex.ShieldOnly) > 0)
            //    {
            //        args.baseShieldAdd += ((sender.maxShield * baseStackHPPercent) + ((sender.maxShield * addStackHPPercent * (inventoryCount - 1))));
            //    }
            //    else
            //    {
            //        args.baseShieldAdd += ((sender.maxHealth * baseStackHPPercent) + ((sender.maxHealth * addStackHPPercent) * (inventoryCount - 1)));
            //    };
            //}


            if (inventoryCount > 0) //Personal Shield Generator behavior.
            {
                if (sender.inventory.GetItemCount(RoR2Content.Items.ShieldOnly) > 0) //Retained the if-else statement to increase compatibility with mods that add HP in unexpected ways when the player does not have Transcendence.
                {
                    //Max health before Transcendence transformation is not stored in any way. It must be recalculated manually. The following code was adapted from CharacterBody.RecalculateStats().

                    float calcHealthMultiplier = 1f //Base multiplier
                        + (float)sender.inventory.GetItemCount(RoR2Content.Items.BoostHp) * 0.1f //BoostHp
                        + (float)(sender.inventory.GetItemCount(RoR2Content.Items.Pearl) + sender.inventory.GetItemCount(RoR2Content.Items.ShinyPearl)) * 0.1f; //Pearls;


                    float calcMaxHealth = ((sender.baseMaxHealth + sender.levelMaxHealth * (sender.level - 1)) //Base max HP
                        + (float)sender.inventory.GetItemCount(RoR2Content.Items.Knurl) * 40f //Knurls
                        + (sender.inventory.GetItemCount(RoR2Content.Items.Infusion) > 0 ? sender.inventory.infusionBonus : 0)) //Infusion
                        * calcHealthMultiplier //Health multiplier - pearls and BoostHp
                        / ((float)sender.inventory.GetItemCount(RoR2Content.Items.CutHp) + 1) //Shaped Glass
                        * (sender.inventory.GetItemCount(RoR2Content.Items.InvadingDoppelganger) > 0 ? 10 : 1); //Check if you're a doppelganger.
                    args.baseShieldAdd += (calcMaxHealth * baseStackHPPercent) + (calcMaxHealth * addStackHPPercent) * (inventoryCount - 1);
                }
                else
                {
                    args.baseShieldAdd += ((sender.maxHealth * baseStackHPPercent) + ((sender.maxHealth * addStackHPPercent) * (inventoryCount - 1)));
                };
            }
        }
    }
}