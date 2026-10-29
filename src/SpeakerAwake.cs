// SpeakerAwake - keeps external speakers out of auto-standby.
// Copyright (c) 2026 mdsajedul. Licensed under the MIT License (see LICENSE).
//
// Plays an inaudible signal on the default multimedia output device (WASAPI shared mode)
// so external speakers do not drop into hardware auto-standby.
//
// Written for the C# 5 compiler that ships with Windows (.NET Framework 4.x); see build.cmd.

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using Microsoft.Win32;

[assembly: System.Reflection.AssemblyTitle("SpeakerAwake")]
[assembly: System.Reflection.AssemblyProduct("SpeakerAwake")]
[assembly: System.Reflection.AssemblyDescription("Keeps external speakers out of auto-standby")]
[assembly: System.Reflection.AssemblyCopyright("Copyright (c) 2026 mdsajedul, MIT License")]
[assembly: System.Reflection.AssemblyVersion("1.0.1.0")]
[assembly: System.Reflection.AssemblyFileVersion("1.0.1.0")]

namespace SpeakerAwake
{
    // ------------------------------------------------------------------
    // Core Audio (MMDevice / WASAPI) COM interop
    // ------------------------------------------------------------------

    enum EDataFlow { Render = 0, Capture = 1, All = 2 }
    enum ERole { Console = 0, Multimedia = 1, Communications = 2 }

    [StructLayout(LayoutKind.Sequential)]
    struct PropertyKey
    {
        public Guid FormatId;
        public int PropertyId;
        public PropertyKey(Guid formatId, int propertyId) { FormatId = formatId; PropertyId = propertyId; }
    }

    [StructLayout(LayoutKind.Sequential)]
    struct PropVariant
    {
        public ushort vt;
        public ushort reserved1, reserved2, reserved3;
        public IntPtr p;
        public IntPtr p2;
    }

