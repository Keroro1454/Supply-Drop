using System.Collections.ObjectModel;
using System.Collections.Generic;
using R2API;
using RoR2;
using UnityEngine;
using TILER2;
using static TILER2.StatHooks;
using K1454.SupplyDrop;


namespace SupplyDrop.Items
{
    class BloodBook : Item<BloodBook>
    {
        public override string displayName => "Tome of Bloodletting";

        public override ItemTier itemTier => RoR2.ItemTier.Tier3;

        public override ReadOnlyCollection<ItemTag> itemTags => new ReadOnlyCollection<ItemTag>(new[] { ItemTag.Damage });
        protected override string NewLangName(string langid = null) => displayName;

        protected override string NewLangPickup(string langID = null) => "Convert some damage taken into a temporary damage boost.";

        protected override string NewLangDesc(string langID = null) => "Convert <style=cIsDamage>10%</style> <style=cStack>(+10% per stack)</style> of the damage you take into a <style=cIsDamage>temporary damage boost</style>, " +
            "up to <style=cIsDamage>20</style> <style=cStack>(+10 per stack)</style>. The boost is powered up based on damage taken; every 10% max HP, up to 50%, that was depleted increases the base duration of 4s by +2s.";

        protected override string NewLangLore(string landID = null) => "Nature learns from pain. Nature willingly suffers, without protest. Nature studies what gifts pain provides. " +
            "It takes the lessons that pain gives out freely, and it grows stronger, more capable of survival." +
            "\n\nHumanity has disrupted this order. It no longer wishes to learn from its greatest teacher. It is an insolent pupil, forgetting its place." +
            "\n\nHumanity will learn.But you may spare yourself of the harsh lesson coming. Pain offers its tutelage to all." +
            "\n\nSimply turn the page." +
            "\n\nSteel yourself." +
            "\n\nAnd begin.";

        private static List<RoR2.CharacterBody> Playername = new List<RoR2.CharacterBody>();
        public static GameObject ItemBodyModelPrefab;
        public BuffIndex PatheticBloodBuff { get; private set; }
        public BuffIndex WeakBloodBuff { get; private set; }
        public BuffIndex AverageBloodBuff { get; private set; }
        public BuffIndex StrongBloodBuff { get; private set; }
        public BuffIndex InsaneBloodBuff { get; private set; }
        public BuffIndex DevotedBloodBuff { get; private set; }

        public BloodBook()
        {
            modelPathName = "@SupplyDrop:Assets/Main/Models/Prefabs/BloodBook.prefab";
            iconPathName = "@SupplyDrop:Assets/Main/Textures/Icons/BloodBookIcon.png";

            onAttrib += (tokenIdent, namePrefix) =>
            {
                var patheticBloodBuff = new CustomBuff(
                    new BuffDef
                    {
                        canStack = false,
                        isDebuff = false,
                        name = namePrefix + "PatheticBloodBuff",
                        iconPath = "@SupplyDrop:Assets/Main/Textures/Icons/PBloodBuffIcon.png"
                    });
                PatheticBloodBuff = BuffAPI.Add(patheticBloodBuff);

                var weakBloodBuff = new CustomBuff(
                    new BuffDef
                    {
                        canStack = false,
                        isDebuff = false,
                        name = namePrefix + "WeakBloodBuff",
                        iconPath = "@SupplyDrop:Assets/Main/Textures/Icons/WBloodBuffIcon.png"
                    });
                WeakBloodBuff = BuffAPI.Add(weakBloodBuff);

                var averageBloodBuff = new R2API.CustomBuff(
                    new BuffDef
                    {
                        canStack = false,
                        isDebuff = false,
                        name = namePrefix + "AverageBloodBuff",
                        iconPath = "@SupplyDrop:Assets/Main/Textures/Icons/ABloodBuffIcon.png"
                    });
                AverageBloodBuff = BuffAPI.Add(averageBloodBuff);

                var strongBloodBuff = new CustomBuff(
                    new BuffDef
                    {
                        canStack = false,
                        isDebuff = false,
                        name = namePrefix + "StrongBloodBuff",
                        iconPath = "@SupplyDrop:Assets/Main/Textures/Icons/SBloodBuffIcon.png"
                    });
                StrongBloodBuff = BuffAPI.Add(strongBloodBuff);

                var insaneBloodBuff = new CustomBuff(
                    new BuffDef
                    {
                        canStack = false,
                        isDebuff = false,
                        name = namePrefix + "InsaneBloodBuff",
                        iconPath = "@SupplyDrop:Assets/Main/Textures/Icons/IBloodBuffIcon.png"
                    });
                InsaneBloodBuff = BuffAPI.Add(insaneBloodBuff);

                var devotedBloodBuff = new CustomBuff(
                    new BuffDef
                    {
                        canStack = false,
                        isDebuff = false,
                        name = namePrefix + "DevotedBloodBuff",
                        iconPath = "@SupplyDrop:Assets/Main/Textures/Icons/DBloodBuffIcon.png"
                    });
                DevotedBloodBuff = BuffAPI.Add(devotedBloodBuff);
            };
        }

