using System.Collections.ObjectModel;
using System.Collections.Generic;
using R2API;
using RoR2;
using UnityEngine;
using TILER2;
using static TILER2.StatHooks;
using SupplyDrop.Utils;
using System;
using System.Linq;

namespace SupplyDrop.Items
{
    class BloodBook : Item_V2<BloodBook>
    {
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("If set to true, the tome will become haunted with the spirit of a wise-cracking, explosives-loving cursed book.", AutoConfigFlags.None)]
        public bool fearOfReading { get; private set; } = false;

        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("If fear of reading is enabled, this sets the chance the book will speak. A value of 10 equals a 10% chance.", AutoConfigFlags.None)]
        public int chanceBookReads { get; private set; } = 10;

        public override string displayName => "Tome of Bloodletting";

        public override ItemTier itemTier => ItemTier.Tier3;

        public override ReadOnlyCollection<ItemTag> itemTags => new ReadOnlyCollection<ItemTag>(new[] { ItemTag.Damage });
        protected override string GetNameString(string langid = null) => displayName;

        protected override string GetPickupString(string langID = null) => "Convert some damage taken into a temporary damage boost.";

        protected override string GetDescString(string langID = null) => "Convert <style=cIsDamage>10%</style> <style=cStack>(+5% per stack)</style> of the damage you take into a <style=cIsDamage>damage boost</style> " +
            "of up to <style=cIsDamage>20</style> <style=cStack>(+10 per stack)</style> for <style=cIsDamage>4s</style>. The <style=cIsDamage>boost</style> duration is increased based on damage taken; " +
            "every <style=cIsHealth>10%</style> max health that was depleted, up to <style=cIsHealth>50%</style>, increases the duration by <style=cIsDamage>+2s</style>.";

        protected override string GetLoreString(string landID = null) => "Nature learns from pain. Nature willingly suffers, without protest. Nature studies what gifts pain provides. " +
            "It takes the lessons that pain gives out freely, and it grows stronger, more capable of survival." +
            "\n\nHumanity has disrupted this order. It no longer wishes to learn from its greatest teacher. It is an insolent pupil, forgetting its place." +
            "\n\nHumanity will learn. But you may spare yourself of the harsh lesson coming. Pain offers its tutelage to all." +
            "\n\nSimply turn the page." +
            "\n\nSteel yourself." +
            "\n\nAnd begin.";

        private static List<CharacterBody> Playername = new List<CharacterBody>();
        public static GameObject ItemBodyModelPrefab;
        public static GameObject ItemFollowerPrefab;

        public static Range[] ranges;
        public static BuffIndex PatheticBloodBuff { get; private set; }
        public static BuffIndex WeakBloodBuff { get; private set; }
        public static BuffIndex AverageBloodBuff { get; private set; }
        public static BuffIndex StrongBloodBuff { get; private set; }
        public static BuffIndex InsaneBloodBuff { get; private set; }
        public static BuffIndex DevotedBloodBuff { get; private set; }

        public BloodBook()
        {
            modelResourcePath = "@SupplyDrop:Assets/Main/Models/Prefabs/BloodBook.prefab";
            iconResourcePath = "@SupplyDrop:Assets/Main/Textures/Icons/BloodBookIcon.png";
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

            var patheticBloodBuff = new CustomBuff(
                    new BuffDef
                    {
                        canStack = false,
                        isDebuff = false,
                        name = "PatheticBloodBuff",
                        iconPath = "@SupplyDrop:Assets/Main/Textures/Icons/BloodBookBuffIcon1.png"
                    });
            PatheticBloodBuff = BuffAPI.Add(patheticBloodBuff);

            var weakBloodBuff = new CustomBuff(
                new BuffDef
                {
                    canStack = false,
                    isDebuff = false,
                    name = "WeakBloodBuff",
                    iconPath = "@SupplyDrop:Assets/Main/Textures/Icons/BloodBookBuffIcon2.png"
                });
            WeakBloodBuff = BuffAPI.Add(weakBloodBuff);

            var averageBloodBuff = new R2API.CustomBuff(
                new BuffDef
                {
                    canStack = false,
                    isDebuff = false,
                    name = "AverageBloodBuff",
                    iconPath = "@SupplyDrop:Assets/Main/Textures/Icons/BloodBookBuffIcon3.png"
                });
            AverageBloodBuff = BuffAPI.Add(averageBloodBuff);

            var strongBloodBuff = new CustomBuff(
                new BuffDef
                {
                    canStack = false,
                    isDebuff = false,
                    name = "StrongBloodBuff",
                    iconPath = "@SupplyDrop:Assets/Main/Textures/Icons/BloodBookBuffIcon4.png"
                });
            StrongBloodBuff = BuffAPI.Add(strongBloodBuff);

            var insaneBloodBuff = new CustomBuff(
                new BuffDef
                {
                    canStack = false,
                    isDebuff = false,
                    name = "InsaneBloodBuff",
                    iconPath = "@SupplyDrop:Assets/Main/Textures/Icons/BloodBookBuffIcon5.png"                    
                });
            InsaneBloodBuff = BuffAPI.Add(insaneBloodBuff);

            var devotedBloodBuff = new CustomBuff(
                new BuffDef
                {
                    canStack = false,
                    isDebuff = false,
                    name = "DevotedBloodBuff",
                    iconPath = "@SupplyDrop:Assets/Main/Textures/Icons/BloodBookBuffIcon6.png"                    
                });
            DevotedBloodBuff = BuffAPI.Add(devotedBloodBuff);
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

        private static ItemDisplayRuleDict GenerateItemDisplayRules()
        {           
            var ItemFollower = ItemBodyModelPrefab.AddComponent<Utils.ItemFollower>();
            ItemFollower.itemDisplay = ItemBodyModelPrefab.AddComponent<RoR2.ItemDisplay>();
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
                    localPos = new Vector3(-0.5f, -0.5f, -0.2f),
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

            itemDef.pickupModelPrefab.transform.localScale = new Vector3(.85f, .85f, .85f);
            
            On.RoR2.HealthComponent.TakeDamage += ApplyBloodBookBuff;
            GetStatCoefficients += AddBloodBuffStats;
            On.RoR2.CharacterBody.RemoveBuff -= DamageBoostReset;
        }
        public override void Uninstall()
        {
            base.Uninstall();
            On.RoR2.HealthComponent.TakeDamage -= ApplyBloodBookBuff;
            GetStatCoefficients -= AddBloodBuffStats;
            On.RoR2.CharacterBody.RemoveBuff -= DamageBoostReset;
        }
        public struct Range
        {
            public double Lower;
            public double Upper;
            public BuffIndex Buff;
            public int Duration;

            public Range(double lower, double upper, BuffIndex buff, int duration)
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

                    //Bombinomicon stuff is here 
                    if (fearOfReading == true)
                    {
                        int willBookRead = new System.Random().Next(1, 101);

                        if (willBookRead <= chanceBookReads)
                        {
                            AkSoundEngine.PostEvent(4030648726u, self.body.gameObject);
                        }
                    }
                }               
            }
            orig(self, damageInfo);
        }
        private void DamageBoostReset(On.RoR2.CharacterBody.orig_RemoveBuff orig, CharacterBody self, BuffIndex buffType)
        {
            orig(self, buffType);

            if (buffType == PatheticBloodBuff || buffType == WeakBloodBuff || buffType == AverageBloodBuff || buffType == StrongBloodBuff || buffType == InsaneBloodBuff || buffType == DevotedBloodBuff)
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
                    args.baseDamageAdd += Mathf.Min(.1f * cachedDamageComponent.cachedDamage + (.05f * (inventoryCount - 1)), (20 + ((inventoryCount - 1) * 10)));
                }
            }
        }
        public class DamageComponent : MonoBehaviour
        {
            public float cachedDamage = 0f;
        }
    }
}