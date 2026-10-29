# Supported Devices

SpeakerAwake works with **any audio output that Windows lists under Settings → System →
Sound → Output**. It uses the standard Windows audio API (WASAPI), so no vendor
drivers or pairing are needed.

The real question is whether a particular speaker's auto-standby is triggered by
*silence*, because that is the only kind SpeakerAwake can prevent.

## Device types

| Type | Examples | Works? | Notes |
|---|---|---|---|
| Bluetooth speakers (A2DP) | Awei, JBL, Anker Soundcore, Sony, Tribit, Xiaomi | ✅ Yes, the main use case | Use the *Stereo* / *Speakers* endpoint, not *Hands-Free*. If −70 dB isn't enough, use −60 dB. |
| Bluetooth headphones and earbuds | AirPods, Galaxy Buds, Sony WH series | ✅ Yes | Stops the "power off after silence" behaviour and the delay before sound starts |
| Powered speakers with auto-standby (3.5 mm or optical) | Edifier, Creative, Logitech, studio monitors with auto-standby | ✅ Usually | Analog signal detectors can need a stronger signal. Start at −60 dB. |
| USB speakers and DACs | USB soundbars, USB DACs, docks | ✅ Yes | — |
| HDMI / DisplayPort audio | Monitors, TVs, AV receivers | ✅ Yes | Also stops some TVs and receivers losing the first second of sound |
| Laptop built-in speakers | — | ✅ Plays fine | Not needed for these, but harmless |
| Virtual devices | Voicemeeter, NVIDIA Broadcast, Camo | ✅ Plays | Only useful if the virtual device forwards to a real speaker |

## Tested

| Device | Connection | Result |
|---|---|---|
| Conexant SmartAudio HD (laptop) | Internal | ✅ Stream active at −70 dBFS |
| Awei Bluetooth speaker | Bluetooth A2DP | ⏳ Target device, awaiting report |

Tested a device? Please open an issue or PR to add it to this table, with the model,
the connection type, and the lowest preset that kept it awake.

## Known limitations

- **Fixed-timer speakers.** Some speakers turn off after a set time no matter what is
  playing, or only respond to their own button. Nothing on the PC side can prevent this.
- **Hands-Free / Headset profile.** If Windows uses a Bluetooth speaker's *Hands-Free* endpoint
  (for example during a call), audio quality drops. SpeakerAwake simply follows whichever
  device is the default.
- **Muted or zero volume.** Windows sends true silence and the speaker will sleep.
- **Exclusive-mode apps.** While an app has exclusive use of the device, SpeakerAwake waits
  (orange icon) and resumes when the device is released.
- **Very low frequencies.** Some Bluetooth codecs and small drivers remove 10–15 Hz,
  so the sine presets may not register. Prefer the noise presets.
- **Not for microphones or inputs.** Only output devices are supported.
