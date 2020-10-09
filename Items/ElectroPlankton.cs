using System.Collections.ObjectModel;
using System.Collections.Generic;
using R2API;
using RoR2;
using UnityEngine;
using TILER2;
using static TILER2.StatHooks;
using K1454.SupplyDrop;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using System.Reflection;

//TO-DO: Rig model to Loader, Captain
namespace SupplyDrop.Items
{
    class ElectroPlankton : Item<ElectroPlankton>
    {
        public override string displayName => "Echo-Voltaic Plankton";

        public override ItemTier itemTier => RoR2.ItemTier.Tier2;

        public override ReadOnlyCollection<ItemTag> itemTags => new ReadOnlyCollection<ItemTag>(new[] { ItemTag.Utility });
        protected override string NewLangName(string langid = null) => displayName;

        protected override string NewLangPickup(string langID = null) => "Recharge shield upon dealing damage.";

        protected override string NewLangDesc(string langID = null) => "Gain a <style=cIsUtility>shield</style> equal to <style=cIsUtility>8%</style> of your maximum health. " +
            "Dealing damage recharges <style=cIsUtility>1</style> <style=cStack>(+1 per stack)</style> <style=cIsUtility>shield</style>.";

        protected override string NewLangLore(string landID = null) => "<style=cMono>ACCESSING EXPERIMENT DATA FOR PROJECT B:O:M:37543</style>" +
            "\n\n<style=cMono>EXPERIMENT LOG B:O:M:37543:1</style>\nNewly-minted Senior Researcher Thomas here! This is my first time as project lead, pretty exciting.\n\nThese micro-organisms were recovered under the ice around Titan's equator region. " +
            "They appear to be identical in every way to Earth zooplankton, though substantially larger. I intend to run a full dissection of several tomorrow." +
            "\n\n<style=cMono>EXPERIMENT LOG B:O:M:37543:2</style>\nDissection proved to be...valuable. Upon dissecting the specimens, we found that every single one of them possess a small cellular 'sac' of sorts, " +
            "though its purpose is unclear. It was unresponsive to almost all stimuli except damaging it, which caused it to immediately disintegrate into dead...bone cells?" +
            "\n\nI hope we find something of interest, sac or otherwise, because otherwise this project is a bust..." +
            "\n\n<style=cMono>EXPERIMENT LOG B:O:M:37543:3</style>\nShit. That was a [REDACTED]." +
            "\n\nJR John was putting away his tools when he accidentally cut himself with a scalpel. Nothing major, but he didn't sound happy.\n\nOut of nowhere, " +
            "JR Dave...well he was scooping a fresh sample container out of the tank when he suddenly started screaming. Thrashing all over the place. " +
            "John ran over and pulled Dave's arm out of the tank.\n\nI'm outside their room now. Med-bot said their condition is critical. " +
            "\n\n<style=cMono>EXPERIMENT LOG B:O:M:37543:4</style>\nThese things...they're evil. I know, I know" +
            "but it's true. I've tested my suspicions...these things are even more disturbing than I thought.\n\nThe plankton respond to sound by seemingly converting it to energy, and in massive amounts too. " +
            "But the only sound they react to is vocalizations of pain. The worse, the more energy they release. And you can't trick them either. It has to be genuine. It can't be a recording. They know. Somehow." +
            "\n\n<style=cMono>END OF EXPERIMENT DATA</style>" +
            "\n<style=cMono>> DELETE FILE\n> DO YOU WANT TO PROCEED AND DELETE THIS FILE? [Y/N]\n> Y</style>" +
            "\n\n<style=cMono>ERROR: FILE CANNOT BE REMOVED. S.R. THOMAS ENCRYPTION KEY REQUIRED.\n\n> SEND MESSAGE TO |||||||||||</style>" +
            "\n\nJ-\nI want those logs off the server now. The test generator has already been shipped off to M.\n\nOh, and I want that idiot dealt with.\n- S";

        private static List<RoR2.CharacterBody> Playername = new List<RoR2.CharacterBody>();
        public static GameObject ItemBodyModelPrefab;

        public ElectroPlankton()
        {
            modelPathName = "@SupplyDrop:Assets/Main/Models/Prefabs/Plankton.prefab";
            iconPathName = "@SupplyDrop:Assets/Main/Textures/Icons/PlanktonIcon.png";
        }

        private static ItemDisplayRuleDict GenerateItemDisplayRules()
        {
            ItemBodyModelPrefab.AddComponent<ItemDisplay>();
            ItemBodyModelPrefab.GetComponent<ItemDisplay>().rendererInfos = SupplyDropPlugin.ItemDisplaySetup(ItemBodyModelPrefab);

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
                    localScale = new Vector3(0.85f, 0.85f, 0.85f)
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

        protected override void LoadBehavior()
        {

            if (ItemBodyModelPrefab == null)
            {
                ItemBodyModelPrefab = regDef.pickupModelPrefab;
                regItem.ItemDisplayRules = GenerateItemDisplayRules();
                var meshes = regDef.pickupModelPrefab.GetComponentsInChildren<MeshRenderer>();
                meshes[3].gameObject.AddComponent<Wobble>();
                regDef.pickupModelPrefab.transform.localScale = new Vector3(1.75f, 1.75f, 1.75f);
            }
            IL.RoR2.CharacterBody.RecalculateStats += IL_AddMaxShield;
            On.RoR2.HealthComponent.TakeDamage += ShieldOnHit;
        }

        protected override void UnloadBehavior()
        {
            IL.RoR2.CharacterBody.RecalculateStats -= IL_AddMaxShield;
            On.RoR2.HealthComponent.TakeDamage -= ShieldOnHit;
        }
            
        private void IL_AddMaxShield(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            c.GotoNext(
                x => x.MatchLdloc(43),
                x => x.MatchCallvirt(typeof(CharacterBody).GetMethod("set_maxShield", BindingFlags.Instance | BindingFlags.NonPublic))
                );

            c.Emit(OpCodes.Ldarg, 0);
            c.Emit(OpCodes.Ldloc, 43);
            c.EmitDelegate<Func<CharacterBody, float, float>>((characterBody, shield) =>
            {
                    if (GetCount(characterBody) > 0)
                    {
                        return shield + (characterBody.maxHealth * 0.08f);
                    }
                return shield;
            }
            );
            c.Emit(OpCodes.Stloc, 43);
        }
        private void ShieldOnHit(On.RoR2.HealthComponent.orig_TakeDamage orig, HealthComponent self, DamageInfo damageInfo)
        {
            var Attacker = damageInfo.attacker.GetComponent<CharacterBody>();
            var InvenCount = GetCount(Attacker);
            if (Attacker && InvenCount > 0)
            {
                Attacker.healthComponent.RechargeShield(InvenCount);
            }
            orig(self, damageInfo);
        }
    }
}