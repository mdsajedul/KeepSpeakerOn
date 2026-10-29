<img src="assets/SpeakerAwake-256.png" alt="SpeakerAwake: green speaker icon" width="96" align="right">

# SpeakerAwake: stop your Bluetooth speaker turning off automatically (Windows)

[![Latest release](https://img.shields.io/github/v/release/mdsajedul/SpeakerAwake?label=download)](https://github.com/mdsajedul/SpeakerAwake/releases/latest)
[![Windows 10 | 11](https://img.shields.io/badge/Windows-10%20%7C%2011-0078D4)](docs/REQUIREMENTS.md)
[![License: MIT](https://img.shields.io/badge/license-MIT-green)](LICENSE)
[![Downloads](https://img.shields.io/github/downloads/mdsajedul/SpeakerAwake/total)](https://github.com/mdsajedul/SpeakerAwake/releases)

**SpeakerAwake is a free, open-source Windows tray app that keeps Bluetooth, USB, HDMI and
powered speakers from going into auto-standby.** It plays an inaudible signal, so your
speaker never sees silence and never switches itself off.

Does your speaker **turn off by itself after a few minutes of no sound**? Do you lose the
**first second of every notification, video or song**? Do you keep having to press the power
button on your **Bluetooth speaker, soundbar or studio monitors**? SpeakerAwake fixes that.

**[⬇ Download SpeakerAwake.exe](https://github.com/mdsajedul/SpeakerAwake/releases/latest)**: 64 KB, no installer, Windows 10 and 11.

## Features

- **Inaudible keep-alive signal:** very quiet white noise (−60 to −90 dB) or a 10–15 Hz sub-audible tone. You won't hear it.
- **Works alongside everything:** shared-mode audio, so music, YouTube, games and Zoom/Teams calls play normally.
- **Follows your default speaker:** connect your Bluetooth speaker and SpeakerAwake moves to it automatically.
- **Reconnects automatically:** if the speaker disconnects, it waits and resumes when it comes back.
- **Sleep-aware:** pauses when the PC sleeps and resumes on wake.
- **Featherweight:** about 0% CPU, about 2 MB of RAM, no dependencies, no admin rights, no telemetry.
- **Starts with Windows:** one click in the tray menu.

## Which speakers does it work with?

SpeakerAwake is designed for **any speaker that turns off after a period of silence**, on any
output Windows can play to:

- **Bluetooth speakers** (Awei, JBL, Anker Soundcore, Sony, Tribit, Xiaomi, Bose, Marshall and others)
- **Bluetooth headphones and earbuds** that power down when idle
- **Soundbars and AV receivers** over HDMI, optical or USB
- **Powered PC speakers and studio monitors** with auto-standby (Edifier, Creative, Logitech, KRK and others)
- **USB DACs and USB speakers**

See [Supported Devices](docs/SUPPORTED_DEVICES.md) for tested models and known limitations.
Tested a speaker? [Tell us whether it worked](https://github.com/mdsajedul/SpeakerAwake/issues/new?title=Device%20report:%20&body=Model:%0AConnection%20(Bluetooth/USB/HDMI/3.5mm):%0AWorked%20(yes/no):%0ALowest%20preset%20that%20kept%20it%20awake:).

## How to use it

1. **Download** `SpeakerAwake.exe` from the [latest release](https://github.com/mdsajedul/SpeakerAwake/releases/latest) and put it in a permanent folder.
2. **Double-click it.** A speaker icon appears in the system tray. If you don't see it, click the **^** arrow.
3. Check that the icon is **green**. Hover over it to see which device it's playing on.
4. Right-click the icon and tick **Start with Windows**.

> **Seeing a blue "Windows protected your PC" screen?** That's Microsoft Defender
> SmartScreen. It appears for new apps that aren't code-signed yet, not because anything
> was found in the file. Click **More info**, then **Run anyway**. You only need to do this once.
> See [how to check the file is genuine](docs/USER_GUIDE.md#first-launch-windows-protected-your-pc).

If your speaker still turns off, right-click the icon and choose **Signal → White noise, −60 dB**.
Full details are in the [User Guide](docs/USER_GUIDE.md).

## FAQ

### How do I stop my Bluetooth speaker from turning off automatically on Windows?
Most Bluetooth speakers switch off after 5–30 minutes without sound. Run SpeakerAwake and leave it
in the tray. It keeps sending a signal too quiet to hear, so the speaker stays on as long as your PC is awake.

### Why does my speaker cut off the first second of sound?
When there's no audio, many speakers and soundbars go into a power-saving standby, and they take a
moment to wake up when sound starts again. That's why the start of notifications, videos and songs
gets clipped. SpeakerAwake keeps the speaker active, so nothing gets cut off.

### Can I hear the signal SpeakerAwake plays?
No. The default is white noise at −70 dB, far below normal listening levels. You can switch to −80
or −90 dB, or to a 10–15 Hz tone, which is below the range of human hearing.

### Why not just play a silent MP3 on loop?
A silent file is *digital silence*: every sample is zero. Most speakers detect that as "no signal"
and go to sleep anyway. SpeakerAwake sends a real, tiny signal that the speaker detects.
It also follows device changes and survives sleep and disconnects, which a media player won't.

### Does it affect sound quality or battery life?
Sound quality: no. The signal is mixed in at a level far below what you can hear. Laptop battery: the
impact is negligible (about 0% CPU). Your Bluetooth speaker *will* use its own battery while it's
kept on, so on battery-powered speakers it's best used while they're plugged in.

### Is SpeakerAwake safe? Why does Windows warn me?
Yes. It's open source under the MIT license, and you can read every line in
[`src/SpeakerAwake.cs`](src/SpeakerAwake.cs) or build it yourself. It doesn't connect to the internet,
needs no admin rights, and only writes its settings to `HKCU\Software\SpeakerAwake`. The SmartScreen
warning appears because the exe isn't code-signed yet.

### Does it work on Windows 10, Windows 11, macOS or Linux?
Windows 10 (64-bit or 32-bit) and Windows 11. macOS and Linux aren't supported.

### Does it prevent my PC from sleeping?
No. SpeakerAwake doesn't keep the computer awake. It pauses when Windows sleeps and resumes on wake.

## Documentation

| Document | Contents |
|---|---|
| [User Guide](docs/USER_GUIDE.md) | Install, tray menu, signal presets, troubleshooting, uninstall |
| [Supported Devices](docs/SUPPORTED_DEVICES.md) | Which speakers and outputs work, and known limitations |
| [Requirements](docs/REQUIREMENTS.md) | System requirements and technical specification |
| [Contributing](CONTRIBUTING.md) | Build from source, code structure, development guidelines |
| [Changelog](CHANGELOG.md) | Release history |

## Contributing

Bug reports, device test results and pull requests are welcome. See [CONTRIBUTING.md](CONTRIBUTING.md).
If SpeakerAwake fixed your speaker, a ⭐ on GitHub helps other people find it.

## License

[MIT](LICENSE) © 2026 mdsajedul
