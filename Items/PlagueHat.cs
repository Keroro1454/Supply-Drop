using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using R2API;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;
using static R2API.RecalculateStatsAPI;

using SupplyDrop.Utils;
using static SupplyDrop.Utils.ItemHelpers;
using static K1454.SupplyDrop.SupplyDropPlugin;

using BepInEx.Configuration;


namespace SupplyDrop.Items
{
    public class PlagueHat : ItemBase<PlagueHat>
    {
        //Config Stuff

        public static ConfigOption<float> baseStackHPPercent;
        public static ConfigOption<float> addStackHPPercent;

        //Item Data

        public override string ItemName => "Vintage Plague Hat";

        public override string ItemLangTokenName => "PLAGUE_HAT";

        public override string ItemPickupDesc => "Gain <style=cIsHealing>HP</style> the more <style=cIsUtility>utility items</style> you have.";

        public override string ItemFullDescription => "Increase your <style=cIsHealing>health permanently</style> by <style=cIsHealing>1%</style> " +
            "<style=cStack>(+1% per stack)</style> for every <style=cIsUtility>utility item</style> you possess.";

        public override string ItemLore => "Order: \"NR-G Sports Soda (49)\"\nTracking Number: 49******\n" +
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

        public override ItemTier Tier => ItemTier.Tier2;
        public override ItemTag[] ItemTags => new ItemTag[] { ItemTag.Healing, ItemTag.AIBlacklist };


        public override GameObject ItemModel => MainAssets.LoadAsset<GameObject>("PlagueHat.prefab");
        public override Sprite ItemIcon => MainAssets.LoadAsset<Sprite>("PlagueHatIcon");


        public static GameObject ItemBodyModelPrefab;

        private ItemIndex[] indiciiToCheck;
        Dictionary<NetworkInstanceId, int> UtilityItemCounts = new Dictionary<NetworkInstanceId, int>();

        public override void Init(ConfigFile config)
        {
            CreateConfig(config);
            CreateLang();
            CreateItem();
            Hooks();
        }

        private void CreateConfig(ConfigFile config)
        {
            baseStackHPPercent = config.ActiveBind<float>("Item: " + ItemName, "Base HP Gained for Each Utility Item with 1 Vintage Plague Hat", .01f, "How much HP, as a % of max HP, should you gain with a single vintage plague hat? (.01 = 1%)");
            addStackHPPercent = config.ActiveBind<float>("Item: " + ItemName, "Additional HP Gained for Each Utility Item per Vintage Plague Hat", .01f, "How much additional HP, as a % of max HP, should each vintage plague hat after the first give?");
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            ItemBodyModelPrefab = ItemModel;
            var itemDisplay = ItemBodyModelPrefab.AddComponent<RoR2.ItemDisplay>();
            itemDisplay.rendererInfos = ItemDisplaySetup(ItemBodyModelPrefab);

            ItemDisplayRuleDict rules = new ItemDisplayRuleDict(new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0, 0, 0),
                    localAngles = new Vector3(0, 0, 0),
                    localScale = new Vector3(1, 1, 1)
                }
            });
            rules.Add("mdlCommandoDualies", new ItemDisplayRule[]
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
            rules.Add("mdlBandit2", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0.00841F, 0.20958F, -0.0078F),
                    localAngles = new Vector3(358.3102F, 185.285F, 4.45487F),
                    localScale = new Vector3(0.17303F, 0.17303F, 0.17303F)
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
            rules.Add("mdlHeretic", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(-0.24177F, 0.02394F, -0.04916F),
                    localAngles = new Vector3(47.29686F, 7.17607F, 118.0969F),
                    localScale = new Vector3(0.2196F, 0.2196F, 0.2196F)
                }
            });
            rules.Add("mdlRailGunner", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0.00752F, 0.19722F, -0.00225F),
                    localAngles = new Vector3(357.2906F, 357.6486F, 350.4565F),
                    localScale = new Vector3(0.14173F, 0.14173F, 0.1406F)
                }
            });
            rules.Add("mdlVoidSurvivor", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0.03852F, 0.15149F, -0.07248F),
                    localAngles = new Vector3(328.2166F, 343.6819F, 1.48388F),
                    localScale = new Vector3(0.18024F, 0.18024F, 0.18024F)
                }
            });
            return rules;
        }
        public override void Hooks()
        {
            On.RoR2.Run.Start += UtilityItemListCreator;
            On.RoR2.CharacterBody.OnInventoryChanged += GetTotalUtilityItems;
            GetStatCoefficients += GainBonusHP;
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
                args.healthMultAdd += (UtilityItemCounts[sender.netId] * (baseStackHPPercent + ((inventoryCount - 1) * addStackHPPercent)));
            }
        }
    }
}