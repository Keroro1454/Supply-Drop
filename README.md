# It's time to receive...a Supply Drop!
Supply Drop is a mod for Risk of Rain 2 designed to add a number of items.
Items are designed with not just variety in mind, but with the goal of staying true to the game's design aesthetic, as well as balance.
More items are coming soon--over a dozen are planned as of now!

# What's New with Update 1.5: By the Light of the Moon!
Historically, the moon has been a pain in the butt of humanity. I know all you lunar nerds are upset to hear it, but it's true. Consider what the moon has ever done for us:

Created werewolves? Terrible dinner party guests.

Housed Nazis? Those guys suck. And were also probably terrible dinner party guests.

Driven us crazy? They put me in a room for that!

Enabled Halloween cosmetics? If I hear that fly set one more time...

But today, for the first time, the moon has done something halfway decent: It's given us some brand new items to play with! Admittedly, because the moon can't help but be annoying, these items may be of...dubious benefit, but still,
maybe that damn rock has turned over a new leaf. We doubt it though. No leaves on the moon.

----
# Included Items:
Icon | Item | Description | Rarity
---|---:|---:|---
<img src="https://i.imgur.com/nv4V8R8.png" width="100"/> | **Hardened Bone Fragments** | Killing an enemy temporarily grants 5 (+1 per stack) armor. Some armor is lost upon taking damage; higher damage loses more armor. | Common 
<img src="https://i.imgur.com/VTys6yV.png" width="100"/> | **Numbing Berries** | Gain 10 (+5 per stack) armor upon taking damage for 2 (+0.5 per stack) seconds. | Common 
<img src="https://i.imgur.com/0DuVrzU.png" width="100"/> | **Unassuming Tie** |  Gain shield equal to 4% (+4% per stack) of your max HP. Breaking your shield gives you a Second Wind for 5 seconds, plus an additional amount based on max shield. Second Wind increases movement speed by 15% (+15% per stack). | Common 
<img src="https://i.imgur.com/WIuK51q.png" width="100"/> | **Salvaged Wires** | Gain shield equal to 4% (+4% per stack) of your max HP. While shields are active, gain 10% (+10% per stack) increased attack speed. | Common 
<img src="https://i.imgur.com/Rw2A1He.png" width="100"/> | **Shell Plating** | Killing an enemy increases your armor permanently by .5, up to a maximum of 25 (+10 per stack) armor. |  Uncommon 
<img src="https://i.imgur.com/4BJoA04.png" width="100"/> | **Echo-Voltaic Plankton** | Gain shield equal to 8% of your max HP. Dealing damage recharges 1 (+1 per stack) shield. |  Uncommon 
<img src="https://i.imgur.com/kuqjqSn.png" width="100"/> |  **Vintage Plague Mask** |  All healing is increased by 2% (+2% per stack) for every damage item you possess. | Uncommon 
<img src="https://i.imgur.com/5slZP70.png" width="100"/> | **Vintage Plague Hat** | Increase maximum HP by 1% (+1% per stack) for every utility item you possess. | Uncommon 
<img src="https://i.imgur.com/0L658Aq.png" width="100"/> | **Quantum Shield Stabilizer** | Gain shield equal to 16% of your max HP. If an attack exceeds your active shields, the excess damage is negated. This ability has a cooldown of 5s (-1s per stack). | Legendary 
<img src="https://i.imgur.com/2Fgehyx.png" width="100"/> | **Tome of Bloodletting** | Convert 10% (+10% per stack) of the damage you take into a damage boost of up to 20 (+10 per stack) for 4s. The boost duration is increased based on damage taken; every 10% max health that was depleted, up to 50%, increases the duration by +2s. |  Legendary 
<img src="https://i.imgur.com/9ee2ayr.png" width="100"/> | **Afterlife Insurance** | Gain 25% (+10% per stack) less money from killing monsters. 100% (+25% per stack) of money lost is invested into upgrading your insurance to cover more threats, up to 10 times. Upon dying to an source you are insured for, you will be revived, and your insurance level will be reset to zero. | Lunar 
<img src="https://i.imgur.com/2lSag6M.png" width="100"/> |  **Silver Pocketwatch** | All buff durations are increased by 50% (+25% per stack). All debuff durations are also increased by 50% (+25% per stack). | Lunar
<img src="https://i.imgur.com/I8mSekS.png" width="100"/> |  **Arrogant Canting** | Elite enemies have a 6% (+6% per stack) chance to drop a random item on death, but they also gain 25% (+25% per stack) more HP and 15% (+15% per stack) more damage. | Lunar
<img src="https://i.imgur.com/FGIJv4a.png" width="100"/> |  **Two-Sided Die** | Whenever you use a shrine, 2 (+2 per stack) stats are picked at random. One is buffed by 50%, the other is nerfed by 25%. | Lunar

