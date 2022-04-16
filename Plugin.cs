global using KiraiMod.Core;

using BepInEx;
using BepInEx.Configuration;
using BepInEx.IL2CPP;
using BepInEx.Logging;
using HarmonyLib;
using KiraiMod.Core.UI;
using System;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace KiraiMod.Private
{
    [BepInPlugin(GUID, "KM.Private", "0.0.0")]
    [BepInDependency("me.kiraihooks.KiraiMod", BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency("me.kiraihooks.KiraiMod.Core", BepInDependency.DependencyFlags.HardDependency)]
    public class Plugin : BasePlugin
    {
        public const string GUID = "me.kiraihooks.KiraiMod.Private";

        internal static Harmony harmony;
        internal static ManualLogSource log;
        internal static ConfigFile cfg;
        internal static event Action<UIGroup> UIReady;

        private AssetBundle bundle;

        public override void Load()
        {
            log = Log;
            harmony = new(GUID);
            cfg = Config;

            Managers.ModuleManager.Register();

            LegacyGUIManager.OnLoad += GUIManager_OnLoad;
        }

        private void GUIManager_OnLoad()
        {
            UIGroup ui = new("Private");
            ui.RegisterAsHighest();
            UIReady?.Invoke(ui);

            MemoryStream mem = new();
            Assembly.GetExecutingAssembly().GetManifestResourceStream("KiraiMod.Private.Lib.KiraiMod.Private.GUI.AssetBundle").CopyTo(mem);
            bundle = AssetBundle.LoadFromMemory(mem.ToArray());
            bundle.hideFlags |= HideFlags.HideAndDontSave;

            Transform GUI = bundle.LoadAsset("assets/private.gui.prefab")
                .Cast<GameObject>()
                .Instantiate()
                .transform;

            for (int i = 0; i < GUI.childCount; i++)
                GUI.GetChild(i).SetParent(LegacyGUIManager.UserInterface.transform);

            GUI.Destroy();
        }
    }
}