    [ComImport, Guid("A95664D2-9614-4F35-A746-DE8DB63617E6"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    interface IMMDeviceEnumerator
    {
        void EnumAudioEndpoints(EDataFlow dataFlow, int stateMask, out IntPtr devices);
        void GetDefaultAudioEndpoint(EDataFlow dataFlow, ERole role, out IMMDevice device);
        void GetDevice([MarshalAs(UnmanagedType.LPWStr)] string id, out IMMDevice device);
        void RegisterEndpointNotificationCallback(IMMNotificationClient client);
        void UnregisterEndpointNotificationCallback(IMMNotificationClient client);
    }

    [ComImport, Guid("D666063F-1587-4E43-81F1-B948E807363F"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    interface IMMDevice
    {
        void Activate(ref Guid iid, int clsCtx, IntPtr activationParams, [MarshalAs(UnmanagedType.IUnknown)] out object iface);
        void OpenPropertyStore(int access, out IPropertyStore store);
        void GetId([MarshalAs(UnmanagedType.LPWStr)] out string id);
        void GetState(out int state);
    }

    [ComImport, Guid("886D8EEB-8CF2-4446-8D02-CDBA1DBDCF99"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    interface IPropertyStore
    {
        void GetCount(out int count);
        void GetAt(int index, out PropertyKey key);
        void GetValue(ref PropertyKey key, out PropVariant value);
        void SetValue(ref PropertyKey key, ref PropVariant value);
        void Commit();
    }

    [ComImport, Guid("7991EEC9-7E89-4D85-8390-6C703CEC60C0"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    interface IMMNotificationClient
    {
        void OnDeviceStateChanged([MarshalAs(UnmanagedType.LPWStr)] string deviceId, int newState);
        void OnDeviceAdded([MarshalAs(UnmanagedType.LPWStr)] string deviceId);
        void OnDeviceRemoved([MarshalAs(UnmanagedType.LPWStr)] string deviceId);
        void OnDefaultDeviceChanged(EDataFlow flow, ERole role, [MarshalAs(UnmanagedType.LPWStr)] string defaultDeviceId);
        void OnPropertyValueChanged([MarshalAs(UnmanagedType.LPWStr)] string deviceId, PropertyKey key);
    }

    [ComImport, Guid("1CB9AD4C-DBFA-4C32-B178-C2F568A703B2"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    interface IAudioClient
    {
        void Initialize(int shareMode, int streamFlags, long bufferDuration, long periodicity, IntPtr format, IntPtr audioSessionGuid);
        void GetBufferSize(out uint bufferFrames);
        void GetStreamLatency(out long latency);
        void GetCurrentPadding(out uint paddingFrames);
        void IsFormatSupported(int shareMode, IntPtr format, out IntPtr closestMatch);
        void GetMixFormat(out IntPtr format);
        void GetDevicePeriod(out long defaultPeriod, out long minimumPeriod);
        void Start();
        void Stop();
        void Reset();
        void SetEventHandle(IntPtr eventHandle);
        void GetService(ref Guid iid, [MarshalAs(UnmanagedType.IUnknown)] out object service);
    }

    [ComImport, Guid("F294ACFC-3146-4483-A7BF-ADDCA7C260E2"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    interface IAudioRenderClient
    {
        void GetBuffer(uint framesRequested, out IntPtr data);
        void ReleaseBuffer(uint framesWritten, int flags);
    }

    static class CoreAudio
    {
        public static readonly Guid CLSID_MMDeviceEnumerator = new Guid("BCDE0395-E52F-467C-8E3D-C4579291692E");
        public static readonly Guid IID_IAudioClient = new Guid("1CB9AD4C-DBFA-4C32-B178-C2F568A703B2");
        public static readonly Guid IID_IAudioRenderClient = new Guid("F294ACFC-3146-4483-A7BF-ADDCA7C260E2");
        public static readonly Guid KSDATAFORMAT_SUBTYPE_IEEE_FLOAT = new Guid("00000003-0000-0010-8000-00AA00389B71");
        public static readonly PropertyKey PKEY_Device_FriendlyName =
            new PropertyKey(new Guid("A45C254E-DF1C-4EFD-8020-67D146A850E0"), 14);

        public const int CLSCTX_ALL = 23;
        public const int AUDCLNT_SHAREMODE_SHARED = 0;
        public const int AUDCLNT_STREAMFLAGS_AUTOCONVERTPCM = unchecked((int)0x80000000);
        public const int AUDCLNT_STREAMFLAGS_SRC_DEFAULT_QUALITY = 0x08000000;
        public const ushort WAVE_FORMAT_EXTENSIBLE = 0xFFFE;
        public const ushort VT_LPWSTR = 31;

        public const int E_NOTFOUND = unchecked((int)0x80070490);
        public const int AUDCLNT_E_DEVICE_INVALIDATED = unchecked((int)0x88890004);
        public const int AUDCLNT_E_DEVICE_IN_USE = unchecked((int)0x8889000A);
        public const int AUDCLNT_E_SERVICE_NOT_RUNNING = unchecked((int)0x88890010);

        [DllImport("ole32.dll")]
        public static extern int PropVariantClear(ref PropVariant pvar);

        public static string GetFriendlyName(IMMDevice device)
        {
            IPropertyStore store = null;
            try
            {
                device.OpenPropertyStore(0 /* STGM_READ */, out store);
                PropertyKey key = PKEY_Device_FriendlyName;
                PropVariant value;
                store.GetValue(ref key, out value);
                try
                {
                    return value.vt == VT_LPWSTR ? Marshal.PtrToStringUni(value.p) : "Default output device";
                }
                finally { PropVariantClear(ref value); }
            }
            catch (Exception) { return "Default output device"; }
            finally { if (store != null) Marshal.ReleaseComObject(store); }
        }

        public static string Describe(Exception ex)
        {
            COMException com = ex as COMException;
            if (com == null) return ex.Message;
            switch (com.ErrorCode)
            {
                case E_NOTFOUND: return "no output device";
                case AUDCLNT_E_DEVICE_INVALIDATED: return "device disconnected";
                case AUDCLNT_E_DEVICE_IN_USE: return "device in exclusive use";
                case AUDCLNT_E_SERVICE_NOT_RUNNING: return "audio service not running";
                default: return "error 0x" + com.ErrorCode.ToString("X8");
            }
        }
    }

    // ------------------------------------------------------------------
    // Signal presets and the pre-rendered 1-second loop
    // ------------------------------------------------------------------

    sealed class SignalPreset
    {
        public readonly string Key;
        public readonly string Label;
        public readonly bool IsSine;
        public readonly double LevelDb;   // peak level in dBFS
        public readonly int Hz;           // sine only; integer so a 1 s loop is seamless

        SignalPreset(string key, string label, bool isSine, double levelDb, int hz)
        {
            Key = key; Label = label; IsSine = isSine; LevelDb = levelDb; Hz = hz;
        }

        public static readonly SignalPreset[] All =
        {
            new SignalPreset("noise-60", "White noise, -60 dB (strongest)", false, -60, 0),
            new SignalPreset("noise-70", "White noise, -70 dB (default)", false, -70, 0),
            new SignalPreset("noise-80", "White noise, -80 dB", false, -80, 0),
            new SignalPreset("noise-90", "White noise, -90 dB (weakest)", false, -90, 0),
            new SignalPreset("sine-10", "Sub-audible sine, 10 Hz", true, -30, 10),
            new SignalPreset("sine-15", "Sub-audible sine, 15 Hz", true, -30, 15),
        };

        public static SignalPreset Default { get { return All[1]; } }

        public static SignalPreset Find(string key)
        {
            foreach (SignalPreset p in All)
                if (p.Key == key) return p;
            return Default;
        }

        // Interleaved float32 samples for exactly one second at the given format.
        public float[] Render(int sampleRate, int channels)
        {
            float[] loop = new float[sampleRate * channels];
            double amplitude = Math.Pow(10.0, LevelDb / 20.0);
            Random rnd = new Random(0x5EED);
            for (int frame = 0; frame < sampleRate; frame++)
            {
                double sine = Math.Sin(2.0 * Math.PI * Hz * frame / sampleRate) * amplitude;
                for (int ch = 0; ch < channels; ch++)
                {
                    double s = IsSine ? sine : (rnd.NextDouble() * 2.0 - 1.0) * amplitude;
                    loop[frame * channels + ch] = (float)s;
                }
            }
            return loop;
        }
    }

    // ------------------------------------------------------------------
    // Streaming engine (dedicated MTA background thread)
    // ------------------------------------------------------------------

    enum EngineState { Starting, Playing, Waiting, Paused }

    sealed class KeepAliveEngine
    {
        const long BufferHns = 10000000;   // 1 s WASAPI buffer
        const int PollMs = 250;            // top up the buffer 4x per second
        const int RetryMs = 2000;          // device retry interval

        readonly AutoResetEvent signal = new AutoResetEvent(false);
        readonly Action<EngineState, string> onStatus;
        Thread thread;

        volatile bool stopping;
        volatile bool userPaused;
        volatile bool suspended;
        volatile bool waitingForDevice;
        volatile SignalPreset preset;

        // Cached loop so it is rendered once per format/preset, not continuously.
        float[] loop;
        string loopKey;

        public KeepAliveEngine(SignalPreset preset, Action<EngineState, string> onStatus)
        {
            this.preset = preset;
            this.onStatus = onStatus;
        }

        public bool UserPaused { get { return userPaused; } }

        public void Start()
        {
            thread = new Thread(Run);
            thread.Name = "SpeakerAwake.Stream";
            thread.IsBackground = true;
            thread.SetApartmentState(ApartmentState.MTA);
            thread.Start();
        }

        public void Stop()
        {
            stopping = true;
            signal.Set();
            if (thread != null) thread.Join(3000);
        }

        public void SetUserPaused(bool value) { userPaused = value; signal.Set(); }
        public void SetSuspended(bool value) { suspended = value; signal.Set(); }
        public void SetPreset(SignalPreset value) { preset = value; signal.Set(); }

        // Called from Core Audio notification threads; must not block.
        public void RequestReopen() { signal.Set(); }
        public void NudgeIfWaiting() { if (waitingForDevice) signal.Set(); }

        void Report(EngineState state, string text)
        {
            try { onStatus(state, text); } catch (Exception) { }
        }

        void Run()
        {
            IMMDeviceEnumerator enumerator = null;
            NotificationClient notifier = null;
            try
            {
                enumerator = (IMMDeviceEnumerator)Activator.CreateInstance(
                    Type.GetTypeFromCLSID(CoreAudio.CLSID_MMDeviceEnumerator));
                notifier = new NotificationClient(this);
                enumerator.RegisterEndpointNotificationCallback(notifier);

                while (!stopping)
                {
                    if (userPaused || suspended)
                    {
                        Report(EngineState.Paused, suspended ? "Paused (system sleeping)" : "Paused");
                        signal.WaitOne();
                        continue;
                    }

                    try
                    {
                        PlayUntilInterrupted(enumerator);
                    }
                    catch (Exception ex)
                    {
                        // Device lost / none available: retry until a default endpoint appears.
                        waitingForDevice = true;
                        Report(EngineState.Waiting, "Waiting for device (" + CoreAudio.Describe(ex) + ")");
                        signal.WaitOne(RetryMs);
                        waitingForDevice = false;
                    }
                }
            }
            catch (Exception ex)
            {
                Report(EngineState.Waiting, "Audio system unavailable (" + CoreAudio.Describe(ex) + ")");
            }
            finally
            {
                if (enumerator != null)
                {
                    if (notifier != null)
                    {
                        try { enumerator.UnregisterEndpointNotificationCallback(notifier); } catch (Exception) { }
                    }
                    Marshal.ReleaseComObject(enumerator);
                }
            }
        }

        // Opens the current default device and streams until any state change is signalled.
        void PlayUntilInterrupted(IMMDeviceEnumerator enumerator)
        {
            IMMDevice device = null;
            IAudioClient client = null;
            IAudioRenderClient render = null;
            IntPtr format = IntPtr.Zero;
            try
            {
                enumerator.GetDefaultAudioEndpoint(EDataFlow.Render, ERole.Multimedia, out device);
                string deviceName = CoreAudio.GetFriendlyName(device);

                object obj;
                Guid iid = CoreAudio.IID_IAudioClient;
                device.Activate(ref iid, CoreAudio.CLSCTX_ALL, IntPtr.Zero, out obj);
                client = (IAudioClient)obj;

                int sampleRate, channels, channelMask;
                ReadMixFormat(client, out sampleRate, out channels, out channelMask);
                format = BuildFloatFormat(sampleRate, channels, channelMask);

                client.Initialize(CoreAudio.AUDCLNT_SHAREMODE_SHARED,
                    CoreAudio.AUDCLNT_STREAMFLAGS_AUTOCONVERTPCM | CoreAudio.AUDCLNT_STREAMFLAGS_SRC_DEFAULT_QUALITY,
                    BufferHns, 0, format, IntPtr.Zero);

                uint bufferFrames;
                client.GetBufferSize(out bufferFrames);

                iid = CoreAudio.IID_IAudioRenderClient;
                client.GetService(ref iid, out obj);
                render = (IAudioRenderClient)obj;

                float[] samples = GetLoop(preset, sampleRate, channels);
                int loopPos = 0;

                TopUp(client, render, bufferFrames, samples, channels, ref loopPos);
                client.Start();
                Report(EngineState.Playing, deviceName);

                while (!signal.WaitOne(PollMs))
                    TopUp(client, render, bufferFrames, samples, channels, ref loopPos);

                try { client.Stop(); } catch (COMException) { }
            }
            finally
            {
                if (render != null) Marshal.ReleaseComObject(render);
                if (client != null) Marshal.ReleaseComObject(client);
                if (device != null) Marshal.ReleaseComObject(device);
                if (format != IntPtr.Zero) Marshal.FreeHGlobal(format);
            }
        }

        float[] GetLoop(SignalPreset p, int sampleRate, int channels)
        {
            string key = p.Key + "/" + sampleRate + "/" + channels;
            if (loop == null || loopKey != key)
            {
                loop = null;
                loop = p.Render(sampleRate, channels);
                loopKey = key;
            }
            return loop;
        }

        static void TopUp(IAudioClient client, IAudioRenderClient render, uint bufferFrames,
                          float[] samples, int channels, ref int loopPos)
        {
            uint padding;
            client.GetCurrentPadding(out padding);
            int available = (int)(bufferFrames - padding);
            if (available <= 0) return;

            IntPtr data;
            render.GetBuffer((uint)available, out data);
            int loopFrames = samples.Length / channels;
            int written = 0;
            while (written < available)
            {
                int n = Math.Min(available - written, loopFrames - loopPos);
                Marshal.Copy(samples, loopPos * channels, IntPtr.Add(data, written * channels * 4), n * channels);
                written += n;
                loopPos = (loopPos + n) % loopFrames;
            }
            render.ReleaseBuffer((uint)available, 0);
        }

        static void ReadMixFormat(IAudioClient client, out int sampleRate, out int channels, out int channelMask)
        {
            IntPtr mix;
            client.GetMixFormat(out mix);
            try
            {
                ushort tag = (ushort)Marshal.ReadInt16(mix, 0);
                channels = (ushort)Marshal.ReadInt16(mix, 2);
                sampleRate = Marshal.ReadInt32(mix, 4);
                ushort cbSize = (ushort)Marshal.ReadInt16(mix, 16);
                if (tag == CoreAudio.WAVE_FORMAT_EXTENSIBLE && cbSize >= 22)
                    channelMask = Marshal.ReadInt32(mix, 20);
                else
                    channelMask = channels == 1 ? 0x4 : channels == 2 ? 0x3 : 0;
            }
            finally { Marshal.FreeCoTaskMem(mix); }
        }

        // WAVEFORMATEXTENSIBLE, 32-bit IEEE float, at the device's mix rate/channel layout.
        static IntPtr BuildFloatFormat(int sampleRate, int channels, int channelMask)
        {
            IntPtr p = Marshal.AllocHGlobal(40);
            int blockAlign = channels * 4;
            Marshal.WriteInt16(p, 0, unchecked((short)CoreAudio.WAVE_FORMAT_EXTENSIBLE));
            Marshal.WriteInt16(p, 2, (short)channels);
            Marshal.WriteInt32(p, 4, sampleRate);
            Marshal.WriteInt32(p, 8, sampleRate * blockAlign);
            Marshal.WriteInt16(p, 12, (short)blockAlign);
            Marshal.WriteInt16(p, 14, 32);
            Marshal.WriteInt16(p, 16, 22);
            Marshal.WriteInt16(p, 18, 32);
            Marshal.WriteInt32(p, 20, channelMask);
            byte[] sub = CoreAudio.KSDATAFORMAT_SUBTYPE_IEEE_FLOAT.ToByteArray();
            Marshal.Copy(sub, 0, IntPtr.Add(p, 24), 16);
            return p;
        }

        sealed class NotificationClient : IMMNotificationClient
        {
            readonly KeepAliveEngine engine;
            public NotificationClient(KeepAliveEngine engine) { this.engine = engine; }

            public void OnDefaultDeviceChanged(EDataFlow flow, ERole role, string defaultDeviceId)
            {
                if (flow == EDataFlow.Render && role == ERole.Multimedia) engine.RequestReopen();
            }
            public void OnDeviceStateChanged(string deviceId, int newState) { engine.NudgeIfWaiting(); }
            public void OnDeviceAdded(string deviceId) { engine.NudgeIfWaiting(); }
            public void OnDeviceRemoved(string deviceId) { }
            public void OnPropertyValueChanged(string deviceId, PropertyKey key) { }
        }
    }

    // ------------------------------------------------------------------
    // Persistence: settings + run-at-logon
    // ------------------------------------------------------------------

    static class Settings
    {
        const string KeyPath = @"Software\SpeakerAwake";

        public static SignalPreset LoadPreset()
        {
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(KeyPath))
                return SignalPreset.Find(key == null ? null : key.GetValue("Signal") as string);
        }

        public static void SavePreset(SignalPreset preset)
        {
            using (RegistryKey key = Registry.CurrentUser.CreateSubKey(KeyPath))
                key.SetValue("Signal", preset.Key);
        }
    }

    static class Startup
    {
        const string RunKey = @"Software\Microsoft\Windows\CurrentVersion\Run";
        const string ValueName = "SpeakerAwake";

        static string Command { get { return "\"" + Application.ExecutablePath + "\""; } }

        public static bool IsEnabled()
        {
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(RunKey))
            {
                string value = key == null ? null : key.GetValue(ValueName) as string;
                return value != null && string.Equals(value, Command, StringComparison.OrdinalIgnoreCase);
            }
        }

        public static void SetEnabled(bool enabled)
        {
            using (RegistryKey key = Registry.CurrentUser.CreateSubKey(RunKey))
            {
                if (enabled) key.SetValue(ValueName, Command);
                else key.DeleteValue(ValueName, false);
            }
        }
    }

    // ------------------------------------------------------------------
    // Tray UI (main thread)
    // ------------------------------------------------------------------

    sealed class TrayApp : ApplicationContext
    {
        [DllImport("user32.dll")] static extern bool DestroyIcon(IntPtr handle);
        [DllImport("kernel32.dll")] static extern bool SetProcessWorkingSetSize(IntPtr process, IntPtr min, IntPtr max);
        [DllImport("kernel32.dll")] static extern IntPtr GetCurrentProcess();

        readonly Control invoker;
        readonly NotifyIcon tray;
        readonly ContextMenuStrip menu;
        readonly ToolStripMenuItem statusItem;
        readonly ToolStripMenuItem pauseItem;
        readonly ToolStripMenuItem startupItem;
        readonly List<ToolStripMenuItem> presetItems = new List<ToolStripMenuItem>();
        readonly Dictionary<EngineState, Icon> icons = new Dictionary<EngineState, Icon>();
        readonly KeepAliveEngine engine;
        readonly System.Windows.Forms.Timer trimTimer;

        public TrayApp()
        {
            invoker = new Control();
            invoker.CreateControl();
            IntPtr forceHandle = invoker.Handle;

            icons[EngineState.Starting] = MakeIcon(Color.FromArgb(150, 150, 150));
            icons[EngineState.Paused] = MakeIcon(Color.FromArgb(150, 150, 150));
            icons[EngineState.Playing] = MakeIcon(Color.FromArgb(46, 184, 92));
            icons[EngineState.Waiting] = MakeIcon(Color.FromArgb(240, 160, 30));

            SignalPreset current = Settings.LoadPreset();

            menu = new ContextMenuStrip();
            statusItem = new ToolStripMenuItem("Starting...");
            statusItem.Enabled = false;
            pauseItem = new ToolStripMenuItem("Pause", null, OnPauseClick);
            ToolStripMenuItem signalMenu = new ToolStripMenuItem("Signal");
            foreach (SignalPreset p in SignalPreset.All)
            {
                ToolStripMenuItem item = new ToolStripMenuItem(p.Label, null, OnPresetClick);
                item.Tag = p;
                item.Checked = p == current;
                presetItems.Add(item);
                signalMenu.DropDownItems.Add(item);
            }
            startupItem = new ToolStripMenuItem("Start with Windows", null, OnStartupClick);
            startupItem.Checked = Startup.IsEnabled();

            menu.Items.Add(statusItem);
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add(pauseItem);
            menu.Items.Add(signalMenu);
            menu.Items.Add(startupItem);
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add(new ToolStripMenuItem("Exit", null, OnExitClick));

            tray = new NotifyIcon();
            tray.Icon = icons[EngineState.Starting];
            tray.Text = "SpeakerAwake";
            tray.ContextMenuStrip = menu;
            tray.Visible = true;

            SystemEvents.PowerModeChanged += OnPowerModeChanged;

            engine = new KeepAliveEngine(current, OnEngineStatus);
            engine.Start();

            trimTimer = new System.Windows.Forms.Timer();
            trimTimer.Interval = 5000;
            trimTimer.Tick += delegate { trimTimer.Stop(); TrimMemory(); };
            trimTimer.Start();
        }

        // Called on the engine thread.
        void OnEngineStatus(EngineState state, string text)
        {
            if (invoker.IsDisposed) return;
            invoker.BeginInvoke((MethodInvoker)delegate { ApplyStatus(state, text); });
        }

        void ApplyStatus(EngineState state, string text)
        {
            string line = state == EngineState.Playing ? "Playing on " + text : text;
            statusItem.Text = line;
            tray.Icon = icons[state];
            string tip = "SpeakerAwake\n" + line;
            tray.Text = tip.Length > 63 ? tip.Substring(0, 60) + "..." : tip;
            if (state == EngineState.Playing) trimTimer.Start();
        }

        void OnPowerModeChanged(object sender, PowerModeChangedEventArgs e)
        {
            if (e.Mode == PowerModes.Suspend) engine.SetSuspended(true);
            else if (e.Mode == PowerModes.Resume) engine.SetSuspended(false);
        }

        void OnPauseClick(object sender, EventArgs e)
        {
            bool pause = !engine.UserPaused;
            engine.SetUserPaused(pause);
            pauseItem.Text = pause ? "Resume" : "Pause";
        }

        void OnPresetClick(object sender, EventArgs e)
        {
            ToolStripMenuItem clicked = (ToolStripMenuItem)sender;
            SignalPreset p = (SignalPreset)clicked.Tag;
            foreach (ToolStripMenuItem item in presetItems) item.Checked = item == clicked;
            Settings.SavePreset(p);
            engine.SetPreset(p);
        }

        void OnStartupClick(object sender, EventArgs e)
        {
            try
            {
                Startup.SetEnabled(!startupItem.Checked);
                startupItem.Checked = Startup.IsEnabled();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not change the startup setting:\n" + ex.Message, "SpeakerAwake",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        void OnExitClick(object sender, EventArgs e)
        {
            ExitThread();
        }

        protected override void ExitThreadCore()
        {
            SystemEvents.PowerModeChanged -= OnPowerModeChanged;
            engine.Stop();
            tray.Visible = false;
            tray.Dispose();
            menu.Dispose();
            trimTimer.Dispose();
            invoker.Dispose();
            foreach (Icon icon in icons.Values) icon.Dispose();
            base.ExitThreadCore();
        }

        static void TrimMemory()
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            SetProcessWorkingSetSize(GetCurrentProcess(), new IntPtr(-1), new IntPtr(-1));
        }

        static Icon MakeIcon(Color color)
        {
            using (Bitmap bmp = new Bitmap(16, 16))
            {
                using (Graphics g = Graphics.FromImage(bmp))
                using (SolidBrush brush = new SolidBrush(color))
                using (Pen pen = new Pen(color, 1.6f))
                {
                    g.SmoothingMode = SmoothingMode.AntiAlias;
                    g.Clear(Color.Transparent);
                    g.FillPolygon(brush, new[]
                    {
                        new Point(1, 6), new Point(4, 6), new Point(8, 2),
                        new Point(8, 14), new Point(4, 10), new Point(1, 10)
                    });
                    g.DrawArc(pen, 6, 5, 6, 6, -55, 110);
                    g.DrawArc(pen, 4, 2, 11, 12, -55, 110);
                }
                IntPtr handle = bmp.GetHicon();
                using (Icon temp = Icon.FromHandle(handle))
                {
                    Icon owned = (Icon)temp.Clone();
                    DestroyIcon(handle);
                    return owned;
                }
            }
        }
    }

    static class Program
    {
        [STAThread]
        static int Main(string[] args)
        {
            foreach (string arg in args)
            {
                string a = arg.ToLowerInvariant();
                if (a == "--enable-startup") { Startup.SetEnabled(true); return 0; }
                if (a == "--disable-startup") { Startup.SetEnabled(false); return 0; }
            }

            bool firstInstance;
            using (Mutex mutex = new Mutex(true, @"Local\SpeakerAwake.SingleInstance", out firstInstance))
            {
                if (!firstInstance) return 1;
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new TrayApp());
            }
            return 0;
        }
    }
}
