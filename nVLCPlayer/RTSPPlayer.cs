using System;
using System.ComponentModel;
using System.Windows.Forms;
using Declarations;
using Declarations.Events;
using Declarations.Media;
using Declarations.Players;
using Implementation;
using System.Security.Permissions;

namespace SDK.Player
{
    public partial class RTSPPlayer : UserControl, IDisposable
    {
        IMediaPlayerFactory m_factory;
        IDiskPlayer m_player;
        IMedia m_media;
        bool isInit = false;
        MediaState _Status = MediaState.NothingSpecial;
        string _version = "0.1";
        private bool _autoconnect = true;
        private string _rtsp_str;
        private System.Timers.Timer CheckConnectionTimer;

        public enum PlayerState  { PlayerPositionChanged , TimeChanged, MediaEnded , PlayerStopped };

        [Category("Custom")]
        public MediaState MediaStatus { get { return _Status; } }

        [Category("Custom")]
        public string Version { get { return _version; } }

        /// <summary> 
        /// auto reconnect if value is true
        /// </summary>
        [Category("Custom")]
        public bool Autoconnect { get { return _autoconnect; } set { _autoconnect = value; } }

        public delegate void Handler_rtspPlayer<T>(object sender, T Status);
        [Category("Custom")]
        public event Handler_rtspPlayer<PlayerState> OnPlayerEvent = null;
        [Category("Custom")]
        public event Handler_rtspPlayer<MediaState> OnMediaStatusEvent = null;

        [SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.ControlAppDomain)]
        public RTSPPlayer()
        {
            // Add the event handler for handling UI thread exceptions to the event.
            Application.ThreadException += new System.Threading.ThreadExceptionEventHandler(UIThreadException);
            // Set the unhandled exception mode to force all Windows Forms errors to go through our handler.
            //Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            // Add the event handler for handling non-UI thread exceptions to the event. 
            AppDomain.CurrentDomain.UnhandledException +=
                new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);

            InitializeComponent();

            CheckConnectionTimer = new System.Timers.Timer();
            CheckConnectionTimer.Interval = 100;
            CheckConnectionTimer.Elapsed += CheckConnectionTimer_Elapsed;

        }


        private void CheckConnectionTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (_autoconnect)
            {
                if (_Status == MediaState.Ended || _Status == MediaState.Error || _Status == MediaState.Stopped)
                {
                    CheckConnectionTimer.Stop();
                    this.Invoke(new Action(() => Play(_rtsp_str)));
                }
            }
        }

        public void ReleasenVLC()
        {
            CheckConnectionTimer.Stop();

            DisposeMedia();

            if( m_player!=null)
            {
                m_player.Events.PlayerPositionChanged -= Events_PlayerPositionChanged;
                m_player.Events.TimeChanged -= Events_TimeChanged;
                m_player.Events.MediaEnded -= Events_MediaEnded;
                m_player.Events.PlayerStopped -= Events_PlayerStopped;
                m_player.Dispose();
            }
            if (m_factory != null)
                m_factory.Dispose();

            //GC.SuppressFinalize(this);
        }


        public void Play(string RTSPstr)
        {
            try
            {
                _rtsp_str = RTSPstr;
                if (isInit == false)
                {
                    Init_nVLC();
                    isInit = true;
                }

                DisposeMedia();

                m_media = m_factory.CreateMedia<IMedia>(RTSPstr);
                m_media.Events.DurationChanged += new EventHandler<MediaDurationChange>(Events_DurationChanged);
                m_media.Events.StateChanged += new EventHandler<MediaStateChange>(Events_StateChanged);
                m_media.Events.ParsedChanged += new EventHandler<MediaParseChange>(Events_ParsedChanged);

                m_player.Open(m_media);
                m_media.Parse(true);

                m_player.Stop();
                m_player.Play();

                CheckConnectionTimer.Start();
            }
            catch(Exception Err)
            {
                throw new Exception("Play Faile, Error: "+ Err.StackTrace);
            }
        }

        public void Stop()
        {
            //var thread = new System.Threading.Thread(delegate () { m_player.Stop(); });
            //thread.Start();
            m_player.Stop();
            CheckConnectionTimer.Stop();
        }

        public void Pause()
        {
            m_player.Pause();
        }

        private void DisposeMedia()
        {
            if (m_media != null)
            {
                m_media.Events.DurationChanged -= Events_DurationChanged;
                m_media.Events.StateChanged -= Events_StateChanged;
                m_media.Events.ParsedChanged -= Events_ParsedChanged;
                m_media.Dispose();
            }
        }
        /// <summary> 
        /// Initialize nVLC Component
        /// </summary>
        private void Init_nVLC()
        {
            m_factory = new MediaPlayerFactory(true);
            m_player = m_factory.CreatePlayer<IDiskPlayer>();

            m_player.Events.PlayerPositionChanged += new EventHandler<MediaPlayerPositionChanged>(Events_PlayerPositionChanged);
            m_player.Events.TimeChanged += new EventHandler<MediaPlayerTimeChanged>(Events_TimeChanged);
            m_player.Events.MediaEnded += new EventHandler(Events_MediaEnded);
            m_player.Events.PlayerStopped += new EventHandler(Events_PlayerStopped);

            m_player.WindowHandle = panel1.Handle;
        }

        void Events_PlayerPositionChanged(object sender, MediaPlayerPositionChanged e)
        {
            if (OnPlayerEvent != null)
                OnPlayerEvent(this, PlayerState.PlayerPositionChanged);
        }

        void Events_TimeChanged(object sender, MediaPlayerTimeChanged e)
        {
            if (OnPlayerEvent != null)
                OnPlayerEvent(this, PlayerState.TimeChanged);
        }

        void Events_PlayerStopped(object sender, EventArgs e)
        {
            if (OnPlayerEvent != null)
                OnPlayerEvent(this, PlayerState.PlayerStopped);
        }

        void Events_MediaEnded(object sender, EventArgs e)
        {
            if (OnPlayerEvent != null)
                OnPlayerEvent(this, PlayerState.MediaEnded);
        }

        /// <summary> 
        /// Callback receives the new media state
        /// </summary>
        void Events_StateChanged(object sender, MediaStateChange e)
        {
            _Status = e.NewState;
            if (OnMediaStatusEvent != null)
                OnMediaStatusEvent(this, e.NewState);
        }

        /// <summary> 
        /// Callback receives the new duration
        /// </summary>
        void Events_DurationChanged(object sender, MediaDurationChange e)
        {
            //UISync.Execute(() => lblDuration.Text = TimeSpan.FromMilliseconds(e.NewDuration).ToString().Substring(0, 8));
        }

        /// <summary> 
        /// Ccallback receives the new parsed state
        /// </summary>
        void Events_ParsedChanged(object sender, MediaParseChange e)
        {
            Console.WriteLine(e.Parsed);
        }

        
    }
}
