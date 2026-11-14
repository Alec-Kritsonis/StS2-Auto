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
        IReadOnlyList<CardPlayOption> playableCards,
        TargettingManager targettingManager)
    {
        CombatState = combatState;
        Player = player;
        Hand = hand;
        PlayableCards = playableCards;
        TargettingManager = targettingManager;
        IncomingDamage = CalculateIncomingDamage(combatState, player.Creature);
        LivingEnemies = combatState.Enemies.Where(enemy => enemy.IsAlive).ToArray();
    }

    public CombatState CombatState { get; }
    public Player Player { get; }
    public CardPile Hand { get; }
    public IReadOnlyList<CardPlayOption> PlayableCards { get; }
    public TargettingManager TargettingManager { get; }
    public CombatOverride? ActiveOverride { get; set; }
    public int IncomingDamage { get; }
    public IReadOnlyList<Creature> LivingEnemies { get; }

    public static bool TryCreate(
        CombatState combatState,
        TargettingManager targettingManager,
        out AutoPlayGameState? state)
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
            if (TryCreatePlayOption(card, combatState, out CardPlayOption? option) &&
                option != null)
            {
                playableCards.Add(option);
            }
        }

        state = new AutoPlayGameState(combatState, player, hand, playableCards, targettingManager);
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

    private static bool TryCreatePlayOption(
        CardModel card,
        ICombatState combatState,
        out CardPlayOption? option)
    {
        option = null;

        if (card.TargetType == TargetType.AnyEnemy)
        {
            Creature[] validTargets = combatState.HittableEnemies
                .Where(card.CanPlayTargeting)
                .ToArray();

            if (validTargets.Length == 0)
                return false;

            option = new CardPlayOption(card, validTargets, null);
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

            Creature target = validTargets[Random.Shared.Next(validTargets.Length)];
            option = new CardPlayOption(card, Array.Empty<Creature>(), target);
            return true;
        }

        if (!card.CanPlayTargeting(null))
            return false;

        option = new CardPlayOption(card, Array.Empty<Creature>(), null);
        return true;
    }
}
