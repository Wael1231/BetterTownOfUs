using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Reactor.Extensions;
using BetterTownOfUs.Extensions;
using UnhollowerBaseLib;
using UnityEngine;

namespace BetterTownOfUs.CustomOption
{
    public static class Patches
    {
        public static Export ExportButton;
        public static Import ImportButton;
        private static float LobbyTextRowHeight { get; set; } = 0.081F;


        private static List<OptionBehaviour> CreateOptions(GameOptionsMenu __instance)
        {
            var options = new List<OptionBehaviour>();

            var togglePrefab = Object.FindObjectOfType<ToggleOption>();
            var numberPrefab = Object.FindObjectOfType<NumberOption>();
            var stringPrefab = Object.FindObjectOfType<StringOption>();


            if (ExportButton.Setting != null)
            {
                ExportButton.Setting.gameObject.SetActive(true);
                options.Add(ExportButton.Setting);
            }
            else
            {
                var toggle = Object.Instantiate(togglePrefab, togglePrefab.transform.parent);
                toggle.transform.GetChild(2).gameObject.SetActive(false);
                toggle.transform.GetChild(0).localPosition += new Vector3(1f, 0f, 0f);

                ExportButton.Setting = toggle;
                ExportButton.OptionCreated();
                options.Add(toggle);
            }

            if (ImportButton.Setting != null)
            {
                ImportButton.Setting.gameObject.SetActive(true);
                options.Add(ImportButton.Setting);
            }
            else
            {
                var toggle = Object.Instantiate(togglePrefab, togglePrefab.transform.parent);
                toggle.transform.GetChild(2).gameObject.SetActive(false);
                toggle.transform.GetChild(0).localPosition += new Vector3(1f, 0f, 0f);

                ImportButton.Setting = toggle;
                ImportButton.OptionCreated();
                options.Add(toggle);
            }

            // TODO: Is there a better place to do this?
            Object
                .FindObjectsOfType<NumberOption>().First(o => "CrewmateVision".Equals(o.name))
                .Increment = 0.125f;

            Object
                .FindObjectsOfType<NumberOption>().First(o => "ImpostorVision".Equals(o.name))
                .Increment = 0.125f;

            List<OptionBehaviour> defaultOptions = __instance.Children.ToList();
            options.AddRange(defaultOptions);

            foreach (var option in CustomOption.AllOptions)
            {
                if (option.Setting != null)
                {
                    option.Setting.gameObject.SetActive(true);
                    options.Add(option.Setting);
                    continue;
                }

                switch (option.Type)
                {
                    case CustomOptionType.Header:
                        var toggle = Object.Instantiate(togglePrefab, togglePrefab.transform.parent);
                        toggle.transform.GetChild(1).gameObject.SetActive(false);
                        toggle.transform.GetChild(2).gameObject.SetActive(false);
                        option.Setting = toggle;
                        options.Add(toggle);
                        break;
                    case CustomOptionType.Toggle:
                        var toggle2 = Object.Instantiate(togglePrefab, togglePrefab.transform.parent);
                        option.Setting = toggle2;
                        options.Add(toggle2);
                        break;
                    case CustomOptionType.Number:
                        var number = Object.Instantiate(numberPrefab, numberPrefab.transform.parent);
                        option.Setting = number;
                        options.Add(number);
                        break;
                    case CustomOptionType.String:
                        var str = Object.Instantiate(stringPrefab, stringPrefab.transform.parent);
                        if (option.Name == "Anon Imp")
                        {
                            var setTrans = str.transform;
                            var value = setTrans.GetChild(3);
                            var inc = setTrans.GetChild(0);
                            setTrans.GetChild(2).gameObject.SetActive(false);
                            inc.localPosition = new Vector3(inc.localPosition.x - 0.6f, 0, 0);
                            setTrans.GetChild(1).localPosition = new Vector3(setTrans.position.x - 0.25f, 0, 0);
                            value.localPosition = new Vector3(setTrans.localPosition.x / 2, 0, 0);
                            value.localScale = new Vector3(1, 1, 1);
                        }
                        option.Setting = str;
                        options.Add(str);
                        break;
                }

                option.OptionCreated();
            }

            return options;
        }


