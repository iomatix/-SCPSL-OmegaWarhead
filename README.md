# BetterOmegaWarhead

![GitHub release (latest by date)](https://img.shields.io/github/downloads/iomatix/-SCPSL-BetterOmegaWarhead/6.5.0/total?style=for-the-badge)

This plugins adds a new Warhead to SCP:SL. It can be activated replacing Alpha Warhead with the Omega Warhead (deactivated by default) or via commands (activateomegawarhead). The ways to survive the Omega Warhead are 2:
1. Stay in the Breach/Evacuation Shelter
2. Escape in the rescue Helicopter at surface zone.


### Contributors

<a href="https://github.com/iomatix/-SCPSL-BetterOmegaWarhead/graphs/contributors">
  <img src="https://contrib.rocks/image?repo=iomatix/-SCPSL-BetterOmegaWarhead" />
</a>

#### Config:
```
# Plugin enabled?
is_enabled: true
# Amount of engaged generators guarantee 100% chance for the launch of the Omega Warhead.
generators_num_guarantee_omega: 3
# Chance that the Alpha Warhead will be replaced with Omega Warhead
replace_alpha_chance: 15
# Time to the Omega Warhead detonation in seconds.
time_to_detonation: 360
# Time to the open and lock checkpoint doors after Omega activation.
open_and_lock_checkpoint_doors_delay: 215
# Time to the rescue helicopter after Omega activation.
helicopter_broadcast_delay: 255
# Size of the rescue zone.
helicopter_zone_size: 7.75
# Indicates that the player can stop OmegaWarhead after activation.
is_stop_allowed: false
# Red channel of the lights color in the rooms during OmegaWarhead event
lights_color_r: 0.0500000007
# Green channel of the lights color in the rooms during OmegaWarhead event
lights_color_g: 0.850000024
# Blue channel of the lights color in the rooms during OmegaWarhead event
lights_color_b: 0.349999994
# Broadcast that will appear when the player escapes in the helicopter.
helicopter_escape:
# The broadcast content
  content: 'You escaped in the helicopter.'
  # The broadcast duration
  duration: 10
  # The broadcast type
  type: Normal
  # Indicates whether the broadcast should be shown
  show: true
# Broadcast that will appear when the Omega Warhead is activated.
activated_message:
# The broadcast content
  content: |-
    <b><color=red>OMEGA WARHEAD ACTIVATED</color></b>
    PLEASE EVACUATE IMMEDIATELY
  # The broadcast duration
  duration: 10
  # The broadcast type
  type: Normal
  # Indicates whether the broadcast should be shown
  show: true
# Cassie message when Omega Warhead is stopped
stopping_omega_cassie: 'pitch_0.9 Omega Warhead detonation has been stopped'
# Cassie message of Omega Warhead activation
starting_omega_cassie: 'pitch_0.2 .g3 .g3 .g3 pitch_0.9 attention . attention . activating omega warhead . Please evacuate in the . breach shelter or in the helicopter . please evacuate immediately .'
# Cassie message of Omega Warhead detonation
detonating_omega_cassie: 'pitch_0.65 Detonating pitch_0.5 Warhead'
# Cassie message regarding the incoming Helicopter
heli_incoming_cassie: 'pitch_0.25 .g3 .g3 .g3 pitch_0.9 attention . attention . the helicopter is in comeing . Please evacuate . Attention . the helicopter is in comeing . Please evacuate immediately'
# Cassie message regarding the checkpoints unlock
checkpoint_unlock_cassie: 'pitch_0.25 .g3 .g3 .g3 pitch_0.9 attention . attention . the checkpoint doors are open . Attention . the checkpoint doors are open . Please evacuate immediately'
# Permissions of the plugin.
permissions: 'omegawarhead'
# Debug enabled?
debug: false

```
