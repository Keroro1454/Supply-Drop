using R2API;
using RoR2;
using UnityEngine;
using static R2API.RecalculateStatsAPI;

using SupplyDrop.Utils;
using static SupplyDrop.Utils.ItemHelpers;
using static SupplyDrop.Utils.MathHelpers;
using static K1454.SupplyDrop.SupplyDropPlugin;

using BepInEx.Configuration;

namespace SupplyDrop.Items
{
    public class SilverWatch : ItemBase<SilverWatch>
    {
        //Config Stuff

        public static ConfigOption<float> baseDurationIncrease;
        public static ConfigOption<float> addDurationIncrease;

        //Item Data

        public override string ItemName => "Silver Pocketwatch";

        public override string ItemLangTokenName => "SILVERWATCH";

        public override string ItemPickupDesc => "<style=cIsUtility>Buffs</style> last longer...but so do <style=cDeath>debuffs</style>.";

        public override string ItemFullDescription => $"All <style=cIsUtility>buff</style> durations are increased by {FloatToPercentageString(baseDurationIncrease)} (+{FloatToPercentageString(addDurationIncrease)} " +
            $"per stack). All <style=cDeath>debuff</style> durations are also increased by {FloatToPercentageString(baseDurationIncrease)} (+{FloatToPercentageString(addDurationIncrease)} " +
            $"per stack).";

        public override string ItemLore => "\"Relative to true time, everything is moving at blazing fast speeds. True time is in no hurry, after all.\n\n" +
            "If you were to wind a watch in the primordial stream of time, you would create a watch that, taken anywhere, would always seem to be off. Somehow, it would always be just a bit slow.\n\n" +
            "One can only imagine what a creature acclimated to such a life would look like. I like to think something so relaxed would be quite pleasant company.\"";

        public override ItemTier Tier => ItemTier.Lunar;

        public override ItemTag[] ItemTags => new ItemTag[] { ItemTag.Utility, ItemTag.AIBlacklist };

        public override GameObject ItemModel => MainAssets.LoadAsset<GameObject>("SilverWatch.prefab");
        public override Sprite ItemIcon => MainAssets.LoadAsset<Sprite>("SilverWatchIcon");
        public static GameObject ItemBodyModelPrefab;

        public override void Init(ConfigFile config)
        {
            CreateConfig(config);
            CreateLang();
            CreateItem();
            Hooks();

            ItemDef.pickupModelPrefab.transform.localScale = new Vector3(.85f, .85f, .85f);
            ItemDef.pickupModelPrefab.transform.localRotation = new Quaternion(0, 90, 0, 0);
        }

