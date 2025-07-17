using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityStandardAssets.Characters.FirstPerson;
using static AllahWare.AntiCheatKill; // Ensure this namespace is correctly defined elsewhere

namespace AllahWare
{
    public class GBCheatBehaviour : MonoBehaviourPunCallbacks
    {
        public GUISkin customSkin;
        public int badgeId = 12;
        private float badgeCycleTimer = 0f;
        private float badgeCycleInterval = 0.25f;
        private int badgeCycleIndex = 0;
        private bool shouldCycle = false;
        private bool isCycling = false;
        private readonly List<ValueTuple<Vector3, string, bool>> labelsToRender = new List<ValueTuple<Vector3, string, bool>>();
        private bool autoHost;
        private Player cachedTrollTarget = null;
        private bool drawFOVCircle = true;
        private bool espEnabled = true;
        public Camera gameCamera;
        public List<PlayerMaster> playerList = new List<PlayerMaster>();
        public GameObject localPlayer;
        public float fovRadius = 100f;

        private AimbotMode aimbotMode = AimbotMode.Normal;
        private bool aimbotEnabled = false;

        private BadgeSpoofMode bsm = BadgeSpoofMode.None;
        private BadgeSpoofMode activeBSM = BadgeSpoofMode.None;
        private readonly Regex playerNameRegex = new Regex("^Player\\d+$");
        private int minBadgeId = 12;
        private int maxBadgeId = 27;

        private string badgeInput = "";
        private string confirmedBadgeId = "";

        private string nameInput = "";
        private string confirmedName = "";

        private Rect windowRect = new Rect(10, 10, 350, 550); // Increased width and height for tabs
        private Vector2 dragOffset;
        private bool isDragging = false;
        private bool showWindow = true;

        private int selectedTab = 0; // 0: Server, 1: Player, 2: PlayFab, 3: Badges

        public enum AimbotMode { Silent, Normal }
        public enum BadgeSpoofMode { None, Client, Server, TROLL }

        void Start()
        {
            Bypass.randomID = Bypass.GenerateRandomId();
            ACKill.StartMonitoring(this);
            nameInput = PlayerPrefs.GetString("GBCHEATNAME");
            badgeInput = PlayerPrefs.GetString("GBCHEATCUSTOMID");
            Bypass.name = nameInput;
            Bypass.customID = badgeInput;

            // Initialize confirmed values from PlayerPrefs
            confirmedName = nameInput;
            confirmedBadgeId = badgeInput;
        }

        public override void OnDisconnected(DisconnectCause cause)
        {
            Debug.Log(cause.ToString());
        }

        private Vector2 scrollPos;

