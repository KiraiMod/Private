using ExitGames.Client.Photon;
using KiraiMod.Core.Utils;
using System;

namespace KiraiMod.Private.Modules
{
    public static class E1
    {
        public static Bound<bool> AutoDeafen = new();

        static E1()
        {
            Plugin.UIReady += ui => ui.AddElement("Auto Deafen", AutoDeafen);

            AutoDeafen.ValueChanged += state =>
            {
                if (state)
                    Events.Update += Deafen;
                else Events.Update -= Deafen;
            };
        }

        public static void Deafen()
        {
            // if anyone asks: i got these bytes from abbez
            byte[] bytes = {
                0, 0, 0, 0, // actor id
                0, 0, 0, 0, // server time
                187, 134, 59,  0,   248, 125, 232, 192,
                92,  160, 82,  254, 48,  228, 30,  187,
                149, 196, 177, 215, 140, 223, 127, 209,
                66,  60,  0,   226, 53,  180, 176, 97,
                104, 4,   248, 238, 195, 125, 44, 185,
                182, 68,  94,  114, 205, 181, 150, 56,
                232, 126, 247, 155, 123, 172, 108, 98,
                80,  56,  113, 89,  160, 125, 221
            };

            unsafe
            {
                fixed (byte* ptr = bytes)
                {
                    *(int*)ptr = VRC.SDKBase.Networking.LocalPlayer.playerId;
                    *(((int*)ptr) + 1) = VRC.SDKBase.Networking.GetServerTimeInMilliseconds();
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
