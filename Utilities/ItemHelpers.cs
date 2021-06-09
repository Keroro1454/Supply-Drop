using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static TILER2.StatHooks;

namespace SupplyDrop.Utils
{
    public class ItemHelpers
    {
        public static CharacterModel.RendererInfo[] ItemDisplaySetup(GameObject obj)
        {
            MeshRenderer[] meshes = obj.GetComponentsInChildren<MeshRenderer>();
            CharacterModel.RendererInfo[] renderInfos = new CharacterModel.RendererInfo[meshes.Length];

            for (int i = 0; i < meshes.Length; i++)
            {
                renderInfos[i] = new CharacterModel.RendererInfo
                {
                    defaultMaterial = meshes[i].material,
                    renderer = meshes[i],
                    defaultShadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On,
                    ignoreOverlays = false
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