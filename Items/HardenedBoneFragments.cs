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
    class HardenedBoneFragments : Item<HardenedBoneFragments>
    {
        public override string displayName => "Hardened Bone Fragments";

        public override ItemTier itemTier => RoR2.ItemTier.Tier1;

        public override ReadOnlyCollection<ItemTag> itemTags => new ReadOnlyCollection<ItemTag>(new[] { ItemTag.Utility });
        protected override string NewLangName(string langid = null) => displayName;

        protected override string NewLangPickup(string langID = null) => "Gain temporary armor on kill. Armor is lost when injured.";

        protected override string NewLangDesc(string langID = null) => "Gain <style=cIsUtility>1</style> <style=cStack>(+1 per stack)</style> <style=cIsUtility>armor</style> on kill. All <style=cIsUtility>armor</style> is lost upon taking damage.";

        protected override string NewLangLore(string landID = null) => "The last attacker hissed its final breath before falling silent next to its brethren. \n\n The man holstered his weapon, wisps of smoke curling around the barrel, before falling to his knees with a sigh. Taking his knife from its hilt, he stabbed through the purple scales of his defeated foes, taking meat to eat but also the creatures' bones, which he began wittling to flat surfaces. \n\n A woman came over, shouldering her bow as she stopped and looked upon the gory scene before her. \n\n 'What are you doing?!' \n\n 'What does it look like? I'm making myself some protection. Want some?' \n\n 'Don't be ridiculous. What you're doing...that's just grotesque. There's no way bits of bone will do anything other than make you look like some kind of monster.' \n\n 'Good. I'll fit right in here then.' \n\n The man stood back up, covered in scales. Not purple like those of the creatures he had just felled, but a brilliant white. He moved on from the scene, seeking to complete his suit of armor.";

        private static List<RoR2.CharacterBody> Playername = new List<RoR2.CharacterBody>();
        public static GameObject ItemBodyModelPrefab;
        public BuffIndex BFBuff { get; private set; }

        public HardenedBoneFragments()
        {
            modelPathName = "@SupplyDrop:Assets/Main/Models/Prefabs/Bone.prefab";
            iconPathName = "@SupplyDrop:Assets/Main/Textures/Icons/BoneIcon.png";

            onAttrib += (tokenIdent, namePrefix) =>
            {
                var bFBuff = new R2API.CustomBuff(
                    new BuffDef
                    {
                        buffColor = Color.white,
                        canStack = true,
                        isDebuff = false,
                        name = namePrefix + "BoneFragArmor",
                        iconPath = "@SupplyDrop:Assets/Main/Textures/Icons/BoneBuffIcon.png"
                    });
                BFBuff = R2API.BuffAPI.Add(bFBuff);
            };
        }

        private static ItemDisplayRuleDict GenerateItemDisplayRules()
        //TO-DO: Need to add proper display to MUL-T, Arti, Merc, REX, Acrid, Loader, Captain
        {
            ItemBodyModelPrefab.AddComponent<ItemDisplay>();
            ItemBodyModelPrefab.GetComponent<ItemDisplay>().rendererInfos = SupplyDropPlugin.ItemDisplaySetup(ItemBodyModelPrefab);

            Vector3 generalScale = new Vector3(1.5f, 1.5f, 1.5f);
            ItemDisplayRuleDict rules = new ItemDisplayRuleDict(new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "ThighL",
                    localPos = new Vector3(0f, 0.35f, 0.15f),
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
                    childName = "ThighL",
                    localPos = new Vector3(0f, 0.30f, 0.15f),
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
                    childName = "null",
                    localPos = new Vector3(0f, 2.3f, 2f),
                    localAngles = new Vector3(90f, 0f, 0f),
                    localScale = generalScale * 6
                }
            });
            rules.Add("mdlEngi", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "ThighL",
                    localPos = new Vector3(0.05f, 0.15f, 0.15f),
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
                    childName = "null",
                    localPos = new Vector3(0f, 0.15f, -0.05f),
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
                    childName = "null",
                    localPos = new Vector3(0f, 0.25f, -0.05f),
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
                    childName = "null",
                    localPos = new Vector3(0f, 1.4f, 0f),
                    localAngles = new Vector3(0f, 0f, 0f),
                    localScale = generalScale * 5
                }
            });
            rules.Add("mdlLoader", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "null",
                    localPos = new Vector3(0f, 0.2f, -0.05f),
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
                    childName = "null",
                    localPos = new Vector3(0f, 0f, 0.75f),
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
                    childName = "null",
                    localPos = new Vector3(0f, 0.20f, -0.05f),
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
            On.RoR2.DeathRewards.OnKilledServer += CalculateBFBuffGain;
            On.RoR2.HealthComponent.TakeDamage += BFBuffLoss;

            GetStatCoefficients += AddBFBuff;
        }

        protected override void UnloadBehavior()
        {
            On.RoR2.DeathRewards.OnKilledServer -= CalculateBFBuffGain;
            On.RoR2.HealthComponent.TakeDamage -= BFBuffLoss;

            GetStatCoefficients -= AddBFBuff;
        }

        private void CalculateBFBuffGain(On.RoR2.DeathRewards.orig_OnKilledServer orig, RoR2.DeathRewards self, RoR2.DamageReport damageReport)
        {
            if(damageReport?.attackerBody)
            {
                var inventoryCount = GetCount(damageReport.attackerBody);
                if(GetCount(damageReport.attackerBody) > 0)
                {
                    damageReport.attackerBody.AddBuff(BFBuff);
                }
            }
            orig(self, damageReport);
        }
        private void BFBuffLoss(On.RoR2.HealthComponent.orig_TakeDamage orig, RoR2.HealthComponent self, RoR2.DamageInfo damageInfo)
        {
            var BuffCount = self.body.GetBuffCount(BFBuff);
            while (BuffCount > 0)
            {
                self.body.RemoveBuff(BFBuff);
                BuffCount -= 1;
            }
            orig(self, damageInfo);
        }

        private void AddBFBuff(CharacterBody sender, StatHookEventArgs args)
        {
            var InventoryCount = GetCount(sender);
            if (sender.HasBuff(BFBuff))
            {
                args.armorAdd += 1f * sender.GetBuffCount(BFBuff) * InventoryCount;
            }
        }
    }
}
