# It's time to receive...a Supply Drop!
Supply Drop is a mod for Risk of Rain 2 designed to add a number of items.
Items are designed with not just variety in mind, but with the goal of staying true to the game's design aesthetic, as well as balance.
More items are coming soon--over a dozen are planned as of now!

# What's New with Update 1.3 Part One: The Doctor is In!
Modern problems may require modern solutions, but medieval problems like a pandemic require equally time-appropriate solutions. And that may or may not mean the medicinal practices showcased in this update will be somewhat...unconventional. But despite these questionable methods, we can guarantee the items in this update are sure to cure your what ails\* your patients\**! 

<sub>\* Disclaimer: Applicable ailments are: being alive.</sub>

<sub>** Disclaimer: Your patients refers to anything that moves.</sub>

A little note from the developer: Hey everyone, going forward I intend to split major releases into two parts. The first part will reveal the update theme and what rarity the items of the second part will be. Then, the second part will reveal the name/rarity of ONE item in the next major release. Both parts, of course, will include items, as well as other major improvements/reworks. This method will allow me to get some items to you all faster, when otherwise pushing full releases would take a while.

# Included Items:
## Common Items:
- **Hardened Bone Fragments**
	- Killing an enemy temporarily grants 2 (+1 per stack) armor. All armor is lost upon taking damage.
- **Numbing Berries**
	- Gain 5 (+5 per stack) armor upon taking damage for 2 (+0.5 per stack) seconds.
- **Unassuming Tie**
	- Gain shield equal to 4% (+4% per stack) of your max HP. Breaking your shield gives you a Second Wind for 4 seconds, plus an additional amount based on max shield. Second Wind increases movement speed by 15% (+15% per stack).
- **Salvaged Wires**
	- Gain shield equal to 4% (+4% per stack) of your max HP. While shields are active, gain 10% (+10% per stack) increased attack speed.

## Uncommon Items:
- **Shell Plating**
	- Killing an enemy increases your armor permanently by .2, up to a maximum of 10 (+10 per stack) armor.
	
- **Echo-Voltaic Plankton**
	- Gain shield equal to 8% of your max HP. Dealing damage recharges 1 (+1 per stack) shield.

- **UNKNOWN ITEM**
	- `ERROR: Item log read-out scrambled during transmission. Please wait for update...`

- **UNKNOWN ITEM**
	- `ERROR: Item log read-out scrambled during transmission. Please wait for update...`
	
## Legendary Items:
- **Quantum Shield Stabilizer**
	- Gain shield equal to 16% of your max HP. If an attack exceeds your active shields, the excess damage is negated. This ability has a cooldown of 5s (-0.5s per stack).

- **Tome of Bloodletting**
	- Convert 10% (+5% per stack) of the damage you take into a damage boost of up to 20 (+10 per stack) for 4s. The boost duration is increased based on damage taken; every 10% max health that was depleted, up to 50%, increases the duration by +2s.
	
## Lunar Items:

## Equipment:

## Changelog:
- 1.3.2
	- Publicized item displays so devs of modded characters can add displays for Supply Drop items
	- Cleaned stuff up a bit

- 1.3.1
	- Patched a bug related to using the Tome of Bloodletting on Bandit that caused frame drops
	- Added rigging for ALL items to Bandit!
	- Fixed shield items still using manual IL hooks instead of new TILER2 functionality, should make things run a smidge faster
	
- 1.3.0
	- Added one new item: The Tome of Bloodletting. KNOWN ISSUE: Display in the logbook is weirdly off-center, this will be addressed in an upcoming patch.
	- Re-worked Quantum Shield Stabilizer. Still provides 16% max HP shield. Now completely negates excess damage when an attack breaks your shields, but has a 5 second cooldown before the effect can activate again.
	- Fixed issue causing console to throw warnings regarding missing scripts
	- Fixed a couple remaining minor text errors/awkwardness in logbook entries
	- Made a few minor under-the-hood optimizations

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

- 1.1.0: The Mi A(r)mor! Update
	- Added two new items, Numbing Berries and Shell Plating
	- Minor adjustments to rigging on the survivors with Hardened Bone Fragments (the rest are coming soon, I swear)
	- Re-adjusted values for Hardened Bone Fragments from .5 back to 1. This is fixing an error caused in 1.0.2.
	- A new legendary item transmission has appeared, but it is sadly scrambled. Perhaps it will be unscrambled with the remaining transmissions soon...

- 1.0.2
	- Fixed Hardened Bone Fragments to be compatible with Forgive Me Please

- 1.0.1
	- Edited README to have the correct stats for the Hardened Bone Fragments

- 1.0.0
	- Release! Includes just one item right now, the Hardened Bone Fragments. More to come!

# Special Thanks:
**KomradeSpectre:** Their code served as a foundation for this mod, and their advice and help was instrumental in getting this mod off the ground.

**ThinkInvis:** I utilize their config/logger system, as well as their TILER2 API in this mod.

**Sheybey:** They answered so many of my C# questions.

**Bord Listian:** They have been a huge help in teaching me about components and bugfixing my attempts.

**Rico:** Modeling master. They provided the model for the Numbing Berries and base canister for the Echo-Voltaic Plankton.

**XoXFaby:** They have been super helpful in explaining IL stuff, and dealing with my cluelessness towards it!

**WaltzingPhantom:** They provided the lovely item icon designs for Hardened Bone Fragments and Numbing Berries!

**Rein:** They provided so much help with getting the different components in the Tome of Bloodletting working. I can't thank them enough!
