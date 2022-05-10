using KiraiMod.Core.ModuleAPI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace KiraiMod.Private.Modules
{
    [Module]
    public static class AutoKick
    {
        private static Coroutine coroutine;
        private static WaitForSeconds wait = new(5);
        private static WaitForSeconds wait2 = new(0.1f);
        private static int counter = 1;

        private static GameObject qm;
        private static Transform list;
        private static UnityEngine.UI.Button here;
        private static UnityEngine.UI.Button kick;
        private static UnityEngine.UI.Button yeah;
        private static GameObject fren;
        private static UnityEngine.UI.Button popup;

        static AutoKick()
        {
            Plugin.UIReady += ui => ui.AddElement("Auto Kick", false).Bound.ValueChanged += OnChange;
        }

        private static void OnChange(bool state)
        {
            if (state)
            {
                qm = GameObject.Find("UserInterface").transform.Find("Canvas_QuickMenu(Clone)").gameObject;
                popup = GameObject.Find("UserInterface").transform.Find("MenuContent/Popups/AlertPopup/Button").GetComponent<UnityEngine.UI.Button>();

                Transform window = qm.transform.Find("Container/Window");

                list = window.transform.Find("QMParent/Menu_Here/ScrollRect/Viewport/VerticalLayoutGroup/QM_Grid_UsersInWorld").transform;
                here = window.transform.Find("Page_Buttons_QM/HorizontalLayoutGroup/Page_Here")?.GetComponent<UnityEngine.UI.Button>();
                kick = window.transform.Find("QMParent/Menu_SelectedUser_Local/ScrollRect/Viewport/VerticalLayoutGroup/Buttons_UserActions/Button_VoteKick")?.GetComponent<UnityEngine.UI.Button>();
                yeah = window.transform.Find("QMParent/Modal_ConfirmDialog/MenuPanel/Buttons/Button_Yes")?.GetComponent<UnityEngine.UI.Button>();
                fren = window.transform.Find("QMParent/Menu_SelectedUser_Local/ScrollRect/Viewport/VerticalLayoutGroup/Buttons_UserActions/Button_Unfriend").gameObject;

                coroutine = AutoKickRoutine().Start();
            }
            else if (coroutine != null)
            {
                coroutine.Stop();
                coroutine = null;
            }
        }

        private static IEnumerator AutoKickRoutine()
        {
            for (; ; )
            {
                popup.Press();

                bool state = qm.active;
                qm.SetActive(true);

                yield return null;

                here.Press();

                yield return null;

                int children = list.GetChildCount();

                counter++;
                if (counter >= children)
                    counter = 1;

                try
                {
                    if (children == 1)
                    {
                        qm.active = state;
                        break;
                    }

                    list.GetChild(counter)
                        ?.GetComponent<UnityEngine.UI.Button>()
                        ?.Press();
                }
                catch (Exception ex) { Plugin.log.LogError("Exception occurred in Auto Kick coroutine: " + ex); }

                yield return null;

                if (fren.active)
                {
                    qm.active = state;
                    yield return wait2;
                    continue;
                }

                yield return null;

                kick.Press();
                yeah.Press();

                qm.active = state;

                yield return wait;
            }
        }
    }
}
