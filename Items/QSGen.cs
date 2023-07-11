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
    public class QSGen : ItemBase<QSGen>
    {
        //Config Stuff

        public static ConfigOption<float> baseStackHPPercent;
        public static ConfigOption<float> shieldGateCooldownAmount;
        public static ConfigOption<float> shieldGateCooldownReduction;

        //Item Data
        public override string ItemName => "Quantum Shield Stabilizer";

        public override string ItemLangTokenName => "QS_GEN";

        public override string ItemPickupDesc => "If <style=cIsUtility>shields</style> are active, any damage that exceeds the <style=cIsUtility>active shield amount</style> is negated.";

        public override string ItemFullDescription => $"Gain a <style=cIsUtility>shield</style> equal to <style=cIsUtility>{FloatToPercentageString(baseStackHPPercent)}</style> " +
            $"of your maximum health. If an attack exceeds your active shields, the excess damage is <style=cIsUtility>negated</style>. " +
            $"This ability has a cooldown of {shieldGateCooldownAmount}s <style=cStack>(-{shieldGateCooldownReduction}s per stack)</style>.";

        public override string ItemLore => "Order: \"Quantum Shield Stabilizer\"\nTracking Number: 06******\nEstimated Delivery: 12/21/2055\nShipping Method: High Priority/Fragile" +
            "\nShipping Address: 6900 West, Advanced Warfare, Mars\nShipping Details:\n\nAfter months of development, we finally have a functioning prototype for your approval." +
            "\n\nThe stabilizer functions mostly off of uncertainty. Basing the design entirely off uncertainty would make the functionality too limited; as a result, some shield is guaranteed, " +
            "to serve as the foundation. In addition to this foundation, the stabilizer is also providing, yet not providing, additional shield. This shield both exists, and doesn't, until the stabilizer is activated, " +
            "at which point it observes itself based upon the damage it is stabilizing. This observation by the stabilizer causes the amount of additional shield to be 'locked down'." +
            "\n\nThe benefit of this design is the fact traditional shield storage and generation is nearly completely omitted. Additional generation/storage would guarantee the existence of more shield, " +
            "and thus is actively excluded of favor of a small, lightweight uncertainty drive. \n\nA Class-B Hyper-Condensed Star Engine (HCSE) is utilized to power the uncertainty drive. " +
            "It should be noted that the HCSE appears to exhibit unusual properties due to its constant exposure to uncertainty. " +
            "Namely, the star is constantly expelling particulate, created by it undergoing inexplicable micronization and regeneration. The particulate and effects on the star are completely benign, " +
            "but further iterations could include dispersion/collection methods, if desired.\n\nAn invoice is attached to this shipment, " +
            "and should be responded to within 3 solar months. Once the invoice has been paid, please contact the usual representative with your thoughts on the prototype. " +
            "If it is satisfactory, we can begin moving forward releasing the blueprints, for the additional agreed-upon cost.\n\nThe Order thanks you for your patronage.";

        public override ItemTier Tier => ItemTier.Tier3;
        public override ItemTag[] ItemTags => new ItemTag[] { ItemTag.Utility, ItemTag.AIBlacklist };


        public override GameObject ItemModel => MainAssets.LoadAsset<GameObject>("QSGen.prefab");
        public override Sprite ItemIcon => MainAssets.LoadAsset<Sprite>("QSGenIcon");


        public static GameObject ItemBodyModelPrefab;
        public BuffDef ShieldGateCooldown { get; private set; }

        public override void Init(ConfigFile config)
        {
            CreateConfig(config);
            CreateLang();
            CreateBuff();
            CreateItem();
            Hooks();
        }

        private void CreateConfig(ConfigFile config)
        {
            baseStackHPPercent = config.ActiveBind<float>("Item: " + ItemName, "Shield Gained with First Stack of Quantum Shield Stabilizer", .16f, "How much shield as a % of max HP should you gain with the first quantum shield stabilizer? (.16 = 16%)");
            shieldGateCooldownAmount = config.ActiveBind<float>("Item: " + ItemName, "Base Cooldown on Shield Gate Ability with 1 Quantum Shield Stabilizer", 5f, "How long should the QSS take before its effect is off cooldown, in seconds?");
            shieldGateCooldownReduction = config.ActiveBind<float>("Item: " + ItemName, "Additional Reduction of Cooldown on Shield Gate Ability per Quantum Shield Stabilizer", 1f, "How much cooldown reduction should each quantum shield stabilizer after the first give, in seconds?");
        }
        private void CreateBuff()
        {
            ShieldGateCooldown = ScriptableObject.CreateInstance<BuffDef>();
            ShieldGateCooldown.name = "SupplyDrop QSS Cooldown Debuff";
            ShieldGateCooldown.canStack = false;
            ShieldGateCooldown.isDebuff = true;
            ShieldGateCooldown.iconSprite = MainAssets.LoadAsset<Sprite>("ShieldGateCooldownIcon");

            ContentAddition.AddBuffDef(ShieldGateCooldown);
        }
        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            ItemBodyModelPrefab = ItemModel;
            var meshes = ItemBodyModelPrefab.GetComponentsInChildren<MeshRenderer>();
            meshes[1].gameObject.AddComponent<Spin>();
            //Still unsure why the orb freaks out. Disabled until further notice.
            //meshes[2].gameObject.AddComponent<Bobbing>();
            var itemDisplay = ItemBodyModelPrefab.AddComponent<RoR2.ItemDisplay>();
            itemDisplay.rendererInfos = ItemDisplaySetup(ItemBodyModelPrefab);

            Vector3 generalScale = new Vector3(.5f, .5f, .5f);

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
                    childName = "UpperArmR",
                    localPos = new Vector3(-0.05793F, 0.13034F, 0.01476F),
                    localAngles = new Vector3(0.91185F, 3.21117F, 91.96462F),
                    localScale = new Vector3(0.03F, 0.03F, 0.03F)
                }
            });
            rules.Add("mdlHuntress", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "UpperArmR",
                    localPos = new Vector3(0.01172F, 0.15095F, -0.02392F),
                    localAngles = new Vector3(285.0728F, 60.66216F, 284.8535F),
                    localScale = new Vector3(0.03393F, 0.03393F, 0.03393F)
                }
            });
            rules.Add("mdlBandit2", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "UpperArmR",
                    localPos = new Vector3(0.00184F, 0.21628F, -0.05484F),
                    localAngles = new Vector3(54.23124F, 255.4654F, 69.69495F),
                    localScale = new Vector3(0.03254F, 0.03254F, 0.03254F)
                }
            });
            rules.Add("mdlToolbot", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "UpperArmR",
                    localPos = new Vector3(-0.25249F, 0.39355F, -0.10601F),
                    localAngles = new Vector3(359.8874F, 357.4833F, 265.8989F),
                    localScale = new Vector3(0.75F, 0.75F, 0.75F)
                }
            });
            rules.Add("mdlEngi", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "UpperArmR",
                    localPos = new Vector3(0.00152F, 0.09321F, -0.03032F),
                    localAngles = new Vector3(353.6305F, 345.9563F, 83.27012F),
                    localScale = new Vector3(0.07732F, 0.07732F, 0.07732F)
                }
            });
            rules.Add("mdlMage", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "UpperArmR",
                    localPos = new Vector3(0.01941F, 0.05903F, 0.03653F),
                    localAngles = new Vector3(0F, 0F, 90F),
                    localScale = new Vector3(0.05461F, 0.05461F, 0.05461F)
                }
            });
            rules.Add("mdlMerc", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "UpperArmR",
                    localPos = new Vector3(-0.1059F, 0.09663F, -0.01866F),
                    localAngles = new Vector3(331.6755F, 344.3694F, 102.1405F),
                    localScale = new Vector3(0.06033F, 0.06033F, 0.06033F)
                }
            });
            rules.Add("mdlTreebot", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "FlowerBase",
                    localPos = new Vector3(0f, -1f, 0f),
                    localAngles = new Vector3(0f, 0f, 180f),
                    localScale = new Vector3(0.2f, 0.2f, 0.2f)
                }
            });
            rules.Add("mdlLoader", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "UpperArmR",
                    localPos = new Vector3(-0.31076F, 0.09974F, 0.01683F),
                    localAngles = new Vector3(359.9377F, 359.8866F, 89.7636F),
                    localScale = new Vector3(0.06733F, 0.06733F, 0.06733F)
                }
            });
            rules.Add("mdlCroco", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "UpperArmR",
                    localPos = new Vector3(0.59086F, 0.54968F, 0.06733F),
                    localAngles = new Vector3(0F, 0F, 270F),
                    localScale = new Vector3(0.76166F, 0.76166F, 0.76166F)
                }
            });
            rules.Add("mdlCaptain", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "UpperArmR",
                    localPos = new Vector3(0.03058F, 0.21142F, 0.01927F),
                    localAngles = new Vector3(341.8543F, 356.4473F, 82.38717F),
                    localScale = new Vector3(0.09366F, 0.09366F, 0.09366F)
                }
            });
            rules.Add("mdlRailGunner", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Backpack",
                    localPos = new Vector3(-0.30694F, 0.21186F, 0.00899F),
                    localAngles = new Vector3(20.47733F, 358.8252F, 82.82342F),
                    localScale = new Vector3(0.05565F, 0.05565F, 0.05565F)
                }
            });
            rules.Add("mdlVoidSurvivor", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "UpperArmR",
                    localPos = new Vector3(0.13335F, 0.00423F, -0.00548F),
                    localAngles = new Vector3(54.94378F, 331.8732F, 219.9172F),
                    localScale = new Vector3(0.06222F, 0.06222F, 0.06222F)
                }
            });

            //MODDED CHARACTER IDRs START HERE

            rules.Add("mdlNemCommando", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "ShoulderL",
                    localPos = new Vector3(0.24734F, 0.39872F, -0.06874F),
                    localAngles = new Vector3(354.5952F, 350.9987F, 274.3998F),
                    localScale = new Vector3(0.32325F, 0.32325F, 0.32325F)
                }
            });
            rules.Add("mdlHANDOverclocked", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "ShoulderR",
                    localPos = new Vector3(0F, 0.74927F, -0.01797F),
                    localAngles = new Vector3(0F, 0F, 0F),
                    localScale = new Vector3(0.40594F, 0.40594F, 0.40594F)
                }
            });
            rules.Add("mdlEnforcer", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Shield",
                    localPos = new Vector3(0.15254F, -0.51955F, 0.22596F),
                    localAngles = new Vector3(281.5181F, 165.6866F, 140.199F),
                    localScale = new Vector3(0.12386F, 0.12386F, 0.12386F)
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
                    childName = "UpperArmL",
                    localPos = new Vector3(0.06668F, 0.24488F, -0.01256F),
                    localAngles = new Vector3(6.19951F, 17.19497F, 284.9348F),
                    localScale = new Vector3(0.10175F, 0.10175F, 0.10175F)
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
                    childName = "UpperArmL",
                    localPos = new Vector3(-0.0388F, 0.06996F, -0.00301F),
                    localAngles = new Vector3(295.2136F, 26.57254F, 265.8721F),
                    localScale = new Vector3(0.08539F, 0.08539F, 0.08539F)
                }
            });
            rules.Add("mdlExecutioner2", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "ShoulderR",
                    localPos = new Vector3(0.00854F, 0.03245F, -0.00487F),
                    localAngles = new Vector3(13.60338F, 332.1542F, 104.3571F),
                    localScale = new Vector3(0.06694F, 0.06694F, 0.06694F)
                }
            });
            rules.Add("mdlHouse(Clone)", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "RightShoulder",
                    localPos = new Vector3(-0.02319F, 0.17243F, 0.03753F),
                    localAngles = new Vector3(16.65907F, 20.37583F, 329.8076F),
                    localScale = new Vector3(0.06522F, 0.06522F, 0.06522F)
                }
            });
            rules.Add("mdlTeslaTrooper", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "ShoulderCoil",
                    localPos = new Vector3(0.02613F, -0.08649F, -0.0255F),
                    localAngles = new Vector3(342.7851F, 172.9204F, 179.0239F),
                    localScale = new Vector3(0.04971F, 0.04971F, 0.04971F)
                }
            });
            rules.Add("mdlDesolator", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "ShoulderR",
                    localPos = new Vector3(0.18414F, 0.37863F, -0.0451F),
                    localAngles = new Vector3(0F, 0F, 0F),
                    localScale = new Vector3(0.08012F, 0.08012F, 0.08012F)
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
                    childName = "ShoulderR",
                    localPos = new Vector3(0.06479F, 0.19934F, -0.05262F),
                    localAngles = new Vector3(25.07744F, 350.4428F, 350.0462F),
                    localScale = new Vector3(0.0664F, 0.0664F, 0.0664F)
                }
            });
            rules.Add("mdlRocket", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "upper_arm.L",
                    localPos = new Vector3(-0.00657F, 0.07232F, 0.05788F),
                    localAngles = new Vector3(43.67332F, 275.8606F, 261.565F),
                    localScale = new Vector3(0.0683F, 0.0683F, 0.0683F)
                }
            });
            return rules;
        }
        public override void Hooks()
        {
            On.RoR2.HealthComponent.TakeDamage += CalculateDamageReduction;
            GetStatCoefficients += AddMaxShield;
        }
        private void AddMaxShield(CharacterBody sender, StatHookEventArgs args)
        {
            var inventoryCount = GetCount(sender);
            if (inventoryCount > 0)
            {
                ItemHelpers.AddMaxShieldHelper(sender, args, inventoryCount, baseStackHPPercent, 0);
            }
        }
        private void CalculateDamageReduction(On.RoR2.HealthComponent.orig_TakeDamage orig, HealthComponent self, DamageInfo damageInfo)
        {
            if (damageInfo.attacker)
            {
                var inventoryCount = GetCount(self.body);
                if (inventoryCount > 0 && self.body.GetBuffCount(ShieldGateCooldown) <= 0)
                {
                    float currentShield = self.body.healthComponent.shield;
                    float dmgTaken = damageInfo.damage * (100 / (100 + self.body.armor));

                    if (currentShield > 0 && dmgTaken > currentShield)
                    {
                        damageInfo.damage = currentShield * (100 / (100 + self.body.armor));

                        float timerReduction = Mathf.Min(((inventoryCount - 1) * shieldGateCooldownReduction), shieldGateCooldownAmount);
                        self.body.AddTimedBuffAuthority(ShieldGateCooldown.buffIndex, (shieldGateCooldownAmount - timerReduction));
                    }
                }
            }
            orig(self, damageInfo);
        }
    }
}