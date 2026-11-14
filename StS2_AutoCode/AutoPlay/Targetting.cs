using System.Collections.Generic;
using MegaCrit.Sts2.Core.Entities.Creatures;

namespace StS2_Auto.StS2_AutoCode.AutoPlay;

internal enum TargettingIntent
{
    Default,
    ApplyDebuff
}

internal abstract class Targetting(string name)
{
    public string Name { get; } = name;

    public abstract bool Check(IReadOnlyList<Creature> validTargets);

    public abstract Creature? GetBestTarget(
        IReadOnlyList<Creature> validTargets,
        TargettingIntent intent);
}