----
## Changelog:
- 1.5.0: By the Light of the Moon!
	- Additions:
		- Added four new items: Arrogant Canting, Afterlife Insurance, Silver Pocketwatch, and Two-Sided Die
	- Bug Fixes:
		- Fixed the Tome of Bloodletting bleeding visual not appearing -------> TO BE DONE


<details>
<summary>
1.4: The Doctor is Back! and Finally Back!
</summary>

- 1.4.12
	- Updated for Seekers of the Storm
- 1.4.11
	- Improvements:
		- Changed the README to not display all patchnotes by default anymore
		- Fixed some typos in logbook entries
	- Bug Fixes:
		- Fixed Hardened Shell Plating incorrectly calculating armor, which spammed NREs and made you invincible?
		- Fixed the item display for the Unassuming Tie on Miner being massive
		- Fixed item displays for unsupported modded characters often being way too big
- 1.4.10: Finally Back!
	- Updated for Survivors of the Void
	- Code has been completely re-worked to remove dependency on TILER2
	- Completely reworked how Plague Mask works unde the hood--it should actually work now! (Thanks to Phreel for showing me how to hook Healing without needing IL anymore!)
	- Reworked the README to be fancy!
	- Additions:
		- Added configuration options to the Vintage Plague Mask and Vintage Plague Hat items
		- Added item displays for Railgunner and Void Fiend (plus finished the missing ones for Bandit)
		- Added item displays to all characters for Hardened Bone Fragments
		- Added item displays for a large number of modded characters (Nemmando, HAN-D, Enforcer, Nemforcer, Paladin, Miner, Pathfinder, Executioner, House, Tesla, Desolator, Arsonist, Rocket)
	- Improvements:
		- Fixed or improved item displays for numerous items on all characters
		- Made configs of all items more clear
		- Tweaked lore entries for almost every item
	- Balance Changes:
		- BUFF: Increased Hardened Bone Fragment's base armor granted per buff stack, from 2 to 5
		- BUFF: Increased Shell Plating's armor granted per kill, from .2 to .5
		- BUFF: Increased Shell Plating's base max armor, from 10 to 25
		- BUFF: Increased Unassuming Tie's base duration of Second Wind, from 4 to 5 seconds
		- BUFF: Increased Quantum Shield Stabilizer's cooldown reduction from additional stacks, from 0.5 to 1 seconds
		- BUFF: Increased Tome of Bloodletting's damage conversion percentage from additional stacks, from 5% to 10%
		- BUFF: Increased Numbing Berries' base armor granted, from 5 to 10
	- Bug Fixes:
		- Fixed Hardened Bone Fragments not adjusting its values to the config values
		- Fixed many of the pickup displays being exceedingly big (or very small in some cases)
		- Fixed Quantum Shield Generator not taking barrier into consideration during its shield-gate calculations
		- Fixed Shell Plating throwing a whole mess of an NRE (and not working as a result)
		- Fixed Vintage Plague Mask and Hat not updating their descriptions based on changes in their configs
- 1.4.9
	- Removed a debugging measure that was spamming the console
- 1.4.8
	- Shifted ReCalculateStats hook over from using the defunct TILER2 version to the new R2API version
	- Nerfed Vintage Plague Hat bonus to maximum HP per Utility item from +2% to +1%. Will keep my eye on this change to see if it's too severe
	- Fixed Vintage Plague Mask reducing healing per Damage item instead of increasing it
