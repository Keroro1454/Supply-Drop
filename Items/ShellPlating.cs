using R2API;
using RoR2;
using UnityEngine;
using static R2API.RecalculateStatsAPI;

using SupplyDrop.Utils;
using static SupplyDrop.Utils.ItemHelpers;
using static K1454.SupplyDrop.SupplyDropPlugin;

using BepInEx.Configuration;

namespace SupplyDrop.Items
{
    public class ShellPlating : ItemBase<ShellPlating>
    {
        //Config Stuff

        public static ConfigOption<float> armorOnKillAmount;
        public static ConfigOption<float> baseMaxArmorGain;
        public static ConfigOption<float> addMaxArmorGain;

        //Item Data

        public override string ItemName => "Shell Plating";

        public override string ItemLangTokenName => "SHELL_PLATING";

        public override string ItemPickupDesc => "Gain permanent <style=cIsUtility>armor</style> on kill.";

        public override string ItemFullDescription => $"Killing an enemy increases your <style=cIsUtility>armor</style> permanently by " +
            $"<style=cIsUtility>{armorOnKillAmount}</style>, up to a maximum of <style=cIsUtility>{baseMaxArmorGain}</style> <style=cStack>(+{addMaxArmorGain} per stack)</style>" +
            $" <style=cIsUtility>armor</style>.";

        public override string ItemLore => "Order: \"Shell Plating\"\nTracking Number: 02******\n" +
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

        public override ItemTier Tier => ItemTier.Tier2;
        public override ItemTag[] ItemTags => new ItemTag[] { ItemTag.Utility, ItemTag.AIBlacklist };


        public override GameObject ItemModel => MainAssets.LoadAsset<GameObject>("Shell.prefab");
        public override Sprite ItemIcon => MainAssets.LoadAsset<Sprite>("ShellIcon");


        public static GameObject ItemBodyModelPrefab;
        public BuffDef ShellStackMax { get; private set; }

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
            armorOnKillAmount = config.ActiveBind<float>("Item: " + ItemName, "Armor Gained on Kill", .5f, "How much armor should you gain on kill? (Multiplies by # of stacks of Shell Plating you have)");
            baseMaxArmorGain = config.ActiveBind<float>("Item: " + ItemName, "Base Max Armor Obtainable with 1 Shell Plating", 25f, "What should be the max armor obtainable with a single shell plating?");
            addMaxArmorGain = config.ActiveBind<float>("Item: " + ItemName, "Additional Max Armor Obtainable per Shell Plating", 10f, "How much should the max armor obtainable increase by for each shell plating after the first?");
        }
        private void CreateBuff()
        {
            ShellStackMax = ScriptableObject.CreateInstance<BuffDef>();
            ShellStackMax.name = "SupplyDrop Shell Stack Max";
            ShellStackMax.canStack = false;
            ShellStackMax.isDebuff = false;
            ShellStackMax.iconSprite = MainAssets.LoadAsset<Sprite>("ShellBuffIcon");

            ContentAddition.AddBuffDef(ShellStackMax);
        }
        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            ItemBodyModelPrefab = ItemModel;
            var itemDisplay = ItemBodyModelPrefab.AddComponent<RoR2.ItemDisplay>();
            itemDisplay.rendererInfos = ItemDisplaySetup(ItemBodyModelPrefab);

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
            rules.Add("mdlBandit2", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Pelvis",
                    localPos = new Vector3(-0.02917F, 0.09019F, -0.21039F),
                    localAngles = new Vector3(318F, 180F, 180F),
                    localScale = new Vector3(0.12394F, 0.12394F, 0.12394F)
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
            rules.Add("mdlHeretic", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Pelvis",
                    localPos = new Vector3(-0.15032F, 0.42318F, 0.02504F),
                    localAngles = new Vector3(311.809F, 76.40948F, 10.95981F),
                    localScale = new Vector3(0.15043F, 0.15043F, 0.15043F)
                }
            });
            rules.Add("mdlRailGunner", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Pelvis",
                    localPos = new Vector3(-0.01218F, 0.21942F, -0.16733F),
                    localAngles = new Vector3(321.0629F, 173.2427F, 182.1251F),
                    localScale = new Vector3(0.11245F, 0.11245F, 0.11245F)
                }
            });
            rules.Add("mdlVoidSurvivor", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Pelvis",
                    localPos = new Vector3(0.20855F, 0.19396F, 0.03138F),
                    localAngles = new Vector3(323.7909F, 76.01437F, 180.1294F),
                    localScale = new Vector3(0.13743F, 0.13675F, 0.13675F)
                }
            });
            return rules;
        }
        public override void Hooks()
        {
            On.RoR2.GlobalEventManager.OnCharacterDeath += CalculateShellBuffApplications;

            On.RoR2.CharacterBody.RecalculateStats += AddShellPlateStats;
        }

        private void CalculateShellBuffApplications(On.RoR2.GlobalEventManager.orig_OnCharacterDeath orig, GlobalEventManager self, DamageReport damageReport)
        {
            orig(self, damageReport);
            var inventoryCount = GetCount(damageReport.attackerBody);

            if (inventoryCount > 0)
            {
                var shellStackTrackerComponent = damageReport.attackerBody.gameObject.GetComponent<ShellStackTracker>();
                if (!shellStackTrackerComponent)
                {
                    damageReport.attackerBody.gameObject.AddComponent<ShellStackTracker>();
                }

                if (damageReport.attackerBody)
                {
                    var currentShellStackMax = (baseMaxArmorGain / armorOnKillAmount + ((inventoryCount - 1) * addMaxArmorGain / armorOnKillAmount));
                    var currentShellStack = shellStackTrackerComponent.shellStacks;
                    if (currentShellStack < currentShellStackMax)
                    {
                        shellStackTrackerComponent.shellStacks++;
                    }
                }
            }

        }
        private void AddShellPlateStats(On.RoR2.CharacterBody.orig_RecalculateStats orig, CharacterBody sender)
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
                sender.baseArmor += (armorOnKillAmount * currentShellStack);
            }
            orig(sender);
        }
    }
}