        void OnGUI()
        {
            if (!showWindow) return;

            ApplyGuiSkin();
            GUI.skin.label.richText = true; // Ensure rich text is enabled for labels

            // Make the entire windowRect draggable
            GUI.Box(windowRect, ""); // Draw an empty box for the background

            // Draggable title bar for the window
            Rect titleBarRect = new Rect(windowRect.x, windowRect.y, windowRect.width, 25);
            GUI.Label(titleBarRect, "<color=#00FFFF><b>[ ALLAHWARE.CC ] - [VERSION 0.6]</b></color>", GUI.skin.box); // Title with color

            HandleWindowDrag(titleBarRect);

            // Tab Buttons (placed above the content area)
            GUILayout.BeginArea(new Rect(windowRect.x + 5, windowRect.y + 30, windowRect.width - 10, 30));
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Server", GUILayout.Height(25))) selectedTab = 0;
            if (GUILayout.Button("Player", GUILayout.Height(25))) selectedTab = 1;
            if (GUILayout.Button("PlayFab", GUILayout.Height(25))) selectedTab = 2;
            if (GUILayout.Button("Badges", GUILayout.Height(25))) selectedTab = 3;
            GUILayout.EndHorizontal();
            GUILayout.EndArea();


            // Content area inside the window, below the tabs
            Rect contentRect = new Rect(windowRect.x + 10, windowRect.y + 65, windowRect.width - 20, windowRect.height - 75);

            // Begin scroll view for the content
            scrollPos = GUI.BeginScrollView(
                contentRect,
                scrollPos,
                new Rect(0, 0, contentRect.width - 20, GetContentHeightForSelectedTab()) // Dynamic content height
            );

            GUILayout.BeginArea(new Rect(0, 0, contentRect.width - 20, GetContentHeightForSelectedTab()));

            // Render content based on selected tab
            switch (selectedTab)
            {
                case 0: // Server Tab
                    DrawServerTab();
                    break;
                case 1: // Player Tab
                    DrawPlayerTab();
                    break;
                case 2: // PlayFab Tab
                    DrawPlayFabTab();
                    break;
                case 3: // Badges Tab
                    DrawBadgesTab();
                    break;
            }

            GUILayout.EndArea();
            GUI.EndScrollView();
            if (drawFOVCircle)
            {
                DrawFOVCircle(fovRadius, Color.green);
            }

            if (espEnabled)
            {
                foreach (var label in labelsToRender)
                {
                    GUI.color = label.Item3 ? Color.cyan : Color.gray;
                    GUI.Label(new Rect(label.Item1.x, label.Item1.y, 200f, 20f), label.Item2);
                }
            }
        }
        private void DrawFOVCircle(float radius, Color color)
        {
            Vector2 center = new Vector2(Screen.width / 2f, Screen.height / 2f);
            int segments = 100;
            float angle = 2 * Mathf.PI / segments;
            Vector2 lastPoint = Vector2.zero;

            for (int i = 0; i <= segments; i++)
            {
                float x = Mathf.Cos(angle * i) * radius;
                float y = Mathf.Sin(angle * i) * radius;
                Vector2 currentPoint = new Vector2(center.x + x, center.y + y);

                if (i > 0)
                {
                    DrawLine(lastPoint, currentPoint, color);
                }

                lastPoint = currentPoint;
            }
        }
        private Texture2D lineTex;
        private void DrawLine(Vector2 start, Vector2 end, Color color)
        {
            if (lineTex == null)
            {
                lineTex = new Texture2D(1, 1);
            }

            lineTex.SetPixel(0, 0, color);
            lineTex.Apply();

            float angle = Mathf.Atan2(end.y - start.y, end.x - start.x) * Mathf.Rad2Deg;
            float length = Vector2.Distance(start, end);

            GUIUtility.RotateAroundPivot(angle, start);
            GUI.DrawTexture(new Rect(start.x, start.y, length, 1), lineTex);
            GUIUtility.RotateAroundPivot(-angle, start);
        }

        private float GetContentHeightForSelectedTab()
        {
            // Adjust this based on the actual content of each tab
            switch (selectedTab)
            {
                case 0: return 200; // Server
                case 1: return 300; // Player
                case 2: return 250; // PlayFab
                case 3: return 700; // Badges (original height)
                default: return 500;
            }
        }

        private void ApplyGuiSkin()
        {
            if (customSkin != null)
            {
                GUI.skin = customSkin;
            }
            else
            {
                // Fallback to Unity's default skin with some basic styling
                GUIStyle boxStyle = new GUIStyle(GUI.skin.box);
                boxStyle.normal.background = MakeTex(2, 2, new Color(0.15f, 0.15f, 0.15f, 0.9f));
                boxStyle.normal.textColor = Color.white;
                boxStyle.fontStyle = FontStyle.Bold;
                boxStyle.alignment = TextAnchor.UpperCenter;
                GUI.skin.box = boxStyle;

                GUIStyle labelStyle = new GUIStyle(GUI.skin.label);
                labelStyle.normal.textColor = Color.white;
                labelStyle.richText = true;
                GUI.skin.label = labelStyle;

                GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
                buttonStyle.normal.background = MakeTex(2, 2, new Color(0.25f, 0.25f, 0.25f, 1f));
                buttonStyle.hover.background = MakeTex(2, 2, new Color(0.35f, 0.35f, 0.35f, 1f));
                buttonStyle.normal.textColor = Color.white;
                buttonStyle.fontStyle = FontStyle.Bold;
                GUI.skin.button = buttonStyle;

                GUIStyle toggleStyle = new GUIStyle(GUI.skin.toggle);
                toggleStyle.normal.textColor = Color.white;
                GUI.skin.toggle = toggleStyle;

                GUIStyle textFieldStyle = new GUIStyle(GUI.skin.textField);
                textFieldStyle.normal.background = MakeTex(2, 2, new Color(0.1f, 0.1f, 0.1f, 0.9f));
                textFieldStyle.normal.textColor = Color.white;
                GUI.skin.textField = textFieldStyle;
            }
        }