- 1.4.7
	- Compatibility update for the Anniversary Update
	- Theoretically fully fixed the QSS and funky shield interactions with Transcendence. Hopefully it decides to work for real now.
	- Added item displays for all items for the new survivor, Bandit
- 1.4.6
	- Fully fixed Quantum Shield Stabilizer determining if OSP needed to trigger off pre-armor damage calculations. Thanks gaforb for the follow-up!
	- Fixed Quantum Shield Stabilizer activating as a result of fall damage, despite fall damage not actually damaging shield.
	- Set 'Fear of Reading' config option for the Tome of Bloodletting to 'false' by default. You must now choose to be haunted by this timeshare-possessing spectre!
- 1.4.5
	- Fixed issue in the Transcendence fix from 1.4.2 that was causing unending shield gains if you stacked 49+ Salvaged Wires/Unassuming Ties. Thanks Omnishade for the report!
- 1.4.4
	- Actually fixed Vintage Plague Mask not properly granting heal bonus. Thanks SyfleNov for the follow-up!
	- Fixed Quantum Shield Stabilizer determining if OSP needed to trigger off pre-armor damage calculations. Thanks gaforb for the report!
	- Nerfed Vintage Plague Mask and Vintage Plague Hat initial stack bonuses, from 4% to 2%
	- Publicized Vintage Plague Mask and Vintage Plague Hat for access by other mod creators
	- Implemented a new physics technique to calculate Unassuming Tie movement (also allows character creators to implement it for their displays). Now with over 100% more wiggly formalwear!
- 1.4.3
	- Fixed Vintage Plague Mask not properly granting heal bonus. Thanks SyfleNov for the report!
- 1.4.2
	- Fixed Quantum Shield Stabilizer proc-ing inconsistently. It should now activate when its supposed to, and not when it shouldn't. Thanks MightyW0lf for the report!
	- Fixed Shell Plating config options not properly affecting the item in-game. They will actually do something now! Thanks AndyDoe for the report!
	- (Somewhat) Fixed shield items granting shield based on your post-Transcendence HP (i.e., 1). They will now grant shield based on your HP + Shield, which isn't quite how vanilla does it, but I find is a better alternative. Thanks sanity-dance for the report!
- 1.4.1
	- Fixed formatting errors in the logbook descriptions of Unassuming Tie, Salvaged Wires, and Plague Hat
	- Added missing credit to Aaron for the help with the IL hook of Plague Hat
- 1.4.0: The Doctor is BACK!
	- Added two new items: The Vintage Plague Mask and Vintage Plague Hat
	- Added configuration options to ALL pre-existing items (excludes newest additions). Any config changes are reflected in item logs/descriptions
	- Remade ALL item icons to be more visually-consistent with vanilla icons
	- Reworked Hardened Bone Fragments slightly; the number of fragment stacks lost on hit is now tied to the amount of damage you receive
	- Fixed shield items not granting reduced shield gains on additional stacks
	- Fixed a typo in the QSS pick-up description
	- Cleaned stuff up a lot
	
</details>

<details>
<summary>
1.3: The Doctor is In...
</summary>

- 1.3.2
	- Publicized item displays so devs of modded characters can add displays for Supply Drop items
	- Cleaned stuff up a bit
- 1.3.1
	- Patched a bug related to using the Tome of Bloodletting on Bandit that caused frame drops
	- Added rigging for ALL items to Bandit!
	- Fixed shield items still using manual IL hooks instead of new TILER2 functionality, should make things run a smidge faster
- 1.3.0: The Doctor is In...
	- Added one new item: The Tome of Bloodletting. KNOWN ISSUE: Display in the logbook is weirdly off-center, this will be addressed in an upcoming patch.
	- Re-worked Quantum Shield Stabilizer. Still provides 16% max HP shield. Now completely negates excess damage when an attack breaks your shields, but has a 5 second cooldown before the effect can activate again.
	- Fixed issue causing console to throw warnings regarding missing scripts
	- Fixed a couple remaining minor text errors/awkwardness in logbook entries
	- Made a few minor under-the-hood optimizations
	
