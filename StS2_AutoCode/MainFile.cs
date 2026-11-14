using System.Reflection;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Modding;
using StS2_Auto.StS2_AutoCode.Logging;

namespace StS2_Auto.StS2_AutoCode;

//You're recommended but not required to keep all your code in this package and all your assets in the StS2_Auto folder.d
[ModInitializer(nameof(Initialize))]
public partial class MainFile : Node
{
  public const string ModId = "StS2_Auto"; //At the moment, this is used only for the Logger and harmony names.

  public static void Initialize()
  {
    AutoPlayLogger.Initialize();
    AutoPlayLogger.postInfo("Mod initialized.");
    var assembly = Assembly.GetExecutingAssembly();

    //If you want to use scripts defined in your mod for Godot scenes, uncomment the following line.
    //Godot.Bridge.ScriptManagerBridge.LookupScriptsInAssembly(assembly);

    Harmony harmony = new(ModId);

    harmony.PatchAll(assembly);
    MegaCrit.Sts2.Core.Nodes.NGame? game = MegaCrit.Sts2.Core.Nodes.NGame.Instance;
    if (game != null && GodotObject.IsInstanceValid(game))
    {
      AutoPlayController.Install(game);
    }

    // global::CombatInspector.Initialize();
  }
}
