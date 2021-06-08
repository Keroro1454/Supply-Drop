using System.Collections.ObjectModel;
using System.Collections.Generic;
using R2API;
using RoR2;
using UnityEngine;
using TILER2;
using static TILER2.StatHooks;
using SupplyDrop.Utils;
using static K1454.SupplyDrop.SupplyDropPlugin;

namespace SupplyDrop.Items
{
    public class ShellPlating : Item<ShellPlating>
    {
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("The amount of armor granted on kill. Default: .2", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
        public float armorOnKillAmount { get; private set; } = .2f;

        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("The maximum amount of armor obtainable from the first stack of the item.", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
        public float baseMaxArmorGain { get; private set; } = 10f;

        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("The maximum amount of armor obtainable from additional stacks of the item.", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
        public float addMaxArmorGain { get; private set; } = 10f;
        public override string displayName => "Shell Plating";
        public override ItemTier itemTier => ItemTier.Tier2;
        public override ReadOnlyCollection<ItemTag> itemTags => new ReadOnlyCollection<ItemTag>(new[] { ItemTag.Utility });
        protected override string GetNameString(string langid = null) => displayName;
        protected override string GetPickupString(string langID = null) => "Gain armor on kill.";
        protected override string GetDescString(string langID = null) => $"Killing an enemy increases your <style=cIsUtility>armor</style> permanently by " +
            $"<style=cIsUtility>{armorOnKillAmount}</style>, up to a maximum of <style=cIsUtility>{baseMaxArmorGain}</style> <style=cStack>(+{addMaxArmorGain} per stack)</style>" +
            $" <style=cIsUtility>armor</style>.";
        protected override string GetLoreString(string landID = null) => "Order: \"Shell Plating\"\nTracking Number: 02******\n" +
            "Estimated Delivery: 2/02/2056\nShipping Method: Priority\nShipping Address: Titan Museum of History and Culture, Titan\nShipping Details:\n\n" +
            "I've enclosed your payment, as well as a token of my goodwill, in hopes of a continued relationship. " +
            "The story behind this piece should be especially interesting to you, given your fascination with sea-faring cultures.\n\n" +
            "The artifact comes from a small tribal community that lived on Earth long ago. The tribe would pay tributes into the sea, " +
            "though it's not clear if this was in appeasement, celebration; in fact, it's unknown to what they were even paying tribute to.\n\n" +
            "Either way, legend goes that one day, invaders appeared on the horizon in mighty vessels. The people, sensing the impending danger, " +
            "sacrificed all they had in a terrified frenzy. Texts of theirs mention blood, possibly human, staining the foam red. " +
            "In return...something...gave them shells to adorn their bodies with." +
            "\n\nGovernment reports state casualities were in the hundreds. The few that survived described those clad with shells as literally invincible, " +
            "grinning like madmen and shouting praises as armaments hit them without effect." +
            "\n\nThere's still a few shells floating out there today, including this one here. " +
            "Of course, no one has found them to be quite as...effective as those old reports claimed them to be. But it's still a neat little trinket, eh?";

        private static List<CharacterBody> Playername = new List<CharacterBody>();
        public static GameObject ItemBodyModelPrefab;
        public BuffDef ShellStackMax { get; private set; }
        public ShellPlating()
        {
            modelResource = MainAssets.LoadAsset<GameObject>("Main/Models/Prefabs/Shell.prefab");
            iconResource = MainAssets.LoadAsset<Sprite>("Main/Textures/Icons/ShellIcon.png");
        }
        public override void SetupAttributes()
        {
            if (ItemBodyModelPrefab == null)
            {
                ItemBodyModelPrefab = modelResource;
                displayRules = GenerateItemDisplayRules();
            }

            base.SetupAttributes();

            ShellStackMax = ScriptableObject.CreateInstance<BuffDef>();
            ShellStackMax.name = "SupplyDrop Shell Stack Max";
            ShellStackMax.canStack = false;
            ShellStackMax.isDebuff = false;
            ShellStackMax.iconSprite = MainAssets.LoadAsset<Sprite>("ShellBuffIcon.png");
            BuffAPI.Add(new CustomBuff(ShellStackMax));
        }
        private static ItemDisplayRuleDict GenerateItemDisplayRules()
        {

            ItemBodyModelPrefab.AddComponent<ItemDisplay>();
            ItemBodyModelPrefab.GetComponent<ItemDisplay>().rendererInfos = ItemHelpers.ItemDisplaySetup(ItemBodyModelPrefab);

            ItemDisplayRuleDict rules = new ItemDisplayRuleDict(new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Pelvis",
                    localPos = new Vector3(-0.025f, 0.05f, -.23f),
                    localAngles = new Vector3(-138f, 0f, 0f),
                    localScale = new Vector3(.2f, .2f, .2f)

        }
            });
            rules.Add("mdlHuntress", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Pelvis",
                    localPos = new Vector3(0f, 0.05f, -0.2f),
                    localAngles = new Vector3(-138f, 0f, 0f),
                    localScale = new Vector3(0.125f, 0.125f, 0.125f)
        }
            });
            rules.Add("mdlToolbot", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Hip",
                    localPos = new Vector3(2.37f, 2.3f, -0.4f),
                    localAngles = new Vector3(-30f, 90f, 180f),
                    localScale = new Vector3(2f, 2f, 2f)
                }
            });
            rules.Add("mdlEngi", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Pelvis",
                    localPos = new Vector3(0f, 0f, -0.3f),
                    localAngles = new Vector3(-138f, 0f, 0f),
                    localScale = new Vector3(.2f, .2f, .2f)
                }
            });
            rules.Add("mdlMage", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Pelvis",
                    localPos = new Vector3(-0.02f, -0.05f, -0.23f),
                    localAngles = new Vector3(-138f, 0f, 0f),
                    localScale = new Vector3(0.14f, 0.14f, 0.14f)
                }
            });
            rules.Add("mdlMerc", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Pelvis",
                    localPos = new Vector3(0.025f, 0.15f, -0.28f),
                    localAngles = new Vector3(-149f, 0f, 0f),
                    localScale = new Vector3(0.17f, 0.17f, 0.17f)
                }
            });
            rules.Add("mdlTreebot", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "FlowerBase",
                    localPos = new Vector3(0f, -1f, -0.9f),
                    localAngles = new Vector3(-138f, 0f, 0f),
                    localScale = new Vector3(0.5f, 0.5f, 0.5f)
                }
            });
            rules.Add("mdlLoader", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Pelvis",
                    localPos = new Vector3(-0.02f, 0.19f, -0.285f),
                    localAngles = new Vector3(-149f, 0f, 0f),
                    localScale = new Vector3(.2f, .2f, .2f)
                }
            });
            rules.Add("mdlCroco", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Hip",
                    localPos = new Vector3(0f, 0.7f, -3f),
                    localAngles = new Vector3(-138f, 0f, 0f),
                    localScale = new Vector3(2f, 2f, 2f)
                }
            });
            rules.Add("mdlCaptain", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Pelvis",
                    localPos = new Vector3(0f, -0.1f, -0.28f),
                    localAngles = new Vector3(-138f, 0f, 0f),
                    localScale = new Vector3(0.15f, 0.15f, 0.15f)
                }
            });
            return rules;
        }
        public override void Install()
        {
            base.Install();

            On.RoR2.GlobalEventManager.OnCharacterDeath += CalculateShellBuffApplications;

            GetStatCoefficients += AddShellPlateStats;
        }
        public override void Uninstall()
        {
            base.Uninstall();
            On.RoR2.GlobalEventManager.OnCharacterDeath -= CalculateShellBuffApplications;

            GetStatCoefficients -= AddShellPlateStats;
        }
        private void CalculateShellBuffApplications(On.RoR2.GlobalEventManager.orig_OnCharacterDeath orig, GlobalEventManager self, DamageReport damageReport)
        {
            orig(self, damageReport);

            var shellStackTrackerComponent = damageReport.attackerBody.gameObject.GetComponent<ShellStackTracker>();
            if (!shellStackTrackerComponent)
            {
                damageReport.attackerBody.gameObject.AddComponent<ShellStackTracker>();
            }

            if (damageReport.attackerBody)
            {
                var inventoryCount = GetCount(damageReport.attackerBody);
                var currentShellStackMax = (baseMaxArmorGain / armorOnKillAmount + ((inventoryCount - 1) * addMaxArmorGain / armorOnKillAmount));
                var currentShellStack = shellStackTrackerComponent.shellStacks;
                if (inventoryCount > 0 && currentShellStack < currentShellStackMax)
                {
                    shellStackTrackerComponent.shellStacks++;
                }
            }
        }
        private void AddShellPlateStats(CharacterBody sender, StatHookEventArgs args)
        {
            var inventoryCount = GetCount(sender);
            if (inventoryCount > 0)
            {

                var shellStackTrackerComponent = sender.gameObject.GetComponent<ShellStackTracker>();
                if (!shellStackTrackerComponent)
                {
                    sender.gameObject.AddComponent<ShellStackTracker>();
                }

                var currentShellStackMax = (((inventoryCount - 1) * (addMaxArmorGain/armorOnKillAmount)) + (baseMaxArmorGain/armorOnKillAmount));
                if (shellStackTrackerComponent.shellStacks >= currentShellStackMax && sender.GetBuffCount(ShellStackMax) <= 0)
                {
                    sender.AddBuff(ShellStackMax);
                }

                if (shellStackTrackerComponent.shellStacks < currentShellStackMax && sender.GetBuffCount(ShellStackMax) > 0)
                {
                    sender.RemoveBuff(ShellStackMax);
                }
                var currentShellStack = shellStackTrackerComponent.shellStacks;
                args.armorAdd += (armorOnKillAmount * currentShellStack);
            }
        }
    }
}
