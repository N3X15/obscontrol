using ABI_RC.Core.InteractionSystem;
using BTKUILib;
using BTKUILib.UIObjects;
using MelonLoader;
using System.Linq;

[assembly: MelonInfo(typeof(OBSControl.OBSControlMod), "OBSControl", "1.0.0", "N3X15 + Animal", "https://github.com/N3X15/OBSControl")]
[assembly: MelonGame("Alpha Blend Interactive", "ChilloutVR")]
[assembly: MelonColor(255, 95, 158, 160)] // Color.CadetBlue
[assembly: MelonOptionalDependencies("BTKUILib")] // not required, but qol to change settings

namespace OBSControl
{

    public class OBSControlMod : MelonMod
    {
        private static MelonPreferences_Category category;

        public override void OnInitializeMelon()
        {
            Logger.Logs = LoggerInstance;

            if (!RegisteredMelons.Any(x => x.Info.Name.Equals("BTKUILib") && x.Info.SemanticVersion != null/* && x.Info.SemanticVersion.CompareTo(new SemVersion(0, 3)) >= 0*/))
            {
                Logger.Error("BTKUILib was not detected or it is outdated! OBSControl cannot function without it!");
                Logger.Error("Please download an updated copy of BTKUILib!");
                return;
            }
            InitPrefs();
            //OBSMenu.LoadIcons();

            QuickMenuAPI.OnMenuRegenerate += LateStartup;

            client = new OBSWebSocketClient(HostIP.Value, HostPassword.Value);

        }

        public override void OnSceneWasInitialized(int buildIndex, string sceneName)
        {
            if (!SwitchWorldScene.Value) return;
            client.CurrentProgramScene = string.IsNullOrEmpty(WorldJoinScene.Value) ? oldScene : WorldJoinScene.Value;
        }

        public override void OnSceneWasUnloaded(int buildIndex, string sceneName)
        {
            if (!SwitchWorldScene.Value) return;
            oldScene = client.CurrentProgramScene;
            if (!string.IsNullOrEmpty(WorldLeaveScene.Value))
            {
                client.CurrentProgramScene = WorldLeaveScene.Value;
            }
        }

        private void LateStartup(CVR_MenuManager unused)
        {
            if (initialized) return;

            initialized = true;

            SetupUI();
        }

        private void SetupUI()
        {
            QuickMenuAPI.PrepareIcon("OBSControl", "OBS", global::System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("OBSControl.Images.obs.png"));
            QuickMenuAPI.PrepareIcon("OBSControl", "Download", global::System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("OBSControl.Images.download.png"));
            QuickMenuAPI.PrepareIcon("OBSControl", "Start", global::System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("OBSControl.Images.play.png"));
            QuickMenuAPI.PrepareIcon("OBSControl", "Stop", global::System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("OBSControl.Images.stop.png"));
            QuickMenuAPI.PrepareIcon("OBSControl", "Record", global::System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("OBSControl.Images.record.png"));


            var rootPage = new Page("OBSControl", "Main", true, "OBSIcon");
            rootPage.MenuTitle = "OBS Controls";
            rootPage.MenuSubtitle = "Perform OBS actions here.";

            /*
             * [>] Replay Buffer
             *  Start | Stop | Download
             * [>] Recording
             *  Start | Stop
             * [>] Streaming
             *  Start | Stop
             * [>] Misc
             *  Settings
             */
            var catReplay = rootPage.AddCategory("Replay Buffer");
            var btnReplayBufferStart = catReplay.AddButton("Start Replay Buffer", "Start", "Start the replay buffer.");
            btnReplayBufferStart.OnPress += () =>
            {
                if (client == null) return;
                client.IsReplayBufferOn = true;
            };
            var btnReplayBufferStop = catReplay.AddButton("Stop Replay Buffer", "Stop", "Stop the replay buffer.");
            btnReplayBufferStop.OnPress += () =>
            {
                if (client == null) return;
                client.IsReplayBufferOn = false;
            };
            var btnReplayBufferSave = catReplay.AddButton("Save Replay Buffer", "Download", "Save the replay buffer.");
            btnReplayBufferSave.OnPress += () =>
            {
                if (client == null) return;
                client.IsReplayBufferOn = false;
            };

            var catRecording = rootPage.AddCategory("Recording");
            var btnRecordingStart = catRecording.AddButton("Start Recording", "Start", "Start recording.");
            btnRecordingStart.OnPress += () =>
            {
                if (client == null) return;
                client.StartRecording();
                // TODO: Spawn recording stats tablet or something
            };
            var btnRecordingStop = catRecording.AddButton("Stop Recording", "Stop", "Stop recording.");
            btnRecordingStop.OnPress += () =>
            {
                if (client == null) return;
                client.StopRecording();
                // TODO: Despawn recording stats tablet or something
            };

            var catStream = rootPage.AddCategory("Stream");
            var btnStreamingStart = catStream.AddButton("Start Streaming", "Start", "Start streaming.");
            btnStreamingStart.OnPress += () =>
            {
                if (client == null) return;
                client.StartStreaming();
                // TODO: Spawn streaming stats tablet or something
            };
            var btnStreamingStop = catStream.AddButton("Stop Streaming", "Stop", "Stop streaming.");
            btnStreamingStop.OnPress += () =>
            {
                if (client == null) return;
                client.StopStreaming();
                // TODO: Despawn streaming stats tablet or something
            };
        }

        public static MelonPreferences_Entry<string> HostIP, HostPassword, WorldLeaveScene, WorldJoinScene;
        public static MelonPreferences_Entry<bool> AutoConnect, SwitchWorldScene;
        private bool initialized;
        private OBSWebSocketClient client;
        private string oldScene;

        private static void InitPrefs()
        {
            category = MelonPreferences.CreateCategory("OBSControl", "OBS Control");
            HostIP = category.CreateEntry("HostIP", "ws://localhost:4444", "Host Address");
            HostPassword = category.CreateEntry("HostPassword", "", "Host Password");
            AutoConnect = category.CreateEntry("AutoConnect", false, "Connect when VRChat starts");
            SwitchWorldScene = category.CreateEntry("SwitchWorldScene", false, "Switch scenes when switching worlds");
            WorldJoinScene = category.CreateEntry("WorldJoinScene", "", "Destination scene (leave blank for prior scene)");
            WorldLeaveScene =
                category.CreateEntry("WorldLeaveScene", "", "Switching worlds scene");
        }
    }
}