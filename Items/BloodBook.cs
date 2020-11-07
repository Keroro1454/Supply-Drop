using System.Collections.ObjectModel;
using System.Collections.Generic;
using R2API;
using RoR2;
using UnityEngine;
using TILER2;
using static TILER2.StatHooks;
using K1454.Utils;
using System;
using System.Linq;


//STILL NEEDS: Rigging to characters, improved icon. That's it tho!
namespace SupplyDrop.Items
{
    class BloodBook : Item_V2<BloodBook>
    {
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("If set to true, the tome will become haunted with the spirit of a wise-cracking, explosives-loving cursed book.", AutoConfigFlags.None)]
        public bool fearOfReading { get; private set; } = true;

        public override string displayName => "Tome of Bloodletting";

        public override ItemTier itemTier => ItemTier.Tier3;

        public override ReadOnlyCollection<ItemTag> itemTags => new ReadOnlyCollection<ItemTag>(new[] { ItemTag.Damage });
        protected override string GetNameString(string langid = null) => displayName;

        protected override string GetPickupString(string langID = null) => "Convert some damage taken into a temporary damage boost.";

        protected override string GetDescString(string langID = null) => "Convert <style=cIsDamage>10%</style> <style=cStack>(+10% per stack)</style> of the damage you take into a <style=cIsDamage>temporary damage boost</style>, " +
            "up to <style=cIsDamage>20</style> <style=cStack>(+10 per stack)</style>. The boost is powered up based on damage taken; every 10% max HP, up to 50%, that was depleted increases the base duration of 4s by +2s.";

        protected override string GetLoreString(string landID = null) => "Nature learns from pain. Nature willingly suffers, without protest. Nature studies what gifts pain provides. " +
            "It takes the lessons that pain gives out freely, and it grows stronger, more capable of survival." +
            "\n\nHumanity has disrupted this order. It no longer wishes to learn from its greatest teacher. It is an insolent pupil, forgetting its place." +
            "\n\nHumanity will learn. But you may spare yourself of the harsh lesson coming. Pain offers its tutelage to all." +
            "\n\nSimply turn the page." +
            "\n\nSteel yourself." +
            "\n\nAnd begin.";

        private static List<CharacterBody> Playername = new List<CharacterBody>();
        public static GameObject ItemBodyModelPrefab;
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
                ItemBodyModelPrefab = Resources.Load<GameObject>(modelResourcePath);
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
            ItemBodyModelPrefab.AddComponent<ItemDisplay>();
            ItemBodyModelPrefab.GetComponent<ItemDisplay>().rendererInfos = ItemHelpers.ItemDisplaySetup(ItemBodyModelPrefab);

