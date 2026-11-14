# StS2_Auto

## Random card-play prototype

During a single-player combat, press **F8** to toggle card autoplay on or off. Press **F9** to cycle logging between **INFO** (INFO, WARN, and ERROR messages; includes state at turn start) and **DEBUG** (all messages, including state after each played card). The hardcoded priority order is:

1. **CanKill** — if one or more damage cards can defeat the only living enemy within current energy, play the first card in that combination.
2. **GoldGlow** — play the first playable card that the game marks with a gold glow, indicating its conditional effect is active.
3. **EfficientDefend** — when predicted unblocked incoming damage exceeds 10% of current health, play the block card that brings it closest to that threshold.
4. **DebuffSetup** — when current energy is at least maximum energy, play a random playable card with a debuff hover tip.
5. **RandomAttackDefault** — play a random playable attack card.
6. **RandomDefault** — play a random playable card from hand.

Combat starts and ends are marked in the mod log. Cards are queued through the game's normal manual-play action. Lethal plans choose their targets as part of the plan; debuff cards prefer the legal enemy with the fewest existing debuffs. Other enemy attacks prefer non-summons over minions, then target the lowest-health valid enemy. When no useful card can be selected by the active priorities, autoplay automatically ends the turn, unless combat has no living enemies. Other screens are not automated.

Combat overrides can provide their own circumstance and targetting priority lists while reusing the default strategies. The Damp and Calcified Cultists currently use the standard priorities without **EfficientDefend**. If a Fat Gremlin is present, it is always prioritized as the target of enemy-targeting cards. Other combats use the default priority order above.

Before combat priorities run, card overrides can select preferred playable cards. **AlwaysPlayFirst** prioritizes Stampede, then Rage.

Mod diagnostics are appended to `%APPDATA%\SlayTheSpire2\logs\Sts2_Auto\auto.log`.
