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
            "up to 5 times. <style=cDeath>Upon dying</style> to an source you are <style=cIsUtility>insured</style> for, you will return to life, " +
            "and your <style=cIsUtility>insurance</style> level will be reset to zero.";
        protected override string GetLoreString(string langID = null) => "Oops, no lore here. Try back later!";

        private static List<CharacterBody> Playername = new List<CharacterBody>();
        public static GameObject ItemBodyModelPrefab;
        public static GameObject ItemFollowerPrefab;

        public static GameObject InsuranceBar;
        public static GameObject InsuranceBarImage;
        public static GameObject InsuranceBarOutline;

        public Dictionary<string, Range> InsuranceDictionary = new Dictionary<string, Range>();
        //Navigation customNav = new Navigation();

        public HolyInsurance()
        {
            //Don't forget to change these, currently using test model/icon
            modelResourcePath = "@SupplyDrop:Assets/Main/Models/Prefabs/TestModel.prefab";
            iconResourcePath = "@SupplyDrop:Assets/Main/Models/Prefabs/TestIcon.prefab";
        }
        public override void SetupAttributes()
        {
            if (ItemBodyModelPrefab == null)
            {
                ItemBodyModelPrefab = Resources.Load<GameObject>("@SupplyDrop:Assets/Main/Models/Prefabs/BloodBookTracker.prefab");
                ItemFollowerPrefab = Resources.Load<GameObject>(modelResourcePath);
                displayRules = GenerateItemDisplayRules();
            }
            base.SetupAttributes();

            //T1 Coverage
            InsuranceDictionary.Add("BeetleMonster", new Range(0, 1));
            InsuranceDictionary.Add("BeetleGuardMonster", new Range(0, 1));
            //T2 Coverage
            InsuranceDictionary.Add("LemurianMonster", new Range(1, 2));
            InsuranceDictionary.Add("LemurianBruiserMonster", new Range(1, 2));
            //T3 Coverage
            InsuranceDictionary.Add("Wisp1Monster", new Range(2, 3));
            InsuranceDictionary.Add("GreaterWispMonster", new Range(2, 3));
            //T4 Coverage
            InsuranceDictionary.Add("MagmaWorm", new Range(3, 4));
            //T5 Coverage
            InsuranceDictionary.Add("BrotherMonster", new Range(4, 5));
            //T6 Coverage (Not an actual tier, just an catch-all for money past T5)
            InsuranceDictionary.Add("FullyCovered", new Range(5, uint.MaxValue));
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

            On.RoR2.Run.RecalculateDifficultyCoefficentInternal += PolicyUpgradePriceCalculator;
            On.RoR2.DeathRewards.OnKilledServer += MoneyReduction;
            On.RoR2.CharacterMaster.OnBodyDeath += CoverageCheck;
            On.RoR2.UI.HUD.Awake += InsuranceBarAwake;
            On.RoR2.UI.HUD.Update += InsuranceBarUpdate;
        }

        public override void Uninstall()
        {
            base.Uninstall();

            On.RoR2.Run.RecalculateDifficultyCoefficentInternal -= PolicyUpgradePriceCalculator;
            On.RoR2.DeathRewards.OnKilledServer -= MoneyReduction;
            On.RoR2.CharacterMaster.OnBodyDeath -= CoverageCheck;
            On.RoR2.UI.HUD.Awake += InsuranceBarAwake;
            On.RoR2.UI.HUD.Update += InsuranceBarUpdate;
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
        private void PolicyUpgradePriceCalculator(On.RoR2.Run.orig_RecalculateDifficultyCoefficentInternal orig, Run self)
        {
            orig(self);

            var diffCoeff = self.difficultyCoefficient;
            var baseCost = 25 * Mathf.Pow(diffCoeff, 1.25f);

            //Have to redefine all the dictionary entries here to set the Range values to their proper costs
            InsuranceDictionary["BeetleMonster"] = new Range(0, baseCost);
            InsuranceDictionary["BeetleGuardMonster"] = new Range(0, baseCost);
            InsuranceDictionary["LemurianMonster"] = new Range(baseCost, baseCost * 2);
            InsuranceDictionary["LemurianBruiserMonster"] = new Range(baseCost, baseCost * 2);
            InsuranceDictionary["Wisp1Monster"] = new Range(baseCost * 2, baseCost * 4);
            InsuranceDictionary["GreaterWispMonster"] = new Range(baseCost * 2, baseCost * 4);
            InsuranceDictionary["MagmaWorm"] = new Range(baseCost * 4, baseCost * 8);
            InsuranceDictionary["BrotherMonster"] = new Range(baseCost * 8, baseCost * 16);
            InsuranceDictionary["FullyCovered"] = new Range(baseCost * 16, uint.MaxValue);
        }
        private void MoneyReduction(On.RoR2.DeathRewards.orig_OnKilledServer orig, DeathRewards self, DamageReport rep)
        {
            if (rep.attackerBody && self)
            {
                var inventoryCount = GetCount(rep.attackerMaster);
                if (inventoryCount > 0)
                {
                    var insuranceSavingsTrackerComponent = rep.attackerMaster.gameObject.GetComponent<InsuranceSavingsTracker>();
                    if (!insuranceSavingsTrackerComponent)
                    {
                        rep.attackerMaster.gameObject.AddComponent<InsuranceSavingsTracker>();
                    }

                    uint origGold = self.goldReward;
                    uint reducedGold = (uint)Mathf.FloorToInt(self.goldReward * (1 - ((.5f * inventoryCount) / (inventoryCount + 1))));
                    uint investedGold = origGold - reducedGold;
                    self.goldReward = reducedGold;

                    //Could you theoretically go over uint.MaxValue here? idk
                    insuranceSavingsTrackerComponent.insuranceSavings += investedGold;
                    Debug.LogError("The money is actually being tracked. Rn you have " + insuranceSavingsTrackerComponent.insuranceSavings);
                }
            }

            orig(self, rep);
        }
        private void CoverageCheck(On.RoR2.CharacterMaster.orig_OnBodyDeath orig, CharacterMaster self, CharacterBody body)
        {
            if (GetCount(body) > 0)
            {
                var attackerComponent = self.gameObject.GetComponent<DamageReport>();

                var insuranceSavingsTrackerComponent = self.gameObject.GetComponent<InsuranceSavingsTracker>();
                if (!insuranceSavingsTrackerComponent)
                {
                    self.gameObject.AddComponent<InsuranceSavingsTracker>();
                }

                //Tries to get a key with the killer's name. If one exists, throws out the corresponding Range to that key.
                //Then check to ensure that Range's Upper value is less than the amount of gold you saved (confirms you're above that tier)
                if (InsuranceDictionary.TryGetValue(attackerComponent.attacker.name, out Range insuranceRange) && insuranceRange.Upper < insuranceSavingsTrackerComponent.insuranceSavings)
                {
                    self.Invoke("RespawnExtraLife", 2f);
                    self.Invoke("PlayExtraLifeSFX", 1f);

                    //This chunk ensures the extra money you are forced to save once you unlock the final coverage tier isn't wasted,
                    //by only reducing your savings = to the cost of unlocking the final coverage tier (or just to zero, if you haven't unlocked that tier)
                    var savingsComponent = body.gameObject.GetComponent<Run>();
                    if (!savingsComponent)
                    {
                        body.gameObject.AddComponent<Run>();
                    }
                    var diffCoeff = savingsComponent.difficultyCoefficient;
                    var baseCost = Convert.ToUInt32(25 * Math.Pow(diffCoeff, 1.25f));
                    insuranceSavingsTrackerComponent.insuranceSavings = Math.Max(insuranceSavingsTrackerComponent.insuranceSavings - baseCost * 16, 0);
                }
            }
            orig(self, body);
        }
        public void InsuranceBarAwake(On.RoR2.UI.HUD.orig_Awake orig, RoR2.UI.HUD self)
        {
            orig(self);

            var prefab = Resources.Load<GameObject>("@SupplyDrop:Assets/Main/Textures/UI/InsuranceBar.prefab");
            InsuranceBar = GameObject.Instantiate(prefab, self.mainContainer.transform);
            if (InsuranceBar)
            {
                var cachedSavingsComponent = self.targetMaster.gameObject.GetComponent<InsuranceSavingsTracker>();
                if (!cachedSavingsComponent)
                {
                    self.targetMaster.gameObject.AddComponent<InsuranceSavingsTracker>();
                }

                foreach (Range range in InsuranceDictionary.Values)
                {
                    if (cachedSavingsComponent.insuranceSavings >= range.Lower && cachedSavingsComponent.insuranceSavings < range.Upper)
                    {
                        InsuranceBar.GetComponentInChildren<Slider>().maxValue = Convert.ToSingle(range.Upper);
                    }
                }
                InsuranceBar.GetComponentInChildren<Slider>().value = cachedSavingsComponent.insuranceSavings;
            }
        }
        public void InsuranceBarUpdate(On.RoR2.UI.HUD.orig_Update orig, RoR2.UI.HUD self)
        {
            orig(self);

            if (InsuranceBar)
            {
                var cachedSavingsComponent = self.targetMaster.gameObject.GetComponent<InsuranceSavingsTracker>();
                if (!cachedSavingsComponent)
                {
                    self.targetMaster.gameObject.AddComponent<InsuranceSavingsTracker>();
                }

                foreach (Range range in InsuranceDictionary.Values)
                {
                    if (cachedSavingsComponent.insuranceSavings >= range.Lower && cachedSavingsComponent.insuranceSavings < range.Upper)
                    {
                        InsuranceBar.GetComponentInChildren<Slider>().maxValue = Convert.ToSingle(range.Upper);
                    }
                }

                InsuranceBar.AddComponent<InsuranceBarController>();
            }
        }
    }
}