        private static ItemDisplayRuleDict GenerateItemDisplayRules()
        {
            ItemBodyModelPrefab.AddComponent<ItemDisplay>();
            ItemBodyModelPrefab.GetComponent<ItemDisplay>().rendererInfos = SupplyDropPlugin.ItemDisplaySetup(ItemBodyModelPrefab);

            Vector3 generalScale = new Vector3(1f, 1f, 1f);
            ItemDisplayRuleDict rules = new ItemDisplayRuleDict(new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Stomach",
                    localPos = new Vector3(0f, 0.1f, 0f),
                    localAngles = new Vector3(-85f, 0f, 0f),
                    localScale = generalScale

                }
            });
            rules.Add("mdlHuntress", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Stomach",
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
                    childName = "Head",
                    localPos = new Vector3(0.75f, 3.7f, -2.3f),
                    localAngles = new Vector3(90f, 0f, 0f),
                    localScale = new Vector3(0.5f, 0.5f, 0.5f)
                }
            });
            rules.Add("mdlEngi", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Stomach",
                    localPos = new Vector3(0f, 0.15f, 0f),
                    localAngles = new Vector3(-75f, 0f, 0f),
                    localScale = generalScale
                }
            });
            rules.Add("mdlMage", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Stomach",
                    localPos = new Vector3(0f, 0.1f, 0f),
                    localAngles = new Vector3(-22.5f, 0f, 0f),
                    localScale = generalScale
                }
            });
            rules.Add("mdlMerc", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Stomach",
                    localPos = new Vector3(0f, 0f, 0f),
                    localAngles = new Vector3(-25f, 0f, 0f),
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
                    localPos = new Vector3(1.5f, -0.1f, -0.3f),
                    localAngles = new Vector3(0f, 0f, 40f),
                    localScale = new Vector3(0.3f, 0.3f, 0.3f)
                }
            });
            rules.Add("mdlLoader", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Stomach",
                    localPos = new Vector3(0f, 0f, 0f),
                    localAngles = new Vector3(-25f, 0f, 0f),
                    localScale = generalScale
                }
            });
            rules.Add("mdlCroco", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0f, 0f, 0f),
                    localAngles = new Vector3(115f, 0f, 0f),
                    localScale = generalScale * 8
                }
            });
            rules.Add("mdlCaptain", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Stomach",
                    localPos = new Vector3(0f, 0.2f, 0f),
                    localAngles = new Vector3(-25f, 0f, 0f),
                    localScale = generalScale
                }
            });
            return rules;
        }

        protected override void LoadBehavior()
        {
            if (ItemBodyModelPrefab == null)
            {
                ItemBodyModelPrefab = regDef.pickupModelPrefab;
                regItem.ItemDisplayRules = GenerateItemDisplayRules();
            }

            On.RoR2.HealthComponent.TakeDamage += CalculateBloodBookBuff;
            On.RoR2.CharacterBody.FixedUpdate += BloodBookBleedManager;
            GetStatCoefficients += AddBloodBuffStats;
        }

        protected override void UnloadBehavior()
        {
            On.RoR2.HealthComponent.TakeDamage -= CalculateBloodBookBuff;
            On.RoR2.CharacterBody.FixedUpdate -= BloodBookBleedManager;
            GetStatCoefficients -= AddBloodBuffStats;
        }
        private void CalculateBloodBookBuff(On.RoR2.HealthComponent.orig_TakeDamage orig, RoR2.HealthComponent self, RoR2.DamageInfo damageInfo)
        {
            var inventoryCount = GetCount(self.body);
            var patheticBuffCount = self.body.GetBuffCount(PatheticBloodBuff);
            var weakBuffCount = self.body.GetBuffCount(WeakBloodBuff);
            var averageBuffCount = self.body.GetBuffCount(AverageBloodBuff);
            var strongBuffCount = self.body.GetBuffCount(StrongBloodBuff);
            var insaneBuffCount = self.body.GetBuffCount(InsaneBloodBuff);
            var devotedBuffCount = self.body.GetBuffCount(DevotedBloodBuff);

            float dmgTaken = damageInfo.damage;
            float maxHealth = self.body.maxHealth;

            if (inventoryCount > 0)
            {
                //Each of these checks to make sure a higher-tier buff isn't already active, and cleanses any lower-tier buffs before applying their buff. Maybe there's a cleaner way to do it but oh well...
                if (dmgTaken <= maxHealth * .1 && weakBuffCount == 0 && averageBuffCount == 0 && strongBuffCount == 0 && insaneBuffCount == 0 && devotedBuffCount == 0)
                {
                    self.body.AddTimedBuffAuthority(PatheticBloodBuff, 4f);
                }     
                if (dmgTaken >= maxHealth * .1 && dmgTaken <= maxHealth * .2 && averageBuffCount == 0 && strongBuffCount == 0 && insaneBuffCount == 0 && devotedBuffCount == 0)
                {
                    self.body.RemoveBuff(PatheticBloodBuff);
                    self.body.AddTimedBuffAuthority(WeakBloodBuff, 6f);
                }
                if (dmgTaken >= maxHealth * .2 && dmgTaken <= maxHealth * .3 && strongBuffCount == 0 && insaneBuffCount == 0 && devotedBuffCount == 0)
                {
                    self.body.RemoveBuff(PatheticBloodBuff);
                    self.body.RemoveBuff(WeakBloodBuff);
                    self.body.AddTimedBuffAuthority(AverageBloodBuff, 8f);
                }
                if (dmgTaken >= maxHealth * .3 && dmgTaken <= maxHealth * .4 && insaneBuffCount == 0 && devotedBuffCount == 0)
                {
                    self.body.RemoveBuff(PatheticBloodBuff);
                    self.body.RemoveBuff(WeakBloodBuff);
                    self.body.RemoveBuff(AverageBloodBuff);
                    self.body.AddTimedBuffAuthority(StrongBloodBuff, 10f);
                }
                if (dmgTaken >= maxHealth * .4 && dmgTaken <= maxHealth * .5 && devotedBuffCount == 0)
                {
                    self.body.RemoveBuff(PatheticBloodBuff);
                    self.body.RemoveBuff(WeakBloodBuff);
                    self.body.RemoveBuff(AverageBloodBuff);
                    self.body.RemoveBuff(StrongBloodBuff);
                    self.body.AddTimedBuffAuthority(InsaneBloodBuff, 12f);
                }
                if (dmgTaken >= maxHealth * .5)
                {
                    self.body.RemoveBuff(PatheticBloodBuff);
                    self.body.RemoveBuff(WeakBloodBuff);
                    self.body.RemoveBuff(AverageBloodBuff);
                    self.body.RemoveBuff(StrongBloodBuff);
                    self.body.RemoveBuff(InsaneBloodBuff);
                    self.body.AddTimedBuffAuthority(InsaneBloodBuff, 14f);
                }
            }
            orig(self, damageInfo);
        }
        private void BloodBookBleedManager(On.RoR2.CharacterBody.orig_FixedUpdate orig, RoR2.CharacterBody self)
        {
            if (self.modelLocator && self.modelLocator.modelTransform && self.HasBuff(InsaneBloodBuff) && !self.GetComponent<InsaneBleed>())
            {
                var Meshes = BloodBook.ItemBodyModelPrefab.GetComponentsInChildren<MeshRenderer>();
                Meshes[0].gameObject.AddComponent<InsaneBleed>();                
            }
        }
        public class BleedDestroyer : MonoBehaviour
        {
            public ParticleSystem insaneParticle;
            public CharacterBody Body;

            public void FixedUpdate()
            {
                if (!Body.HasBuff(InsaneBloodBuff))
                {
                    Destroy(insaneParticle);
                    Destroy(this);
                }
            }
        }
        private void AddBloodBuffStats(CharacterBody sender, StatHookEventArgs args)
        {
            var InventoryCount = GetCount(sender);
            if (sender.HasBuff(InsaneBloodBuff))
            {
                args.armorAdd += 5f * (5 * (InventoryCount - 1));
            }
        }
    }
}