using System.Collections.Generic;
using System.Linq;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace StS2_Auto.StS2_AutoCode.AutoPlay;

internal sealed class SummonsTargetting : Targetting
{
    public SummonsTargetting()
        : base("SummonsTargetting")
    {
    }

    public override bool Check(IReadOnlyList<Creature> validTargets) =>
        validTargets.Any(target => target.HasPower<MinionPower>()) &&
        validTargets.Any(target => !target.HasPower<MinionPower>());

    public override Creature? GetBestTarget(
        IReadOnlyList<Creature> validTargets,
        TargettingIntent intent)
    {
        Creature[] nonSummons = validTargets
            .Where(target => !target.HasPower<MinionPower>())
            .ToArray();

        IReadOnlyList<Creature> preferredTargets =
            TargettingManager.ApplyIntent(nonSummons, intent);
        return preferredTargets.OrderBy(target => target.CurrentHp).First();
    }
}
