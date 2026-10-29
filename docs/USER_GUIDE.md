# SpeakerAwake User Guide

## 1. Install

SpeakerAwake is a single portable exe with no installer.

1. Download `SpeakerAwake.exe` from the [latest release](../../../releases/latest).
2. Move it to a permanent folder, for example `C:\Tools\SpeakerAwake\`.
   Do this *before* you turn on **Start with Windows**, because that setting remembers where the exe is.
3. Double-click it. If SmartScreen warns you, choose **More info → Run anyway**.

Only one copy runs at a time. Starting it again does nothing.

## 2. The tray icon

SpeakerAwake has no window. It lives in the notification area next to the clock.

| Icon | Meaning |
|---|---|
| 🟢 Green speaker | Playing. Hover over it to see the device name, for example *Playing on Speakers (AWEI A8)*. |
| 🟠 Orange speaker | Waiting for an output device. This is normal while a Bluetooth speaker is connecting. It retries every 2 seconds. |
| ⚪ Grey speaker | Paused, either by you or because the PC is going to sleep. |

**Tip:** to keep the icon always visible, go to **Settings → Personalization → Taskbar →
Other system tray icons** and turn on **SpeakerAwake**.

## 3. Menu

Right-click the icon:

| Item | What it does |
|---|---|
| *Status line* | Shows the current state and device |
| **Pause / Resume** | Stops or restarts the signal. Your speaker may then go to sleep as usual. |
| **Signal** | Chooses the keep-alive signal (see below). Your choice is remembered. |
| **Start with Windows** | Starts SpeakerAwake automatically when you sign in |
| **Exit** | Closes the app |

## 4. Choosing a signal

| Preset | When to use it |
|---|---|
| White noise, −60 dB | Your speaker still turns off with the default. This is the strongest option. |
| **White noise, −70 dB** | **The default. Works for most speakers.** |
| White noise, −80 dB | You can faintly hear hiss at very high volume. |
| White noise, −90 dB | The quietest option. Some speakers won't detect it. |
| Sub-audible sine, 10 Hz / 15 Hz | An alternative if noise doesn't suit your speaker. Some Bluetooth codecs remove frequencies this low. |

**How to find the right level:** start at −70 dB and leave the PC idle for longer than
your speaker's auto-off time (usually 10–30 minutes). If the speaker turns off, switch to
−60 dB. If you can hear hiss with your ear close to the speaker at full volume, go one
step quieter.

## 5. Things that defeat it

- **Muted or 0% volume.** Windows then sends real silence. Keep the volume above 0 and unmuted.
- **Wrong default device.** SpeakerAwake plays to the *default* output. Pick your speaker
  in the volume flyout (click the speaker icon in the taskbar, then **>**).
- **Speakers with a fixed timer.** Some models turn off after a set time no matter what's playing.
  See [Supported Devices](SUPPORTED_DEVICES.md).

## 6. Troubleshooting

| Problem | Fix |
|---|---|
| The icon stays orange | No output device is available. Connect or turn on your speaker, or check **Settings → System → Sound**. |
| The tooltip shows the wrong device | Set your speaker as the default output device. SpeakerAwake switches within a second. |
| The speaker still sleeps | Use **Signal → White noise, −60 dB**. Check that the volume isn't muted. |
| I can hear a faint hiss | Choose a quieter preset, −80 dB or −90 dB. |
| It doesn't start at sign-in | Untick and re-tick **Start with Windows** (needed if you moved the exe). Also check **Task Manager → Startup apps**. |
| *"Device in exclusive use"* | Another app (for example a DAW or an ASIO driver) has taken over the device. Close it, or turn off exclusive mode in the device's Sound properties. |

## 7. Uninstall

1. Right-click the tray icon and untick **Start with Windows**, then click **Exit**.
2. Delete `SpeakerAwake.exe`.
3. Optional: delete the settings key `HKEY_CURRENT_USER\Software\SpeakerAwake`.

SpeakerAwake writes nothing else to your PC.

## 8. Command line

```
SpeakerAwake.exe                    Start the tray app
SpeakerAwake.exe --enable-startup   Add to Windows startup and exit
SpeakerAwake.exe --disable-startup  Remove from Windows startup and exit
```
