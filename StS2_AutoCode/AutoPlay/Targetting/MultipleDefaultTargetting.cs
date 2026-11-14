using System.Collections.Generic;
using System.Linq;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;

namespace StS2_Auto.StS2_AutoCode.AutoPlay;

internal sealed class MultipleDefaultTargetting : Targetting
{
    public MultipleDefaultTargetting()
        : base("MultipleDefaultTargetting")
    {
    }

    public override bool Check(IReadOnlyList<Creature> validTargets) =>
        validTargets.Count > 0;

    public override Creature? GetBestTarget(
        IReadOnlyList<Creature> validTargets,
        TargettingIntent intent)
    {
        IReadOnlyList<Creature> preferredTargets =
            TargettingManager.ApplyIntent(validTargets, intent);
        return preferredTargets.OrderBy(target => target.CurrentHp).First();
    }
}
