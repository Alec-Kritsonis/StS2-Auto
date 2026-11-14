using System.Collections.Generic;
using System.Linq;
using MegaCrit.Sts2.Core.Models.Monsters;
using StS2_Auto.StS2_AutoCode.AutoPlay;
using StS2_Auto.StS2_AutoCode.AutoPlay.Circumstances;

namespace StS2_Auto.StS2_AutoCode.AutoPlay.Overrides.Enemies.Cultist;

internal sealed class CultistOverride : CombatOverride
{
    private readonly IReadOnlyList<Circumstance> _priorities =
    [
        new CanKillCircumstance(),
        new GoldGlowCircumstance(),
        new DebuffSetupCircumstance(),
        new RandomAttackDefaultCircumstance(),
        new RandomDefaultCircumstance()
    ];

    public CultistOverride()
        : base("Cultist")
    {
    }

    public override IReadOnlyList<Circumstance> Priorities => _priorities;
    public override IReadOnlyList<Targetting> TargettingPriorities =>
        TargettingManager.DefaultPriorities;

    public override bool AppliesTo(AutoPlayGameState state) =>
        state.LivingEnemies.Any(enemy => enemy.Monster is DampCultist or CalcifiedCultist);
}
