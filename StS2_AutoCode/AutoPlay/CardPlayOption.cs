using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;

namespace StS2_Auto.StS2_AutoCode.AutoPlay;

internal sealed record CardPlayOption(CardModel Card, Creature? Target);