        private void DrawSectionHeader(string title)
        {
            GUILayout.Label("<color=#FFD700><b>" + title + "</b></color>", GetCustomHeaderStyle());
            GUILayout.Space(5);
        }

        private GUIStyle GetCustomHeaderStyle()
        {
            GUIStyle style = new GUIStyle(GUI.skin.label);
            style.alignment = TextAnchor.MiddleCenter;
            style.fontSize = 14;
            style.fontStyle = FontStyle.Bold;
            return style;
        }

        private Texture2D MakeTex(int width, int height, Color col)
        {
            Color[] pix = new Color[width * height];
            for (int i = 0; i < pix.Length; ++i)
            {
                pix[i] = col;
            }
            Texture2D result = new Texture2D(width, height);
            result.SetPixels(pix);
            result.Apply();
            return result;
        }

        private void HandleWindowDrag(Rect dragRect)
        {
            if (Event.current.type == EventType.MouseDown && dragRect.Contains(Event.current.mousePosition))
            {
                isDragging = true;
                dragOffset = Event.current.mousePosition - new Vector2(windowRect.x, windowRect.y);
                Event.current.Use(); // Consume the event
            }
            else if (Event.current.type == EventType.MouseUp)
            {
                isDragging = false;
            }
            else if (isDragging && Event.current.type == EventType.MouseDrag)
            {
                windowRect.x = Event.current.mousePosition.x - dragOffset.x;
                windowRect.y = Event.current.mousePosition.y - dragOffset.y;
                // Clamp window to screen bounds
                windowRect.x = Mathf.Clamp(windowRect.x, 0, Screen.width - windowRect.width);
                windowRect.y = Mathf.Clamp(windowRect.y, 0, Screen.height - windowRect.height);
                Event.current.Use(); // Consume the event
            }
        }

        // --- TAB CONTENT DRAWING METHODS ---
        private void DrawServerTab()
        {
            DrawSectionHeader("Server Tools");
            GUILayout.Space(10);

            if (PhotonNetwork.InRoom)
            {
                if (GUILayout.Button("<color=yellow>Steal Host (MasterClientBypass)</color>")) MasterClientBypass();
                if (GUILayout.Button("<color=red>Kick Everyone</color>")) KickEveryone();
                if (GUILayout.Button("Spoof Server Properties")) SpoofServer();
                GUILayout.Space(10);
                autoHost = GUILayout.Toggle(autoHost, "Auto Host (on room join)");
            }
            else
            {
                GUILayout.Label("<color=grey>Not in a room. Join one to use server tools.</color>");
            }
        }

