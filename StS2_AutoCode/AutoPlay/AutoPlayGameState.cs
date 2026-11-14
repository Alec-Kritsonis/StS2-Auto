using System;
using System.Collections.Generic;
using System.Linq;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.Runs;

namespace StS2_Auto.StS2_AutoCode.AutoPlay;

internal sealed class AutoPlayGameState
{
    public AutoPlayGameState(
        CombatState combatState,
        Player player,
        CardPile hand,
        IReadOnlyList<CardPlayOption> playableCards)
    {
        CombatState = combatState;
        Player = player;
        Hand = hand;
        PlayableCards = playableCards;
        IncomingDamage = CalculateIncomingDamage(combatState, player.Creature);
        LivingEnemies = combatState.Enemies.Where(enemy => enemy.IsAlive).ToArray();
    }

    public CombatState CombatState { get; }
    public Player Player { get; }
    public CardPile Hand { get; }
    public IReadOnlyList<CardPlayOption> PlayableCards { get; }
    public int IncomingDamage { get; }
    public IReadOnlyList<Creature> LivingEnemies { get; }

    public static bool TryCreate(CombatState combatState, out AutoPlayGameState? state)
    {
        state = null;
        if (combatState.CurrentSide != CombatSide.Player ||
            CombatManager.Instance.IsOverOrEnding)
        {
            return false;
        }

        RunState? runState = RunManager.Instance.DebugOnlyGetState();
        if (runState == null || runState.Players.Count != 1)
            return false;

        Player player = runState.Players[0];
        PlayerCombatState? playerCombatState = player.PlayerCombatState;
        if (playerCombatState == null)
            return false;

        CardPile hand = playerCombatState.Hand;
        var playableCards = new List<CardPlayOption>();

        foreach (CardModel card in hand.Cards)
        {
            if (TryChooseTarget(card, combatState, out Creature? target))
                playableCards.Add(new CardPlayOption(card, target));
        }

        state = new AutoPlayGameState(combatState, player, hand, playableCards);
        return true;
    }

    private static int CalculateIncomingDamage(CombatState combatState, Creature player)
    {
        int totalDamage = 0;
        Creature[] targets = { player };

        foreach (Creature enemy in combatState.Enemies.Where(creature => creature.IsAlive))
        {
            if (enemy.Monster == null)
                continue;

            foreach (AttackIntent attack in enemy.Monster.NextMove.Intents.OfType<AttackIntent>())
                totalDamage += attack.GetTotalDamage(targets, enemy);
        }

        return totalDamage;
    }

    private static bool TryChooseTarget(
        CardModel card,
        ICombatState combatState,
        out Creature? target)
    {
        target = null;

        if (card.TargetType == TargetType.AnyEnemy)
        {
            Creature[] validTargets = combatState.HittableEnemies
                .Where(card.CanPlayTargeting)
                .ToArray();

            if (validTargets.Length == 0)
                return false;

            target = validTargets[Random.Shared.Next(validTargets.Length)];
            return true;
        }

        if (card.TargetType == TargetType.AnyAlly)
        {
            Creature[] validTargets = combatState.Allies
                .Where(creature => creature.IsAlive)
                .Where(card.CanPlayTargeting)
                .ToArray();

            if (validTargets.Length == 0)
                return false;

            target = validTargets[Random.Shared.Next(validTargets.Length)];
            return true;
        }

        return card.CanPlayTargeting(null);
    }
}
