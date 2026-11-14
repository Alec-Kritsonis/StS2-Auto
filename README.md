# StS2_Auto

## Random card-play prototype

During a single-player combat, press **F8** to toggle card autoplay on or off. The hardcoded priority order is:

1. **CanKill** — if one or more damage cards can defeat the only living enemy within current energy, play the first card in that combination.
2. **MaxValueDefend** — play the highest-block card whose block does not exceed predicted incoming damage after current block.
3. **DebuffSetup** — when current energy is at least maximum energy, play a random playable card with a debuff hover tip.
4. **RandomAttackDefault** — play a random playable attack card.
5. **RandomDefault** — play a random playable card from hand.

Cards are queued through the game's normal manual-play action. Cards that require a target are given a random valid target. This prototype does not end the turn or automate other screens.

Mod diagnostics are appended to `%APPDATA%\SlayTheSpire2\logs\Sts2_Auto\auto.log`.
