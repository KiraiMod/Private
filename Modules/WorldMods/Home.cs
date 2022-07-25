using System.Linq;
using UnityEngine;
using VRC.Udon;

namespace KiraiMod.Private.Modules.WorldMods
{
    [Module]
    public class Home
    {
        static Home() => Managers.WorldManager.WorldIDMods.Add(("wrld_4432ea9b-729c-46e3-8eaf-846aa0a37fdd", typeof(Home)));

        private readonly UdonBehaviour[] pedestals;

        public Home()
        {
            pedestals = UnityEngine.Object.FindObjectsOfType<UdonBehaviour>()
                .Where(x => x._eventTable.ContainsKey("Trigger"))
                .ToArray();

            Events.Update += OnUpdate;
        }

        ~Home() => Events.Update -= OnUpdate;

        private void OnUpdate()
        {
            if (Input.GetKeyDown(KeyCode.Keypad1) || Input.GetKey(KeyCode.Keypad2))
                foreach (UdonBehaviour ub in pedestals)
                    ub.SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "Trigger");
        }
    }
}
