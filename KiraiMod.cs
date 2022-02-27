global using KiraiMod.Core;

using BepInEx;
using BepInEx.IL2CPP;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace KiraiMod.Private
{
    [BepInPlugin("me.kiraihooks.KiraiMod.Private", "KM.Private", "0.0.0")]
    public class KiraiMod : BasePlugin
    {
        AssetBundle bundle;

        public override void Load()
        {
            Managers.ModuleManager.Register();
            Managers.GUIManager.OnLoad += GUIManager_OnLoad;
        }

        private void GUIManager_OnLoad()
        {
            MemoryStream mem = new();
            Assembly.GetExecutingAssembly().GetManifestResourceStream("KiraiMod.Private.Lib.KiraiMod.Private.GUI.AssetBundle").CopyTo(mem);
            bundle = AssetBundle.LoadFromMemory(mem.ToArray());
            bundle.hideFlags |= HideFlags.HideAndDontSave;

            Transform GUI = bundle.LoadAsset("assets/private.gui.prefab")
                .Cast<GameObject>()
                .Instantiate()
                .transform;

            for (int i = 0; i < GUI.childCount; i++)
                GUI.GetChild(i).SetParent(Managers.GUIManager.UserInterface.transform);

            GUI.Destroy();
        }
    }
}
