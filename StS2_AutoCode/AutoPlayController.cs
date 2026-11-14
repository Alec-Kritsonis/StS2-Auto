using System;
using System.Linq;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Rooms;
using StS2_Auto.StS2_AutoCode.AutoPlay;
using StS2_Auto.StS2_AutoCode.Logging;

namespace StS2_Auto.StS2_AutoCode;

public sealed partial class AutoPlayController : Node
{
    private const double RetryDelay = 0.75;
    private const double PendingCardTimeout = 5.0;
    private readonly PriorityManager _priorityManager = new();
    private readonly TargettingManager _targettingManager = new();

    private bool _enabled;
    private bool _playerTurnStarted;
    private bool _endTurnRequested;
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
        _combatManager.CombatEnded += OnCombatEnded;
    }

    public override void _ExitTree()
    {
        if (_combatManager == null)
            return;

        _combatManager.TurnStarted -= OnTurnStarted;
        _combatManager.TurnEnded -= OnTurnEnded;
        _combatManager.CombatEnded -= OnCombatEnded;
        _combatManager = null;
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is not InputEventKey key ||
            !key.Pressed ||
            key.Echo)
        {
            return;
        }

        if (key.Keycode == Key.F9)
        {
            AutoPlayLogger.CycleLogLevel();
            GetViewport().SetInputAsHandled();
            return;
        }

        if (key.Keycode != Key.F8)
            return;

        _enabled = !_enabled;
        _retryTimer = 0;
        GetViewport().SetInputAsHandled();
        AutoPlayLogger.postInfo($"{(_enabled ? "Enabled" : "Disabled")} (F8 to toggle).");
    }

    public override void _Process(double delta)
    {
        if (!_enabled || !_playerTurnStarted || _endTurnRequested)
            return;

        if (_pendingCard != null)
        {
            _pendingCardTimer -= delta;

            if (ReferenceEquals(_pendingCard.Pile, _pendingHand) &&
                _pendingCardTimer > 0)
            {
                return;
            }

            bool cardLeftHand = !ReferenceEquals(_pendingCard.Pile, _pendingHand);
            string playedCardTitle = _pendingCard.Title;
            if (!cardLeftHand)
                AutoPlayLogger.postWarn("Timed out waiting for the selected card to leave the hand.");

            _pendingCard = null;
            _pendingHand = null;
            _retryTimer = RetryDelay;
            if (cardLeftHand)
            {
                LogCurrentState(
                    $"AFTER CARD: {playedCardTitle}",
                    CombatManager.Instance.DebugOnlyGetState(),
                    debug: true);
            }
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
            !AutoPlayGameState.TryCreate(
                combatState,
                _targettingManager,
                out AutoPlayGameState? gameState) ||
            gameState == null)
            return;

        if (gameState.LivingEnemies.Count == 0)
        {
            AutoPlayLogger.postDebug("Skipping autoplay because combat has no living enemies.");
            return;
        }

        if (_priorityManager.TryGetCard(gameState, out CardPlayOption? selectedOption) &&
            selectedOption != null)
        {
            if (!selectedOption.Card.TryManualPlay(selectedOption.Target))
                return;

            _pendingCard = selectedOption.Card;
            _pendingHand = gameState.Hand;
            _pendingCardTimer = PendingCardTimeout;
            return;
        }

        if (gameState.Player.PlayerCombatState is not { } playerCombatState)
        {
            return;
        }

        _endTurnRequested = true;
        PlayerCmd.EndTurn(gameState.Player, canBackOut: false);
        AutoPlayLogger.postInfo(
            $"No useful playable cards remain; ending turn " +
            $"(energy: {playerCombatState.Energy}, " +
            $"playable: {gameState.PlayableCards.Count}, " +
            $"hand: {gameState.Hand.Cards.Count}, enemies: {gameState.LivingEnemies.Count}).");
    }

    private void OnTurnStarted(CombatState state)
    {
        if (CombatManager.Instance.IsEnemyTurnStarted)
            return;

        _playerTurnStarted = true;
        _endTurnRequested = false;
        AutoPlayLogger.WriteSeparator("START PLAYER TURN");
        LogCurrentState("PLAYER TURN START", state, debug: false);
    }

    private void OnTurnEnded(CombatState state)
    {
        if (CombatManager.Instance.IsEnemyTurnStarted)
            return;

        _playerTurnStarted = false;
        _endTurnRequested = false;
        AutoPlayLogger.WriteSeparator("END PLAYER TURN");
    }

    private void OnCombatEnded(CombatRoom room)
    {
        ResetCombatState();
        AutoPlayLogger.WriteSeparator("END COMBAT");
    }

    private void ResetCombatState()
    {
        _playerTurnStarted = false;
        _endTurnRequested = false;
        _retryTimer = 0;
        _pendingCardTimer = 0;
        _pendingCard = null;
        _pendingHand = null;
    }

    private void LogCurrentState(string label, CombatState? combatState, bool debug)
    {
        if (combatState == null ||
            !AutoPlayGameState.TryCreate(
                combatState,
                _targettingManager,
                out AutoPlayGameState? gameState) ||
            gameState?.Player.PlayerCombatState is not { } playerCombatState)
        {
            LogStateMessage($"{label}: combat state unavailable.", debug);
            return;
        }

        string hand = string.Join(", ", gameState.Hand.Cards.Select(card => card.Title));
        string enemies = string.Join(
            "; ",
            gameState.LivingEnemies.Select(enemy =>
                $"{(enemy.Monster == null ? "Unknown" : enemy.Monster.Title.ToString())} " +
                $"HP {enemy.CurrentHp}/{enemy.MaxHp}, block {enemy.Block}"));

        LogStateMessage(
            $"{label}: player HP {gameState.Player.Creature.CurrentHp}/" +
            $"{gameState.Player.Creature.MaxHp}, block {gameState.Player.Creature.Block}, " +
            $"energy {playerCombatState.Energy}/{playerCombatState.MaxEnergy}, " +
            $"incoming damage {gameState.IncomingDamage}, playable cards " +
            $"{gameState.PlayableCards.Count}, hand [{hand}], enemies [{enemies}].",
            debug);
    }

    private static void LogStateMessage(string message, bool debug)
    {
        if (debug)
            AutoPlayLogger.postDebug(message);
        else
            AutoPlayLogger.postInfo(message);
    }

    internal static void OnCombatStarting()
    {
        AutoPlayLogger.WriteSeparator("START COMBAT");

        NGame? game = NGame.Instance;
        if (game == null || !GodotObject.IsInstanceValid(game))
            return;

        game.GetNodeOrNull<AutoPlayController>(nameof(AutoPlayController))?
            .ResetCombatState();
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
        AutoPlayLogger.postInfo("Ready. Press F8 to toggle card autoplay; F9 to cycle log level.");
    }
}

[HarmonyPatch(typeof(CombatRoom), "StartCombat")]
internal static class CombatLifecyclePatch
{
    [HarmonyPrefix]
    private static void Prefix()
    {
        AutoPlayController.OnCombatStarting();
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
