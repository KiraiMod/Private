using System.Collections.Generic;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common.Interfaces;

namespace KiraiMod.Private.Modules.WorldMods
{
    [Module]
    public class Murder4
    {
        static Murder4() => Managers.WorldManager.WorldIDMods.Add(("wrld_858dfdfc-1b48-4e1e-8a43-f0edc611e5fe", typeof(Murder4)));

        public static UdonBehaviour Revolver;
        public static UdonBehaviour Grenade;
        public static UdonBehaviour GameLogic;
        public static List<UdonBehaviour> lights = new();

        private static int randCount = 0;

        public Murder4()
        {
            DelayedInit().Start();
            Events.Update += OnUpdate;
        }

        ~Murder4() => Events.Update -= OnUpdate;

        private static System.Collections.IEnumerator DelayedInit()
        {
            yield return new WaitForSeconds(5);

            Initialize();
            HelpMessage();
        }

        private static void Initialize()
        {
            Plugin.log.LogDebug("GameLogic::UdonBehaviour = " + (GameLogic = GameObject.Find("Game Logic")?.GetComponent<UdonBehaviour>()) is not null);
            Plugin.log.LogDebug("Revolver::UdonBehaviour = " + (Revolver = GameObject.Find("Game Logic/Weapons/Revolver")?.GetComponent<UdonBehaviour>()) is not null);
            Plugin.log.LogDebug("Grenade::UdonBehaviour = " + (Grenade = GameObject.Find("Game Logic/Weapons/Unlockables/Frag (0)")?.GetComponent<UdonBehaviour>()) is not null);

            Revolver.transform.Find("Recoil Anim/Recoil/Laser Sight").gameObject.active = true;

            GameObject.Find("Game Logic/Player HUD/Blind HUD Anim").active = false;
            GameObject.Find("Game Logic/Player HUD/Flashbang HUD Anim").active = false;
            Plugin.log.LogDebug("Disabled blinding screens");

            Transform parent = GameObject.Find("Game Logic/Switch Boxes").transform;
            for (int i = 0; i < parent.childCount; i++)
                lights.Add(parent.GetChild(i).GetComponent<UdonBehaviour>());
            Plugin.log.LogDebug("Found all the light switches");
        }

        private static void HelpMessage()
        {
            Plugin.log.LogMessage("KeyPad1 - Portal detective room");
            Plugin.log.LogMessage("KeyPad2 - Lights out after 15 seconds");
            Plugin.log.LogMessage("KeyPad3 - End the round");
            Plugin.log.LogMessage("KeyPad4 - Fire revolver once");
            Plugin.log.LogMessage("KeyPad5 - Explode a random player once");
            Plugin.log.LogMessage("KeyPad6 - Fix Bounds");
            Plugin.log.LogMessage("KeyPad7 - Fire revolver every frame");
            Plugin.log.LogMessage("KeyPad8 - Explode a random player every frame");
            Plugin.log.LogMessage("KeyPad9 - Restore Bounds");
        }

        private static void OnUpdate()
        {
            if (Input.GetKeyDown(KeyCode.Keypad1)) SpawnDetectivePortal();
            if (Input.GetKeyDown(KeyCode.Keypad2)) LightsOut();
            if (Input.GetKeyDown(KeyCode.Keypad3)) AbortRound();

            if (Input.GetKeyDown(KeyCode.Keypad4)) FireDetectiveRevolver();
            if (Input.GetKey(KeyCode.Keypad7)) FireDetectiveRevolver();

            if (Input.GetKeyDown(KeyCode.Keypad5)) ExplodeRandom();
            if (Input.GetKey(KeyCode.Keypad8)) ExplodeRandom();

            if (Input.GetKeyDown(KeyCode.Keypad6)) FixBounds();
            if (Input.GetKeyDown(KeyCode.Keypad9)) RestoreBounds();
        }

        private static void SpawnDetectivePortal()
        {
            GameObject portal = Networking.Instantiate(VRC_EventHandler.VrcBroadcastType.Always, "Portals/PortalInternalDynamic", new Vector3(4.9533f, 2.995f, 126.9965f), Quaternion.identity);

            if (portal == null) return;

            Networking.RPC(RPC.Destination.AllBufferOne, portal, "ConfigurePortal", new Il2CppSystem.Object[] {
                "wrld_9cebd25e-2f85-4808-9ff2-c0aedf60c207",
                "\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n",
                new Il2CppSystem.Int32
                {
                    m_value = 1
                }.BoxIl2CppObject()
            });
        }

        private static void FireDetectiveRevolver() => Revolver?.SendCustomNetworkEvent(NetworkEventTarget.All, "SyncFire");

        private static void ExplodeRandom()
        {
            randCount++;
            if (randCount >= VRCPlayerApi.AllPlayers.Count)
                randCount = 0;

            var target = VRCPlayerApi.AllPlayers[randCount];
            if (target.isLocal)
            {
                ExplodeRandom();
                return;
            }

            if (Networking.GetOwner(Grenade.gameObject) != Networking.LocalPlayer)
                Networking.SetOwner(Networking.LocalPlayer, Grenade.gameObject);

            Grenade.gameObject.transform.position = target.gameObject.transform.position;
            Grenade.SendCustomNetworkEvent(NetworkEventTarget.All, "Explode");
        }

        public static void LightsOut() => lights.ForEach(light => light.SendCustomNetworkEvent(NetworkEventTarget.All, "SwitchDown"));
        public static void AbortRound() => GameLogic.SendCustomNetworkEvent(NetworkEventTarget.All, "SyncAbort");

        public static void RestoreBounds()
        {
            GameLogic.enabled = true;
            GameObject.Find("Game Logic/Game Area Bounds").transform.localScale = new Vector3(77.2646f, 7.938f, 43.289f);
        }

        public static void FixBounds()
        {
            GameLogic.enabled = false;
            GameObject.Find("Game Logic/Game Area Bounds").transform.localScale = new Vector3(1000, 1000, 1000);
        }
    }
}
