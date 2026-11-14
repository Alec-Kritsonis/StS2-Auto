
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Godot;
using MegaCrit.Sts2.Core.Combat;
using StS2_Auto.StS2_AutoCode.Logging;

public static class CombatInspector
{
    private static bool _initialized;

    public static void Initialize()
    {
        if (_initialized)
            return;

        var manager = CombatManager.Instance;

        manager.CombatSetUp += OnCombatSetUp;
        manager.TurnStarted += OnTurnStarted;
        manager.CreaturesChanged += OnCreaturesChanged;

        _initialized = true;

        AutoPlayLogger.postInfo("CombatInspector initialized. Waiting for combat...");
    }

    private static void OnCombatSetUp(CombatState state)
    {
        LogCombat("Combat setup");
    }

    private static void OnTurnStarted(CombatState state)
    {
        LogCombat("Turn started");
    }

    private static void OnCreaturesChanged(CombatState state)
    {
        LogCombat("Creatures changed");
    }

    private static void LogCombat(string reason)
    {
        try
        {
            var state = CombatManager.Instance.DebugOnlyGetState();

            if (state == null)
            {
                AutoPlayLogger.postDebug($"{reason}: no active combat state.");
                return;
            }

            AutoPlayLogger.postDebug($"========== {reason} ==========");

            var visited = new HashSet<object>(
                ReferenceEqualityComparer.Instance);

            DumpObject("CombatState", state, 0, visited);

            AutoPlayLogger.postDebug("========== End ==========");
        }
        catch (Exception ex)
        {
            AutoPlayLogger.postError($"Combat inspection failed: {ex}");
        }
    }

    private static void DumpObject(
        string label,
        object? obj,
        int depth,
        HashSet<object> visited)
    {
        if (obj == null || depth > 5)
            return;

        Type type = obj.GetType();

        if (IsSimple(type))
        {
            AutoPlayLogger.postDebug($"{label}: {obj}");
            return;
        }

        if (!type.IsValueType && !visited.Add(obj))
            return;

        AutoPlayLogger.postDebug($"{label} [{type.Name}]");

        if (obj is IEnumerable items && obj is not string)
        {
            int index = 0;

            foreach (object? item in items)
            {
                if (index >= 20)
                {
                    AutoPlayLogger.postDebug($"{label}: stopped after 20 items");
                    break;
                }

                DumpObject(
                    $"{label}[{index}]",
                    item,
                    depth + 1,
                    visited);

                index++;
            }

            return;
        }

        foreach (PropertyInfo property in type.GetProperties(
                     BindingFlags.Instance | BindingFlags.Public))
        {
            if (!property.CanRead ||
                property.GetIndexParameters().Length > 0)
                continue;

            string propertyName = property.Name;

            try
            {
                object? value = property.GetValue(obj);

                if (value == null)
                    continue;

                if (IsSimple(value.GetType()))
                {
                    if (IsInteresting(propertyName))
                    {
                        AutoPlayLogger.postDebug(
                            $"{label}.{propertyName} = {value}");
                    }
                }
                else if (ShouldExplore(propertyName))
                {
                    DumpObject(
                        $"{label}.{propertyName}",
                        value,
                        depth + 1,
                        visited);
                }
            }
            catch (Exception ex)
            {
                AutoPlayLogger.postWarn(
                    $"Could not read " +
                    $"{type.Name}.{propertyName}: {ex.GetType().Name}");
            }
        }
    }

    private static bool IsSimple(Type type)
    {
        Type actualType = Nullable.GetUnderlyingType(type) ?? type;

        return actualType.IsPrimitive
            || actualType.IsEnum
            || actualType == typeof(string)
            || actualType == typeof(decimal)
            || actualType == typeof(DateTime)
            || actualType == typeof(Guid);
    }

    private static bool IsInteresting(string name)
    {
        string n = name.ToLowerInvariant();

        return n.Contains("hp")
            || n.Contains("health")
            || n.Contains("energy")
            || n.Contains("block")
            || n.Contains("hand")
            || n.Contains("card")
            || n.Contains("name")
            || n.Contains("title")
            || n.Contains("intent")
            || n.Contains("strength")
            || n.Contains("dead")
            || n.Contains("turnnumber")
            || n.Contains("currentside")
            || n.Contains("phase")
            || n.Contains("amount")
            || n.Contains("count")
            || n.Contains("id");
    }

    private static bool ShouldExplore(string name)
    {
        string n = name.ToLowerInvariant();

        return n.Contains("player")
            || n.Contains("monster")
            || n.Contains("creature")
            || n.Contains("combatstate")
            || n.Contains("hand")
            || n.Contains("card")
            || n.Contains("power")
            || n.Contains("intent")
            || n == "players"
            || n == "monsters";
    }
}
