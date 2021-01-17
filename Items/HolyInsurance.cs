using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using R2API;
using RoR2;
using UnityEngine;
using TILER2;
using SupplyDrop.Utils;
using UnityEngine.UI;

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
        Dictionary<string, Range> InsuranceDictionary = new Dictionary<string, Range>();

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

            InsuranceDictionary.Add("Tier0", new Range(0, 1));
            InsuranceDictionary.Add("BeetleMonster", new Range(1, 2));
            InsuranceDictionary.Add("BeetleGuardMonster", new Range(1, 2));
            InsuranceDictionary.Add("LemurianMonster", new Range(2, 3));
            InsuranceDictionary.Add("LemurianBruiserMonster", new Range(2, 3));
            InsuranceDictionary.Add("Wisp1Monster", new Range(3, 4));
            InsuranceDictionary.Add("GreaterWispMonster", new Range(3, 4));
            InsuranceDictionary.Add("MagmaWorm", new Range(4, 5));
            InsuranceDictionary.Add("BrotherMonster", new Range(6, uint.MaxValue));
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

            On.RoR2.Run.Start += PolicyUpgradePriceCalculator;
            On.RoR2.DeathRewards.OnKilledServer += MoneyReduction;
            On.RoR2.CharacterMaster.OnBodyDeath += CoverageCheck;
            On.RoR2.UI.HUD.Awake += InsuranceUpgradeBar;
        }

        public override void Uninstall()
        {
            base.Uninstall();

            On.RoR2.Run.Start -= PolicyUpgradePriceCalculator;
            On.RoR2.DeathRewards.OnKilledServer -= MoneyReduction;
            On.RoR2.CharacterMaster.OnBodyDeath -= CoverageCheck;
            On.RoR2.UI.HUD.Awake -= InsuranceUpgradeBar;
        }
        public struct Range
        {
            public double Lower;
            public double Upper;
            public Range(double lower, double upper)
            {
                Lower = lower;
                Upper = upper;
            }
            public bool Contains(double value)
            {
                return value >= Lower && value < Upper;
            }
        }
        private void PolicyUpgradePriceCalculator(On.RoR2.Run.orig_Start orig, RoR2.Run self)
        {
            orig(self);

            var xyz = self.difficultyCoefficient;

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

            var insuranceSavingsTrackerComponent = self.gameObject.GetComponent<InsuranceSavingsTracker>();
            if (!insuranceSavingsTrackerComponent)
            {
                self.gameObject.AddComponent<InsuranceSavingsTracker>();
            }

            if (InsuranceDictionary.TryGetValue(attackerComponent.attacker.name, out Range insuranceRange) && insuranceRange.Upper < insuranceSavingsTrackerComponent.insuranceSavings)
            {
                    self.Invoke("RespawnExtraLife", 2f);
                    self.Invoke("PlayExtraLifeSFX", 1f);
                    insuranceSavingsTrackerComponent.insuranceSavings = 0;
            }
            orig(self, body);
        }
        private void InsuranceUpgradeBar(On.RoR2.UI.HUD.orig_Awake orig, RoR2.UI.HUD self)
        {
            orig(self);
            var HUDRoot = self.transform.root;
        }
    }
}
