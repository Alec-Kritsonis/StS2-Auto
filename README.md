# StS2_Auto

## Random card-play prototype

During a single-player combat, press **F8** to toggle card autoplay on or off. Press **F9** to cycle logging between **INFO** (INFO, WARN, and ERROR messages; includes state at turn start) and **DEBUG** (all messages, including state after each played card). The hardcoded priority order is:

1. **CanKill** — if one or more damage cards can defeat the only living enemy within current energy, play the first card in that combination.
2. **EfficientDefend** — when predicted unblocked incoming damage exceeds 10% of current health, play the block card that brings it closest to that threshold.
3. **DebuffSetup** — when current energy is at least maximum energy, play a random playable card with a debuff hover tip.
4. **RandomAttackDefault** — play a random playable attack card.
5. **RandomDefault** — play a random playable card from hand.

Combat starts and ends are marked in the mod log. Cards are queued through the game's normal manual-play action. Lethal plans choose their targets as part of the plan; debuff cards prefer the legal enemy with the fewest existing debuffs. Other enemy attacks prefer non-summons over minions, then target the lowest-health valid enemy. When the player's energy is depleted and no cards can be played, autoplay automatically ends the turn, unless combat has no living enemies. Other screens are not automated.

Mod diagnostics are appended to `%APPDATA%\SlayTheSpire2\logs\Sts2_Auto\auto.log`.
