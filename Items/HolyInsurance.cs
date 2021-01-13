using System.Collections.ObjectModel;
using System.Collections.Generic;
using R2API;
using RoR2;
using UnityEngine;
using TILER2;
using static TILER2.StatHooks;
using static TILER2.MiscUtil;
using SupplyDrop.Utils;
using System;
using System.Linq;

namespace SupplyDrop.Items
{
    public class HolyInsurance : Item_V2<HolyInsurance>
    {
        public override string displayName => "Afterlife Insurance";
        public override ItemTier itemTier => ItemTier.Lunar;
        public override ReadOnlyCollection<ItemTag> itemTags => new ReadOnlyCollection<ItemTag>(new[] { ItemTag.Utility });
        protected override string GetNameString(string langID = null) => displayName;
        protected override string GetPickupString(string langID = null) => "Some of the money you earn is invested into divine insurance (Coverage may vary).";
        protected override string GetDescString(string langID = null) => "Gain 25% <style=cStack>(+25% per stack)</style> less money from all sources. " +
            "100% <style=cStack>(+25% per stack)</style> of money lost is <style=cUtility>invested into upgrading your insurance</style> to cover more threats, " +
            "up to 5 times. <style=cDeath>Upon dying</style> to an source you are <style=cUtility>insured</style> for, you will return to life, " +
            "and your <style=cUtility>insurance</style> level will be reset to zero.";
        protected override string GetLoreString(string langID = null) => "Oops, no lore here. Try back later!";

        private static List<CharacterBody> Playername = new List<CharacterBody>();
        public static GameObject ItemBodyModelPrefab;
        public static GameObject ItemFollowerPrefab;

        public static Range[] ranges;

        public HolyInsurance()
        {
            modelResourcePath = "@SupplyDrop:Assets/Main/Models/Prefabs/HolyInsurance.prefab";
            iconResourcePath = "@SupplyDrop:Assets/Main/Models/Prefabs/HolyInsuranceIcon.prefab";
        }