        private static bool OnEnable(OptionBehaviour opt)
        {
            if (opt == ExportButton.Setting)
            {
                ExportButton.OptionCreated();
                return false;
            }

            if (opt == ImportButton.Setting)
            {
                ImportButton.OptionCreated();
                return false;
            }


            var customOption =
                CustomOption.AllOptions.FirstOrDefault(option =>
                    option.Setting == opt); // Works but may need to change to gameObject.name check

            if (customOption == null)
            {
                customOption = ExportButton.SlotButtons.FirstOrDefault(option => option.Setting == opt);
                if (customOption == null)
                {
                    customOption = ImportButton.SlotButtons.FirstOrDefault(option => option.Setting == opt);
                    if (customOption == null) return true;
                }
            }

            customOption.OptionCreated();

            return false;
        }

        [HarmonyPatch(typeof(GameSettingMenu), nameof(GameSettingMenu.Start))]
        private class OptionsMenuBehaviour_Start
        {
            public static void Postfix(GameSettingMenu __instance)
            {
                var obj = __instance.RolesSettingsHightlight.gameObject.transform.parent.parent;
                var btouSettings = Object.Instantiate(__instance.RegularGameSettings, __instance.RegularGameSettings.transform.parent);
                btouSettings.SetActive(false);
                btouSettings.name = "BTOUSettings";

                var gameGroup = btouSettings.transform.FindChild("GameGroup");
                var title = gameGroup?.FindChild("Text");

                if (title != null)
                {
                    title.GetComponent<TextTranslatorTMP>().Destroy();
                    title.GetComponent<TMPro.TextMeshPro>().m_text = "Better Town Of Us Settings";
                }
                var sliderInner = gameGroup?.FindChild("SliderInner");
                if (sliderInner != null)
                    sliderInner.GetComponent<GameOptionsMenu>().name = "BtouGameOptionsMenu";

                var ourSettingsButton = Object.Instantiate(obj.gameObject, obj.transform.parent);
                ourSettingsButton.transform.localPosition = new Vector3(obj.localPosition.x + 0.906f, obj.localPosition.y, obj.localPosition.z);
                ourSettingsButton.name = "BTOUtab";
                var hatButton = ourSettingsButton.transform.GetChild(0); //TODO:  change to FindChild I guess to be sure
                var hatIcon = hatButton.GetChild(0);
                var tabBackground = hatButton.GetChild(1);

                var renderer = hatIcon.GetComponent<SpriteRenderer>();
                renderer.sprite = BetterTownOfUs.SettingsButtonSprite;
                var btouSettingsHighlight = tabBackground.GetComponent<SpriteRenderer>();
                PassiveButton passiveButton = __instance.GameSettingsHightlight.GetComponent<PassiveButton>();
                passiveButton.OnClick = new UnityEngine.UI.Button.ButtonClickedEvent();
                passiveButton.OnClick.AddListener(ToggleButton(__instance, btouSettings, btouSettingsHighlight, 0));
                passiveButton = __instance.RolesSettingsHightlight.GetComponent<PassiveButton>();
                passiveButton.OnClick = new UnityEngine.UI.Button.ButtonClickedEvent();
                passiveButton.OnClick.AddListener(ToggleButton(__instance, btouSettings, btouSettingsHighlight, 1));
                passiveButton = tabBackground.GetComponent<PassiveButton>();
                passiveButton.OnClick = new UnityEngine.UI.Button.ButtonClickedEvent();
                passiveButton.OnClick.AddListener(ToggleButton(__instance, btouSettings, btouSettingsHighlight, 2));

                //fix for scrollbar (bug in among us)
                btouSettings.GetComponentInChildren<Scrollbar>().parent = btouSettings.GetComponentInChildren<Scroller>();
                __instance.RegularGameSettings.GetComponentInChildren<Scrollbar>().parent = __instance.RegularGameSettings.GetComponentInChildren<Scroller>();
                __instance.RolesSettings.GetComponentInChildren<Scrollbar>().parent = __instance.RolesSettings.GetComponentInChildren<Scroller>();
            }
        }

        public static System.Action ToggleButton(GameSettingMenu settingMenu, GameObject BtouSettings, SpriteRenderer highlight, int id)
        {
            return new System.Action(() =>
            {
                settingMenu.RegularGameSettings.SetActive(id == 0);
                settingMenu.GameSettingsHightlight.enabled = id == 0;
                settingMenu.RolesSettings.gameObject.SetActive(id == 1);
                settingMenu.RolesSettingsHightlight.enabled = id == 1;
                highlight.enabled = id == 2;
                BtouSettings.SetActive(id == 2);
            });
        }

