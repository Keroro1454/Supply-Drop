using System.Collections.ObjectModel;
using System.Collections.Generic;
using R2API;
using RoR2;
using UnityEngine;
using TILER2;
using static TILER2.StatHooks;
using SupplyDrop.Utils;
using static TILER2.MiscUtil;

namespace SupplyDrop.Items
{
    public class ElectroPlankton : Item_V2<ElectroPlankton>
    {
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("In percentage, amount of maximum HP granted as bonus shield for first stack of the item. Default: 8% (.08)", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
        public float baseStackHPPercent { get; private set; } = .08f;

        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Amount of shield gained on hit. Is multiplied by the number of stacks of the item you have. Default: 1 ", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
        public float shieldAmountOnHit { get; private set; } = 1f;
        public override string displayName => "Echo-Voltaic Plankton";

        public override ItemTier itemTier => ItemTier.Tier2;

        public override ReadOnlyCollection<ItemTag> itemTags => new ReadOnlyCollection<ItemTag>(new[] { ItemTag.Utility });
        protected override string GetNameString(string langid = null) => displayName;

        protected override string GetPickupString(string langID = null) => "Recharge shield upon dealing damage.";

        protected override string GetDescString(string langID = null) => $"Gain a <style=cIsUtility>shield</style> equal to <style=cIsUtility>{Pct(baseStackHPPercent)}</style> of your maximum health. " +
            $"Dealing damage recharges <style=cIsUtility>{shieldAmountOnHit}</style> <style=cStack>(+{shieldAmountOnHit} per stack)</style> <style=cIsUtility>shield</style>.";

        protected override string GetLoreString(string landID = null) => "<style=cMono>ACCESSING EXPERIMENT DATA FOR PROJECT B:O:M:37543</style>" +
            "\n\n<style=cMono>EXPERIMENT LOG B:O:M:37543:1</style>\nNewly-minted Senior Researcher Thomas here! This is my first time as project lead, pretty exciting.\n\nThese micro-organisms were recovered under the ice around Titan's equator region. " +
            "They appear to be identical in every way to Earth zooplankton, though substantially larger. I intend to run a full dissection of several tomorrow." +
            "\n\n<style=cMono>EXPERIMENT LOG B:O:M:37543:2</style>\nDissection proved to be...valuable. Upon dissecting the specimens, we found that every single one of them possess a small cellular 'sac' of sorts, " +
            "though its purpose is unclear. It was unresponsive to almost all stimuli except damaging it, which caused it to immediately disintegrate into dead...bone cells?" +
            "\n\nI hope we find something of interest, sac or otherwise, because otherwise this project is a bust..." +
            "\n\n<style=cMono>EXPERIMENT LOG B:O:M:37543:3</style>\nShit. That was a [REDACTED]." +
            "\n\nJR John was putting away his tools when he accidentally cut himself with a scalpel. Nothing major, but he didn't sound happy.\n\nOut of nowhere, " +
            "JR Dave...well he was scooping a fresh sample container out of the tank when he suddenly started screaming. Thrashing all over the place. " +
            "John ran over and pulled Dave's arm out of the tank.\n\nI'm outside their room now. Med-bot said their condition is critical. " +
            "\n\n<style=cMono>EXPERIMENT LOG B:O:M:37543:4</style>\nThese things...they're evil. I know, I know " +
            "but it's true. I've tested my suspicions...these things are even more disturbing than I thought.\n\nThe plankton respond to sound by seemingly converting it to energy, and in massive amounts too. " +
            "But the only sound they react to is vocalizations of pain. The worse, the more energy they release. And you can't trick them either. It has to be genuine. It can't be a recording. They know. Somehow." +
            "\n\n<style=cMono>END OF EXPERIMENT DATA</style>" +
            "\n\n<style=cMono>> DELETE FILE\n\n> DO YOU WANT TO PROCEED AND DELETE THIS FILE? [Y/N]\n> Y</style>" +
            "\n\n<style=cMono>ERROR: FILE CANNOT BE REMOVED. S.R. THOMAS ENCRYPTION KEY REQUIRED.\n\n> SEND MESSAGE TO |||||||||||</style>" +
            "\n\nJ-\nI want those logs off the server now. The test generator has already been shipped off to M.\n\nOh, and I want that idiot dealt with.\n- S";

        private static List<CharacterBody> Playername = new List<CharacterBody>();
        public static GameObject ItemBodyModelPrefab;

        public ElectroPlankton()
        {
            modelResourcePath = "@SupplyDrop:Assets/Main/Models/Prefabs/Plankton.prefab";
            iconResourcePath = "@SupplyDrop:Assets/Main/Textures/Icons/PlanktonIcon.png";
        }
        public override void SetupAttributes()
        {
            if (ItemBodyModelPrefab == null)
            {
                ItemBodyModelPrefab = Resources.Load<GameObject>(modelResourcePath);
                var meshes = ItemBodyModelPrefab.GetComponentsInChildren<MeshRenderer>();
                meshes[3].gameObject.AddComponent<Wobble>();
                displayRules = GenerateItemDisplayRules();
            }

            base.SetupAttributes();
        }
        private static ItemDisplayRuleDict GenerateItemDisplayRules()
        {
            ItemBodyModelPrefab.AddComponent<ItemDisplay>();
            ItemBodyModelPrefab.GetComponent<ItemDisplay>().rendererInfos = ItemHelpers.ItemDisplaySetup(ItemBodyModelPrefab);

            Vector3 generalScale = new Vector3(.125f, .125f, .125f);
            ItemDisplayRuleDict rules = new ItemDisplayRuleDict(new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0f, 0.25f, -0.36f),   
                    localAngles = new Vector3(180f, 180f, 180f),
                    localScale = generalScale

                }
            });
            rules.Add("mdlHuntress", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0f, -0.025f, -0.12f),
                    localAngles = new Vector3(0f, 0f, 90f),
                    localScale = new Vector3(.09f, .09f, .09f)
                }
            });
            rules.Add("mdlToolbot", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0f, 0.8f, -2.5f),
                    localAngles = new Vector3(0f, 0f, 90f),
                    localScale = new Vector3(0.75f, 0.75f, 0.75f)
                }
            });
            rules.Add("mdlEngi", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0f, 0.15f, -0.4f),
                    localAngles = new Vector3(0f, 0f, 0f),
                    localScale = generalScale
                }
            });
            rules.Add("mdlMage", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0f, 0.1f, -0.38f),
                    localAngles = new Vector3(0f, 0f, 0f),
                    localScale = new Vector3(0.1f, 0.125f, 0.1f)
                }
            });
            rules.Add("mdlMerc", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0f, 0.2f, -0.41f),
                    localAngles = new Vector3(80f, 180f, 0f),
                    localScale = new Vector3(0.08f, 0.08f, 0.08f)
                }
            });
            rules.Add("mdlTreebot", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "WeaponPlatform",
                    localPos = new Vector3(0f, -0.5f, 0.4f),
                    localAngles = new Vector3(0f, 0f, 180f),
                    localScale = new Vector3(0.1f, 0.1f, 0.1f)
                }
            });
            rules.Add("mdlLoader", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0f, 0.25f, -0.37f),
                    localAngles = new Vector3(0f, 0f, 90f),
                    localScale = new Vector3(0.1f, 0.1f, 0.1f)
                }
            });
            rules.Add("mdlCroco", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0f, 3f, 4f),
                    localAngles = new Vector3(-30f, 0f, 0f),
                    localScale = new Vector3(0.75f, 0.75f, 0.75f)
                }
            });
            rules.Add("mdlCaptain", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0f, 0.2f, -0.35f),
                    localAngles = new Vector3(0f, 0f, 0f),
                    localScale = new Vector3(0.1f, 0.1f, 0.1f)
                }
            });
            return rules;
        }

        public override void Install()
        {
            base.Install();

            var meshes = itemDef.pickupModelPrefab.GetComponentsInChildren<MeshRenderer>();
            meshes[3].gameObject.AddComponent<Wobble>();
            On.RoR2.HealthComponent.TakeDamage += ShieldOnHit;
            GetStatCoefficients += AddMaxShield;
        }

        public override void Uninstall()
        {
            base.Uninstall();

            GetStatCoefficients -= AddMaxShield;
            On.RoR2.HealthComponent.TakeDamage -= ShieldOnHit;
        }
        private void AddMaxShield(CharacterBody sender, StatHookEventArgs args)
        {
            var inventoryCount = GetCount(sender);
            if (inventoryCount > 0)
            {
                args.baseShieldAdd += (sender.maxHealth * baseStackHPPercent);
            }
        }

        private void ShieldOnHit(On.RoR2.HealthComponent.orig_TakeDamage orig, HealthComponent self, DamageInfo damageInfo)
        {
            orig(self, damageInfo);
            if (damageInfo.attacker)
            {
                var cbAttacker = damageInfo.attacker.GetComponent<CharacterBody>();
                if (cbAttacker)
                {
                    var invenCount = GetCount(cbAttacker);
                    if (invenCount > 0)
                    {
                        float procCoeff = damageInfo.procCoefficient;
                        cbAttacker.healthComponent.RechargeShield((invenCount * shieldAmountOnHit) * procCoeff);
                    }
                }
            }     
        }
    }
}