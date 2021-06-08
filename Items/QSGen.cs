using System.Collections.ObjectModel;
using System.Collections.Generic;
using R2API;
using RoR2;
using UnityEngine;
using TILER2;
using static TILER2.StatHooks;
using static TILER2.MiscUtil;
using SupplyDrop.Utils;
using static K1454.SupplyDrop.SupplyDropPlugin;

namespace SupplyDrop.Items
{
    public class QSGen : Item<QSGen>
    {
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("In percentage, amount of maximum HP granted as bonus shield for first stack of the item. Default: .16 = 16%", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
        public float baseStackHPPercent { get; private set; } = .16f;

        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("In seconds, length of time for the shield gate cooldown.", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
        public float shieldGateCooldownAmount { get; private set; } = 5f;

        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("In seconds, amount of shield gate cooldown reduced by additional stacks of the item.", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
        public float shieldGateCooldownReduction { get; private set; } = .5f;
        public override string displayName => "Quantum Shield Generator";
        public override ItemTier itemTier => ItemTier.Tier3;
        public override ReadOnlyCollection<ItemTag> itemTags => new ReadOnlyCollection<ItemTag>(new[] { ItemTag.Utility });
        protected override string GetNameString(string langid = null) => displayName;
        protected override string GetPickupString(string langID = null) => "If shields are active, any damage that exceeds the active shield amount is negated.";
        protected override string GetDescString(string langID = null) => $"Gain a <style=cIsUtility>shield</style> equal to <style=cIsUtility>{Pct(baseStackHPPercent)}</style> " +
            $"of your maximum health. If an attack exceeds your active shields, the excess damage is <style=cIsUtility>negated</style>. " +
            $"This ability has a cooldown of {shieldGateCooldownAmount}s <style=cStack>(-{shieldGateCooldownReduction}s per stack)</style>.";

        protected override string GetLoreString(string landID = null) => "Order: \"Quantum Shield Generator\"\nTracking Number: 06******\nEstimated Delivery: 12/21/2055\nShipping Method: High Priority/Fragile" +
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

        private static List<CharacterBody> Playername = new List<CharacterBody>();
        public static GameObject ItemBodyModelPrefab;
        public BuffDef ShieldGateCooldown { get; private set; }
        public QSGen()
        {
            modelResource = MainAssets.LoadAsset<GameObject>("Main/Models/Prefabs/QSGen.prefab");
            iconResource = MainAssets.LoadAsset<Sprite>("Main/Textures/Icons/QSGenIcon.png");
        }
        public override void SetupAttributes()
        {
            if (ItemBodyModelPrefab == null)
            {
                ItemBodyModelPrefab = modelResource;
                var meshes = ItemBodyModelPrefab.GetComponentsInChildren<MeshRenderer>();
                meshes[1].gameObject.AddComponent<Spin>();
                displayRules = GenerateItemDisplayRules();
            }

            base.SetupAttributes();

            ShieldGateCooldown = ScriptableObject.CreateInstance<BuffDef>();
            ShieldGateCooldown.name = "SupplyDrop QSS Cooldown Debuff";
            ShieldGateCooldown.canStack = false;
            ShieldGateCooldown.isDebuff = true;
            ShieldGateCooldown.iconSprite = MainAssets.LoadAsset<Sprite>("ShieldGateCooldownIcon.png");
            BuffAPI.Add(new CustomBuff(ShieldGateCooldown));
        }
        private static ItemDisplayRuleDict GenerateItemDisplayRules()
        {
            ItemBodyModelPrefab.AddComponent<ItemDisplay>();
            ItemBodyModelPrefab.GetComponent<ItemDisplay>().rendererInfos = ItemHelpers.ItemDisplaySetup(ItemBodyModelPrefab);

            Vector3 generalScale = new Vector3(.1f, .1f, .1f);
            ItemDisplayRuleDict rules = new ItemDisplayRuleDict(new ItemDisplayRule[]
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
            return rules;
        }
        public override void Install()
        {
            base.Install();

            var meshes = itemDef.pickupModelPrefab.GetComponentsInChildren<MeshRenderer>();
            meshes[1].gameObject.AddComponent<Spin>();
            //meshes[2].gameObject.AddComponent<Bobbing>();
            itemDef.pickupModelPrefab.transform.localScale = new Vector3(1f, 1f, 1f);

            On.RoR2.HealthComponent.TakeDamage += CalculateDamageReduction;
            GetStatCoefficients += AddMaxShield;
        }
        public override void Uninstall()
        {
            base.Uninstall();

            On.RoR2.HealthComponent.TakeDamage -= CalculateDamageReduction;
            GetStatCoefficients -= AddMaxShield;
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