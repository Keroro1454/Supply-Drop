using System;
using System.Collections.Generic;
using System.Linq;
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
    public class BloodBook : ItemBase<BloodBook>
    {
        //Config Stuff

        public static ConfigOption<bool> fearOfReading;
        public static ConfigOption<float> chanceBookReads;
        public static ConfigOption<float> baseDamageBoostLimit;
        public static ConfigOption<float> addDamageBoostLimit;
        public static ConfigOption<float> baseDamageConversionPercent;
        public static ConfigOption<float> addDamageConversionPercent;

        //Item Data
        public override string ItemName => "Tome of Bloodletting";

        public override string ItemLangTokenName => "BLOOD_BOOK";

        public override string ItemPickupDesc => "Convert some damage taken into a temporary <style=cIsDamage>damage boost</style>.";

        public override string ItemFullDescription => $"Convert <style=cIsDamage>{FloatToPercentageString(baseDamageConversionPercent)}</style> " +
            $"<style=cStack>(+{FloatToPercentageString(addDamageConversionPercent)} per stack)</style> of the damage you take into a <style=cIsDamage>damage boost</style> " +
            $"of up to <style=cIsDamage>{baseDamageBoostLimit}</style> <style=cStack>(+{addDamageBoostLimit} per stack)</style> for <style=cIsDamage>4s</style>. " +
            $"The <style=cIsDamage>boost</style> duration is increased based on damage taken; every <style=cIsHealth>10%</style> max health that was depleted, " +
            $"up to <style=cIsHealth>50%</style>, increases the duration by <style=cIsDamage>+2s</style>.";

        public override string ItemLore => "Nature learns from pain. Nature willingly suffers, without protest. Nature studies what gifts pain provides. " +
            "It takes the lessons that pain gives out freely, and it grows stronger, more capable of survival." +
            "\n\nHumanity has disrupted this order. It no longer wishes to learn from its greatest teacher. It is an insolent pupil, forgetting its place." +
            "\n\nHumanity will learn. But you may spare yourself of the harsh lesson coming. Pain offers its tutelage to all." +
            "\n\nSimply turn the page." +
            "\n\nSteel yourself." +
            "\n\nAnd begin.";

        public override ItemTier Tier => ItemTier.Tier3;
        public override ItemTag[] ItemTags => new ItemTag[] { ItemTag.Damage, ItemTag.AIBlacklist };


        public override GameObject ItemModel => MainAssets.LoadAsset<GameObject>("BloodBook.prefab");
        public override Sprite ItemIcon => MainAssets.LoadAsset<Sprite>("BloodBookIcon");


        public static GameObject ItemBodyModelPrefab;
        public static GameObject ItemFollowerPrefab;

        private static List<CharacterBody> Playername = new List<CharacterBody>();

        public static Range[] ranges;
        public static BuffDef PatheticBloodBuff { get; private set; }
        public static BuffDef WeakBloodBuff { get; private set; }
        public static BuffDef AverageBloodBuff { get; private set; }
        public static BuffDef StrongBloodBuff { get; private set; }
        public static BuffDef InsaneBloodBuff { get; private set; }
        public static BuffDef DevotedBloodBuff { get; private set; }

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
            fearOfReading = config.ActiveBind<bool>("Item: " + ItemName, "Dare Ye Gaze Upon This Accursed Tome?", false, "Should the tome of bloodletting be haunted with the spirit of a wise-cracking, explosives-loving cursed book?");
            chanceBookReads = config.ActiveBind<float>("Item: " + ItemName, "Chance of Book Speaking", .1f, "How often should the tome speak when you are damaged, in percentage? (.1 = 10%)");
            baseDamageBoostLimit = config.ActiveBind<float>("Item: " + ItemName, "Max Damage Boost Obtainable with 1 Tome of Bloodletting", 20f, "What should be the max damage boost obtainable with a single tome of bloodletting?");
            addDamageBoostLimit = config.ActiveBind<float>("Item: " + ItemName, "Additional Max Damage Boost Obtainable per Tome of Bloodletting", 10f, "How much should the max damage boost obtainable increase by for each tome of bloodletting after the first??");
            baseDamageConversionPercent = config.ActiveBind<float>("Item: " + ItemName, "Percent of Damage Taken Converted Into Damage Boost", .1f, "What percentage of the damage you take should be converted into the damage boost with a single tome of bloodletting? (.1 = 10%)");
            addDamageConversionPercent = config.ActiveBind<float>("Item: " + ItemName, "Additional Armor Gained on Kill per Hardened Bone Fragments", .1f, "What additional percentage of the damage you take should each tome of bloodletting, after the first, convert into the damage boost?");
        }
        private void CreateBuff()
        {
            PatheticBloodBuff = ScriptableObject.CreateInstance<BuffDef>();
            PatheticBloodBuff.name = "SupplyDrop Blood Book Buff 1";
            PatheticBloodBuff.canStack = false;
            PatheticBloodBuff.isDebuff = false;
            PatheticBloodBuff.iconSprite = MainAssets.LoadAsset<Sprite>("BloodBookBuffIcon1");

            ContentAddition.AddBuffDef(PatheticBloodBuff);

            WeakBloodBuff = ScriptableObject.CreateInstance<BuffDef>();
            WeakBloodBuff.name = "SupplyDrop Blood Book Buff 2";
            WeakBloodBuff.canStack = false;
            WeakBloodBuff.isDebuff = false;
            WeakBloodBuff.iconSprite = MainAssets.LoadAsset<Sprite>("BloodBookBuffIcon2");

            ContentAddition.AddBuffDef(WeakBloodBuff);

            AverageBloodBuff = ScriptableObject.CreateInstance<BuffDef>();
            AverageBloodBuff.name = "SupplyDrop Blood Book Buff 3";
            AverageBloodBuff.canStack = false;
            AverageBloodBuff.isDebuff = false;
            AverageBloodBuff.iconSprite = MainAssets.LoadAsset<Sprite>("BloodBookBuffIcon3");

            ContentAddition.AddBuffDef(AverageBloodBuff);

            StrongBloodBuff = ScriptableObject.CreateInstance<BuffDef>();
            StrongBloodBuff.name = "SupplyDrop Blood Book Buff 4";
            StrongBloodBuff.canStack = false;
            StrongBloodBuff.isDebuff = false;
            StrongBloodBuff.iconSprite = MainAssets.LoadAsset<Sprite>("BloodBookBuffIcon4");

            ContentAddition.AddBuffDef(StrongBloodBuff);

            InsaneBloodBuff = ScriptableObject.CreateInstance<BuffDef>();
            InsaneBloodBuff.name = "SupplyDrop Blood Book Buff 5";
            InsaneBloodBuff.canStack = false;
            InsaneBloodBuff.isDebuff = false;
            InsaneBloodBuff.iconSprite = MainAssets.LoadAsset<Sprite>("BloodBookBuffIcon5");

            ContentAddition.AddBuffDef(InsaneBloodBuff);

            DevotedBloodBuff = ScriptableObject.CreateInstance<BuffDef>();
            DevotedBloodBuff.name = "SupplyDrop Blood Book Buff 6";
            DevotedBloodBuff.canStack = false;
            DevotedBloodBuff.isDebuff = false;
            DevotedBloodBuff.iconSprite = MainAssets.LoadAsset<Sprite>("BloodBookBuffIcon6");

            ContentAddition.AddBuffDef(DevotedBloodBuff);
        }
        public void SetupAttributes()
        {
            ranges = new Range[]
            {
                new Range(0, 10, PatheticBloodBuff, 4),
                new Range(10, 20, WeakBloodBuff, 6),
                new Range(20, 30, AverageBloodBuff, 8),
                new Range(30, 40, StrongBloodBuff, 10),
                new Range(40, 50, InsaneBloodBuff, 12),
                new Range(50, double.PositiveInfinity, DevotedBloodBuff, 14)
            };
        }
        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            ItemBodyModelPrefab = MainAssets.LoadAsset<GameObject>("BloodBookTracker.prefab");
            ItemFollowerPrefab = ItemModel;

            var ItemFollower = ItemBodyModelPrefab.AddComponent<Utils.ItemFollower>();
            ItemFollower.itemDisplay = ItemBodyModelPrefab.AddComponent<ItemDisplay>();
            ItemFollower.itemDisplay.rendererInfos = ItemHelpers.ItemDisplaySetup(ItemBodyModelPrefab);
            ItemFollower.followerPrefab = ItemFollowerPrefab;
            ItemFollower.targetObject = ItemBodyModelPrefab;
            ItemFollower.distanceDampTime = 0.15f;
            ItemFollower.distanceMaxSpeed = 100;
            ItemFollower.SmoothingNumber = 0.25f;

            Vector3 generalScale = new Vector3(0.08f, 0.08f, 0.08f);

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
            rules.Add("mdlBandit2", new ItemDisplayRule[]
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
        public override void Hooks()
        {
            //itemDef.pickupModelPrefab.transform.localScale = new Vector3(.85f, .85f, .85f);
            
            On.RoR2.HealthComponent.TakeDamage += ApplyBloodBookBuff;
            GetStatCoefficients += AddBloodBuffStats;
            On.RoR2.CharacterBody.RemoveBuff_BuffIndex -= DamageBoostReset;
        }
        public struct Range
        {
            public double Lower;
            public double Upper;
            public BuffDef Buff;
            public int Duration;

            public Range(double lower, double upper, BuffDef buff, int duration)
            {
                Lower = lower;
                Upper = upper;
                Buff = buff;
                Duration = duration;
            }
            public bool Contains(double value)
            {
                return value >= Lower && value <= Upper;
            }
        }
        private void ApplyBloodBookBuff(On.RoR2.HealthComponent.orig_TakeDamage orig, RoR2.HealthComponent self, RoR2.DamageInfo damageInfo)
        {
            int inventoryCount = GetCount(self.body);
            float dmgTaken = damageInfo.damage;
            float maxHealth = self.body.maxHealth;

            if (inventoryCount > 0)
            {
                //This bit will cache the damage you took for use by the actual damage boost calculator, only if the damage exceeds any previous cached damage numbers
                var cachedDamageComponent = self.body.gameObject.GetComponent<DamageComponent>();
                if (!cachedDamageComponent)
                {
                    cachedDamageComponent = self.body.gameObject.AddComponent<DamageComponent>();
                }

                if (cachedDamageComponent.cachedDamage < dmgTaken)
                {
                    cachedDamageComponent.cachedDamage = dmgTaken;
                }

                //Check your current buff, and what your potential next buff would be. currentBuffLevel returns -1 if you don't have a buff already
                int currentBuffLevel = Array.FindIndex(ranges, r => self.body.HasBuff(r.Buff));
                int nextBuffLevel = Array.FindIndex(ranges, r => r.Contains((dmgTaken / maxHealth) * 100));
                if (nextBuffLevel > currentBuffLevel)
                {
                    if (currentBuffLevel != -1)
                    {
                        self.body.RemoveBuff(ranges[currentBuffLevel].Buff);
                    }

                    self.body.AddTimedBuff(ranges[nextBuffLevel].Buff, ranges[nextBuffLevel].Duration);

                    //Bombinomicon (Fear of Reading) stuff is handled here 
                    if (fearOfReading == true)
                    {
                        int willBookRead = new System.Random().Next(1, 101);

                        if (willBookRead <= (chanceBookReads * 100))
                        {
                           // AkSoundEngine.PostEvent(4030648726u, self.body.gameObject);
                        }
                    }
                }               
            }
            orig(self, damageInfo);
        }
        private void DamageBoostReset(On.RoR2.CharacterBody.orig_RemoveBuff_BuffIndex orig, CharacterBody self, BuffIndex buffType)
        //Resets the cachedDamageComponent variable, preventing it from getting stuck on a value from a high-damage attack
        {
            orig(self, buffType);

            if (buffType == PatheticBloodBuff.buffIndex || buffType == WeakBloodBuff.buffIndex || buffType == AverageBloodBuff.buffIndex || buffType == StrongBloodBuff.buffIndex || buffType == InsaneBloodBuff.buffIndex || buffType == DevotedBloodBuff.buffIndex)
            {
                var cachedDamageComponent = self.gameObject.GetComponent<DamageComponent>();
                cachedDamageComponent.cachedDamage = 0;
            }
        }
        private void AddBloodBuffStats(CharacterBody sender, StatHookEventArgs args)
        {
            var cachedDamageComponent = sender.gameObject.GetComponent<DamageComponent>();
            if (!cachedDamageComponent)
            {
                cachedDamageComponent = sender.gameObject.AddComponent<DamageComponent>();
            }

            var inventoryCount = GetCount(sender);
           
            if (inventoryCount > 0)
            {
                int currentBuffLevel = Array.FindIndex(ranges, r => sender.HasBuff(r.Buff));
                if (Enumerable.Range(0, 5).Contains(currentBuffLevel))
                {
                    args.baseDamageAdd += Mathf.Min(baseDamageConversionPercent * cachedDamageComponent.cachedDamage + (addDamageConversionPercent * (inventoryCount - 1)), (baseDamageBoostLimit + ((inventoryCount - 1) * addDamageBoostLimit)));
                }
            }
        }
        public class DamageComponent : MonoBehaviour
        {
            public float cachedDamage = 0f;
        }
    }
}