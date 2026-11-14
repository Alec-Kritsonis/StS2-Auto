using System.Collections.Generic;
using System.Linq;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models.Monsters;
using StS2_Auto.StS2_AutoCode.AutoPlay;
using StS2_Auto.StS2_AutoCode.AutoPlay.Circumstances;
using FatGremlinMonster = MegaCrit.Sts2.Core.Models.Monsters.FatGremlin;

namespace StS2_Auto.StS2_AutoCode.AutoPlay.Overrides.Enemies.FatGremlin;

internal sealed class FatGremlinOverride : CombatOverride
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

    private readonly IReadOnlyList<Targetting> _targettingPriorities =
    [
        new FatGremlinTargetting(),
        .. TargettingManager.DefaultPriorities
    ];

    public FatGremlinOverride()
        : base("FatGremlin")
    {
    }

    public override IReadOnlyList<Circumstance> Priorities => _priorities;
    public override IReadOnlyList<Targetting> TargettingPriorities => _targettingPriorities;

    public override bool AppliesTo(AutoPlayGameState state) =>
        state.LivingEnemies.Any(enemy => enemy.Monster is FatGremlinMonster);

    private sealed class FatGremlinTargetting : Targetting
    {
        public FatGremlinTargetting()
            : base("FatGremlinTargetting")
        {
        }

        public override bool Check(IReadOnlyList<Creature> validTargets) =>
            validTargets.Any(target => target.Monster is FatGremlinMonster);

        public override Creature? GetBestTarget(
            IReadOnlyList<Creature> validTargets,
            TargettingIntent intent) =>
            validTargets.FirstOrDefault(target => target.Monster is FatGremlinMonster);
    }
}
