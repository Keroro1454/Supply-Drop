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
            "\n\nThe stabilizer functions mostly, but not entirely, off of uncertainty. Basing the design entirely off uncertainty would make the functionality too limited; as a result some shield is guaranteed, " +
            "to serve as the foundation. In addition to this foundation, the stabilizer is also providing, yet not providing, additional shield. This shield both exists and doesn't until the stabilizer is activated, " +
            "at which point it observes itself based upon the damage it is stabilizing. This observation by the stabilizer causes the amount of additional shield to be 'locked down'." +
            "\n\nThe benefit of this design is the fact traditional shield storage and generation is nearly completely omitted. Additional generation/storage would guarantee the existence of more shield, " +
            "and thus is actively excluded of favor of a small, lightweight uncertainty drive. \n\nA Class-B Hyper-Condensed Star Engine is utilized to power the uncertainty drive. " +
            "It should be noted that the HCSE appears to exhibit unusual properties due to its constant exposure to uncertainty. " +
            "Namely, the star is constantly expelling particulate created by it undergoing inexplicable micronization and regeneration. The particulate and effects on the star are completely benign, " +
            "but further iterations could include dispersion/collection methods if desired.\n\nAn invoice is attached to this shipment. It details the cost of developing this prototype, " +
            "and should be responded to within 3 solar months. Once the invoice has been paid please contact the usual representative with your thoughts on the prototype. " +
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
            var itemDisplay = ItemBodyModelPrefab.AddComponent<RoR2.ItemDisplay>();
            itemDisplay.rendererInfos = ItemDisplaySetup(ItemBodyModelPrefab);

            Vector3 generalScale = new Vector3(1.5f, 1.5f, 1.5f);

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
                    localPos = new Vector3(0.05f, 0.2f, 0f),
                    localAngles = new Vector3(0f, 0f, 90f),
                    localScale = generalScale
                }
            });
            rules.Add("mdlHuntress", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "UpperArmR",
                    localPos = new Vector3(0f, 0.2f, 0.1f),
                    localAngles = new Vector3(-80f, 0f, 0f),
                    localScale = generalScale
                }
            });
            rules.Add("mdlToolbot", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "UpperArmR",
                    localPos = new Vector3(-0.9f, 1f, 0f),
                    localAngles = new Vector3(0f, 0f, -90f),
                    localScale = new Vector3(0.75f, 0.75f, 0.75f)
                }
            });
            rules.Add("mdlEngi", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "UpperArmR",
                    localPos = new Vector3(0.075f, 0.2f, 0f),
                    localAngles = new Vector3(0f, 0f, 90f),
                    localScale = generalScale
                }
            });
            rules.Add("mdlMage", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "UpperArmR",
                    localPos = new Vector3(0.12f, 0.2f, 0f),
                    localAngles = new Vector3(0f, 0f, 90f),
                    localScale = generalScale
                }
            });
            rules.Add("mdlMerc", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "UpperArmR",
                    localPos = new Vector3(-0.03f, 0.15f, 0f),
                    localAngles = new Vector3(-25f, 0f, 110f),
                    localScale = generalScale
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
                    localPos = new Vector3(-0.13f, 0.3f, 0.4f),
                    localAngles = new Vector3(0f, 0f, 90f),
                    localScale = generalScale
                }
            });
            rules.Add("mdlCroco", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "UpperArmR",
                    localPos = new Vector3(0f, 2f, 0f),
                    localAngles = new Vector3(0f, 0f, -90f),
                    localScale = generalScale * 8
                }
            });
            rules.Add("mdlCaptain", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "UpperArmR",
                    localPos = new Vector3(0.065f, 0.2f, 0f),
                    localAngles = new Vector3(-0f, 0f, 90f),
                    localScale = generalScale
                }
            });
            rules.Add("mdlBandit2", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "UpperArmR",
                    localPos = new Vector3(0.05f, 0.2f, 0f),
                    localAngles = new Vector3(0f, 0f, 90f),
                    localScale = generalScale
                }
            });
            return rules;
        }
        public override void Hooks()
        {
            //var meshes = itemDef.pickupModelPrefab.GetComponentsInChildren<MeshRenderer>();
            //meshes[1].gameObject.AddComponent<Spin>();
            //meshes[2].gameObject.AddComponent<Bobbing>();
            //itemDef.pickupModelPrefab.transform.localScale = new Vector3(1f, 1f, 1f);

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