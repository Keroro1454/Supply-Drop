using System.Collections.ObjectModel;
using System.Collections.Generic;
using R2API;
using RoR2;
using UnityEngine;
using TILER2;
using static TILER2.StatHooks;
using K1454.SupplyDrop;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using System.Reflection;
using System.Timers;

//TO-DO: Duplicate Model, make it not schmove, set that as the pickupmodel, I can't deal with this bs
namespace SupplyDrop.Items
{
    class UnassumingTie : Item<UnassumingTie>
    {
        public override string displayName => "Unassuming Tie";

        public override ItemTier itemTier => RoR2.ItemTier.Tier1;

        public override ReadOnlyCollection<ItemTag> itemTags => new ReadOnlyCollection<ItemTag>(new[] { ItemTag.Utility });
        protected override string NewLangName(string langid = null) => displayName;

        protected override string NewLangPickup(string langID = null) => "Gain some shield, and receive a speed boost when your shield is broken.";

        protected override string NewLangDesc(string langID = null) => "Gain a <style=cIsUtility>shield</style> equal to <style=cIsUtility>4%</style> <style=cStack>(+4% per stack)</style> of your maximum health. " +
            "Breaking your <style=cIsUtility>shield</style> gives you a <style=cIsUtility>Second Wind</style> for 4s, plus a bonus amount based on your <style=cIsUtility>maximum shield</style>. " +
            "Second Wind increases <style=cIsUtility>movement speed</style> by <style=cIsUtility>15%</style> <style=cStack>(+15% per stack)</style>.";

        protected override string NewLangLore(string landID = null) => "\"This necktie was a staple accessory of one of a notorious group of well-dressed heisters which were active during the early 21st century.The gang was wildly successful while active, breaking into, looting, and escaping from some of the most secure sites on Earth at the time. Even when authorities attempted to apprehend the criminals, reports state that shooting at them 'only seem to make [the heisters] move faster, however the hell that works.' While the identities of these criminals were never discovered, the gang ceased operations for unknown reasons after over a decade of activity. This piece serves as a testament to their dedication to style, no matter the situation.\"\n\n- Placard description for \"Striped Tie\" at the Galactic Museum of Law Enforcement and Criminality";

        private static List<RoR2.CharacterBody> Playername = new List<RoR2.CharacterBody>();
        public static GameObject ItemBodyModelPrefab;
        public static GameObject SecondItemBodyModelPrefab;
        public static GameObject ThirdItemBodyModelPrefab;

        public BuffIndex SecondWindBuff { get; private set; }
        public BuffIndex WindedDebuff { get; private set; }

        public UnassumingTie()
        {
            modelPathName = "@SupplyDrop:Assets/Main/Models/Prefabs/TiePickUp.prefab";
            
            iconPathName = "@SupplyDrop:Assets/Main/Textures/Icons/TieIcon.png";

            onAttrib += (tokenIdent, namePrefix) =>
            {
                var secondWindBuff = new R2API.CustomBuff(
                    new BuffDef
                    {
                        canStack = false,
                        isDebuff = false,
                        name = namePrefix + "SecondWindBuff",
                        iconPath = "@SupplyDrop:Assets/Main/Textures/Icons/SecondWindBuffIcon.png"
                    });
                SecondWindBuff = R2API.BuffAPI.Add(secondWindBuff);

                var windedDebuff = new R2API.CustomBuff(
                    new BuffDef
                    {
                        canStack = false,
                        isDebuff = true,
                        name = namePrefix + "WindedDebuff",
                        iconPath = "@SupplyDrop:Assets/Main/Textures/Icons/WindedDebuffIcon.png"
                    });
                WindedDebuff = R2API.BuffAPI.Add(windedDebuff);
            };
        }

        private static ItemDisplayRuleDict GenerateItemDisplayRules()
        {
            ItemBodyModelPrefab.AddComponent<ItemDisplay>();
            ItemBodyModelPrefab.GetComponent<ItemDisplay>().rendererInfos = SupplyDropPlugin.ItemDisplaySetup(ItemBodyModelPrefab);
            SecondItemBodyModelPrefab.AddComponent<ItemDisplay>();
            SecondItemBodyModelPrefab.GetComponent<ItemDisplay>().rendererInfos = SupplyDropPlugin.ItemDisplaySetup(SecondItemBodyModelPrefab);

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
                    localScale = generalScale * 0.9f
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
        protected override void LoadBehavior()
        {
            ItemBodyModelPrefab = Resources.Load<GameObject>("@SupplyDrop:Assets/Main/Models/Prefabs/Tie.prefab");
            SecondItemBodyModelPrefab = Resources.Load<GameObject>("@SupplyDrop:Assets/Main/Models/Prefabs/TieBig.prefab");
            if (ThirdItemBodyModelPrefab == null)
            {
                ThirdItemBodyModelPrefab = regDef.pickupModelPrefab;
                regItem.ItemDisplayRules = GenerateItemDisplayRules();
            }
            regDef.pickupModelPrefab.transform.localScale = new Vector3(.5f, .5f, .5f);
            On.RoR2.HealthComponent.TakeDamage += CalculateBuff;
            IL.RoR2.CharacterBody.RecalculateStats += IL_AddMaxShield;
            GetStatCoefficients += AddSecondWindBuff;
            On.RoR2.CharacterBody.RemoveBuff += AddWindedDebuff;
        }

        protected override void UnloadBehavior()
        {
            IL.RoR2.CharacterBody.RecalculateStats -= IL_AddMaxShield;
            On.RoR2.HealthComponent.TakeDamage -= CalculateBuff;
            GetStatCoefficients -= AddSecondWindBuff;
            On.RoR2.CharacterBody.RemoveBuff -= AddWindedDebuff;
        }
        private void AddWindedDebuff(On.RoR2.CharacterBody.orig_RemoveBuff orig, RoR2.CharacterBody self, BuffIndex buffType)
        {
            orig(self, buffType);
            if (buffType == SecondWindBuff)
            {
                self.AddTimedBuffAuthority(WindedDebuff, 10f);
            }             
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
                    return shield + (characterBody.maxHealth * (0.04f * GetCount(characterBody)));
                }
                return shield;
            }
            );
            c.Emit(OpCodes.Stloc, 43);
        }
        private void CalculateBuff(On.RoR2.HealthComponent.orig_TakeDamage orig, RoR2.HealthComponent self, RoR2.DamageInfo damageInfo)
        {
            var preDamageShield = self.body.healthComponent.shield; 
            orig(self, damageInfo);
            var inventoryCount = GetCount(self.body);
            if (inventoryCount > 0)
            {
                float dmgTaken = damageInfo.damage;
                if (dmgTaken >= preDamageShield && preDamageShield > 0)
                {
                    if (self.body.GetBuffCount(SecondWindBuff) == 0 && self.body.GetBuffCount(WindedDebuff) == 0)
                    {
                        self.body.AddTimedBuffAuthority(SecondWindBuff, 4f + (5f * (self.body.maxShield / self.body.maxHealth)));
                    }
                }
            }
        }
        private void AddSecondWindBuff(CharacterBody sender, StatHookEventArgs args)
        {
            var InventoryCount = GetCount(sender);
            if (sender.HasBuff(SecondWindBuff))
            {
                args.moveSpeedMultAdd += 0.15f * InventoryCount;

            }
        }

    }
}