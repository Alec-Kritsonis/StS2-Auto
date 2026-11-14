using System.Collections.Generic;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;

namespace StS2_Auto.StS2_AutoCode.AutoPlay;

internal sealed record CardPlayOption(
    CardModel Card,
    IReadOnlyList<Creature> ValidTargets,
    Creature? Target)
{
    public CardPlayOption WithTarget(Creature target) =>
        this with { Target = target };
}
