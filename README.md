# SCP008 [![GitHub release](https://badgen.net/github/release/Rnen/SCP008)](https://GitHub.com/Rnen/SCP008/releases/) [![Github all releases](https://img.shields.io/github/downloads/Rnen/SCP008/total.svg)](https://GitHub.com/Rnen/SCP008/releases/) [![GitHub latest commit](https://badgen.net/github/last-commit/Rnen/SCP008)](https://GitHub.com/Rnen/SCP008/commit/) [![GitHub watchers](https://img.shields.io/github/watchers/Rnen/SCP008.svg?style=social&label=Watch&maxAge=2592000)](https://GitHub.com/Rnen/SCP008/watchers/)




* Zombies (SCP-049-2) will infect players with SCP-008. 
* They will constantly take damage untill death or they heal themselves with a medkit. 
* When the player dies of the SCP-008 effect, they themselves become a Zombie (SCP-049-2) 

> Requires [SMod](https://github.com/Grover-c13/Smod2/)

> How to install: Go to [Releases](https://github.com/Rnen/SCP008/releases) and pick your correct version. Drop it in plugins folder

## Config Options
Config Key | Default Value | Description
--: | :--: | :--
SCP008_enabled | True | Enables / Disables plugin functionality
SCP008_damage_amount | 1 | How much it damages per interval
SCP008_damage_interval | 2 | How often it damages (in seconds)
SCP008_swing_damage | 0 | How much 049-2's attacks deal (0 or below will use default value)
scp008_zombiekill_infects | False | If regular kills by zombies should transform players
scp008_infect_chance | 100 | How much chance there is for infection when hit
scp008_cure_enabled | True | If you can cure infection by health-kit
scp008_cure_chance | 100 | How much chance for beeing cured
scp008_ranklist_commands | | What server ranks can use the plugin commands (Serves as a secondary whitelist)
scp008_roles_caninfect | -1 | What game roles can be infected (-1 is all roles)
scp008_canhit_tutorial | true | If zombies can hit tutorial players or not
scp008_anyDeath | false | If any death after infection should transform players
scp008_assist079_experience | 35f | SCP 079 EXP received for assists 
scp008_broadcast | true | Players receive a personal broadcast upon infection and transformation
scp008_broadcast_duration | 7 | Duration of the personal broadcast above

## Commands
Command | Arguements (If Any) | Description
--: | :--: | :--
scp008 / scp8 |  | Enables / Disables plugin functionality
infect | Player / ID / Steam | Infects / cures player