        private void DrawPlayerTab()
        {
            DrawSectionHeader("Player Tools");
            GUILayout.Space(10);
            Bypass.antiKick = GUILayout.Toggle(Bypass.antiKick, "Bypass Disconnect");
            // Example Player Buttons
            if (localPlayer != null)
            {
                if (GUILayout.Button("Toggle Noclip (Example)"))
                {
                    // Add Noclip logic here
                    Debug.Log("Noclip Toggled!");
                }
                if (GUILayout.Button("Heal Player (Example)"))
                {
                    // Add Heal logic here
                    Debug.Log("Player Healed!");
                }
                // Add more player-related buttons as needed
            }
            else
            {
                GUILayout.Label("<color=grey>Local Player not found.</color>");
            }

            GUILayout.Space(10);
            aimbotEnabled = GUILayout.Toggle(aimbotEnabled, "Enable Aimbot");
            drawFOVCircle = GUILayout.Toggle(drawFOVCircle, "Draw Aimbot FOV Circle");
            espEnabled = GUILayout.Toggle(espEnabled, "Enable ESP");
            if (aimbotEnabled)
            {
                if (GUILayout.Button("Aimbot Mode: <color=yellow>" + aimbotMode.ToString() + "</color>"))
                {
                    aimbotMode = (AimbotMode)(((int)aimbotMode + 1) % Enum.GetNames(typeof(AimbotMode)).Length);
                }
            }

        }

        private void DrawPlayFabTab()
        {
            DrawSectionHeader("PlayFab Info & Customization");
            GUILayout.Space(10);

            if (PhotonNetwork.LocalPlayer != null)
            {
                GUILayout.Label("<b>Player ID:</b> <color=orange>" + Bypass.customID + "</color>");
                GUILayout.Label("<b>Player Name:</b> <color=orange>" + PhotonNetwork.LocalPlayer.NickName + "</color>");
            }
            else
            {
                GUILayout.Label("<color=grey>Not connected to Photon.</color>");
            }

            GUILayout.Space(10);
            if (!PhotonNetwork.InRoom) // Allow setting only when not in a room
            {
                GUILayout.Label("<b>Set Custom ID:</b>");
                badgeInput = GUILayout.TextField(badgeInput, GUILayout.MinWidth(100));
                if (GUILayout.Button("Confirm Custom ID"))
                {
                    confirmedBadgeId = badgeInput;
                    Debug.Log("Custom ID set to: " + confirmedBadgeId);
                    PlayerPrefs.SetString("GBCHEATCUSTOMID", badgeInput);
                    Bypass.customID = confirmedBadgeId;
                    // Note: For custom ID to apply in Photon, you usually set it on connection options or properties
                    // PhotonNetwork.NickName = confirmedName; // Example if NickName also handles ID logic
                }

                GUILayout.Space(5);
                GUILayout.Label("<b>Set Custom Name:</b>");
                nameInput = GUILayout.TextField(nameInput, GUILayout.MinWidth(100));
                if (GUILayout.Button("Confirm Custom Name"))
                {
                    confirmedName = nameInput;
                    Debug.Log("Name set to: " + nameInput);
                    PlayerPrefs.SetString("GBCHEATNAME", nameInput);
                    Bypass.name = confirmedName;
                    // Apply name change for Photon if connected
                    if (PhotonNetwork.IsConnected)
                    {
                        PhotonNetwork.NickName = confirmedName;
                    }
                }
            }
            else
            {
                GUILayout.Label("<color=grey>Custom ID/Name can only be set when not in a room.</color>");
            }
        }

