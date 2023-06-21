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
    public class ElectroPlankton : ItemBase<ElectroPlankton>
    {
        //Config Stuff

        public static ConfigOption<float> baseStackHPPercent;
        public static ConfigOption<float> shieldAmountOnHit;

        //Item Data

        public override string ItemName => "Echo-Voltaic Plankton";

        public override string ItemLangTokenName => "PLANKTON";

        public override string ItemPickupDesc => "Recharge <style=cIsUtility>shield</style> upon dealing damage.";

        public override string ItemFullDescription => $"Gain a <style=cIsUtility>shield</style> equal to <style=cIsUtility>{FloatToPercentageString(baseStackHPPercent)}</style> of your maximum health. " +
            $"Dealing damage recharges <style=cIsUtility>{shieldAmountOnHit}</style> <style=cStack>(+{shieldAmountOnHit} per stack)</style> <style=cIsUtility>shield</style>.";

        public override string ItemLore => "<style=cMono>ACCESSING EXPERIMENT DATA FOR PROJECT B:O:M:37543</style>" +
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

        public override ItemTier Tier => ItemTier.Tier2;
        public override ItemTag[] ItemTags => new ItemTag[] { ItemTag.Utility, ItemTag.AIBlacklist };


        public override GameObject ItemModel => MainAssets.LoadAsset<GameObject>("Plankton.prefab");
        public override Sprite ItemIcon => MainAssets.LoadAsset<Sprite>("PlanktonIcon");


        public static GameObject ItemBodyModelPrefab;

        public override void Init(ConfigFile config)
        {
            CreateConfig(config);
            CreateLang();
            CreateItem();
            Hooks();
        }

        private void CreateConfig(ConfigFile config)
        {
            baseStackHPPercent = config.ActiveBind<float>("Item: " + ItemName, "Shield Gained with First Stack of Echo-Voltaic Plankton", .08f, "How much shield as a % of max HP should you gain with the first echo-voltaic plankton? (.08 = 8%)");
            shieldAmountOnHit = config.ActiveBind<float>("Item: " + ItemName, "Amount of Shield Gained on Hit", 1f, "How much shield should you gain when you hit an enemy?");
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            ItemBodyModelPrefab = ItemModel;
            var meshes = ItemBodyModelPrefab.GetComponentsInChildren<MeshRenderer>();
            meshes[3].gameObject.AddComponent<Wobble>();
            var itemDisplay = ItemBodyModelPrefab.AddComponent<RoR2.ItemDisplay>();
            itemDisplay.rendererInfos = ItemDisplaySetup(ItemBodyModelPrefab);

            Vector3 generalScale = new Vector3(.125f, .125f, .125f);

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
            rules.Add("mdlBandit2", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0.03487F, 0.26702F, -0.23743F),
                    localAngles = new Vector3(16.22462F, 354.7491F, 355.1796F),
                    localScale = new Vector3(0.08257F, 0.08257F, 0.08257F)
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
                    localPos = new Vector3(0F, 0.1F, -0.38F),
                    localAngles = new Vector3(9.86298F, 0.05314F, 357.6569F),
                    localScale = new Vector3(0.1F, 0.125F, 0.1F)
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
            rules.Add("mdlRailGunner", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Backpack",
                    localPos = new Vector3(0.43838F, 0.22354F, 0.00284F),
                    localAngles = new Vector3(10.28261F, 185.1785F, 91.81888F),
                    localScale = new Vector3(0.05926F, 0.05892F, 0.05926F)
                }
            });
            rules.Add("mdlVoidSurvivor", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "ShoulderL",
                    localPos = new Vector3(-0.14555F, 0.12824F, 0.149F),
                    localAngles = new Vector3(35.93249F, 73.72224F, 84.21194F),
                    localScale = new Vector3(0.03568F, 0.03669F, 0.03669F)
                }
            });
            return rules;
        }
        public override void Hooks()
        {
            //var meshes = itemDef.pickupModelPrefab.GetComponentsInChildren<MeshRenderer>();
            //meshes[3].gameObject.AddComponent<Wobble>();

            On.RoR2.HealthComponent.TakeDamage += ShieldOnHit;
            GetStatCoefficients += AddMaxShield;
        }

        private void AddMaxShield(CharacterBody sender, StatHookEventArgs args)
        {
            var inventoryCount = GetCount(sender);
            if (inventoryCount > 0)
            {
                ItemHelpers.AddMaxShieldHelper(sender, args, inventoryCount, baseStackHPPercent, 0);
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