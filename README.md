<img src="assets/SpeakerAwake-256.png" alt="SpeakerAwake icon" width="96" align="right">

# SpeakerAwake

**Stop your speaker from falling asleep.**

Many Bluetooth and powered speakers switch themselves off after a few minutes of
silence, so the start of the next sound gets cut off, or you have to turn the speaker
back on by hand. SpeakerAwake is a tiny Windows tray app that sends a signal you can't
hear to your speaker, so it always thinks audio is playing.

- 🔇 **Inaudible.** It plays very quiet white noise (−60 to −90 dB) or a 10–15 Hz tone.
- 🎵 **Doesn't get in the way.** Music, videos, games and calls play normally alongside it.
- 🔌 **Follows your speaker.** It switches automatically to whatever device is your default output.
- 😴 **Sleep-aware.** It pauses when the PC sleeps and resumes when it wakes.
- 🪶 **Featherweight.** About 0% CPU, about 2 MB working set, and a 64 KB exe with no installer and no dependencies.

## Download

Get **`SpeakerAwake.exe`** from the [latest release](../../releases/latest), put
it anywhere (for example `C:\Tools\SpeakerAwake\`), and double-click it.

> **Seeing a blue "Windows protected your PC" screen?** That's Microsoft Defender
> SmartScreen. It appears for new apps that aren't code-signed yet, not because anything
> was found in the file. Click **More info**, then **Run anyway**. You only need to do this once.
> See [User Guide → First launch](docs/USER_GUIDE.md#first-launch-windows-protected-your-pc)
> to check the file is genuine, or [build it yourself](CONTRIBUTING.md#building) from source.

## Quick start

1. Run `SpeakerAwake.exe`. A speaker icon appears in the system tray. Click the **^** arrow if you don't see it.
2. Make sure the icon is **green**. Hover over it to see which device is playing.
3. Right-click the icon and tick **Start with Windows** so it runs automatically.

That's it. Full details are in the [User Guide](docs/USER_GUIDE.md).

## Documentation

| Document | Contents |
|---|---|
| [User Guide](docs/USER_GUIDE.md) | Using the app, signal presets, troubleshooting, uninstalling |
| [Requirements](docs/REQUIREMENTS.md) | System requirements and the functional/technical specification |
| [Supported Devices](docs/SUPPORTED_DEVICES.md) | Which speakers and outputs work, and known limitations |
| [Contributing](CONTRIBUTING.md) | Building from source, code structure, development guidelines |
| [Changelog](CHANGELOG.md) | Release history |

## License

[MIT](LICENSE) © 2026 mdsajedul
