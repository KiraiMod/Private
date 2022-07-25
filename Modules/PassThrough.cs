using ExitGames.Client.Photon;
using KiraiMod.Core.MessageAPI.Headers;
using System;
using System.Linq;
using System.Reflection;
using UnityEngine;
using VRC.Core;
using VRC.SDKBase;

namespace KiraiMod.Private.Modules
{
    [Module]
    public static class VoicePassThrough
    {
        public static int actor;

        private static bool _enabled;
        [Configure<bool>("Private.Pass Through.Enabled", false, Saved: false)]
        public static bool Enabled
        {
            get => _enabled;
            set
            {
                if (_enabled = value)
                {
                    VRCPlayerApi api = VRCPlayerApi.AllPlayers.Find(
                        UnhollowerRuntimeLib.DelegateSupport.ConvertDelegate<Il2CppSystem.Predicate<VRCPlayerApi>>(
                            new Predicate<VRCPlayerApi>(player =>
                                player.displayName == KiraiMod.Core.Types.UserSelectionManager.SelectedUser.displayName
                            )
                        )
                    );

                    if (api == null)
                        _enabled = false;
                    else actor = api.playerId;
                }
            }
        }

        static VoicePassThrough()
        {
            Core.Managers.RPCManager.listeners.Add(530601, (sender, message) => {
                if (Input.GetKey(KeyCode.V) || sender.VRCPlayerApi.isLocal || (_enabled && sender.VRCPlayerApi.playerId == actor)) return;

                _enabled = true;
                actor = sender.VRCPlayerApi.mPlayerId;
                Events.Update += OnUpdate;
                Plugin.log.LogMessage("You are now passing through " + sender.VRCPlayerApi.displayName);
            });

            Core.Managers.RPCManager.listeners.Add(530602, (sender, message) => {
                if (!_enabled || actor != sender.VRCPlayerApi.playerId) return;

                _enabled = false;
                Plugin.log.LogMessage("Voice pass through ended");
            });


            Plugin.harmony.Patch(
                Core.Types.VRCNetworkingClient.m_OnEvent,
                null,
                typeof(VoicePassThrough).GetMethod(nameof(VoicePassThrough.Hook_OnEvent), BindingFlags.NonPublic | BindingFlags.Static).ToHM()
            );
        }

        [Interact("Private.Pass Through.Request")]
        public static void Request() => new Core.MessageAPI.Message(530601).Send();

        [Interact("Private.Pass Through.EndRequest")]
        public static void EndRequest() => new Core.MessageAPI.Message(530602).Send();

        private static void OnUpdate()
        {
            if (!Input.GetKey(KeyCode.V)) return;

            Events.Update -= OnUpdate;
            _enabled = false;
        }

        private static void Hook_OnEvent(EventData __0)
        {
            if (!_enabled || __0.Code != 1 || __0.sender != actor) return;

            byte[] bytes =  __0.CustomData.Cast<UnhollowerBaseLib.Il2CppStructArray<byte>>();

            unsafe
            {
                fixed (byte* ptr = bytes)
                {
                    *(int*)ptr = Networking.LocalPlayer.playerId;
                }
            }

            Core.Types.VRCNetworkingClient.m_OpRaiseEvent.Invoke(
                Core.Types.VRCNetworkingClient.Type.Singleton().GetValue(null),
                new object[4]
                {
                    (byte)1,
                    new UnhollowerBaseLib.Il2CppStructArray<byte>(bytes).Cast<Il2CppSystem.Object>(),
                    new ObjectPublicObByObInByObObUnique() // convert this to a type scan
                    {
                        field_Public_EnumPublicSealedvaOtAlMa4vUnique_0 = EnumPublicSealedvaOtAlMa4vUnique.Others,
                        field_Public_EnumPublicSealedvaDoMeReAdReSlAd13SlUnique_0 = EnumPublicSealedvaDoMeReAdReSlAd13SlUnique.DoNotCache,
                    },
                    new SendOptions()
                }
            );
        }
    }
}
