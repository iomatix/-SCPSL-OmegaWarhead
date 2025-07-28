# BetterOmegaWarhead 

[![Download Latest Release](https://img.shields.io/badge/Download-Latest%20Release-blue?style=for-the-badge)](https://github.com/iomatix/-SCPSL-OmegaWarhead/releases/latest)
[![GitHub Downloads](https://img.shields.io/github/downloads/iomatix/-SCPSL-OmegaWarhead/latest/total?sort=date&style=for-the-badge)](https://github.com/iomatix/-SCPSL-OmegaWarhead/releases/latest)

This plugins adds a new Warhead to SCP:SL. It can be activated replacing Alpha Warhead with the Omega Warhead (deactivated by default) or via commands (activateomegawarhead). The ways to survive the Omega Warhead are 2:
1. Stay in the Breach/Evacuation Shelter
2. Escape in the rescue Helicopter at surface zone.

## Dependencies:

- **[SCPSL-AudioManagerAPI](https://github.com/iomatix/-SCPSL-AudioManagerAPI/tree/main/AudioManagerAPI)**: `https://github.com/iomatix/-SCPSL-AudioManagerAPI/releases`

### Supporting Development

My mods are **always free to use**.

If you appreciate my work, you can support me by [buying me a coffee](https://buymeacoffee.com/iomatix).

### Contributors

<a href="https://github.com/iomatix/-SCPSL-OmegaWarhead/graphs/contributors">
  <img src="https://contrib.rocks/image?repo=iomatix/-SCPSL-BetterOmegaWarhead" />
</a>

### Example Config:
```
# Plugin enabled?
is_enabled: true
# Chance that the Alpha Warhead will be replaced with Omega Warhead.
replace_alpha_chance: 15
# Number of engaged generators that guarantees Omega Warhead launch.
generators_num_guarantee_omega: 3
# How much the chance is increased per each activated generator.
generators_increase_chance_by: 15
# Whether players can stop the Omega Warhead.
is_stop_allowed: false
# Should Omega countdown be reset on stop?
reset_omega_on_warhead_stop: false
# Time until Omega detonation (in seconds).
time_to_detonation: 320
# Delay before checkpoint doors open and lock (in seconds).
open_and_lock_checkpoint_doors_delay: 225
# Delay before the helicopter broadcast begins (in seconds).
helicopter_broadcast_delay: 250
# Delay before Omega sequence starts (in seconds).
delay_before_omega_sequence: 5
# Size of the escape zone.
escape_zone_size: 7.75
# Size of the breach shelter zone.
shelter_zone_size: 7.75
# Red channel of Omega room lighting (0.0 - 1.0).
lights_color_r: 0.0500000007
# Green channel of Omega room lighting (0.0 - 1.0).
lights_color_g: 0.850000024
# Blue channel of Omega room lighting (0.0 - 1.0).
lights_color_b: 0.349999994
# Hint message displayed when the evacuation helicopter is inbound to the landing zone.
helicopter_incoming_message: 'Incoming evacuation helicopter!'
# Hint message shown when helicopter escape is successful.
helicopter_escape_message: 'You escaped in the helicopter.'
# Message shown to players who successfully survive Omega Warhead detonation.
survivor_message: |-
  The facility may be gone, but hope lives on through survivors like you. A new dawn awaits.
  <b><color=#00FA9A>Your story of survival will inspire generations.</color></b>
# Message shown to players who successfully evacuate during Omega Warhead detonation.
evacuated_message: |-
  Your escape was swift, but the scars will linger.
  <b><color=#00FA9A>You didn’t just run — you made it through hell to tell the tale.</color></b>
# Message shown to players who die during Omega Warhead detonation.
killed_message: |-
  You became part of the blast that erased a legacy.
  <b><color=#6C1133>The facility consumed you... but whispers of your courage echo in the ruins.</color></b>
# Global ending broadcast showing survival statistics with post-nuclear hope theme.
ending_broadcast: |-
  <size=26><color=#FFA500>--- OMEGA WARHEAD AFTERMATH ---</color></size>
  <b>CASUALTY REPORT</b>

  <color=green> 🌿 Survived: {survived}</color> souls preserved in shelters.
  <color=blue> 🚁 Evacuated: {escaped}</color> heroes airlifted to safety.
  <color=red> 💀 Lost: {dead}</color> absorbed by the nuclear blast

  <b><color=#DAA520>FINAL TRANSMISSION FROM SITE-██</color></b>
  Despite catastrophic losses, <color=#FFD700><b>humanity endures</b></color>.
  The Foundation's work continues at <i>Secondary Locations</i>.
  <size=20>Those who remain will rebuild...</size>

  <size=18><color=#A9A9A9>// Automated System Log #{code} from SITE-██</color></size>;
# Hint message when Omega Warhead is activated.
activated_message: |-
  <b><color=#ff0040>OMEGA WARHEAD ACTIVATED</color></b>
  PLEASE EVACUATE IMMEDIATELY
# Cassie message when Omega is stopped.
stopping_omega_cassie: 'pitch_0.9 Omega Warhead detonation has been stopped'
# Cassie message when Omega is activated.
starting_omega_cassie: 'pitch_0.2 .g3 .g3 .g3 pitch_0.9 attention . attention . activating omega warhead . Please evacuate in the . breach shelter or in the helicopter . please evacuate immediately .'
# Cassie message during detonation.
detonating_omega_cassie: 'pitch_0.65 Detonating pitch_0.5 Warhead'
# Cassie message announcing incoming helicopter.
heli_incoming_cassie: 'pitch_0.25 .g3 .g3 .g3 pitch_0.9 attention . attention . the helicopter is in coming . Please evacuate . Attention . the helicopter is in coming . Please evacuate immediately'
# Cassie message announcing checkpoint unlock.
checkpoint_unlock_cassie: 'pitch_0.25 .g3 .g3 .g3 pitch_0.9 attention . attention . the checkpoint doors are open . Attention . the checkpoint doors are open . Please evacuate immediately'
# Message when Omega is stopped.
stopping_omega_message: 'Omega Warhead detonation has been successfully aborted'
# Message when Omega is activated.
starting_omega_message: 'Attention: Omega Warhead activation sequence initiated. All personnel must evacuate through secure shelters or extraction zones'
# Message during detonation.
detonating_omega_message: 'Omega Warhead detonation in progress. Evacuation is no longer possible'
# Message announcing incoming helicopter.
heli_incoming_message: 'Alert: Extraction helicopter approaching the facility. Proceed to the surface zone immediately'
# Message announcing checkpoint unlock.
checkpoint_unlock_message: 'Facility update: Checkpoint doors are now accessible. Proceed with caution'
# Disable Cassie string messages during message broadcasts?
disable_cassie_messages: true
# Clear Cassie queue before important messages?
cassie_message_clear_before_important: true
# Clear Cassie queue before warhead messages?
cassie_message_clear_before_warhead_message: false
# Permission string required to use Omega Warhead commands.
permissions: 'omegawarhead'
# Enable debug logging.
debug: true

```
