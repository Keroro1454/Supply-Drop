using System.Collections.ObjectModel;
using System.Collections.Generic;
using R2API;
using RoR2;
using UnityEngine;
using TILER2;
using static TILER2.StatHooks;
using SupplyDrop.Utils;
using static TILER2.MiscUtil;

namespace SupplyDrop.Items
{
    public class UnassumingTie : Item_V2<UnassumingTie>
    {
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("In percentage, amount of maximum HP granted as bonus shield for first stack of the item. Default: 4% (.04)", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
        public float baseStackHPPercent { get; private set; } = .04f;

        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("In percentage, amount of maximum HP granted as bonus shield for additional stacks of item. Default: 2% (.02)", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
        public float addStackHPPercent { get; private set; } = .02f;

        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("In seconds, the duration of the 'Winded' debuff. Default: 10", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
        public float windedDebuffDuration { get; private set; } = 10f;

        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("In seconds, the base duration of the 'Second Wind' buff. Default: 4", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
        public float secondWindBaseDuration { get; private set; } = 4f;

        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("In seconds, the multiplier applied to the shield/HP ratio, used to calculate the bonus duration of the 'Second Wind' buff. Default: 5", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
        public float secondWindBonusMultiplier { get; private set; } = 5f;

        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("In percentage, the speed boost granted by the 'Second Wind' buff for the first stack of the item. Default: 15%", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
        public float secondWindBaseSpeedPercent { get; private set; } = 15f;

        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("In percentage, the speed boost granted by the 'Second Wind' buff for additional stacks of the item. Default: 10%", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
        public float secondWindAddSpeedPercent { get; private set; } = 10f;
        public override string displayName => "Unassuming Tie";

        public override ItemTier itemTier => ItemTier.Tier1;

        public override ReadOnlyCollection<ItemTag> itemTags => new ReadOnlyCollection<ItemTag>(new[] { ItemTag.Utility });
        protected override string GetNameString(string langid = null) => displayName;

        protected override string GetPickupString(string langID = null) => "Gain some shield, and receive a speed boost when your shield is broken.";

        protected override string GetDescString(string langID = null) => $"Gain a <style=cIsUtility>shield</style> equal to <style=cIsUtility>{Pct(baseStackHPPercent)}</style>" +
            $" <style=cStack>(+{Pct(addStackHPPercent)} per stack)</style> of your maximum health. Breaking your <style=cIsUtility>shield</style> gives you a" +
            $" <style=cIsUtility>Second Wind</style> for {secondWindBaseDuration}s, plus a bonus amount based on your <style=cIsUtility>maximum shield</style>. " +
            $"Second Wind increases <style=cIsUtility>movement speed</style> by <style=cIsUtility>{secondWindBaseSpeedPercent}%</style> <style=cStack>(+{secondWindBonusMultiplier}% per stack)</style>.";

        protected override string GetLoreString(string landID = null) => "\"This necktie was a staple accessory of one of a notorious group of well-dressed heisters which were active during the early 21st century.The gang was wildly successful while active, breaking into, looting, and escaping from some of the most secure sites on Earth at the time. Even when authorities attempted to apprehend the criminals, reports state that shooting at them 'only seem to make [the heisters] move faster, however the hell that works.' While the identities of these criminals were never discovered, the gang ceased operations for unknown reasons after over a decade of activity. This piece serves as a testament to their dedication to style, no matter the situation.\"\n\n- Placard description for \"Striped Tie\" at the Galactic Museum of Law Enforcement and Criminality";

        private static List<CharacterBody> Playername = new List<CharacterBody>();
        public static GameObject ItemBodyModelPrefab;
        public static GameObject SecondItemBodyModelPrefab;
        public static GameObject ThirdItemBodyModelPrefab;

        public BuffIndex SecondWindBuff { get; private set; }
        public BuffIndex WindedDebuff { get; private set; }

        public UnassumingTie()
        {
            modelResourcePath = "@SupplyDrop:Assets/Main/Models/Prefabs/TiePickUp.prefab";
            iconResourcePath = "@SupplyDrop:Assets/Main/Textures/Icons/TieIcon.png";
        }
        public override void SetupAttributes()
        {
            if (ItemBodyModelPrefab == null && SecondItemBodyModelPrefab == null && ThirdItemBodyModelPrefab == null)
            {
                ItemBodyModelPrefab = Resources.Load<GameObject>("@SupplyDrop:Assets/Main/Models/Prefabs/Tie.prefab");
                SecondItemBodyModelPrefab = Resources.Load<GameObject>("@SupplyDrop:Assets/Main/Models/Prefabs/TieBig.prefab");
                ThirdItemBodyModelPrefab = Resources.Load<GameObject>(modelResourcePath);
                displayRules = GenerateItemDisplayRules();
            }

            base.SetupAttributes();
            var secondWindBuff = new R2API.CustomBuff(
                    new BuffDef
                    {
                        canStack = false,
                        isDebuff = false,
                        name = "SecondWindBuff",
                        iconPath = "@SupplyDrop:Assets/Main/Textures/Icons/SecondWindBuffIcon.png"
                    });
                SecondWindBuff = BuffAPI.Add(secondWindBuff);

                var windedDebuff = new R2API.CustomBuff(
                    new BuffDef
                    {
                        canStack = false,
                        isDebuff = true,
                        name = "WindedDebuff",
                        iconPath = "@SupplyDrop:Assets/Main/Textures/Icons/WindedDebuffIcon.png"
                    });
                WindedDebuff = BuffAPI.Add(windedDebuff);
        }

        private static ItemDisplayRuleDict GenerateItemDisplayRules()
        {
            ItemBodyModelPrefab.AddComponent<ItemDisplay>();
            SecondItemBodyModelPrefab.AddComponent<ItemDisplay>();
            ItemBodyModelPrefab.GetComponent<ItemDisplay>().rendererInfos = ItemHelpers.ItemDisplaySetup(ItemBodyModelPrefab);
            SecondItemBodyModelPrefab.GetComponent<ItemDisplay>().rendererInfos = ItemHelpers.ItemDisplaySetup(SecondItemBodyModelPrefab);

            Vector3 generalScale = new Vector3(1f, 1f, 1f);
            ItemDisplayRuleDict rules = new ItemDisplayRuleDict(new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0f, 0.12f, 0.25f),
                    localAngles = new Vector3(0f, 180f, 0f),
                    localScale = generalScale
                }
            });
            rules.Add("mdlHuntress", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(-0.1f, 0.1f, 0.19f),
                    localAngles = new Vector3(0f, 130f, 0f),
                    localScale = generalScale * 0.9f
                }
            });
            rules.Add("mdlToolbot", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = SecondItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(-0.1f, -0.3f, 3.3f),
                    localAngles = new Vector3(0f, 180f, 0f),
                    localScale = generalScale
                }
            });
            rules.Add("mdlEngi", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0f, 0.1f, 0.29f),
                    localAngles = new Vector3(10f, 180f, 0f),
                    localScale = generalScale
                }
            });
            rules.Add("mdlMage", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0f, 0.025f, 0.19f),
                    localAngles = new Vector3(11f, 180f, 0f),
                    localScale = generalScale * 0.9f
                }
            });
            rules.Add("mdlMerc", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0f, 0f, 0.2f),
                    localAngles = new Vector3(0f, 180f, 0f),
                    localScale = generalScale
                }
            });
            rules.Add("mdlTreebot", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Base",
                    localPos = new Vector3(-0.85f, 0.25f, 0f),
                    localAngles = new Vector3(0f, 180f, 90f),
                    localScale = generalScale
                }
            });
            rules.Add("mdlLoader", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0f, 0.05f, 0.3f),
                    localAngles = new Vector3(0f, 180f, 0f),
                    localScale = generalScale
                }
            });
            rules.Add("mdlCroco", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = SecondItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0f, 0.5f, -2.6f),
                    localAngles = new Vector3(-20f, 180f, 0f),
                    localScale = generalScale
                }
            });
            rules.Add("mdlCaptain", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0.05f, 0.1f, 0.22f),
                    localAngles = new Vector3(5f, 210f, 0f),
                    localScale = generalScale
                }
            });
            return rules;
        }
        public override void Install()
        {
            base.Install();

            itemDef.pickupModelPrefab.transform.localScale = new Vector3(3f, 3f, 3f);

            On.RoR2.HealthComponent.TakeDamage += CalculateBuff;
            GetStatCoefficients += AddMaxShield;
            GetStatCoefficients += AddSecondWindBuff;
            On.RoR2.CharacterBody.RemoveBuff += AddWindedDebuff;
        }

        public override void Uninstall()
        {
            base.Uninstall();

            GetStatCoefficients -= AddMaxShield;
            On.RoR2.HealthComponent.TakeDamage -= CalculateBuff;
            GetStatCoefficients -= AddSecondWindBuff;
            On.RoR2.CharacterBody.RemoveBuff -= AddWindedDebuff;
        }
        private void AddMaxShield(CharacterBody sender, StatHookEventArgs args)
        {
            var inventoryCount = GetCount(sender);
            if (inventoryCount > 0)
            {
                args.baseShieldAdd += ((sender.maxHealth * baseStackHPPercent) + ((sender.maxHealth * addStackHPPercent) * (inventoryCount - 1)));
            }
        }
        private void AddWindedDebuff(On.RoR2.CharacterBody.orig_RemoveBuff orig, CharacterBody self, BuffIndex buffType)
        {
            orig(self, buffType);
            if (buffType == SecondWindBuff)
            {
                self.AddTimedBuffAuthority(WindedDebuff, windedDebuffDuration);
            }             
        }

        private void CalculateBuff(On.RoR2.HealthComponent.orig_TakeDamage orig, HealthComponent self, DamageInfo damageInfo)
        {
            var preDamageShield = self.body.healthComponent.shield; 
            orig(self, damageInfo);
            var inventoryCount = GetCount(self.body);
            if (inventoryCount > 0 && damageInfo.rejected != true)
            {
                float dmgTaken = damageInfo.damage;
                if (dmgTaken >= preDamageShield && preDamageShield > 0)
                {
                    if (self.body.GetBuffCount(SecondWindBuff) == 0 && self.body.GetBuffCount(WindedDebuff) == 0)
                    {
                        self.body.AddTimedBuffAuthority(SecondWindBuff, secondWindBaseDuration + (secondWindBonusMultiplier * (self.body.maxShield / self.body.maxHealth)));
                    }
                }
            }
        }
        private void AddSecondWindBuff(CharacterBody sender, StatHookEventArgs args)
        {
            var InventoryCount = GetCount(sender);
            if (sender.HasBuff(SecondWindBuff))
            {
                args.moveSpeedMultAdd += (secondWindBaseSpeedPercent + ((InventoryCount-1) * secondWindAddSpeedPercent));

            }
        }

    }
}