        private void DrawBadgesTab()
        {
            DrawSectionHeader("Badge Spoofer");
            GUILayout.Space(5);

            GUILayout.BeginVertical("box");
            GUILayout.Label(
                "<b>12</b> - Tier 2\n" +
                "<b>13</b> - Tier 3\n" +
                "<b>14</b> - Tier 1\n" +
                "<b>15</b> - Moderator\n" +
                "<b>16</b> - Special\n" +
                "<b>17</b> - Youtuber\n" +
                "<b>18</b> - Blue Squid (?)\n" +
                "<b>19</b> - Pink Squid (?)\n" +
                "<b>20</b> - F2 Games\n" +
                "<b>21</b> - Artist\n" +
                "<b>22</b> - In Game Admin\n" +
                "<b>23</b> - Overseer\n" +
                "<b>24</b> - Helper\n" +
                "<b>25</b> - Master Client\n" +
                "<b>26</b> - Co Master Client\n" +
                "<b>27</b> - Speaking"
            );
            GUILayout.EndVertical();

            GUILayout.Space(10);
            if (GUILayout.Button("Mode: <color=yellow>" + GetBadgeModeText() + "</color>")) CycleBadgeMode();
            GUILayout.Label("Current Badge ID: <b>" + badgeId + "</b>");
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Badge--")) BadgeMinus();
            if (GUILayout.Button("Badge++")) BadgePlus();
            GUILayout.EndHorizontal();

            if (GUILayout.Button("<color=lime>Apply Badges</color>")) ApplyBadges();
            if (GUILayout.Button("<color=red>Stop Badges</color>")) StopBadges();

            shouldCycle = GUILayout.Toggle(shouldCycle, "Cycle Badges");
        }

        // --- EXISTING HELPER METHODS ---

        private string GetBadgeModeText() => bsm.ToString();
        private void CycleBadgeMode()
        {
            bsm = (BadgeSpoofMode)(((int)bsm + 1) % Enum.GetNames(typeof(BadgeSpoofMode)).Length);
        }
        public void BadgePlus()
        {
            badgeId++;
            if (badgeId > maxBadgeId) badgeId = minBadgeId; // Wrap around if exceeding max
        }
        public void BadgeMinus()
        {
            badgeId--;
            if (badgeId < minBadgeId) badgeId = maxBadgeId; // Wrap around if below min
        }
        void SpoofServer()
        {
            if (!PhotonNetwork.InRoom)
            {
                Debug.LogWarning("You must be the Master Client in a room to spoof server properties.");
                return;
            }

            Hashtable props = new Hashtable
            {
                ["mi"] = 64, // Max players example
                ["i"] = 1337, // Custom room ID example
                ["m"] = "ALLAHHHHHHHHHHHHHHHHHHHHHHHHHHH", // Custom message example
                ["di"] = "https://discord.gg/FYufJzh8Jn", // Discord invite example
                ["d"] = "<color=blue>RAPED BY BXDEV, FUCKED BY ALLAHWARE.CC -BXDEV", // Room description example
                ["g"] = "dsc.gg/FYufJzh8Jn" // Custom game property example
            };
            PhotonNetwork.CurrentRoom.SetCustomProperties(props);
            Debug.Log("Server properties spoofed!");
        }

        private string GetAimbotButtonText()
        {
            if (!aimbotEnabled) return "Aimbot: <color=red>OFF</color>";
            return aimbotMode == AimbotMode.Normal ? "Aimbot: <color=lime>Normal</color>" : "Aimbot: <color=orange>Silent</color>";
        }

        // --- Kick Everyone Function ---
        void KickEveryone()
        {
            if (!PhotonNetwork.InRoom)
            {
                Debug.LogWarning("You must be the Master Client in a room to kick everyone.");
                return;
            }

            // Iterate through all players in the room
            foreach (Player p in PhotonNetwork.PlayerList)
            {
                // Do not kick yourself
                if (p.IsLocal) continue;

                // Close connection to the player
                PhotonNetwork.CloseConnection(p);
                Debug.Log($"Kicked player: {p.NickName} ({p.ActorNumber})");
            }
            Debug.Log("Attempted to kick all other players.");
        }

