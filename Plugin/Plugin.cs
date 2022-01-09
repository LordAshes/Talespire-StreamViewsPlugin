using BepInEx;
using BepInEx.Configuration;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

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
        public const string Version = "1.0.0.0";

        // Configuration
        private static string data = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + @"/CustomData/";

        public static KeyboardShortcut streamViewMenuTrigger { get; set; }
        public static KeyboardShortcut streamViewStoreTrigger { get; set; }
        public static bool streamWithPostProcessing = false;
        public static int viewToSend = 0;

        public static bool streamMenuOpen = false;
        public static int streamSelected = 0;
        public static bool streamActive = false;
        public static bool nonStreamingPostProcessingSetting = false;

        public static string identity = "";

        public static Texture2D[] snapshots = new Texture2D[20];
        public static Texture2D border = FileAccessPlugin.Image.LoadTexture("Images/Border.png");
        public static Texture2D menubackground = FileAccessPlugin.Image.LoadTexture("Images/Menubackground.png");

        public static GUIStyle labelStyle = new GUIStyle() { fontSize = 20 };
        public static GUIStyle contentStyle = new GUIStyle() { fontSize = 16 };

        Dictionary<string, object> cameraInfo = new Dictionary<string, object>();

        void Awake()
        {
            UnityEngine.Debug.Log("Stream Views Plugin: Active.");

            ChatServicePlugin.handlers.Add("/View", ViewRequest);

            streamViewStoreTrigger = Config.Bind("Hotkeys", "Save Current View", new KeyboardShortcut(KeyCode.P, KeyCode.LeftControl)).Value;
            streamViewMenuTrigger = Config.Bind("Hotkeys", "Open Cut-Scene Menu", new KeyboardShortcut(KeyCode.P, KeyCode.LeftShift)).Value;
            streamWithPostProcessing = Config.Bind("Settings", "Use Post Processing In Cut-Scene", false).Value;

            for (int i=0; i<snapshots.Length; i++)
            {
                if (FileAccessPlugin.File.Exists(data + "/Snapshot." + (i + 1) + ".png"))
                {
                    Debug.Log("Stream Views Plugin: Loading Snapshot For View "+(i+1));
                    snapshots[i] = FileAccessPlugin.Image.LoadTexture(data + "/Snapshot." + (i + 1) + ".png");
                }
                else
                {
                    snapshots[i] = Texture2D.grayTexture;
                }
            }

            labelStyle.normal.textColor = Color.gray;
            contentStyle.normal.textColor = Color.white;

            Utility.PostOnMainPage(this.GetType());
        }

        void OnGUI()
        {
            if (streamMenuOpen)
            {
                GUI.DrawTexture(new Rect(4, 40, 1750, 950), menubackground, ScaleMode.StretchToFill);
                int snapshotCount = 0;
                for (int y = 0; y < 4; y++)
                {
                    for (int x = 0; x < 4; x++)
                    {
                        if (snapshotCount < snapshots.Length - 1)
                        {
                            if (snapshotCount == streamSelected)
                            {
                                GUI.DrawTexture(new Rect(10 + (x * 360), 50 + (y * 230), 350, 210), border, ScaleMode.StretchToFill);
                            }
                            if (GUI.Button(new Rect(25 + (x * 360), 65 + (y * 230), 320, 180), ""))
                            {
                                Debug.Log("Stream Views Plugin: Selecting Snapshots " + (snapshotCount + 1));
                                streamSelected = snapshotCount;
                            }
                            GUI.DrawTexture(new Rect(25 + (x * 360), 65 + (y * 230), 320, 180), snapshots[snapshotCount], ScaleMode.ScaleToFit);
                        }
                        else
                        {
                            Debug.Log("Stream Views Plugin: Snapshots Count Exceeded Trying To Access " + snapshotCount);
                        }
                        snapshotCount++;
                    }
                }
                int offset = 120;
                GUI.Label(new Rect(1500, 60, 330, 30), "Cut-Scene Options:", labelStyle); 
                foreach (KeyValuePair<PlayerGuid, PlayerInfo> player in CampaignSessionManager.PlayersInfo)
                {
                    if (GUI.Button(new Rect(1500, offset - 5, 30, 30), ""))
                    {
                        Debug.Log("Stream Views Plugin: Sent '/View " + player.Value.Name + "," + RecallCamera((streamSelected + 1)) + "'");
                        ChatManager.SendChatMessage("/View " + identity + "," + RecallCamera((streamSelected + 1)), LocalPlayer.Id.Value);
                    }
                    GUI.Label(new Rect(1540, offset, 330, 30), "Send To " + player.Value.Name, contentStyle);
                    offset = offset + 40;
                }
                offset = offset + 20;
                for (int c = 1; c <= 3; c++)
                {
                    if (GUI.Button(new Rect(1500, offset - 5, 30, 30), ""))
                    {
                        Debug.Log("Stream Views Plugin: Sent '/View Camera" + identity + "," + RecallCamera((streamSelected + 1)) + "'");
                        ChatManager.SendChatMessage("/View Camera" + c + "," + RecallCamera((streamSelected + 1)), LocalPlayer.Id.Value);
                    }
                    GUI.Label(new Rect(1540, offset, 330, 30), "Send To Camera Person " + c, contentStyle);
                    offset = offset + 40;
                }
                offset = offset + 20;
                if (GUI.Button(new Rect(1500, offset - 5, 30, 30), ""))
                {
                    System.IO.File.Delete(data + "/Values." + streamSelected + ".csv");
                    System.IO.File.Delete(data + "/Snapshot." + streamSelected + ".png");
                    snapshots[streamSelected] = Texture2D.grayTexture;
                }
                GUI.Label(new Rect(1540, offset, 330, 30), "Delete View", contentStyle);
                if (GUI.Button(new Rect(1750 - 25, 55, 30, 30), "X", labelStyle))
                {
                    streamMenuOpen = false;
                }
            }
        }

        /// <summary>
        /// Function for determining if view mode has been toggled and, if so, activating or deactivating Character View mode.
        /// This function is called periodically by TaleSpire.
        /// </summary>
        void Update()
        {
            if (Utility.isBoardLoaded())
            {
                if(identity=="")
                {
                    Debug.Log("PlayerGuid = "+LocalPlayer.Id.ToString());
                    foreach (KeyValuePair<PlayerGuid, PlayerInfo> players in CampaignSessionManager.PlayersInfo)
                    {
                        Debug.Log(players.Key.ToString() + " => " + players.Value.Name);
                    }
                    identity = CampaignSessionManager.PlayersInfo[LocalPlayer.Id].Name;
                    Debug.Log("Stream Views Plugin: Identity set to " + identity);
                }

                if (!streamActive)
                {
                    if (Utility.StrictKeyCheck(streamViewMenuTrigger))
                    {
                        // Open View Menu
                        streamMenuOpen = !streamMenuOpen;
                    }
                    else if (Utility.StrictKeyCheck(streamViewStoreTrigger))
                    {
                        // Store Current View
                        for (int i = 0; i < snapshots.Length; i++)
                        {
                            if (snapshots[i] == Texture2D.grayTexture)
                            {
                                SystemMessage.DisplayInfoText("Saving Current View As View " + (i + 1) + "...");
                                SaveCamera(Camera.main, (i + 1)); break;
                            }
                        }
                    }
                    else
                    {
                        for (int i = 1; i < 10; i++)
                        {
                            if (Utility.StrictKeyCheck(new KeyboardShortcut((KeyCode)(i + 48), KeyCode.RightControl)))
                            {
                                // Claim Identity
                                SystemMessage.DisplayInfoText("Setting Role To Camera Person " + i);
                                identity = "Camera" + i;
                                Debug.Log("Stream Views Plugin: Identity changed to " + identity);
                            }
                        }
                    }
                }
                else if(Input.anyKey)
                {
                    SystemMessage.DisplayInfoText("Ending Cut-Scene...");
                    string[] parts = RecallCamera(100).Split(',');
                    Camera.main.transform.position = new Vector3(float.Parse(parts[0]), float.Parse(parts[1]), float.Parse(parts[2]));
                    Camera.main.transform.eulerAngles = new Vector3(float.Parse(parts[3]), float.Parse(parts[4]), float.Parse(parts[5]));
                    SetPostProcessing(nonStreamingPostProcessingSetting);
                    streamActive = false;
                }
            }
        }

        private string ViewRequest(string message, string sender, ChatServicePlugin.ChatSource source)
        {
            Debug.Log("Stream Views Plugin: Message: " + message);
            if(message.StartsWith("/View ")) { message = message.Substring(5).Trim(); }
            string[] parts = message.Split(',');
            Debug.Log("Stream Views Plugin: Message for " + parts[0].Trim());
            if (identity == parts[0].Trim())
            {
                SystemMessage.DisplayInfoText("Showing Cut-Scene...");
                Debug.Log("Stream Views Plugin: Saving Pre-Cut-Scene View");
                SaveCamera(Camera.main, 100, false);
                Debug.Log("Stream Views Plugin: Applying Cut-Scene View");
                Vector3 pos = Vector3.zero;
                Vector3 rot = Vector3.zero;
                if (parts.Length > 1) { Debug.Log("Pos.X=" + parts[1]); pos.x = float.Parse(parts[1]); }
                if (parts.Length > 2) { Debug.Log("Pos.Y=" + parts[2]); pos.y = float.Parse(parts[2]); }
                if (parts.Length > 3) { Debug.Log("Pos.Z=" + parts[3]); pos.z = float.Parse(parts[3]); }
                if (parts.Length > 4) { Debug.Log("Rot.X=" + parts[4]); rot.x = float.Parse(parts[4]); }
                if (parts.Length > 5) { Debug.Log("Rot.Y=" + parts[5]); rot.y = float.Parse(parts[5]); }
                if (parts.Length > 6) { Debug.Log("Rot.Z=" + parts[6]); rot.z = float.Parse(parts[6]); }
                Camera.main.transform.position = pos;
                Camera.main.transform.eulerAngles = rot;
                Debug.Log("Stream Views Plugin: Cut-Scene Camera Adjusted");
                nonStreamingPostProcessingSetting = GetPostProcessing();
                if (!streamWithPostProcessing) { SetPostProcessing(false); }
                streamActive = true;
            }
            return null;
        }

        public void SaveCamera(Camera camera, int viewIndex, bool takeSnapshot = true)
        {
            if (takeSnapshot) { SystemMessage.DisplayInfoText("Storing Current View " + viewIndex + "..."); }
            Vector3 pos = Camera.main.transform.position;
            Vector3 rot = Camera.main.transform.eulerAngles;
            System.IO.File.WriteAllText(data+"/Values."+viewIndex+".csv",pos.x + "," + pos.y + "," + pos.z + "," + rot.x + "," + rot.y + "," + rot.z);

            if (takeSnapshot) { StartCoroutine("CamCapture", new object[] { viewIndex }); }
        }

        public string RecallCamera(int viewIndex)
        {
            SystemMessage.DisplayInfoText("Recalling View '" + viewIndex + "'...");
            try
            {
                string[] parts = System.IO.File.ReadAllText(data + "/Values." + viewIndex + ".csv").Split(',');
                return String.Join(",", parts);
            }
            catch(Exception)
            {
                SystemMessage.DisplayInfoText("View '" + name + " Not Defined");
                return null;
            }
        }

        public WaitForEndOfFrame frameEnd = new WaitForEndOfFrame();

        public IEnumerator CamCapture(object[] inputs)
        {
            int w = Camera.main.pixelWidth;
            int h = Camera.main.pixelHeight;
            int d = 24;
            
            RenderTexture oldTargetTexture = Camera.main.targetTexture;
            RenderTexture cameraRender = new RenderTexture(w,h,d);
            Camera.main.targetTexture = cameraRender;

            Camera.main.Render();

            yield return frameEnd;

            snapshots[(int)inputs[0]-1] = new Texture2D(w, h);
            snapshots[(int)inputs[0]-1].ReadPixels(new Rect(0, 0, w, h), 0, 0);
            snapshots[(int)inputs[0]-1].Apply();
            Camera.main.targetTexture = oldTargetTexture;

            var Bytes = snapshots[(int)inputs[0]-1].EncodeToPNG();
           
            System.IO.File.WriteAllBytes(data+"/Snapshot."+inputs[0].ToString()+".png", Bytes);
        }

        /// <summary>
        /// Function for getting the post processing enabled status
        /// </summary>
        /// <returns>Returns a boolean indicating if post processing is enabled</returns>
        private bool GetPostProcessing()
        {
            var postProcessLayer = Camera.main.GetComponent<PostProcessLayer>();
            return postProcessLayer.enabled;
        }

        /// <summary>
        /// Function for setting the post processing enabled setting
        /// </summary>
        /// <param name="setting">Boolean indicating if post processing is enabled or not</param>
        private void SetPostProcessing(bool setting)
        {
            var postProcessLayer = Camera.main.GetComponent<PostProcessLayer>();
            postProcessLayer.enabled = setting;
        }
    }
}