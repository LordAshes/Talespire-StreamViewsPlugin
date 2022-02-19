using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using Newtonsoft.Json;
using System;
using Unity.Mathematics;
using UnityEngine;
using static RootTargetCameraMode;

namespace LordAshes
{
    [BepInPlugin(Guid, Name, Version)]
    [BepInDependency(FileAccessPlugin.Guid)]
    [BepInDependency(ChatServicePlugin.Guid)]
    public partial class StreamViewsPlugin : BaseUnityPlugin
    {
        // Plugin info
        public const string Name = "Stream Views Plug-In";            
        public const string Guid = "org.lordashes.plugins.streamviews";
        public const string Version = "1.1.0.0";

        // Configuration
        public static KeyboardShortcut streamViewTrigger { get; set; }
        public static KeyboardShortcut streamViewClaimCamera0 { get; set; }
        public static KeyboardShortcut streamViewClaimCamera1 { get; set; }
        public static KeyboardShortcut streamViewClaimCamera2 { get; set; }

        public static string subscription = "";

        public static bool playerMenuOpen = false;
        public static bool cutsceneActive = false;
        public static bool singleSelection = false;

        public static Texture2D menuBackground = null;
        public static Texture2D menuButton = null;
        public static UnityEngine.Color menuFontColor = UnityEngine.Color.white;
        public static int menuFontSize = 12;
        public static float menuSpacing = 30f;
        public static GUIStyle menuTextStyle = default(GUIStyle);

        public static CutsceneData restorePoint = default(CutsceneData);

        public class JsonCustsceneData
        {
            public readonly bool IsValid;

            // Token: 0x04000002 RID: 2
            public string Guid;

            // Token: 0x04000003 RID: 3
            public string Position;

            // Token: 0x04000004 RID: 4
            public float TiltEuler;

            // Token: 0x04000005 RID: 5
            public float RotationEuler;

            // Token: 0x04000006 RID: 6
            public float Zoom;

            // Token: 0x04000007 RID: 7
            public float HidePlaneHeight;

            public JsonCustsceneData(CutsceneData data)
            {
                this.Guid = data.Guid.ToString();
                this.Position = data.Position.x + "," + data.Position.y + "," + data.Position.z;
                this.TiltEuler = data.TiltEuler;
                this.RotationEuler = data.RotationEuler;
                this.Zoom = data.Zoom;
                this.HidePlaneHeight = data.HidePlaneHeight;
            }

            public CutsceneData ToCutsceneData()
            {
                return new CutsceneData()
                {
                    Guid = new Bounce.Unmanaged.NGuid(Guid),
                    Position = new float3(float.Parse(Position.Split(',')[0]), float.Parse(Position.Split(',')[1]), float.Parse(Position.Split(',')[2])),
                    TiltEuler = TiltEuler,
                    RotationEuler = RotationEuler,
                    Zoom = Zoom,
                    HidePlaneHeight = HidePlaneHeight
                };
            }
        }

        void Awake()
        {
            UnityEngine.Debug.Log("Stream Views Plugin: Active.");
                
            streamViewTrigger = Config.Bind("Hotkeys", "Send Cutscene To Player", new KeyboardShortcut(KeyCode.S, KeyCode.RightControl)).Value;
            streamViewClaimCamera1 = Config.Bind("Hotkeys", "Change Identity To Camera 1", new KeyboardShortcut(KeyCode.Alpha1, KeyCode.RightControl)).Value;
            streamViewClaimCamera2 = Config.Bind("Hotkeys", "Change Identity To Camera 2", new KeyboardShortcut(KeyCode.Alpha2, KeyCode.RightControl)).Value;

            singleSelection = Config.Bind("Settings", "Close Player Menu After One Selection", true).Value;

            menuFontColor = Config.Bind("Menu", "Font Color", UnityEngine.Color.black).Value;
            menuFontSize = Config.Bind("Menu", "Font Size", 20).Value;
            menuSpacing = Config.Bind("Menu", "Item Spacing", 70).Value;

            menuTextStyle = new GUIStyle()
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = menuFontSize,
                normal = new GUIStyleState() { textColor = menuFontColor }
            };

            menuBackground = FileAccessPlugin.Image.LoadTexture("Images/" + StreamViewsPlugin.Guid + ".menu.png");
            menuButton = FileAccessPlugin.Image.LoadTexture("Images/" + StreamViewsPlugin.Guid + ".button.png");

            var harmony = new Harmony(Guid);
            harmony.PatchAll();

            Utility.PostOnMainPage(this.GetType());
        }