        void Update()
        {
            bool flag = this.localPlayer == null || this.gameCamera == null;
            if (!flag)
            {
                this.localPlayer.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
                this.HandleAimbot();
                this.UpdateESPLabels();
            }
            if (PhotonNetwork.InRoom)
            {
                playerList = FindObjectsOfType<PlayerMaster>()
                    .Where(pm => pm != null && pm.gameObject != localPlayer)
                    .ToList();
                Debug.Log($"[Aimbot] Players found: {playerList.Count}");
            }
            if (Input.GetKeyDown(KeyCode.F9)) showWindow = !showWindow;
            if (Input.GetKeyDown(KeyCode.F1)) // Example for MasterClientBypass key
            {
                foreach (var comp in localPlayer.GetComponentsInChildren<Component>(true))
                {
                    Debug.Log($"[Aimbot] Found Component: {comp.GetType()} on {comp.gameObject.name}");
                }
            }
            if (isCycling && Time.time >= badgeCycleTimer)
            {
                badgeId = badgeCycleIndex;
                ModifyBadge();

                badgeCycleIndex++;
                if (badgeCycleIndex > maxBadgeId) badgeCycleIndex = minBadgeId;

                badgeCycleTimer = Time.time + badgeCycleInterval;
            }
            if (PhotonNetwork.InRoom)
            {
                if (autoHost)
                {
                    MasterClientBypass();
                }
            }
            IdentifyLocalPlayer(); // Call this in Update to keep localPlayer reference fresh
        }
        private void HandleAimbot()
        {
            bool flag = aimbotEnabled;
            if (flag)
            {
                bool flag2 = !Input.GetMouseButton(1);
                if (!flag2)
                {
                    GameObject gameObject = this.FindTargetClosestToScreenCenter();
                    bool flag3 = gameObject != null;
                    if (flag3)
                    {
                        this.AimAtTarget(gameObject);
                    }
                }
            }
        }

        private GameObject FindTargetClosestToScreenCenter()
        {
            if (gameCamera == null) return null;

            float closestDistance = fovRadius;
            GameObject closestTarget = null;

            Vector2 screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);

            foreach (PlayerMaster player in playerList)
            {
                if (player == null || player.gameObject == localPlayer) continue;
                if (!playerNameRegex.IsMatch(player.name)) continue;

                // Try to find the head transform
                Transform head = player.transform.Find("player/BaseAI/Armature/Model/Motion/B_Pelvis/B_Spine/B_Spine1/B_Spine2/B_Neck/B_Head");
                if (head == null) continue;

                // Check if it's visible
                if (!CheckVisibility(player.gameObject)) continue;

                // Convert to screen space
                Vector3 screenPos = gameCamera.WorldToScreenPoint(head.position);
                Vector2 screenPoint = new Vector2(screenPos.x, Screen.height - screenPos.y);

                // Check distance to center
                float dist = Vector2.Distance(screenCenter, screenPoint);
                if (dist > fovRadius) continue;

                if (dist < closestDistance)
                {
                    closestDistance = dist;
                    closestTarget = head.gameObject;
                }
            }
            Debug.Log(closestTarget.name);
            return closestTarget;
        }

        private void AimAtTarget(GameObject target)
        {
            bool flag = target == null || this.gameCamera == null || this.localPlayer == null;
            if (!flag)
            {
                Vector3 position = target.transform.position;
                Vector3 normalized = (position - this.gameCamera.transform.position).normalized;
                Quaternion rotation = Quaternion.LookRotation(normalized);
                bool flag2 = this.aimbotMode == AimbotMode.Silent;
                if (flag2)
                {
                    this.gameCamera.transform.rotation = rotation;
                }
                else
                {
                    bool flag3 = this.aimbotMode == AimbotMode.Normal;
                    if (flag3)
                    {
                        float num = Mathf.Atan2(normalized.x, normalized.z) * 57.29578f;
                        float num2 = Mathf.Asin(normalized.y) * 57.29578f;
                        num2 = Mathf.Clamp(num2, -75f, 65f);
                        Transform transform = this.localPlayer.transform.Find("player");
                        bool flag4 = transform != null;
                        if (flag4)
                        {
                            Debug.Log("transform isnt null");
                            RigidbodyFirstPersonController component = transform.GetComponent<RigidbodyFirstPersonController>();
                            bool flag5 = component != null;
                            if (flag5)
                            {
                                Debug.Log("component isnt null");
                                MouseLook mouseLook = ReflectionHelper.FindVariableByType<MouseLook>(component);
                                bool flag6 = mouseLook != null;
                                if (flag6)
                                {
                                    Debug.Log("mouse look isnt null");
                                    Quaternion characterTargetRot = Quaternion.Euler(0f, num, 0f);
                                    Quaternion cameraTargetRot = Quaternion.Euler(-num2, 0f, 0f);
                                    mouseLook.m_CharacterTargetRot = characterTargetRot;
                                    mouseLook.m_CameraTargetRot = cameraTargetRot;
                                    transform.localRotation = mouseLook.m_CharacterTargetRot;
                                    this.gameCamera.transform.localRotation = mouseLook.m_CameraTargetRot;
                                    this.gameCamera.transform.rotation = rotation;
                                }
                            }
                        }
                    }
                }
            }
        }

