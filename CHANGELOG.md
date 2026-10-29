# Changelog

## [1.0.0] - 2026-10-29

First release.

- Tray app that plays an inaudible keep-alive signal on the default multimedia output (WASAPI, shared mode)
- Presets: white noise at −60, −70 (default), −80 or −90 dB, and a 10/15 Hz sine
- Follows changes to the default device automatically, and retries when a device is disconnected
- Pauses on sleep and resumes on wake
- Optional start with Windows
- About 0% CPU, about 2 MB working set, 22 KB exe, no dependencies
