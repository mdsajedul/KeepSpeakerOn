# Requirements

## System requirements

| | Minimum | Tested |
|---|---|---|
| OS | Windows 10 (64- or 32-bit) | Windows 11 Home 24H2 (build 26100) |
| Runtime | .NET Framework 4.5 or later (already included in Windows 10/11) | .NET Framework 4.8.1 |
| CPU | Any | — |
| RAM | 25 MB free | — |
| Disk | Under 100 KB | — |
| Permissions | Standard user (no admin needed) | — |
| Audio | Windows Audio service running, plus one output device | Conexant SmartAudio HD |

Windows 7 and 8.1 may work but are not tested or supported.

To build from source you only need the C# compiler that comes with .NET Framework.
See [CONTRIBUTING.md](../CONTRIBUTING.md).

## Functional requirements

| ID | Requirement | How it's met |
|---|---|---|
| F1 | **Continuous inaudible playback.** Output a signal all the time so the speaker never triggers auto-standby. | A WASAPI render stream runs for as long as the app is running and not paused. |
| F2 | **Inaudible to humans.** Use a sub-audible tone (10–15 Hz) or near-zero-amplitude noise (−60 to −90 dB). | Presets: white noise at −60, −70 (default), −80 or −90 dBFS peak, or a 10/15 Hz sine at −30 dBFS. |
| F3 | **Shared audio stream.** Other apps can play at the same time. | `AUDCLNT_SHAREMODE_SHARED` |
| F4 | **Dynamic device targeting.** Always play to the default multimedia output. | `GetDefaultAudioEndpoint(eRender, eMultimedia)` plus `IMMNotificationClient.OnDefaultDeviceChanged`, which reopens the stream on the new default. |

## Technical and performance constraints

| ID | Constraint | How it's met | Measured |
|---|---|---|---|
| T1 | CPU under 1% | 1 s buffer, topped up 4 times per second, with no per-sample work | about 0.000% |
| T2 | RAM 25 MB or less | No dependencies; working set is trimmed after start-up | 2 MB working set, 16 MB private |
| T3 | Sample kept in memory | A 1 s loop is rendered once per preset and format and cached. No disk I/O. | — |
| T4 | Concurrency | Dedicated MTA background thread for streaming. The UI thread only handles the tray. | — |

## Lifecycle and OS integration

| ID | Requirement | How it's met |
|---|---|---|
| L1 | Runs headless in the background | Tray icon only, with no window |
| L2 | Starts at sign-in | Optional `HKCU\Software\Microsoft\Windows\CurrentVersion\Run` entry |
| L3 | Adapts to power state | `SystemEvents.PowerModeChanged`: the stream is released on Suspend and opened again on Resume |
| L4 | Survives disconnection | `AUDCLNT_E_DEVICE_INVALIDATED` and no-device errors are caught. It retries every 2 s, and immediately when a device is added. |
