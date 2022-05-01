using BepInEx.Configuration;
using KiraiMod.Core.UI;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace KiraiMod.Private.Modules
{
    public static class PortalCH
    {
        public static ConfigEntry<bool> Bell = Plugin.cfg.Bind("PortalCH", "Bell", true, "Should portalCH play a bell sound?");
        public static ConfigEntry<bool> Clear = Plugin.cfg.Bind("PortalCH", "Clear", true, "Should portalCH clear the console?");
        public static ConfigEntry<bool> Reset = Plugin.cfg.Bind("PortalCH", "Reset", true, "Should portalCH reset the cursor?");
        public static ConfigEntry<bool> MessageEnabled = Plugin.cfg.Bind("PortalCH", "MessageEnabled", true, "Should portalCH print the message?");
        public static ConfigEntry<string> Message = Plugin.cfg.Bind("PortalCH", "Message", "\x1b[41m\x1b[37mYou disgust me.", "What message portalCH send");

        static PortalCH()
        {
            Plugin.UIReady += ui =>
            {
                UIGroup portal = new("PortalCH", ui);
                portal.AddElement("Fire").Changed += Fire;
                portal.AddElement("Bell", Bell.Value).Bound.Bind(Bell);
                portal.AddElement("Clear", Clear.Value).Bound.Bind(Clear);
                portal.AddElement("Reset", Reset.Value).Bound.Bind(Reset);
                portal.AddElement("Message", MessageEnabled.Value).Bound.Bind(MessageEnabled);
            };
        }

        public static void Fire()
        {
            StringBuilder sb = new();

            if (Bell.Value)
                sb.Append("\x07");

            if (Clear.Value)
                sb.Append("\x1b[41m\x1b[2J");

            if (Reset.Value)
                sb.Append("\x1b[0;0H");

            if (MessageEnabled.Value)
                sb.Append(Message.Value);

            GameObject portal = VRC.SDKBase.Networking.Instantiate(
                VRC.SDKBase.VRC_EventHandler.VrcBroadcastType.Always, 
                "Portals/PortalInternalDynamic", 
                new Vector3(5306, 5306, 5306), 
                Quaternion.identity
            );

            VRC.SDKBase.Networking.RPC(
                VRC.SDKBase.RPC.Destination.AllBufferOne, 
                portal, 
                "ConfigurePortal", 
                new Il2CppSystem.Object[] {
                    "wrld_10000000-0000-0000-0000-000000000000",
                    sb.ToString(),
                    new Il2CppSystem.Int32
                    {
                        m_value = 1
                    }.BoxIl2CppObject()
                }
            );
        }
    }
}