        [HarmonyPatch(typeof(GameOptionsMenu), nameof(GameOptionsMenu.Start))]
        private class GameOptionsMenu_Start
        {
            public static bool Postfix(GameOptionsMenu __instance)
            {
                if (__instance.name != "BtouGameOptionsMenu")
                    return true;
                __instance.Children = new Il2CppReferenceArray<OptionBehaviour>(new OptionBehaviour[0]);
                var childeren = new Transform[__instance.gameObject.transform.childCount];
                for (int k = 0; k < childeren.Length; k++)
                {
                    childeren[k] = __instance.gameObject.transform.GetChild(k); //TODO: Make a better fix for this for example caching the options or creating it ourself.
                }
                var startOption = __instance.gameObject.transform.GetChild(0);
                var customOptions = CreateOptions(__instance);
                var y = startOption.localPosition.y;
                var x = startOption.localPosition.x;
                var z = startOption.localPosition.z;
                for (int k = 0; k < childeren.Length; k++)
                {
                    childeren[k].gameObject.Destroy();
                }

                var i = 0;foreach (var option in customOptions)
                    option.transform.localPosition = new Vector3(x, y - i++ * 0.5f, z);

                __instance.Children = new Il2CppReferenceArray<OptionBehaviour>(customOptions.ToArray());
                return false;
            }
        }

        [HarmonyPatch(typeof(GameOptionsMenu), nameof(GameOptionsMenu.Update))]
        private class GameOptionsMenu_Update
        {
            public static void Postfix(GameOptionsMenu __instance)
            {
                if (__instance.Children == null || __instance.Children.Length == 0)
                    return;
                var y = __instance.GetComponentsInChildren<OptionBehaviour>()
                    .Max(option => option.transform.localPosition.y);
                float x, z;
                if (__instance.Children.Length == 1)
                {
                    x = __instance.Children[0].transform.localPosition.x;
                    z = __instance.Children[0].transform.localPosition.z;
                }
                else
                {
                    x = __instance.Children[1].transform.localPosition.x;
                    z = __instance.Children[1].transform.localPosition.z;
                }

                var i = 0;
                foreach (var option in __instance.Children)
                    option.transform.localPosition = new Vector3(x, y - i++ * 0.5f, z);
            }
        }

        [HarmonyPatch(typeof(ToggleOption), nameof(ToggleOption.OnEnable))]
        private static class ToggleOption_OnEnable
        {
            private static bool Prefix(ToggleOption __instance)
            {
                return OnEnable(__instance);
            }
        }

        [HarmonyPatch(typeof(NumberOption), nameof(NumberOption.OnEnable))]
        private static class NumberOption_OnEnable
        {
            private static bool Prefix(NumberOption __instance)
            {
                return OnEnable(__instance);
            }
        }

        [HarmonyPatch(typeof(StringOption), nameof(StringOption.OnEnable))]
        private static class StringOption_OnEnable
        {
            private static bool Prefix(StringOption __instance)
            {
                return OnEnable(__instance);
            }
        }


        [HarmonyPatch(typeof(ToggleOption), nameof(ToggleOption.Toggle))]
        private class ToggleButtonPatch
        {
            public static bool Prefix(ToggleOption __instance)
            {
                var option =
                    CustomOption.AllOptions.FirstOrDefault(option =>
                        option.Setting == __instance); // Works but may need to change to gameObject.name check
                if (option is CustomToggleOption toggle)
                {
                    toggle.Toggle();
                    return false;
                }

                if (__instance == ExportButton.Setting)
                {
                    if (!AmongUsClient.Instance.AmHost) return false;
                    ExportButton.Do();
                    return false;
                }

                if (__instance == ImportButton.Setting)
                {
                    if (!AmongUsClient.Instance.AmHost) return false;
                    ImportButton.Do();
                    return false;
                }


                if (option is CustomHeaderOption) return false;

                CustomOption option2 = ExportButton.SlotButtons.FirstOrDefault(option => option.Setting == __instance);
                if (option2 is CustomButtonOption button)
                {
                    if (!AmongUsClient.Instance.AmHost) return false;
                    button.Do();
                    return false;
                }

                CustomOption option3 = ImportButton.SlotButtons.FirstOrDefault(option => option.Setting == __instance);
                if (option3 is CustomButtonOption button2)
                {
                    if (!AmongUsClient.Instance.AmHost) return false;
                    button2.Do();
                    return false;
                }

                return true;
            }
        }

        [HarmonyPatch(typeof(NumberOption), nameof(NumberOption.Increase))]
        private class NumberOptionPatchIncrease
        {
            public static bool Prefix(NumberOption __instance)
            {
                var option =
                    CustomOption.AllOptions.FirstOrDefault(option =>
                        option.Setting == __instance); // Works but may need to change to gameObject.name check
                if (option is CustomNumberOption number)
                {
                    number.Increase();
                    return false;
                }

                return true;
            }
        }

