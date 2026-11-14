using System;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes;
using StS2_Auto.StS2_AutoCode.AutoPlay;
using StS2_Auto.StS2_AutoCode.Logging;

namespace StS2_Auto.StS2_AutoCode;

public sealed partial class AutoPlayController : Node
{
    private const double RetryDelay = 0.75;
    private const double PendingCardTimeout = 5.0;
    private readonly PriorityManager _priorityManager = new();

    private bool _enabled;
    private bool _playerTurnStarted;
    private double _retryTimer;
    private double _pendingCardTimer;
    private CardModel? _pendingCard;
    private CardPile? _pendingHand;
    private CombatManager? _combatManager;

    public override void _Ready()
    {
        _combatManager = CombatManager.Instance;
        _combatManager.TurnStarted += OnTurnStarted;
        _combatManager.TurnEnded += OnTurnEnded;
    }

    public override void _ExitTree()
    {
        if (_combatManager == null)
            return;

        _combatManager.TurnStarted -= OnTurnStarted;
        _combatManager.TurnEnded -= OnTurnEnded;
        _combatManager = null;
    }

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
        AutoPlayLogger.postInfo($"{(_enabled ? "Enabled" : "Disabled")} (F8 to toggle).");
    }

    public override void _Process(double delta)
    {
        if (!_enabled || !_playerTurnStarted)
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
                AutoPlayLogger.postWarn("Timed out waiting for the selected card to leave the hand.");

            _pendingCard = null;
            _pendingHand = null;
            _retryTimer = RetryDelay;
            return;
        }

        _retryTimer -= delta;
        if (_retryTimer > 0)
            return;

        _retryTimer = RetryDelay;
        TryPlayByPriority();
    }

    private void TryPlayByPriority()
    {
        CombatState? combatState = CombatManager.Instance.DebugOnlyGetState();
        if (combatState == null ||
            !AutoPlayGameState.TryCreate(combatState, out AutoPlayGameState? gameState) ||
            gameState == null ||
            !_priorityManager.TryGetCard(gameState, out CardPlayOption? selectedOption) ||
            selectedOption == null)
            return;

        if (!selectedOption.Card.TryManualPlay(selectedOption.Target))
            return;

        _pendingCard = selectedOption.Card;
        _pendingHand = gameState.Hand;
        _pendingCardTimer = PendingCardTimeout;
    }

    private void OnTurnStarted(CombatState state)
    {
        if (CombatManager.Instance.IsEnemyTurnStarted)
            return;

        _playerTurnStarted = true;
        AutoPlayLogger.WriteSeparator("START PLAYER TURN");
    }

    private void OnTurnEnded(CombatState state)
    {
        if (CombatManager.Instance.IsEnemyTurnStarted)
            return;

        _playerTurnStarted = false;
        AutoPlayLogger.WriteSeparator("END PLAYER TURN");
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
        AutoPlayLogger.postInfo("Ready. Press F8 to toggle card autoplay.");
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
