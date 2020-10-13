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

namespace SupplyDrop.Items
{
    class SalvagedWires : Item<SalvagedWires>
    {
        public override string displayName => "Salvaged Wires";

        public override ItemTier itemTier => RoR2.ItemTier.Tier1;

        public override ReadOnlyCollection<ItemTag> itemTags => new ReadOnlyCollection<ItemTag>(new[] { ItemTag.Utility });
        protected override string NewLangName(string langid = null) => displayName;

        protected override string NewLangPickup(string langID = null) => "Gain some shield, and gain increased attack speed while your shield is active.";

        protected override string NewLangDesc(string langID = null) => "Gain a <style=cIsUtility>shield</style> equal to <style=cIsUtility>4%</style> <style=cStack>(+4% per stack)</style> of your maximum health. " +
            "While <style=cIsUtility>shield</style> is active, increases <style=cIsDamage>attack speed</style> by <style=cIsUtility>10%</style> <style=cStack>(+10% per stack)</style>.";

        protected override string NewLangLore(string landID = null) => "\"Now remember y'all. There are three rules of Space Scrappin'. You squirts may be dumber than rocks, but I 'spect y'all to remember them.\"" +
            "\n\n." +
            "\n." +
            "\n." +
            "\n\n\"Rule 1: Batteries on, or you're gone.\"" +
            "\n\nThe derelict frigate drifted slowly, almost imperceptibly. If the Safekeepings had found itself abandoned in an atmosphere, " +
            "Alyk would have heard the sounds he was all too accustomed to. The aged guts of the ship creaking, the groaning of rusted hyperdrive intakes, the crackle of errant wisps of energy as they finally escaped the old wiring." +
            "\n\n\"Rule 2: O2 below 20, leave it be.\"" +
            "\n\nBut in the void of space, Alyk could only hear his labored breaths, and his whispered Salvage Mantras. He paused to pull wires off the tracks, " +
            "his headlamp barely illuminating the area of the ducts in front of him. At 11, this was his first off-world salvage. His job was simple; due to his small frame, Alyk made for the perfect wire retriever. " +
            "And the Safekeepings was large enough, and full of enough electrum-alloy wiring, that a successful job here could mean food for his family for weeks." +
            "\n\n\"Rule 3: Pay only comes after a full day.\"" +
            "\n\nThe thought of smiles on his family's faces pushed the young boy forward. He stuffed the wires he had already claimed from the metal husk, and turned to face the dark tunnel before him. " +
            "Alyk took a deep breath, clenched his small hands into tight fists, then clamored down further into the silent wreck.";

        private static List<RoR2.CharacterBody> Playername = new List<RoR2.CharacterBody>();
        public static GameObject ItemBodyModelPrefab;

        public SalvagedWires()
        {
            modelPathName = "@SupplyDrop:Assets/Main/Models/Prefabs/WireBundle.prefab";
            iconPathName = "@SupplyDrop:Assets/Main/Textures/Icons/SalvagedWiresIcon.png";
        }

        private static ItemDisplayRuleDict GenerateItemDisplayRules()
        {
            ItemBodyModelPrefab.AddComponent<ItemDisplay>();
            ItemBodyModelPrefab.GetComponent<ItemDisplay>().rendererInfos = SupplyDropPlugin.ItemDisplaySetup(ItemBodyModelPrefab);

            Vector3 generalScale = new Vector3(0.5f, 0.5f, 0.5f);
            ItemDisplayRuleDict rules = new ItemDisplayRuleDict(new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "ThighR",
                    localPos = new Vector3(0.025f, 0f, 0f),
                    localAngles = new Vector3(350f, 20f, 50f),
                    localScale = generalScale

                }
            });
            rules.Add("mdlHuntress", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "ThighR",
                    localPos = new Vector3(0f, 0.15f, 0.05f),
                    localAngles = new Vector3(350f, 20f, 50f),
                    localScale = generalScale * .8f
                }
            });
            rules.Add("mdlToolbot", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "UpperArmL",
                    localPos = new Vector3(0.5f, 1f, 0f),
                    localAngles = new Vector3(350f, 20f, 50f),
                    localScale = generalScale * 8f
                }
            });
            rules.Add("mdlEngi", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Pelvis",
                    localPos = new Vector3(-0.07f, 0.1f, 0.1f),
                    localAngles = new Vector3(330f, 50f, 0f),
                    localScale = generalScale
                }
            });
            rules.Add("mdlMage", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "ThighR",
                    localPos = new Vector3(0.02f, 0.1f, 0.1f),
                    localAngles = new Vector3(-10f, 20f, 50f),
                    localScale = generalScale * .8f
                }
            });
            rules.Add("mdlMerc", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "ThighR",
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
                    localPos = new Vector3(.7f, -0.3f, -0.1f),
                    localAngles = new Vector3(0f, 30f, 350f),
                    localScale = new Vector3(1f, 1f, 1f)
                }
            });
            rules.Add("mdlLoader", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "ThighR",
                    localPos = new Vector3(0.005f, 0.1f, 0.05f),
                    localAngles = new Vector3(-10f, 20f, 50f),
                    localScale = generalScale
                }
            });
            rules.Add("mdlCroco", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "ThighR",
                    localPos = new Vector3(-.5f, -.5f, 0f),
                    localAngles = new Vector3(350f, 10f, 50f),
                    localScale = new Vector3(3f, 3f, 3f)
                }
            });
            rules.Add("mdlCaptain", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "ThighR",
                    localPos = new Vector3(0f, 0.2f, 0f),
                    localAngles = new Vector3(-10f, 20f, 50f),
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
            regDef.pickupModelPrefab.transform.localScale = new Vector3(1f, 1f, 1f) * 6f;
            GetStatCoefficients += AttackSpeedBonus;
            IL.RoR2.CharacterBody.RecalculateStats += IL_AddMaxShield;
        }

        protected override void UnloadBehavior()
        {
            GetStatCoefficients -= AttackSpeedBonus;
            IL.RoR2.CharacterBody.RecalculateStats -= IL_AddMaxShield;
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
                    return shield + (characterBody.maxHealth * (0.04f * GetCount(characterBody)));
                }
                return shield;
            }
            );
            c.Emit(OpCodes.Stloc, 43);
        }

        private void AttackSpeedBonus(CharacterBody sender, StatHookEventArgs args)
        {
            var InventoryCount = GetCount(sender);
            if (sender.healthComponent.shield != 0)
            {
                args.attackSpeedMultAdd += .1f * InventoryCount;

            }
        }
    }
}