        public void MasterClientBypass()
        {
            if (PhotonNetwork.LocalPlayer == null || !PhotonNetwork.InRoom) return;

            

            // The original logic for setting room properties (may still be useful for persistence)
            int actorNum = PhotonNetwork.LocalPlayer.ActorNumber;
            Hashtable props = new Hashtable
            {
                [248] = actorNum,
                ["h"] = Bypass.realID // Assuming Bypass.realID is a valid property
            };

            if (PhotonNetwork.CurrentRoom != null)
            {
                PhotonNetwork.CurrentRoom.LoadBalancingClient.OpSetPropertiesOfRoom(props, null, null);
                PhotonNetwork.CurrentRoom.SetCustomProperties(props, null, null);
            }
        }
        void ApplyBadges()
        {
            activeBSM = bsm;
            isCycling = shouldCycle;

            if (activeBSM == BadgeSpoofMode.TROLL)
            {
                List<Player> validTargets = new List<Player>();
                foreach (var p in PhotonNetwork.PlayerList) if (!p.IsLocal) validTargets.Add(p);
                if (validTargets.Count > 0)
                    cachedTrollTarget = validTargets[UnityEngine.Random.Range(0, validTargets.Count)];
                else
                    cachedTrollTarget = null; // No valid targets to troll
            }

            ModifyBadge();
        }
        private void UpdateESPLabels()
        {
            this.labelsToRender.Clear();
            bool flag = this.gameCamera == null || this.localPlayer == null;
            if (!flag)
            {
                foreach (PlayerMaster playerMaster in this.playerList)
                {
                    bool flag2 = playerMaster == null || playerMaster.gameObject == this.localPlayer;
                    if (!flag2)
                    {
                        bool flag3 = !this.playerNameRegex.IsMatch(playerMaster.name);
                        if (!flag3)
                        {
                            Transform transform = playerMaster.gameObject.transform.Find("player/BaseAI/Armature/Model/Motion/B_Pelvis/B_Spine/B_Spine1/B_Spine2/B_Neck/B_Head");
                            bool flag4 = transform != null;
                            if (flag4)
                            {
                                Vector3 vector = this.gameCamera.WorldToScreenPoint(transform.position);
                                bool item = this.CheckVisibility(playerMaster.gameObject);
                                bool flag5 = vector.z > 0f && vector.x >= 0f && vector.x <= (float)Screen.width && vector.y >= 0f && vector.y <= (float)Screen.height;
                                if (flag5)
                                {
                                    string playerNickname = this.GetPlayerNickname(playerMaster.gameObject);
                                    this.labelsToRender.Add(new ValueTuple<Vector3, string, bool>(new Vector3(vector.x, (float)Screen.height - vector.y), playerNickname, item));
                                }
                            }
                        }
                    }
                }
            }
        }
        // GoreBoxCheat.GoreBoxCheatBehaviour
        // Token: 0x06000147 RID: 327 RVA: 0x00006834 File Offset: 0x00004A34
        private bool CheckVisibility(GameObject player)
        {
            // Check for null references upfront to avoid NullReferenceExceptions
            if (gameCamera == null || localPlayer == null || player == null)
            {
                return false;
            }

            // Get all colliders on the player and its children
            Collider[] array = player.GetComponentsInChildren<Collider>();
            if (array == null || array.Length == 0)
            {
                return false;
            }

            // Define the ray origin slightly forward from the camera to avoid self-intersections
            Vector3 vector = gameCamera.transform.position + gameCamera.transform.forward * 0.5f;

            // Iterate through each collider of the target player
            foreach (Collider collider in array)
            {
                if (collider == null)
                {
                    continue; // Skip null colliders
                }

                // Calculate direction and distance to the collider's center
                Vector3 vector2 = collider.bounds.center - vector;
                float magnitude = vector2.magnitude;

                // Perform the raycast
                RaycastHit raycastHit; // Declared here
                if (Physics.Raycast(vector, vector2.normalized, out raycastHit, magnitude)) // Corrected: use 'out' and normalized direction
                {
                    // If the ray hits something and that something is the current collider, it's visible
                    if (raycastHit.collider == collider)
                    {
                        return true; // Found a visible part of the player
                    }
                }
            }
            // If no part of the player was hit by a ray from the camera, it's not considered visible
            return false;
        }

