using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Runs;

namespace StS2_Auto.StS2_AutoCode;

public sealed partial class AutoPlayController : Node
{
    private const double RetryDelay = 0.75;
    private const double PendingCardTimeout = 5.0;
    private static readonly Random Random = new();

    private bool _enabled;
    private double _retryTimer;
    private double _pendingCardTimer;
    private CardModel? _pendingCard;
    private CardPile? _pendingHand;

    public override void _Input(InputEvent @event)
    {
        if (@event is not InputEventKey key ||
            key.Keycode != Key.F8 ||
            !key.Pressed ||
            key.Echo)
        {
            return;
        }

        _enabled = !_enabled;
        _retryTimer = 0;
        GetViewport().SetInputAsHandled();
        GD.Print($"[AutoPlay] {(_enabled ? "Enabled" : "Disabled")} (F8 to toggle).");
    }

    public override void _Process(double delta)
    {
        if (!_enabled)
            return;

        if (_pendingCard != null)
        {
            _pendingCardTimer -= delta;

            if (ReferenceEquals(_pendingCard.Pile, _pendingHand) &&
                _pendingCardTimer > 0)
            {
                return;
            }

            if (ReferenceEquals(_pendingCard.Pile, _pendingHand))
                GD.Print("[AutoPlay] Timed out waiting for the selected card to leave the hand.");

            _pendingCard = null;
            _pendingHand = null;
            _retryTimer = RetryDelay;
            return;
        }

        _retryTimer -= delta;
        if (_retryTimer > 0)
            return;

        _retryTimer = RetryDelay;
        TryPlayRandomCard();
    }

    private void TryPlayRandomCard()
    {
        CombatState? combatState = CombatManager.Instance.DebugOnlyGetState();
        if (combatState == null ||
            combatState.CurrentSide != CombatSide.Player ||
            CombatManager.Instance.IsOverOrEnding)
        {
            return;
        }

        RunState? runState = RunManager.Instance.DebugOnlyGetState();
        if (runState == null || runState.Players.Count != 1)
            return;

        Player player = runState.Players[0];
        PlayerCombatState? playerCombatState = player.PlayerCombatState;
        if (playerCombatState == null)
            return;

        CardPile hand = playerCombatState.Hand;
        var playableCards = new List<(CardModel Card, Creature? Target)>();

        foreach (CardModel card in hand.Cards)
        {
            if (TryChooseTarget(card, combatState, out Creature? target))
                playableCards.Add((card, target));
        }

        if (playableCards.Count == 0)
            return;

        (CardModel selectedCard, Creature? selectedTarget) =
            playableCards[Random.Next(playableCards.Count)];

        if (!selectedCard.TryManualPlay(selectedTarget))
            return;

        _pendingCard = selectedCard;
        _pendingHand = hand;
        _pendingCardTimer = PendingCardTimeout;
        GD.Print("[AutoPlay] Queued a random playable card from the hand.");
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

            target = validTargets[Random.Next(validTargets.Length)];
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

            target = validTargets[Random.Next(validTargets.Length)];
            return true;
        }

        return card.CanPlayTargeting(null);
    }

    internal static void Install(NGame game)
    {
        if (game.GetNodeOrNull<AutoPlayController>(nameof(AutoPlayController)) != null)
            return;

        var controller = new AutoPlayController
        {
            Name = nameof(AutoPlayController)
        };
        game.AddChild(controller);
        GD.Print("[AutoPlay] Ready. Press F8 to toggle random card play.");
    }
}

[HarmonyPatch(typeof(NGame), nameof(NGame._Ready))]
internal static class AutoPlayControllerPatch
{
    [HarmonyPostfix]
    private static void Postfix(NGame __instance)
    {
        AutoPlayController.Install(__instance);
    }
}