        /// <summary>
        /// Function for determining if view mode has been toggled and, if so, activating or deactivating Character View mode.
        /// This function is called periodically by TaleSpire.
        /// </summary>
        void Update()
        {
            if (Utility.isBoardLoaded())
            {
                if(Utility.StrictKeyCheck(streamViewTrigger))
                {
                    playerMenuOpen=!playerMenuOpen;
                }
                if (Utility.StrictKeyCheck(streamViewClaimCamera0) || subscription=="")
                {
                    ChatServicePlugin.ChatMessageService.RemoveHandler("/" + StreamViewsPlugin.Guid + "." + subscription);
                    subscription = CampaignSessionManager.GetPlayerName(LocalPlayer.Id);
                    ChatServicePlugin.ChatMessageService.AddHandler("/" + StreamViewsPlugin.Guid + "." + subscription, ViewRequest);
                }
                if (Utility.StrictKeyCheck(streamViewClaimCamera1))
                {
                    ChatServicePlugin.ChatMessageService.RemoveHandler("/" + StreamViewsPlugin.Guid + "." + subscription);
                    subscription = "Camera1";
                    ChatServicePlugin.ChatMessageService.AddHandler("/" + StreamViewsPlugin.Guid + "." + subscription, ViewRequest);
                }
                if (Utility.StrictKeyCheck(streamViewClaimCamera2))
                {
                    ChatServicePlugin.ChatMessageService.RemoveHandler("/" + StreamViewsPlugin.Guid + "." + subscription);
                    subscription = "Camera2";
                    ChatServicePlugin.ChatMessageService.AddHandler("/" + StreamViewsPlugin.Guid + "." + subscription, ViewRequest);
                }
            }
        }

        void OnGUI()
        {
            if (Utility.isBoardLoaded())
            {
                if(cutsceneActive && Input.anyKey)
                {
                    cutsceneActive = false;
                    LegacyCutsceneSetup lcs = CameraController.CutsceneSetup;
                    lcs.PreviewCutsceneState(restorePoint);
                    SystemMessage.DisplayInfoText(Config.Bind("Settings", "Cutscene End Text", "...And Back...").Value);
                }
                if (playerMenuOpen)
                {
                    float h = (CampaignSessionManager.PlayersInfo.Values.Count + 5) * menuSpacing;
                    float hOffset = (Screen.height - h) / 2;
                    float wOffset = (Screen.width - menuButton.width) / 2;
                    GUI.DrawTexture(new Rect(wOffset-30, hOffset-30, menuButton.width+60, h+60), menuBackground, ScaleMode.StretchToFill);
                    foreach (PlayerInfo player in CampaignSessionManager.PlayersInfo.Values)
                    {
                        if (GUI.Button(new Rect(wOffset, hOffset, menuButton.width, menuButton.height), menuButton, GUIStyle.none))
                        {
                            String json = JsonConvert.SerializeObject(new JsonCustsceneData(selectedCutSceneData));
                            Debug.Log("Stream Views Plugin: Sending View To " + player.Name);
                            ChatServicePlugin.ChatMessageService.SendMessage("/" + StreamViewsPlugin.Guid + "." + player.Name + " " + json, LocalClient.Id.Value);
                            if (singleSelection) { playerMenuOpen = false; }
                        }
                        GUI.Label(new Rect(wOffset, hOffset, menuButton.width, menuButton.height), player.Name, menuTextStyle);
                        hOffset = hOffset + menuSpacing;
                    }
                    hOffset = hOffset + menuSpacing;
                    for (int c = 1; c <= 2; c++)
                    {
                        if (GUI.Button(new Rect(wOffset, hOffset, menuButton.width, menuButton.height), menuButton, GUIStyle.none))
                        {
                            String json = JsonConvert.SerializeObject(new JsonCustsceneData(selectedCutSceneData));
                            Debug.Log("Stream Views Plugin: Sending View To Camera " + c);
                            ChatServicePlugin.ChatMessageService.SendMessage("/" + StreamViewsPlugin.Guid + "." + "Camera " + c + " " + json, LocalClient.Id.Value);
                            if (singleSelection) { playerMenuOpen = false; }
                        }
                        GUI.Label(new Rect(wOffset, hOffset, menuButton.width, menuButton.height), "Camera " + c, menuTextStyle);
                        hOffset = hOffset + menuSpacing;
                    }
                    hOffset = hOffset + menuSpacing;
                    if (GUI.Button(new Rect(wOffset, hOffset, menuButton.width, menuButton.height), menuButton, GUIStyle.none))
                    {
                        playerMenuOpen = false;
                    }
                    GUI.Label(new Rect(wOffset, hOffset, menuButton.width, menuButton.height), "Close Menu", menuTextStyle);
                }
            }
        }

        private string ViewRequest(string message, string sender, Talespire.SourceRole role)
        {
            Debug.Log("Stream Views Plugin: Message: " + message);
            string json = message.Substring(message.IndexOf(" ")+1).Trim();
            SystemMessage.DisplayInfoText(Config.Bind("Settings", "Cutscene Start Text", "...Meanwhile...").Value);
            restorePoint = CameraController.CutsceneSetup.GetCutsceneState();
            JsonCustsceneData data = JsonConvert.DeserializeObject<JsonCustsceneData>(json);
            LegacyCutsceneSetup lcs = CameraController.CutsceneSetup;
            lcs.PreviewCutsceneState(data.ToCutsceneData());
            cutsceneActive = true;
            return null;
        }
    }
}