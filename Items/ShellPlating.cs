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
    class ShellPlating : Item<ShellPlating>
    {
        public override string displayName => "Shell Plating";

        public override ItemTier itemTier => RoR2.ItemTier.Tier2;

        public override ReadOnlyCollection<ItemTag> itemTags => new ReadOnlyCollection<ItemTag>(new[] { ItemTag.Utility });
        protected override string NewLangName(string langid = null) => displayName;

        protected override string NewLangPickup(string langID = null) => "Gain armor on kill.";

        protected override string NewLangDesc(string langID = null) => "Killing an enemy increases your <style=cIsUtility>armor</style> permanently by <style=cIsUtility>.2</style>, up to a maximum of <style=cIsUtility>20</style> <style=cStack>(+10 per stack)</style> <style=cIsUtility>armor</style>.";

        protected override string NewLangLore(string landID = null) => "Order: Shell Plating\nTracking Number: 02******\nEstimated Delivery: 2/02/2056\nShipping Method: Priority\nShipping Address: Research Center, Polarity Zone, Neptune\nShipping Details:\n\nI've enclosed your payment, as well as a token of my goodwill, in hopes of a continued relationship. The story behind this piece should be especially interesting to you, given your fascination with sea-faring cultures.\n\nThe artifact comes from a small tribal community that lived on Earth long ago. The tribe would pay tributes into the sea, though it's not clear if this was in appeasement, celebration; in fact, it's unknown to what they were even paying tribute to.\n\nEither way, legend goes that one day, invaders appeared on the horizon in mighty vessels. The people, sensing the impending danger, sacrificed all they had in a terrified frenzy. Texts of theirs mention blood, possibly human, staining the foam red. In return...something...gave them shells to adorn their bodies with.\n\nGovernment reports state casualities were in the hundreds. The few that survived described those clad with shells as literally invincible, grinning like madmen and shouting praises as armaments hit them without effect.\n\nThere's still a few shells floating out there today, including this one here. Of course, no one has found them to be quite as...effective as those old reports claimed them to be. But it's still a neat little trinket, eh?";

        private static List<RoR2.CharacterBody> Playername = new List<RoR2.CharacterBody>();
        public static GameObject ItemBodyModelPrefab;
        public BuffIndex ShellStackMax { get; private set; }

        public ShellPlating()
        {
            modelPathName = "@SupplyDrop:Assets/Main/Models/Prefabs/Shell.prefab";
            iconPathName = "@SupplyDrop:Assets/Main/Textures/Icons/ShellIcon1.png";

            onAttrib += (tokenIdent, namePrefix) =>
            {
                var shellStackMax = new R2API.CustomBuff(
                    new BuffDef
                    {
                        buffColor = Color.magenta,
                        canStack = false,
                        isDebuff = false,
                        name = namePrefix + "ShellStackMax",
                        iconPath = "@SupplyDrop:Assets/Main/Textures/Icons/BoneBuff.png"
                    });
                ShellStackMax = R2API.BuffAPI.Add(shellStackMax);
            };
        }

        private static ItemDisplayRuleDict GenerateItemDisplayRules()
        //TO-DO: Need to add proper display to ALL
        {
            ItemBodyModelPrefab.AddComponent<ItemDisplay>();
            ItemBodyModelPrefab.GetComponent<ItemDisplay>().rendererInfos = SupplyDropPlugin.ItemDisplaySetup(ItemBodyModelPrefab);

            Vector3 generalScale = new Vector3(.25f, .25f, .25f);
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
            On.RoR2.GlobalEventManager.OnCharacterDeath += CalculateShellBuffApplications;

            GetStatCoefficients += AddShellPlateStats;
        }

        protected override void UnloadBehavior()
        {
            On.RoR2.GlobalEventManager.OnCharacterDeath -= CalculateShellBuffApplications;

            GetStatCoefficients -= AddShellPlateStats;
        }
        private void CalculateShellBuffApplications(On.RoR2.GlobalEventManager.orig_OnCharacterDeath orig, GlobalEventManager self, RoR2.DamageReport damageReport)
        {
            var shellComponent = damageReport.attackerBody.gameObject.GetComponent<ShellStacksComponent>();
            if (!shellComponent)
            {
                shellComponent = damageReport.attackerBody.gameObject.AddComponent<ShellStacksComponent>();
            }
            
            if (damageReport.attackerBody)
            {
                var inventoryCount = GetCount(damageReport.attackerBody);
                var CurrentShellStackMax = (((inventoryCount - 1) * 50) + 100);
                if (inventoryCount > 0 && shellComponent.cachedShellStacks < CurrentShellStackMax)
                {
                    shellComponent.cachedShellStacks += 1;
                    if (shellComponent.cachedShellStacks >= CurrentShellStackMax)
                    {
                        damageReport.attackerBody.AddBuff(ShellStackMax);
                    }
                    if (shellComponent.cachedShellStacks < CurrentShellStackMax)
                    {
                        damageReport.attackerBody.RemoveBuff(ShellStackMax);
                    }
                }
            }

            damageReport.attackerBody.statsDirty = true;
            orig(self, damageReport);
        }

        private void AddShellPlateStats(CharacterBody sender, StatHookEventArgs args)
        {
            var InventoryCount = GetCount(sender);
            var shellComponent = sender.GetComponent<ShellStacksComponent>();
            args.armorAdd += (.2f * shellComponent.cachedShellStacks);
        }
        public class ShellStacksComponent : MonoBehaviour
        {
            public int cachedShellStacks = 0;
        }
    }
}