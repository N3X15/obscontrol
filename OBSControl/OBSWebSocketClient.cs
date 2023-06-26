using System;
using ABI_RC.Core.IO;
using OBSWebsocketDotNet;
using OBSWebsocketDotNet.Communication;
using OBSWebsocketDotNet.Types;
using OBSWebsocketDotNet.Types.Events;

namespace OBSControl
{
    public class OBSWebSocketClient
    {
        private readonly OBSWebsocket cli;
        private readonly string addr;
        private readonly string passwd;
        private string _currentProfile;
        private string _currentProgramScene;
        private string _currentPreviewScene;
        private bool _isReplayBufferOn;

        public delegate void DisconnectedDelegate(ObsDisconnectionInfo e);
        public event DisconnectedDelegate Disconnected;

        public delegate void ConnectedDelegate();
        public event ConnectedDelegate Connected;

        public GetSceneListInfo AllScenes { get; protected set; }

        public bool IsReplayBufferOn
        {
            get => _isReplayBufferOn;
            set
            {
                if (value)
                    cli.StartReplayBuffer();
                else
                    cli.StopReplayBuffer();
            }
        }

        public RecordingStatus RecordStatus => cli?.GetRecordStatus();

        public OutputStatus StreamStatus => cli.GetStreamStatus();

        public string CurrentProfile
        {
            get => _currentProfile;
            set
            {
                cli.SetCurrentProfile(value);
            }
        }

        public string CurrentProgramScene
        {
            get => _currentProgramScene;
            set
            {
                cli.SetCurrentProgramScene(value);
            }
        }

        public string CurrentPreviewScene
        {
            get => _currentPreviewScene;
            set
            {
                cli.SetCurrentPreviewScene(value);
            }
        }

        public OBSWebSocketClient(string addr, string passwd)
        {
            this.addr = addr;
            this.passwd = passwd;
            cli = new OBSWebsocket();
            cli.Connected += OnConnected;
            cli.Disconnected += OnDisconnected;
            cli.CurrentProfileChanged += OnCurrentProfileChanged;
            cli.CurrentProgramSceneChanged += OnCurrentProgramSceneChanged;
            cli.CurrentPreviewSceneChanged += OnCurrentPreviewSceneChanged;
            Reconnect();
        }

        private void OnCurrentPreviewSceneChanged(object sender, CurrentPreviewSceneChangedEventArgs e)
        {
            this.CurrentPreviewScene = e.SceneName;
        }

        private void OnCurrentProgramSceneChanged(object sender, ProgramSceneChangedEventArgs e)
        {
            this._currentProgramScene = e.SceneName;
        }

        private void OnCurrentProfileChanged(object sender, OBSWebsocketDotNet.Types.Events.CurrentProfileChangedEventArgs e)
        {
            this._currentProfile = e.ProfileName;
            this.AllScenes = cli.GetSceneList();
        }

        private void OnDisconnected(object sender, OBSWebsocketDotNet.Communication.ObsDisconnectionInfo e)
        {
            Disconnected?.Invoke(e);
            SchedulerSystem.AddJob(Reconnect, 10f, 1f, 1);
        }

        private void Reconnect()
        {
            cli.ConnectAsync(addr, passwd);
        }

        private void OnConnected(object sender, EventArgs e)
        {
            this._currentProfile = "UNKNOWN"; //cli.getCurrentProfile(); Doesn't exist.
            this._currentPreviewScene = cli.GetCurrentPreviewScene();
            this._currentProgramScene = cli.GetCurrentProgramScene();
            this.AllScenes = cli.GetSceneList();
            this._isReplayBufferOn = cli.GetReplayBufferStatus();
            Connected?.Invoke();
        }

        public void StartRecording()
        {
            cli.StartRecord();
        }

        public void StopRecording()
        {
            cli.StopRecord();
        }

        public void StartStreaming()
        {
            cli.StartRecord();
        }

        public void StopStreaming()
        {
            cli.StopRecord();
        }
    }
}
