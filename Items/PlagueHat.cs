using System.Collections.ObjectModel;
using System.Collections.Generic;
using R2API;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;
using TILER2;
using SupplyDrop.Utils;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using System.Linq;
using static TILER2.StatHooks;

namespace SupplyDrop.Items
{
    class PlagueHat : Item_V2<PlagueHat>
    {
        public override string displayName => "Vintage Plague Hat";
        public override ItemTier itemTier => ItemTier.Tier2;
        public override ReadOnlyCollection<ItemTag> itemTags => new ReadOnlyCollection<ItemTag>(new[] { ItemTag.Healing });
        protected override string GetNameString(string langid = null) => displayName;
        protected override string GetPickupString(string langID = null) => "Gain HP the more utility items you have.";
        protected override string GetDescString(string langID = null) => "Increase maximum <style=cIsHealing>HP</style> by <style=cIsHealing>2%</style> " +
            "<style=cStack>(+2% per stack)</style> for every <style=cIsUtility>utility item</style> you possess.";

        protected override string GetLoreString(string landID = null) => "Uh oh, no lore here. Try again later.";

        private static List<CharacterBody> Playername = new List<CharacterBody>();
        public static GameObject ItemBodyModelPrefab;
        private ItemIndex[] indiciiToCheck;
        Dictionary<NetworkInstanceId, int> UtilityItemCounts = new Dictionary<NetworkInstanceId, int>();

        public PlagueHat()
        {
            modelResourcePath = "@SupplyDrop:Assets/Main/Models/Prefabs/PlagueHat.prefab";
            iconResourcePath = "@SupplyDrop:Assets/Main/Textures/Icons/TestIcon.png";
        }

        public override void SetupAttributes()
        {
            if (ItemBodyModelPrefab == null)
            {
                ItemBodyModelPrefab = Resources.Load<GameObject>(modelResourcePath);
                displayRules = GenerateItemDisplayRules();
            }

            base.SetupAttributes();
        }

        private static ItemDisplayRuleDict GenerateItemDisplayRules()
        {
            ItemBodyModelPrefab.AddComponent<ItemDisplay>();
            ItemBodyModelPrefab.GetComponent<ItemDisplay>().rendererInfos = ItemHelpers.ItemDisplaySetup(ItemBodyModelPrefab);

            ItemDisplayRuleDict rules = new ItemDisplayRuleDict(new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(-0.175f, -1.224f, 0.05f),
                    localAngles = new Vector3(0f, 180f, 0f),
                    localScale = new Vector3(.015f, .015f, .015f)

        }
            });
            rules.Add("mdlHuntress", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0f, 0.05f, -0.2f),
                    localAngles = new Vector3(-138f, 0f, 0f),
                    localScale = new Vector3(0.125f, 0.125f, 0.125f)
        }
            });
            rules.Add("mdlToolbot", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(2.37f, 2.3f, -0.4f),
                    localAngles = new Vector3(-30f, 90f, 180f),
                    localScale = new Vector3(2f, 2f, 2f)
                }
            });
            rules.Add("mdlEngi", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0f, 0f, -0.3f),
                    localAngles = new Vector3(-138f, 0f, 0f),
                    localScale = new Vector3(.2f, .2f, .2f)
                }
            });
            rules.Add("mdlMage", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(-0.02f, -0.05f, -0.23f),
                    localAngles = new Vector3(-138f, 0f, 0f),
                    localScale = new Vector3(0.14f, 0.14f, 0.14f)
                }
            });
            rules.Add("mdlMerc", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0.025f, 0.15f, -0.28f),
                    localAngles = new Vector3(-149f, 0f, 0f),
                    localScale = new Vector3(0.17f, 0.17f, 0.17f)
                }
            });
            rules.Add("mdlTreebot", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "FlowerBase",
                    localPos = new Vector3(0f, -1f, -0.9f),
                    localAngles = new Vector3(-138f, 0f, 0f),
                    localScale = new Vector3(0.5f, 0.5f, 0.5f)
                }
            });
            rules.Add("mdlLoader", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(-0.02f, 0.19f, -0.285f),
                    localAngles = new Vector3(-149f, 0f, 0f),
                    localScale = new Vector3(.2f, .2f, .2f)
                }
            });
            rules.Add("mdlCroco", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0f, 0.7f, -3f),
                    localAngles = new Vector3(-138f, 0f, 0f),
                    localScale = new Vector3(2f, 2f, 2f)
                }
            });
            rules.Add("mdlCaptain", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0f, -0.1f, -0.28f),
                    localAngles = new Vector3(-138f, 0f, 0f),
                    localScale = new Vector3(0.15f, 0.15f, 0.15f)
                }
            });
            return rules;
        }

        public override void Install()
        {
            base.Install();

            itemDef.pickupModelPrefab.transform.localPosition = new Vector3(0f, -2f, 0f);

            On.RoR2.Run.Start += UtilityItemListCreator;
            On.RoR2.CharacterBody.OnInventoryChanged += GetTotalUtilityItems;
            GetStatCoefficients += GainBonusHP;
        }

        public override void Uninstall()
        {
            base.Uninstall();

            On.RoR2.Run.Start -= UtilityItemListCreator;
            On.RoR2.CharacterBody.OnInventoryChanged -= GetTotalUtilityItems;
            GetStatCoefficients -= GainBonusHP;
        }

        private void UtilityItemListCreator(On.RoR2.Run.orig_Start orig, Run self)
        //This creates a list of all utility items. May need to be moved to a separate class if multiple items need to access this list
        {
            orig(self);
            indiciiToCheck = ItemCatalog.allItems.Where(x => ItemCatalog.GetItemDef(x).ContainsTag(ItemTag.Utility)).ToArray();
            Debug.Log("Item List Method has been run and a Utility Item List has been created");
            Debug.Log(indiciiToCheck.Length);
        }

        private void GetTotalUtilityItems(On.RoR2.CharacterBody.orig_OnInventoryChanged orig, CharacterBody self)
        //This compares your inventory to the utilitye item list each time your inventory changes, and generates the appropriate value for damageItemCount
        {
            orig(self);
            var utilityItemCount = 0;
            foreach (ItemIndex x in indiciiToCheck)
            {
                utilityItemCount += self.inventory.GetItemCount(x);
            }
            UtilityItemCounts[self.netId] = utilityItemCount;
        }
        private void GainBonusHP(CharacterBody sender, StatHookEventArgs args)
        //This calculates the bonus HP
        {
            if (GetCount(sender) > 0 && UtilityItemCounts.ContainsKey(sender.netId))
            {
                args.healthMultAdd += GetCount(sender) * UtilityItemCounts[sender.netId] * .02f;
            }
        }
    }
}