</details>

<details>
<summary>
1.2: Shield Your Eyes!
</summary>

- 1.2.3
	- Buffed Hardened Bone Fragments to provide 1 -> 2 armor per kill. Additional item stacks still give +1 additional armor on kill, this may be subject to change
	- Fixed Echo-Voltaic Plankton not proc-ing shield against bosses
	- Fixed Echo-Voltaic Plankton having a random reflection probe that messed with a few specific reflection shaders (Thanks Pikmin88 for the report)
	- Fixed some minor text errors in logbook entries
- 1.2.2
	- Completed under-the-hood migration to TILER2's 3.0 version. Thanks Chen and Komrade for help with this
	- Fixed Salvaged Wires and Unassuming Tie logbook entries not reflecting the nerf they received last patch
	- Fixed Quantum Shield Generator's blue projection not spinning all the time, and removed the orb's bobbing due to glitchiness this caused (May patch this back in at some point)
	- Work continues on the next big update...
- 1.2.1
	- Nerfed stacking shield gain on Salvaged Wires and Unassuming Tie (+4% -> +2% per stack)
	- Fixed Numbing Berries not appearing on Acrid
	- Fixed Numbing Berries calculating stacking armor gain incorrectly (Thanks Crescendo for the report)
- 1.2.0: The Shield Your Eyes! Update
	- Added four new items: The Unassuming Tie, Salvaged Wires, Echo-Voltaic Plankton, and Quantum Shield Generator
	- Reduced max armor acquirable from first stack of Shell Plating from 20 to 10 (Each kill grants .2 instead of .4). Increased max stacks granted by additional Shell Platings by 25 to compensate.
	- Fixed Shell Plating not carrying over between stages
	- Fixed Shell Plating sometimes not appearing properly on MUL-T's model
	- Made slight changes/fixes to the logs of Hardened Bone Fragments, Shell Plating, and Numbing Berries
	- Optimized some code with Numbing Berries and Hardened Bone Fragments that was previously causing performance dips when ClassicItems and/or BanditReloaded was present

</details>

<details>
<summary>
1.1: Mi A(rmor)!
</summary>

- 1.1.0: The Mi A(r)mor! Update
	- Added two new items, Numbing Berries and Shell Plating
	- Minor adjustments to rigging on the survivors with Hardened Bone Fragments (the rest are coming soon, I swear)
	- Re-adjusted values for Hardened Bone Fragments from .5 back to 1. This is fixing an error caused in 1.0.2.
	- A new legendary item transmission has appeared, but it is sadly scrambled. Perhaps it will be unscrambled with the remaining transmissions soon...

</details>

<details>
<summary>
1.0: Incoming Supply Drop!
</summary>

- 1.0.2
	- Fixed Hardened Bone Fragments to be compatible with Forgive Me Please
- 1.0.1
	- Edited README to have the correct stats for the Hardened Bone Fragments
- 1.0.0
	- Release! Includes just one item right now, the Hardened Bone Fragments. More to come!

</details>

----
# Special Thanks:
----
**KomradeSpectre:** Their code served as a foundation for this mod, and their advice and help was instrumental in getting this mod off the ground.

**ThinkInvis:** I utilize their config/logger system, as well as their TILER2 API in this mod.

**Sheybey:** They answered so many of my C# questions.

**Bord Listian:** They have been a huge help in teaching me about components and bugfixing my attempts.

**Rico:** Modeling master. They provided the model for the Numbing Berries and base canister for the Echo-Voltaic Plankton.

**XoXFaby:** They have been super helpful in explaining IL stuff, and dealing with my cluelessness towards it!

**WaltzingPhantom:** They provided the lovely old item icon designs for Hardened Bone Fragments and Numbing Berries!

**Rein:** They provided so much help with getting the different components in the Tome of Bloodletting working. I can't thank them enough!

**Aaron:** They helped a ton with puzzling through the Plague Hat's IL hook!

**Phreel:** Helped provide some fixes with porting to SOTV as well as motivation to actually get around to fixing this mod!