        public override void SetupAttributes()
        {
            if (ItemBodyModelPrefab == null)
            {
                ItemBodyModelPrefab = Resources.Load<GameObject>("@SupplyDrop:Assets/Main/Models/Prefabs/HolyInsuranceTracker.prefab");
                ItemBodyModelPrefab = Resources.Load<GameObject>(modelResourcePath);
                displayRules = GenerateItemDisplayRules();
            }
            base.SetupAttributes();

            ranges = new Range[]
            {
                new Range(1, 2, "ICE", "N/A"),
                new Range(2, 3, "BeetleMonster", "BeetleGuardMonster"),
                new Range(3, 4, "LemurianMonster", "LemurianBruiserMonster"),
                new Range(4, 5, "Wisp1Monster", "GreaterWispMonster"),
                new Range(5, 6, "MagmaWorm", "N/A"),
                new Range(6, 7, "Mithrix", "N/A")
            };
        }
        private static ItemDisplayRuleDict GenerateItemDisplayRules()
        {
            var ItemFollower = ItemBodyModelPrefab.AddComponent<Utils.ItemFollower>();
            ItemFollower.itemDisplay = ItemBodyModelPrefab.AddComponent<ItemDisplay>();
            ItemFollower.itemDisplay.rendererInfos = ItemHelpers.ItemDisplaySetup(ItemBodyModelPrefab);
            ItemFollower.followerPrefab = ItemFollowerPrefab;
            ItemFollower.targetObject = ItemBodyModelPrefab;
            ItemFollower.distanceDampTime = 0.15f;
            ItemFollower.distanceMaxSpeed = 100;
            ItemFollower.SmoothingNumber = 0.25f;
            Vector3 generalScale = new Vector3(0.08f, 0.08f, 0.08f);
            ItemDisplayRuleDict rules = new ItemDisplayRuleDict(new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Base",
                    localPos = new Vector3(0.4f, -0.45f, -0.2f),
                    localAngles = new Vector3(-90f, 0f, 0f),
                    localScale = generalScale
                }
            });
            rules.Add("mdlHuntress", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Base",
                    localPos = new Vector3(0.3f, -0.7f, 0f),
                    localAngles = new Vector3(-100f, 0f, 0f),
                    localScale = generalScale
                }
            });
            rules.Add("mdlToolbot", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Base",
                    localPos = new Vector3(-4f, -4f, 8f),
                    localAngles = new Vector3(-90f, 180f, 0f),
                    localScale = generalScale * 1.5f
                }
            });
            rules.Add("mdlEngi", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Base",
                    localPos = new Vector3(0.8f, -0.6f, -0.5f),
                    localAngles = new Vector3(-90f, 0f, 0f),
                    localScale = generalScale * 1.25f
                }
            });
            rules.Add("mdlMage", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Base",
                    localPos = new Vector3(-0.6f, -0.6f, -0.2f),
                    localAngles = new Vector3(-90f, 0f, 0f),
                    localScale = generalScale
                }
            });
            rules.Add("mdlMerc", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Base",
                    localPos = new Vector3(-0.5f, -0.7f, -0.3f),
                    localAngles = new Vector3(-90f, 0f, 0f),
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
                    localPos = new Vector3(-1.5f, -1f, -2.5f),
                    localAngles = new Vector3(-90f, 0f, 0f),
                    localScale = generalScale * 1.5f
                }
            });
            rules.Add("mdlLoader", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Base",
                    localPos = new Vector3(-0.5f, -0.8f, -0.6f),
                    localAngles = new Vector3(-90f, 0f, 0f),
                    localScale = generalScale * 1.25f
                }
            });
            rules.Add("mdlCroco", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Base",
                    localPos = new Vector3(-7f, 7f, 5f),
                    localAngles = new Vector3(90f, 0f, 0f),
                    localScale = generalScale * 2
                }
            });
            rules.Add("mdlCaptain", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Base",
                    localPos = new Vector3(-0.7f, -0.8f, -0.5f),
                    localAngles = new Vector3(-90f, 0f, 0f),
                    localScale = generalScale
                }
            });
            rules.Add("mdlBandit", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Base",
                    localPos = new Vector3(0.4f, -0.45f, -0.2f),
                    localAngles = new Vector3(-90f, 0f, 0f),
                    localScale = generalScale
                }
            });
            return rules;
        }
        public override void Install()
        {
            base.Install();

            On.RoR2.DeathRewards.OnKilledServer += MoneyReduction;
            On.RoR2.CharacterMaster.OnBodyDeath += CoverageCheck;
        }

        public override void Uninstall()
        {
            base.Uninstall();

            On.RoR2.DeathRewards.OnKilledServer -= MoneyReduction;
            On.RoR2.CharacterMaster.OnBodyDeath -= CoverageCheck;
        }
        public struct Range
        {
            public double Lower;
            public double Upper;
            public string Attacker;
            public string Attacker2;
            public Range(double lower, double upper, string attacker, string attacker2)
            {
                Lower = lower;
                Upper = upper;
                Attacker = attacker;
                Attacker2 = attacker2;
            }
            public bool Contains(double value)
            {
                return value >= Lower && value <= Upper;
            }
        }
        private void MoneyReduction(On.RoR2.DeathRewards.orig_OnKilledServer orig, DeathRewards self, DamageReport rep)
        {
            uint inventoryCount = Convert.ToUInt32(GetCount(rep.attackerBody));

            uint reducedGold = (uint)Mathf.FloorToInt(self.goldReward * (1 - ((25 * inventoryCount) / 100 + (25 * (inventoryCount - 1)))));
            uint investedGold = (uint)Mathf.FloorToInt(self.goldReward - reducedGold + ((self.goldReward - reducedGold) * ((inventoryCount - 1) / 4)));
            self.goldReward = reducedGold;

            var insuranceSavingsTrackerComponent = rep.attackerBody.master.gameObject.GetComponent<InsuranceSavingsTracker>();
            if (!insuranceSavingsTrackerComponent)
            {
                rep.attackerBody.master.gameObject.AddComponent<InsuranceSavingsTracker>();
            }

            insuranceSavingsTrackerComponent.insuranceSavings += investedGold;

            orig(self, rep);
        }
        private void CoverageCheck(On.RoR2.CharacterMaster.orig_OnBodyDeath orig, CharacterMaster self, CharacterBody body)
        {
            var attackerComponent = self.gameObject.GetComponent<DamageReport>();

            if (Array.Exists(ranges, r => r.Equals(attackerComponent.attacker.name)))
            {
                var insuranceSavingsTrackerComponent = self.gameObject.GetComponent<InsuranceSavingsTracker>();
                if (!insuranceSavingsTrackerComponent)
                {
                    self.gameObject.AddComponent<InsuranceSavingsTracker>();
                }

                int topTierAffordable = Array.FindIndex(ranges, r => r.Contains(insuranceSavingsTrackerComponent.insuranceSavings));
                int tierNeeded = Array.FindIndex(ranges, r => r.Equals(attackerComponent.attacker.name));
                if (topTierAffordable >= tierNeeded && topTierAffordable != -1 && tierNeeded != -1)
                {
                    self.Invoke("RespawnExtraLife", 2f);
                    self.Invoke("PlayExtraLifeSFX", 1f);
                    insuranceSavingsTrackerComponent.insuranceSavings = 0;
                }
            }
            orig(self, body);
        }
    }
}
