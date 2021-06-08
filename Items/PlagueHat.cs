using System.Collections.ObjectModel;
using System.Collections.Generic;
using R2API;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;
using TILER2;
using SupplyDrop.Utils;
using System.Linq;
using static TILER2.StatHooks;
using static K1454.SupplyDrop.SupplyDropPlugin;

namespace SupplyDrop.Items
{
    public class PlagueHat : Item<PlagueHat>
    {
        public override string displayName => "Vintage Plague Hat";
        public override ItemTier itemTier => ItemTier.Tier2;
        public override ReadOnlyCollection<ItemTag> itemTags => new ReadOnlyCollection<ItemTag>(new[] { ItemTag.Healing });
        protected override string GetNameString(string langid = null) => displayName;
        protected override string GetPickupString(string langID = null) => "Gain HP the more utility items you have.";
        protected override string GetDescString(string langID = null) => "Increase your <style=cIsHealing>health permanently</style> by <style=cIsHealing>2%</style> " +
            "<style=cStack>(+2% per stack)</style> for every <style=cIsUtility>utility item</style> you possess.";
        protected override string GetLoreString(string landID = null) => "Order: \"NR-G Sports Soda (49)\"\nTracking Number: 49******\n" +
            "Estimated Delivery: 3/01/2056\nShipping Method: Priority\nShipping Address: P.O. Box 749, Sector A, Moon\nShipping Details:" +
            "You are cordially invited to the Order's 49th Annual Induction Gala.\n\n" +
            "Enclosed with this invitation is your unique sigil, encoded coordinates for 'Party' cipher decryption, " +
            "and your mask and headwear selected for your pre-destined role within the Order.\n\n" +
            "Please note that attendance is mandatory. Missing the Sigil Ceremony will be noted, " +
            "and absenteeism will result in you being deemed Unreasonable.\n\n" +
            "Please also note the mask and hat are mandatory attire. Deviation of any kind will result in you being deemed Unreasonable.\n\n" +
            "Cordially,\n" +
            "The Administrator's Right Hand\n\n" +
            "<i>We seek truth and knowledge in the darkness\n" +
            "For on the day we achieve True Knowledge\n" +
            "We shall reveal our faces and rule in the light</i>";

        private static List<CharacterBody> Playername = new List<CharacterBody>();
        public static GameObject ItemBodyModelPrefab;
        private ItemIndex[] indiciiToCheck;
        Dictionary<NetworkInstanceId, int> UtilityItemCounts = new Dictionary<NetworkInstanceId, int>();
        public PlagueHat()
        {
            modelResource = MainAssets.LoadAsset<GameObject>("Main/Models/Prefabs/PlagueHat.prefab");
            iconResource = MainAssets.LoadAsset<Sprite>("Main/Textures/Icons/PlagueHatIcon.png");
        }
        public override void SetupAttributes()
        {
            if (ItemBodyModelPrefab == null)
            {
                ItemBodyModelPrefab = modelResource;
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
                    localPos = new Vector3(0f, 0.385f, 0f),
                    localAngles = new Vector3(0f, 180f, 0f),
                    localScale = new Vector3(.2f, .2f, .2f)
        }
            });
            rules.Add("mdlHuntress", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0f, 0.31f, -0.05f),
                    localAngles = new Vector3(12f, 180f, 0f),
                    localScale = new Vector3(0.175f, 0.175f, 0.175f)
        }
            });
            rules.Add("mdlToolbot", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(-0.5f, 2.45f, 1.7f),
                    localAngles = new Vector3(70f, 0f, 20f),
                    localScale = new Vector3(1.25f, 1.25f, 1.25f)
                }
            });
            rules.Add("mdlEngi", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "HeadCenter",
                    localPos = new Vector3(0f, 0.167f, 0f),
                    localAngles = new Vector3(0f, 180f, 0f),
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
                    localPos = new Vector3(0f, 0.15f, -0.1f),
                    localAngles = new Vector3(30f, 180f, 0f),
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
                    localPos = new Vector3(0f, 0.25f, 0f),
                    localAngles = new Vector3(15f, 180f, 0f),
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
                    localPos = new Vector3(-0.6f, 0.05f, 0.2f),
                    localAngles = new Vector3(0f, 180f, -40f),
                    localScale = new Vector3(0.25f, 0.25f, 0.25f)
                }
            });
            rules.Add("mdlLoader", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0f, 0.23f, 0f),
                    localAngles = new Vector3(15f, 180f, 0f),
                    localScale = new Vector3(.18f, .18f, .18f)
                }
            });
            rules.Add("mdlCroco", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0f, 1f, 1.6f),
                    localAngles = new Vector3(90f, 0f, 0f),
                    localScale = new Vector3(1f, 1f, 1f)
                }
            });
            rules.Add("mdlCaptain", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0f, 0.275f, 0f),
                    localAngles = new Vector3(25f, 180f, 0f),
                    localScale = new Vector3(0.15f, 0.15f, 0.15f)
                }
            });
            rules.Add("mdlBandit", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0f, 0.38f, -0.15f),
                    localAngles = new Vector3(30f, 180f, 0f),
                    localScale = new Vector3(0.2f, 0.2f, 0.2f)
                }
            });
            return rules;
        }
        public override void Install()
        {
            base.Install();

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
        //May need to be moved to a separate class if multiple items need to access this list
        {
            orig(self);
            indiciiToCheck = ItemCatalog.allItems.Where(x => ItemCatalog.GetItemDef(x).ContainsTag(ItemTag.Utility)).ToArray();
            UtilityItemCounts = new Dictionary<NetworkInstanceId, int>();
            Debug.Log("Item List Method has been run and a Utility Item List has been created");
            Debug.Log(indiciiToCheck.Length);
        }
        private void GetTotalUtilityItems(On.RoR2.CharacterBody.orig_OnInventoryChanged orig, CharacterBody self)
        //This compares your inventory to the utility item list each time your inventory changes, and generates the appropriate value for damageItemCount
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
        {
            var inventoryCount = GetCount(sender);
            if (GetCount(sender) > 0 && UtilityItemCounts.ContainsKey(sender.netId))
            {
                args.healthMultAdd += (UtilityItemCounts[sender.netId] * (0.02f + ((inventoryCount - 1) * .02f)));
            }
        }
    }
}