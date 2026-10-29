# Contributing to SpeakerAwake

Thanks for helping! Bug reports, device test results and pull requests are all welcome.

## Building

There's no Visual Studio, NuGet or SDK to install. SpeakerAwake builds with the C# compiler
that ships with the .NET Framework on every Windows 10/11 PC.

```
git clone https://github.com/mdsajedul/SpeakerAwake.git
cd SpeakerAwake
build.cmd
```

The output is `bin\SpeakerAwake.exe`. The build uses `/warnaserror+`, so it must compile with no warnings.

## Project layout

```
src/SpeakerAwake.cs     All source code (single file)
assets/                 App icon (.ico) and a 256 px PNG preview
tools/make-icon.ps1     Regenerates the app icon
build.cmd               Build script
docs/                   User guide, requirements, supported devices
```

### Code structure (`src/SpeakerAwake.cs`)

| Section | Responsibility |
|---|---|
| COM interop | `IMMDeviceEnumerator`, `IMMDevice`, `IAudioClient`, `IAudioRenderClient`, `IMMNotificationClient`, `IPropertyStore` declarations |
| `SignalPreset` | Preset table, and rendering of the 1-second float32 loop |
| `KeepAliveEngine` | Streaming state machine on a dedicated MTA thread: open default device → play → tear down on any signal → retry |
| `Settings` / `Startup` | Registry persistence (`HKCU\Software\SpeakerAwake`, the `Run` key) |
| `TrayApp` | Tray icon, menu and power events (UI thread) |
| `Program` | Command-line flags and the single-instance mutex |

### How the engine works

1. Get the default `eRender` / `eMultimedia` endpoint and activate `IAudioClient`.
2. Read the mix format, then initialize a **shared-mode** stream in 32-bit float at the mix rate,
   with a 1 s buffer and `AUTOCONVERTPCM`.
3. Pre-fill the buffer from the cached loop, call `Start()`, then wait on a single
   `AutoResetEvent` with a 250 ms timeout, topping the buffer up after each timeout.
4. Any change (default device changed, pause, sleep, preset change, exit) sets the event,
   and the stream is torn down and the loop decides what to do next.
5. A COM error (device lost, no device) puts the engine into a 2 s retry loop. Device-added and
   device-state notifications cut the wait short.

## Development guidelines

### Language and dependencies
- **C# 5 only.** The built-in `csc.exe` doesn't support newer syntax: no string
  interpolation (`$""`), `?.`, `nameof`, expression-bodied members or `out var`.
  Use `string.Format` and explicit null checks.
- **No third-party dependencies or NuGet packages.** Use only the .NET Framework and Win32/COM.
  Keeping the exe tiny and dependency-free is a core feature.
- Keep everything in a single source file unless a change clearly benefits from splitting it.

### Performance budget (must not regress)
- CPU under 1% while playing. Never generate audio per sample in the streaming loop.
  Render once, then copy.
- RAM: 25 MB private bytes or less. Avoid allocations in the streaming loop.
- Measure before submitting changes to the engine:
  ```powershell
  $p = Start-Process .\bin\SpeakerAwake.exe -PassThru; Start-Sleep 8; $p.Refresh()
  $t = $p.TotalProcessorTime; Start-Sleep 20; $p.Refresh()
  "CPU %: " + (($p.TotalProcessorTime - $t).TotalSeconds / 20 / [Environment]::ProcessorCount * 100)
  "Private MB: " + ($p.PrivateMemorySize64 / 1MB)
  ```

### Threading rules
- All WASAPI objects are created, used and released **on the engine thread only**.
- `IMMNotificationClient` callbacks run on system threads. They may only set flags and signal
  the event. Never call Core Audio or block in them.
- UI updates go through `invoker.BeginInvoke`. The engine never touches WinForms objects.
- Always release COM objects with `Marshal.ReleaseComObject` in `finally` blocks.

### Style
- Follow the existing style: 4-space indent, Allman braces, `readonly` wherever possible,
  `PascalCase` for types and members and `camelCase` for locals and fields.
- Name HRESULTs and flags as constants in `CoreAudio`. No magic numbers inline.
- Comment *why*, not *what*. Keep comments short.

### User-facing behaviour
- The app must never show a window or dialog during normal operation. Only errors from actions
  the user started (for example the startup toggle) may show a message box.
- It must never make an audible sound. Any new preset must stay at −60 dBFS or below for
  broadband signals, or below 20 Hz for tones.
- Settings live only under `HKCU\Software\SpeakerAwake`. No admin rights and no files.

## Manual test checklist

Before opening a PR that touches the engine or the tray UI:

- [ ] `build.cmd` succeeds with no warnings
- [ ] The icon turns green and the tooltip shows the correct device
- [ ] Music or video plays normally at the same time
- [ ] Switching the default output device moves the stream (tooltip updates)
- [ ] Turning off or disconnecting the speaker → orange. Reconnecting → green again with no restart.
- [ ] Sleep and wake the PC → grey during sleep, green after wake
- [ ] Changing the preset takes effect immediately and is remembered after a restart
- [ ] The Start with Windows toggle adds and removes the `Run` entry
- [ ] CPU and RAM stay within budget

## Pull requests

1. Fork the repo and create a branch: `feature/<short-name>` or `fix/<short-name>`.
2. Keep each PR focused on one change. Update the docs and `CHANGELOG.md` when behaviour changes.
3. In the PR description, say what you changed, why, and which checklist items you tested
   (plus the device you tested on).

## Releasing (maintainers)

1. Bump `AssemblyVersion` and `AssemblyFileVersion` in `src/SpeakerAwake.cs` and update `CHANGELOG.md`.
2. Run `build.cmd`.
3. Tag the release (`git tag v1.x.y && git push --tags`), then create a GitHub Release for the
   tag and attach `bin\SpeakerAwake.exe`.

### Icon

The app icon (`assets/SpeakerAwake.ico`) is generated from the same shape as the tray icon.
If you change `TrayApp.MakeIcon`, update `tools/make-icon.ps1` to match, then regenerate it:

```
powershell -ExecutionPolicy Bypass -File tools\make-icon.ps1
```

### SmartScreen and code signing

Release builds are currently **not code-signed**, so Windows SmartScreen shows "Windows
protected your PC" on first launch. Ways to reduce or remove the warning:

| Option | Cost | Effect |
|---|---|---|
| Submit each release exe to [Microsoft's file submission portal](https://www.microsoft.com/en-us/wdsi/filesubmission) as a *Software developer*, asking for it to be marked as clean | Free | Often clears the warning for that exact file within a few days |
| Apply for free OSS signing through the [SignPath Foundation](https://signpath.org) | Free for open-source projects | Signed builds from GitHub Actions; SmartScreen reputation still builds over time |
| [Azure Artifact Signing](https://azure.microsoft.com/products/artifact-signing) (formerly Trusted Signing) | About US$10/month | Signed builds; available to individuals only in some countries |

Never self-sign: a self-signed certificate doesn't help with SmartScreen.

## Reporting bugs

Open an issue with:
- your Windows version (`winver`)
- the speaker model and how it's connected
- the tooltip and status text when the problem happens
- the steps to reproduce it

## License

By contributing, you agree that your contributions are licensed under the [MIT License](LICENSE).
