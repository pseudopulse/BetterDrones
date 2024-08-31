# Better Drones
A mod that tweaks drones to make them more useful and less boring. Fully configurable.

# Changes
## Global
- Drones orbit their owner at a fixed distance like in ROR1
- Drones that are orbiting cannot collide with enemies
- Can be toggled off, overrides movement changes when enabled

- All drones have halved base damage and higher skill damage (so they arent op with drone parts)
- All drones are faster and have higher health and regen

- Drones prioritize enemies that have been recently pinged by their owner

- Allies no longer absorb hitscan attacks like Flamethrower and Double Tap
- Minions no longer absorb projectiles

## Healing Drone
- Longer targeting range
- Faster healing
- Less aggressive
- Sticks closer to owner
- Stops healing targets above 85% health.

## Incinerator Drone
- More range
- More damage
- 100% ignite chance
- More aggressive

## Gunner Drone
- More range
- More aggressive
- More damage
- Higher knockback
- Sticks closer to owner

## Missile Drone
- More range
- Less aggressive
- More damage
- Sticks closer to owner

## Emergency Drone
- Longer targeting range
- Faster healing
- Less aggressive
- Stops healing targets above 85% health.
- Has a passive healing aura that heals nearby allies for 10% max health every 5 seconds
- Spawns in Sulfur Pools, Scorched Acres, Abandoned Aqueduct, and the Artifact Reliquary
- Sticks closer to owner

## TC-280
- Longer targeting range
- More aggressive
- Much tankier
- No longer spawns naturally
- Is now a guaranteed map secret on rallypoint (does NOT count as an interactable for spawn credits)

## Gunner Turret
- Longer targeting range
- Tankier
- Energy Shield that switches on for 5 seconds after dealing 1000% total damage, blocks projectiles from the outside.

# Changelog
## 1.7.4
- updated for SOTS
## 1.7.3
- made the drone sound disabling configurable
- fixed certain config values being ints when they should be floats
## 1.7.2
- fixed bullets that have no attacker assigned breaking entirely
## 1.7.1
- (mostly) fixed jitter with orbital movement (drones will still jitter a bit when running at very high speeds)
- all drones no longer have collision with their owner
## 1.7.0
- nerfed the gunner turret shield (damage req: 1000% -> 2500%; duration: 5s -> 4s)
- nerfed the missile drone's damage output (missile count: 6 -> 5)
- fixed the stat halving not affecting scaling
## 1.6.3
- reverted 1.6.2
## 1.6.2
- updated BepInIncompatibility
## 1.6.1
- orbital drones are no longer targeted by enemies
## 1.6.0
- cloak propagates to drones if orbitals are enabled
## 1.5.5
- fixed healing drone ai for real this time
## 1.5.4
- moredrones compat
## 1.5.3
- fixed healing drone ai being really dumb and not healing the player often
## 1.5.2
- orbiting radius is now larger
- fixed mechanical enemies also being perfectly accurate
## 1.5.1
- tc-280 now scales cost properly
## 1.5.0
- added proper offsets to the tc-280 and empathy cores so you can see now
- made the tc-280 no longer naturally spawn
- made the tc-280 a guaranteed map secret on rallypoint
- tc-280 is now orbit blacklisted by default
## 1.4.1
- fixed gunner turrets orbiting the player (hopoo why are they flagged as flying what)
## 1.4.0
- heavily nerfed tc-280
- tc-280 laser is now disabled by default
- added config for drone transparency (default is 50% transparent)
- made healing drones not ignore you
- drones now evenly position themselves around you while orbiting, instead of randomly clumping up
- incinerator drones no longer orbit by default
- gave an orbit offset to strike drones from the backup
- gave an orbit offset to the tc-280 prototype
## 1.3.0
- made drones not make idle noise
- smoothed out orbital movement for real this time
## 1.2.3
- Added orbit offsets for solus probes and strike drones
## 1.2.2
- Nerfed Gunner Drones to be less free win friday
- Smoothed out the orbital movement to be less snappy
- Gunner Turrets have a periodic shield that activates upon dealing a total of 1000% damage
## 1.2.1
- Added config for orbit height
## 1.2.0
- Minions ignore projectile collision from their owner
- Reduced Gunner Drone knockback
- Reduced Emergency Drone healing aura to 10%
- TC-280 laser no longer applies Beetle Juice
## 1.1.1
- readme change lmao
## 1.1.0
- Added a configurable blacklist for orbital movement
## 1.0.0
- mod exists now