        private string GetPlayerNickname(GameObject player)
        {
            bool flag = player == null;
            string result;
            if (flag)
            {
                result = "Unknown";
            }
            else
            {
                PhotonView component = player.GetComponent<PhotonView>();
                result = ((component != null) ? component.Owner.NickName : "Unknown");
            }
            return result;
        }
        private void IdentifyLocalPlayer()
        {
            bool flag = this.localPlayer != null && this.localPlayer.activeSelf && this.localPlayer.GetComponent<PhotonView>().Owner != null;
            if (!flag)
            {
                foreach (PlayerMaster playerMaster in SceneManager.GetActiveScene().GetRootGameObjects().SelectMany((GameObject root) => root.GetComponentsInChildren<PlayerMaster>(true)))
                {
                    bool flag2 = playerMaster != null && playerMaster.gameObject.activeSelf && !this.playerNameRegex.IsMatch(playerMaster.name) && playerMaster.GetComponent<PhotonView>().Owner == PhotonNetwork.LocalPlayer;
                    if (flag2)
                    {
                        this.localPlayer = playerMaster.gameObject;
                        gameCamera = localPlayer.GetComponentInChildren<Camera>();
                        return;
                    }
                }
                this.localPlayer = null;
            }
        }
        void StopBadges()
        {
            isCycling = false;
            activeBSM = BadgeSpoofMode.None;
            cachedTrollTarget = null;

            Hashtable props = new Hashtable { ["I"] = 5 }; // Reset to default badge ID (assuming 5 is default)
            foreach (var p in PhotonNetwork.PlayerList) p.SetCustomProperties(props);
        }

        void ModifyBadge()
        {
            Hashtable props = new Hashtable { ["I"] = badgeId };

            switch (activeBSM)
            {
                case BadgeSpoofMode.None:
                    props["I"] = 5; // Assuming 5 is the default/none badge
                    foreach (var p in PhotonNetwork.PlayerList)
                        p.SetCustomProperties(props);
                    break;

                case BadgeSpoofMode.Client:
                    props["I"] = badgeId;
                    PhotonNetwork.LocalPlayer.SetCustomProperties(props);
                    break;

                case BadgeSpoofMode.Server:
                      foreach (var p in PhotonNetwork.PlayerList)
                      p.SetCustomProperties(props);
                    break;

                case BadgeSpoofMode.TROLL:
                    if (cachedTrollTarget != null && PhotonNetwork.PlayerList.Contains(cachedTrollTarget))
                        cachedTrollTarget.SetCustomProperties(props);
                    else if (PhotonNetwork.LocalPlayer != null) // Fallback to local if troll target is gone or not found
                        PhotonNetwork.LocalPlayer.SetCustomProperties(props);
                    break;
            }
        }
    }
}