using System.Collections.Generic;
using StS2_Auto.StS2_AutoCode.AutoPlay;
using StS2_Auto.StS2_AutoCode.AutoPlay.Circumstances;

namespace StS2_Auto.StS2_AutoCode.AutoPlay.Overrides.Enemies;

internal sealed class DefaultCombatOverride : CombatOverride
{
    private readonly IReadOnlyList<Circumstance> _priorities =
    [
        new CanKillCircumstance(),
        new GoldGlowCircumstance(),
        new EfficientDefendCircumstance(),
        new DebuffSetupCircumstance(),
        new RandomAttackDefaultCircumstance(),
        new RandomDefaultCircumstance()
    ];

    public DefaultCombatOverride()
        : base("Default")
    {
    }

    public override IReadOnlyList<Circumstance> Priorities => _priorities;
    public override IReadOnlyList<Targetting> TargettingPriorities =>
        TargettingManager.DefaultPriorities;

    public override bool AppliesTo(AutoPlayGameState state) => true;
}
