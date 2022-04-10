using UnityEngine.UI;
using VRC.Udon;
using VRC.Udon.Common.Interfaces;

namespace KiraiMod.Private.Modules
{
    public static class Manipulator
    {
        private static bool repeater;
        public static bool Repeater
        {
            get => repeater;
            set
            {
                if (repeater == value) return;
                repeater = value;

                if (value)
                    Events.Update += Repeat;
                else Events.Update -= Repeat;
            }
        }

        public static bool networked;
        public static bool targeted;

        public static (UdonBehaviour, string) regAlpha;
        public static (UdonBehaviour, string) regBeta;
        public static (UdonBehaviour, string) regGamma;

        public static UdonBehaviour _current;
        public static string _lastEvent;
        public static UdonBehaviour[] _cached;

        static Manipulator() => Events.WorldInstanceLoaded += _ => GUI.Manipulator.ShowBehaviours();

        public static void Send((UdonBehaviour behaviour, string eventName) reg)
        {
            if (targeted)
            {
                VRC.SDKBase.Networking.SetOwner(KiraiMod.Modules.Players.Target.VRCPlayerApi, reg.behaviour.gameObject);
                reg.behaviour.SendCustomNetworkEvent(NetworkEventTarget.Owner, reg.eventName);
            }
            else if (networked)
                reg.behaviour.SendCustomNetworkEvent(NetworkEventTarget.All, reg.eventName);
            else reg.behaviour.SendCustomEvent(reg.eventName);
        }

        private static void Repeat()
        {
            if (regAlpha.Item1 != null) Send(regAlpha);
            if (regBeta.Item1 != null) Send(regBeta);
            if (regGamma.Item1 != null) Send(regGamma);
        }

        public static void SetRegister(ref (UdonBehaviour, string) register, Text display, string placeholder) 
        {
            if (register.Item1 == _current && register.Item2 == _lastEvent)
                register = (null, null);
            else register = (_current, _lastEvent);

            display.text = register.Item1 == null
                ? placeholder 
                : $"{_current.name}.{_lastEvent}";
        }

        public static void Broadcast(string eventName)
        {
            foreach (UdonBehaviour ub in _cached)
                foreach (var kvp in ub._eventTable)
                    if (kvp.key == eventName)
                        Send((ub, eventName));
        }
    }
}