            Vector3 generalScale = new Vector3(0.08f, 0.08f, 0.08f);
            ItemDisplayRuleDict rules = new ItemDisplayRuleDict(new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0.75f, 0.8f, -0.5f),
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
                    childName = "Head",
                    localPos = new Vector3(0f, 0.1f, 0f),
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
                    childName = "Chest",
                    localPos = new Vector3(-6f, 5f, -4f),
                    localAngles = new Vector3(0f, 0f, 90f),
                    localScale = generalScale * 8
                }
            });
            rules.Add("mdlEngi", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(-0.6f, 0.65f, 0.5f),
                    localAngles = new Vector3(0f, 0f, 90f),
                    localScale = new Vector3(0.02f, 0.02f, 0.02f)
                }
            });
            rules.Add("mdlMage", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(-0.5f, 0.6f, -0.5f),
                    localAngles = new Vector3(-22.5f, 0f, 0f),
                    localScale = new Vector3(0.02f, 0.02f, 0.02f)
                }
            });
            rules.Add("mdlMerc", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(-0.5f, 0.5f, -0.45f),
                    localAngles = new Vector3(0f, 0f, 90f),
                    localScale = new Vector3(0.02f, 0.02f, 0.02f)
                }
            });
            rules.Add("mdlTreebot", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "FlowerBase",
                    localPos = new Vector3(-1.4f, 1.5f, -0.3f),
                    localAngles = new Vector3(0f, 0f, 90f),
                    localScale = new Vector3(0.04f, 0.04f, 0.04f)
                }
            });
            rules.Add("mdlLoader", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(-0.6f, 0.75f, -0.6f),
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
                    childName = "Chest",
                    localPos = new Vector3(5f, 7f, 3f),
                    localAngles = new Vector3(340f, 180f, 90f),
                    localScale = generalScale * 8
                }
            });
            rules.Add("mdlCaptain", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(-0.6f, 0.6f, -0.5f),
                    localAngles = new Vector3(0f, 0f, 90f),
                    localScale = new Vector3(0.02f, 0.02f, 0.02f)
                }
            });
            return rules;
        }

        public override void Install()
        {
            base.Install();

            itemDef.pickupModelPrefab.transform.localScale = new Vector3(.5f, .5f, .5f);
            itemDef.pickupModelPrefab.transform.localEulerAngles = new Vector3(0f, 0f, 90f);
            


            On.RoR2.HealthComponent.TakeDamage += ApplyBloodBookBuff;
            GetStatCoefficients += AddBloodBuffStats;
            On.RoR2.CharacterBody.RemoveBuff -= DamageBoostReset;
            ItemBodyModelPrefab.AddComponent<BleedingScript>();
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
            var inventoryCount = GetCount(self.body);
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

                //This block is designed to ensure you have one of the buffs before the array is accessed
                int currentBuffLevel = Array.FindIndex(ranges, r => self.body.HasBuff(r.Buff));
                if (currentBuffLevel != -1)
                {
                    int nextBuffLevel = Array.FindIndex(ranges, r => r.Contains((dmgTaken / maxHealth) * 100));
                    if (nextBuffLevel > currentBuffLevel)
                    {
                        self.body.RemoveBuff(ranges[currentBuffLevel].Buff);
                        self.body.AddTimedBuff(ranges[nextBuffLevel].Buff, ranges[nextBuffLevel].Duration);

                        //Bombinomicon here. Whenever a buff is applied while config is true, there is a 1 in 10 chance 
                        if (fearOfReading == true)
                        {
                            int willBookRead = new System.Random().Next(9);

                            if (willBookRead >= 0)
                            {
                                AkSoundEngine.PostEvent(1656833108u, self.body.gameObject);
                                Chat.AddMessage("Ay, dis is gonna be good!");
                            }
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
            var cachedDamageComponent = sender.GetComponent<DamageComponent>();
            var InventoryCount = GetCount(sender);

            int currentBuffLevel = Array.FindIndex(ranges, r => sender.HasBuff(r.Buff));

            if (Enumerable.Range(0, 5).Contains(currentBuffLevel))
            {
                args.baseDamageAdd += Mathf.Min(.1f * cachedDamageComponent.cachedDamage + (.05f * (InventoryCount - 1)), 20);
            }
        }
        public class BleedingScript : MonoBehaviour
        {
            public ParticleSystem particles;    
            public CharacterModel model;

            public void Awake()
            {
                model = GetComponentInParent<CharacterModel>();
            }
            public void FixedUpdate()
            {
                var particleSystem = particles;
                int currentBuffLevel = Array.FindIndex(ranges, r => model.body.HasBuff(r.Buff));
                if (Enumerable.Range(0, 5).Contains(currentBuffLevel))
                {
                    if (!particleSystem.isPlaying)
                    {
                        if (currentBuffLevel == 0)
                        {
                            var newDripSpeed = particleSystem.emission;
                            newDripSpeed.rateOverTime = 1f;
                        }
                        if (currentBuffLevel == 1)
                        {
                            var newDripSpeed = particleSystem.emission;
                            newDripSpeed.rateOverTime = 2f;
                        }
                        if (currentBuffLevel == 2)
                        {
                            var newDripSpeed = particleSystem.emission;
                            newDripSpeed.rateOverTime = 5f;
                        }
                        if (currentBuffLevel == 3)
                        {
                            var newDripSpeed = particleSystem.emission;
                            newDripSpeed.rateOverTime = 10f;
                        }
                        if (currentBuffLevel == 4)
                        {
                            var newDripSpeed = particleSystem.emission;
                            newDripSpeed.rateOverTime = 15f;
                        }
                        if (currentBuffLevel == 5)
                        {
                            var newDripSpeed = particleSystem.emission;
                            newDripSpeed.rateOverTime = 20f;
                        }
                        particleSystem.Play();
                    }
                }
                else
                {
                    particleSystem.Stop();
                }
            }
        }

        public class DamageComponent : MonoBehaviour
        {
            public float cachedDamage = 0f;
        }
    }
}