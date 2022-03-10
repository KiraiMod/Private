using BepInEx.Configuration;
using ExitGames.Client.Photon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace KiraiMod.Private.Modules
{
    public static class Moderations
    {
        public static ConfigEntry<bool> AntiBlock = Plugin.cfg.Bind("Moderations", "AntiBlock", true, "Should you still see people that block you");

        static Moderations()
        {
            Plugin.log.LogDebug("setting up");
            Plugin.harmony.Patch(
                Core.Types.VRCNetworkingClient.m_OnEvent,
                typeof(Moderations).GetMethod(nameof(OnEvent), BindingFlags.NonPublic | BindingFlags.Static).ToHM()
            );
        }

        private static bool OnEvent(EventData __0)
        {
            if (__0.Code != 33 || __0.CustomData == null) return true;

            var dict = __0.customData.Cast<Il2CppSystem.Collections.Generic.Dictionary<byte, Il2CppSystem.Object>>();
            if ((dict[0].Unbox<byte>() ^ 0x1) == 0)
            {
                if (dict.Count == 4)
                {
                    int actor = dict[1].Unbox<int>();
                    VRC.SDKBase.VRCPlayerApi player = VRC.SDKBase.VRCPlayerApi.AllPlayers.Find(
                        UnhollowerRuntimeLib.DelegateSupport.ConvertDelegate<Il2CppSystem.Predicate<VRC.SDKBase.VRCPlayerApi>>(
                            new Predicate<VRC.SDKBase.VRCPlayerApi>(x => x.playerId == actor)
                        )
                    );

                    // probably was just the config events?
                    if (player == null) return true;

                    bool k10 = dict[10].Unbox<bool>();
                    Plugin.log.LogMessage($"{player.displayName} {(k10 ? "block" : dict[11].Unbox<bool>() ? "mut" : "revert")}ed you");

                    // according to stellar this shouldn't work for when people join
                    // but it seems to work
                    return !k10;

                    // Truth table:
                    //            Key 10  Key 11
                    // blocked    TRUE    FALSE
                    // unblocked  FALSE   FALSE
                    // muted      FALSE   TRUE
                    // unmuted    FALSE   FALSE
                }
            }

            return true;
        }
    }
}
