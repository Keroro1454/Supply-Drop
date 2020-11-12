using System.Collections.ObjectModel;
using System.Collections.Generic;
using R2API;
using RoR2;
using UnityEngine;
using TILER2;
using K1454.SupplyDrop;
using SupplyDrop.Utils;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using System.Reflection;

namespace SupplyDrop.Items
{
    class QSGen : Item_V2<QSGen>
    {
        public override string displayName => "Quantum Shield Generator";

        public override ItemTier itemTier => ItemTier.Tier3;

        public override ReadOnlyCollection<ItemTag> itemTags => new ReadOnlyCollection<ItemTag>(new[] { ItemTag.Utility });
        protected override string GetNameString(string langid = null) => displayName;

        protected override string GetPickupString(string langID = null) => "If shields are active, any damage that exceeds the active shield amount is weakened.";

        protected override string GetDescString(string langID = null) => "Gain a <style=cIsUtility>shield</style> equal to <style=cIsUtility>16%</style> of your maximum health. " +
            "If an attack exceeds your active shields, the excess damage is <style=cIsUtility>negated</style>. This ability has a cooldown of 5s <style=cStack>(-0.5s per stack)</style>.";

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
        public BuffIndex ShieldGateCooldown { get; private set; }

        public QSGen()
        {
            modelResourcePath = "@SupplyDrop:Assets/Main/Models/Prefabs/QSGen.prefab";
            iconResourcePath = "@SupplyDrop:Assets/Main/Textures/Icons/QSGenIcon.png";
        }
        public override void SetupAttributes()
        {
            if (ItemBodyModelPrefab == null)
            {
                ItemBodyModelPrefab = Resources.Load<GameObject>(modelResourcePath);
                var meshes = ItemBodyModelPrefab.GetComponentsInChildren<MeshRenderer>();
                meshes[1].gameObject.AddComponent<Spin>();
                displayRules = GenerateItemDisplayRules();
            }

            base.SetupAttributes();
                    var shieldGateCooldown = new CustomBuff(
                    new BuffDef
                    {
                        canStack = false,
                        isDebuff = true,
                        name = "ShieldGateCooldown",
                        iconPath = "@SupplyDrop:Assets/Main/Textures/Icons/ShieldGateCooldownIcon.png"
                    });
                ShieldGateCooldown = BuffAPI.Add(shieldGateCooldown);
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

            IL.RoR2.CharacterBody.RecalculateStats += IL_AddMaxShield;
        }

        public override void Uninstall()
        {
            base.Uninstall();
            IL.RoR2.CharacterBody.RecalculateStats -= IL_AddMaxShield;

            On.RoR2.HealthComponent.TakeDamage -= CalculateDamageReduction;
        }
        private void IL_AddMaxShield(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            c.GotoNext(
                x => x.MatchLdloc(43),
                x => x.MatchCallvirt(typeof(CharacterBody).GetMethod("set_maxShield", BindingFlags.Instance | BindingFlags.NonPublic))
                );

            c.Emit(OpCodes.Ldarg, 0);
            c.Emit(OpCodes.Ldloc, 43);
            c.EmitDelegate<Func<CharacterBody, float, float>>((characterBody, shield) =>
            {
                if (GetCount(characterBody) > 0)
                {
                    return shield + (characterBody.maxHealth * 0.16f);
                }
                return shield;
            }
            );
            c.Emit(OpCodes.Stloc, 43);
        }
        private void CalculateDamageReduction(On.RoR2.HealthComponent.orig_TakeDamage orig, HealthComponent self, DamageInfo damageInfo)
        {
            float currentShield = self.body.healthComponent.shield;
            orig(self, damageInfo);
            var inventoryCount = GetCount(self.body);
            if (inventoryCount > 0 && self.body.GetBuffCount(ShieldGateCooldown) <= 0)
            {
                float dmgTaken = damageInfo.damage;
                float shieldDamage = Math.Min(dmgTaken, currentShield);
                if (currentShield > 0 && dmgTaken > currentShield)
                {
                    float damageReduction = dmgTaken - shieldDamage;
                    damageInfo.damage += damageReduction;

                    float timerReduction = Mathf.Min(((inventoryCount - 1) * .5f), 5f);
                    self.body.AddTimedBuff(ShieldGateCooldown, (5 - timerReduction));
                }     
            }
        }
    }
}