        private void CreateConfig(ConfigFile config)
        {
           baseDurationIncrease = config.ActiveBind<float>("Item: " + ItemName, "Base Increase to Buff/Debuff Durations with 1 Silver Pocketwatch", .5f, "How much should buff/debuff durations be increased by with 1 Silver Pocketwatch? (.5 = 50%)");
           addDurationIncrease = config.ActiveBind<float>("Item: " + ItemName, "Additional Increase to Buff/Debuff Durations per Silver Pocketwatch", .25f, "How much should buff/debuff durations be increased for each additional Silver Pocketwatch? (.25 = 25%)");
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            ItemBodyModelPrefab = ItemModel;
            var itemDisplay = ItemBodyModelPrefab.AddComponent<RoR2.ItemDisplay>();
            itemDisplay.rendererInfos = ItemDisplaySetup(ItemBodyModelPrefab);

            Vector3 generalScale = new Vector3(.125f, .125f, .125f);

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
                    childName = "Pelvis",
                    localPos = new Vector3(-0.18839F, -0.06187F, 0.07992F),
                    localAngles = new Vector3(81.71718F, 261.1382F, 145.5732F),
                    localScale = new Vector3(1.78882F, 1.78882F, 1.78882F)
                }
            });
            rules.Add("mdlHuntress", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "BowHinge2L",
                    localPos = new Vector3(-0.0584F, 0.00807F, 0.03723F),
                    localAngles = new Vector3(356.096F, 273.2843F, 271.3918F),
                    localScale = new Vector3(1.67827F, 1.67827F, 1.67827F)
                }
            });
            rules.Add("mdlBandit2", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Pelvis",
                    localPos = new Vector3(0.21647F, -0.02029F, -0.04201F),
                    localAngles = new Vector3(82.10283F, 338.4091F, 61.97592F),
                    localScale = new Vector3(1.42662F, 1.42662F, 1.42662F)
                }
            });
            rules.Add("mdlToolbot", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Neck",
                    localPos = new Vector3(-0.05158F, 1.04457F, 0.56118F),
                    localAngles = new Vector3(271.2198F, 44.19891F, 311.8055F),
                    localScale = new Vector3(15F, 15F, 15F)
                }
            });
            rules.Add("mdlEngi", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "LowerArmL",
                    localPos = new Vector3(0.00578F, 0.11629F, -0.11536F),
                    localAngles = new Vector3(346.0882F, 173.7201F, 253.2912F),
                    localScale = new Vector3(1.52273F, 1.52273F, 1.52273F)
                }
            });
            rules.Add("mdlMage", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Pelvis",
                    localPos = new Vector3(0.16943F, -0.10265F, 0.00094F),
                    localAngles = new Vector3(58.87796F, 179.4037F, 293.099F),
                    localScale = new Vector3(1.45515F, 1.45515F, 1.45515F)
                }
            });
            rules.Add("mdlMerc", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Pelvis",
                    localPos = new Vector3(0.22423F, 0.08989F, -0.02379F),
                    localAngles = new Vector3(52.90685F, 272.9258F, 18.80636F),
                    localScale = new Vector3(1.62253F, 1.62253F, 1.62253F)
                }
            });
            rules.Add("mdlTreebot", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "PlatformBase",
                    localPos = new Vector3(0.69909F, -0.71452F, 0.30669F),
                    localAngles = new Vector3(291.0653F, 294.3392F, 348.2942F),
                    localScale = new Vector3(5F, 5F, 5F)
                }
            });
            rules.Add("mdlLoader", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Pelvis",
                    localPos = new Vector3(0.21438F, -0.01343F, -0.06647F),
                    localAngles = new Vector3(84.46038F, 260.8102F, 335.3249F),
                    localScale = new Vector3(2F, 2F, 2F)
                }
            });
            rules.Add("mdlCroco", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "LowerArmL",
                    localPos = new Vector3(2.2221F, 3.9541F, 1.07072F),
                    localAngles = new Vector3(40.90621F, 224.9948F, 354.5094F),
                    localScale = new Vector3(28.05256F, 28.05256F, 28.05256F)
                }
            });
            rules.Add("mdlCaptain", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Stomach",
                    localPos = new Vector3(0.18636F, 0.10735F, 0.16845F),
                    localAngles = new Vector3(290.0197F, 25.72434F, 7.55349F),
                    localScale = new Vector3(2F, 2F, 2F)
                }
            });
            rules.Add("mdlRailGunner", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Stomach",
                    localPos = new Vector3(-0.01465F, -0.10775F, 0.1785F),
                    localAngles = new Vector3(289.6436F, 119.9691F, 249.5762F),
                    localScale = new Vector3(2F, 2F, 2F)
                }
            });
            rules.Add("mdlVoidSurvivor", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Stomach",
                    localPos = new Vector3(0.16766F, -0.02186F, 0.04475F),
                    localAngles = new Vector3(296.1401F, 325.8141F, 105.5126F),
                    localScale = new Vector3(2F, 2F, 2F)
                }
            });

            //MODDED CHARACTER IDRs START HERE

            rules.Add("mdlNemCommando", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Pelvis",
                    localPos = new Vector3(1.18368F, 0.02681F, 0.08969F),
                    localAngles = new Vector3(55.23709F, 254.026F, 343.4777F),
                    localScale = new Vector3(6F, 6F, 6F)
                }
            });
            rules.Add("mdlHANDOverclocked", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(1.50002F, -0.26816F, 0.84401F),
                    localAngles = new Vector3(294.9263F, 183.1238F, 269.201F),
                    localScale = new Vector3(10F, 10F, 10F)
                }
            });
            rules.Add("mdlEnforcer", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Pelvis",
                    localPos = new Vector3(-0.02583F, 0.02634F, 0.40624F),
                    localAngles = new Vector3(72.36713F, 187.0455F, 15.75413F),
                    localScale = new Vector3(2F, 2F, 2F)
                }
            });
            //            rules.Add("mdlNemforcer(Clone)", new RoR2.ItemDisplayRule[]
            //            {
            //                new RoR2.ItemDisplayRule
            //                {
            //                    ruleType = ItemDisplayRuleType.ParentedPrefab,
            //                    followerPrefab = ItemBodyModelPrefab,
            //                    childName = "Chest",
            //                    localPos = new Vector3(-0.25983F, 0.30917F, -0.02484F),
            //                    localAngles = new Vector3(343.1456F, 273.5997F, 0.5956F),
            //                    localScale = new Vector3(0.20149F, 0.20149F, 0.20149F)
            //                }
            //            });
            rules.Add("mdlPaladin", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Pelvis",
                    localPos = new Vector3(0.28473F, 0.17363F, -0.04988F),
                    localAngles = new Vector3(291.3388F, 65.98773F, 38.01163F),
                    localScale = new Vector3(3F, 3F, 3F)
                }
            });
            //            rules.Add("mdlMiner", new RoR2.ItemDisplayRule[]
            //            {
            //                new RoR2.ItemDisplayRule
            //                {
            //                    ruleType = ItemDisplayRuleType.ParentedPrefab,
            //                    followerPrefab = ItemBodyModelPrefab,
            //                    childName = "Chest",
            //                    localPos = new Vector3(-0.04f, 0.26f, 0.22f),
            //                    localAngles = new Vector3(0f, 0f, 0f),
            //                    localScale = generalScale * 0.9f
            //                }
            //            });
            rules.Add("mdlPathfinder", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Pelvis",
                    localPos = new Vector3(-0.13636F, -0.09375F, 0.15186F),
                    localAngles = new Vector3(47.41651F, 178.4204F, 36.48877F),
                    localScale = new Vector3(2F, 2F, 2F)
                }
            });
            rules.Add("mdlExecutioner2", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Pelvis",
                    localPos = new Vector3(0.20612F, -0.00922F, 0.06444F),
                    localAngles = new Vector3(71.61937F, 310.5522F, 58.27919F),
                    localScale = new Vector3(1.65464F, 1.65464F, 1.65464F)
                }
            });
            rules.Add("mdlHouse(Clone)", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "LeftHand",
                    localPos = new Vector3(0.05123F, 0.11971F, -0.00661F),
                    localAngles = new Vector3(69.04691F, 181.8181F, 260.2504F),
                    localScale = new Vector3(2F, 2F, 2F)
                }
            });
            rules.Add("mdlTeslaTrooper", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Pelvis",
                    localPos = new Vector3(0.2547F, -0.12326F, 0.01758F),
                    localAngles = new Vector3(66.98492F, 255.7291F, 9.49379F),
                    localScale = new Vector3(1.5F, 1.5F, 1.5F)
                }
            });
            rules.Add("mdlDesolator", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Pelvis",
                    localPos = new Vector3(0.30376F, -0.08879F, 0.00364F),
                    localAngles = new Vector3(82.56494F, 68.99921F, 180F),
                    localScale = new Vector3(1.49433F, 1.49433F, 1.49433F)
                }
            });
            //            rules.Add("CHEF", new RoR2.ItemDisplayRule[]
            //            {
            //                new RoR2.ItemDisplayRule
            //                {
            //                    ruleType = ItemDisplayRuleType.ParentedPrefab,
            //                    followerPrefab = ItemBodyModelPrefab,
            //                    childName = "Head",
            //                    localPos = new Vector3(0F, 0.01245F, -0.00126F),
            //                    localAngles = new Vector3(0F, 0F, 0F),
            //                    localScale = new Vector3(0.00339F, 0.00339F, 0.00339F)
            //                }
            //            });
            rules.Add("mdlArsonist", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Waist",
                    localPos = new Vector3(0.21708F, -0.01053F, -0.0434F),
                    localAngles = new Vector3(67.91787F, 252.1351F, 318.8398F),
                    localScale = new Vector3(1.51463F, 1.51463F, 1.51463F)
                }
            });
            rules.Add("mdlRocket", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(-0.227F, 0.16905F, 0.26931F),
                    localAngles = new Vector3(283.3134F, 55.48521F, 301.9987F),
                    localScale = new Vector3(1.27161F, 1.27161F, 1.27161F)
                }
            });
            return rules;
        }
        public override void Hooks()
        {
            On.RoR2.CharacterBody.AddTimedBuff_BuffDef_float += BuffManager;
        }

        private void BuffManager(On.RoR2.CharacterBody.orig_AddTimedBuff_BuffDef_float orig, CharacterBody self, BuffDef buffDef, float duration)
        {
            var inventoryCount = GetCount(self);
            if (inventoryCount > 0)
            {
                duration = Mathf.Max(duration * baseDurationIncrease + (duration * addDurationIncrease * (inventoryCount - 1)), duration);
            }

            orig(self, buffDef, duration);
        }
    }
}