        [HarmonyPatch(typeof(NumberOption), nameof(NumberOption.Decrease))]
        private class NumberOptionPatchDecrease
        {
            public static bool Prefix(NumberOption __instance)
            {
                var option =
                    CustomOption.AllOptions.FirstOrDefault(option =>
                        option.Setting == __instance); // Works but may need to change to gameObject.name check
                if (option is CustomNumberOption number)
                {
                    number.Decrease();
                    return false;
                }

                return true;
            }
        }

        [HarmonyPatch(typeof(StringOption), nameof(StringOption.Increase))]
        private class StringOptionPatchIncrease
        {
            public static bool Prefix(StringOption __instance)
            {
                var option =
                    CustomOption.AllOptions.FirstOrDefault(option =>
                        option.Setting == __instance); // Works but may need to change to gameObject.name check
                if (option is CustomStringOption str)
                {
                    str.Increase();
                    return false;
                }

                return true;
            }
        }

        [HarmonyPatch(typeof(StringOption), nameof(StringOption.Decrease))]
        private class StringOptionPatchDecrease
        {
            public static bool Prefix(StringOption __instance)
            {
                var option =
                    CustomOption.AllOptions.FirstOrDefault(option =>
                        option.Setting == __instance); // Works but may need to change to gameObject.name check
                if (option is CustomStringOption str)
                {
                    str.Decrease();
                    return false;
                }

                return true;
            }
        }

        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.RpcSyncSettings))]
        private class PlayerControlPatch
        {
            public static void Postfix()
            {
                if (PlayerControl.AllPlayerControls.Count < 2 || !AmongUsClient.Instance ||
                    !PlayerControl.LocalPlayer || !AmongUsClient.Instance.AmHost) return;

                Rpc.SendRpc();
            }
        }

        [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
        private class HudManagerUpdate
        {
            private const float
                MinX = -5.233334F /*-5.3F*/,
                OriginalY = 2.9F,
                MinY = 3F; // Differs to cause excess options to appear cut off to encourage scrolling

            private static Scroller Scroller;
            private static Vector3 LastPosition = new Vector3(MinX, MinY);

            public static void Prefix(HudManager __instance)
            {
                if (__instance.GameSettings?.transform == null) return;


                // Scroller disabled
                if (!CustomOption.LobbyTextScroller)
                {
                    // Remove scroller if disabled late
                    if (Scroller != null)
                    {
                        __instance.GameSettings.transform.SetParent(Scroller.transform.parent);
                        __instance.GameSettings.transform.localPosition = new Vector3(MinX, OriginalY);

                        Object.Destroy(Scroller);
                    }

                    return;
                }

                CreateScroller(__instance);

                Scroller.gameObject.SetActive(__instance.GameSettings.gameObject.activeSelf);

                if (!Scroller.gameObject.active) return;

                var rows = __instance.GameSettings.text.Count(c => c == '\n');
                var maxY = Mathf.Max(MinY, rows * LobbyTextRowHeight + (rows - 38) * LobbyTextRowHeight);

                Scroller.ContentYBounds = new FloatRange(MinY, maxY);

                // Prevent scrolling when the player is interacting with a menu
                if (PlayerControl.LocalPlayer?.CanMove != true)
                {
                    __instance.GameSettings.transform.localPosition = LastPosition;

                    return;
                }

                if (__instance.GameSettings.transform.localPosition.x != MinX ||
                    __instance.GameSettings.transform.localPosition.y < MinY) return;

                LastPosition = __instance.GameSettings.transform.localPosition;
            }

            private static void CreateScroller(HudManager __instance)
            {
                if (Scroller != null) return;

                Scroller = new GameObject("SettingsScroller").AddComponent<Scroller>();
                Scroller.transform.SetParent(__instance.GameSettings.transform.parent);
                Scroller.gameObject.layer = 5;

                Scroller.transform.localScale = Vector3.one;
                Scroller.allowX = false;
                Scroller.allowY = true;
                Scroller.active = true;
                Scroller.velocity = new Vector2(0, 0);
                Scroller.ScrollbarYBounds = new FloatRange(0, 0);
                Scroller.ContentXBounds = new FloatRange(MinX, MinX);
                Scroller.enabled = true;

                Scroller.Inner = __instance.GameSettings.transform;
                __instance.GameSettings.transform.SetParent(Scroller.transform);
            }
        }
    }
}