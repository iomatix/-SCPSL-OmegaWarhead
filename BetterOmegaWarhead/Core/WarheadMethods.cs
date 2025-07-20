namespace BetterOmegaWarhead
{
    using BetterOmegaWarhead.NotificationUtils;
    using BetterOmegaWarhead.Core.LoggingUtils;
    using LabApi.Features.Wrappers;
    using MEC;
    using System;
    using System.Linq;
    using UnityEngine;

    public class WarheadMethods
    {
        private readonly Plugin _plugin;
        private readonly OmegaWarheadManager _omegaWarheadManager;

        public WarheadMethods(Plugin plugin)
        {
            _plugin = plugin;
            _omegaWarheadManager = _plugin.OmegaManager;
        }

        public void StartSequence(float timeToDetonation)
        {
            LogHelper.Debug("Starting Omega Warhead sequence chain...");
            Activate(timeToDetonation);
        }

        public void Activate(float timeToDetonation)
        {
            LogHelper.Debug($"Activating Omega Warhead with detonation time: {timeToDetonation}s.");
            Color lightColor = new Color(_plugin.Config.LightsColorR, _plugin.Config.LightsColorG, _plugin.Config.LightsColorB);
            LogHelper.Debug($"Changing room lights to color: R={lightColor.r}, G={lightColor.g}, B={lightColor.b}");
            Map.SetColorOfLights(lightColor);

            NotificationUtility.SendImportantCassieMessage(_plugin.Config.StartingOmegaCassie);
            NotificationUtility.BroadcastOmegaActivation();

            float messageDurationAdjustment = NotificationUtility.CalculateTotalMessagesDurations(timeToDetonation);
            LogHelper.Debug($"Adjusting timeToDetonation by {messageDurationAdjustment}s for Cassie messages.");
            float adjustedTime = timeToDetonation - messageDurationAdjustment;
            _omegaWarheadManager.AddCoroutines(
                Timing.RunCoroutine(_plugin.OmegaManager.HandleCountdown(adjustedTime), "OmegaCountdown"),
                Timing.RunCoroutine(_plugin.OmegaManager.HandleHelicopter(), "OmegaHeli"),
                Timing.RunCoroutine(_plugin.OmegaManager.HandleCheckpointDoors(), "OmegaCheckpoints")
            );
            Warhead.DetonationTime = timeToDetonation; // activate synced Alpha
        }

        public void StopSequence()
        {
            LogHelper.Debug("Stopping Omega Warhead sequence.");

            NotificationUtility.SendImportantCassieMessage(Plugin.Singleton.Config.StoppingOmegaCassie);
            _omegaWarheadManager.Cleanup();
        }

        public void ResetSequence()
        {
            LogHelper.Debug("Resetting Omega Warhead state.");
            _omegaWarheadManager.Cleanup();
        }

        public static void DetonateSequence()
        {
            // Alpha warhead detonation  
            Warhead.Detonate();
            Warhead.Shake();

            // Facility-wide door effects  
            foreach (Room room in Room.List)
            {
                foreach (LabApi.Features.Wrappers.Door door in room.Doors)
                {
                    door.IsOpened = true;
                    if (door is LabApi.Features.Wrappers.BreakableDoor breakable && !breakable.IsBroken)
                        breakable.TryBreak();
                    door.Lock(Interactables.Interobjects.DoorUtils.DoorLockReason.Warhead, true);
                }

                // Turn off all lights  
                foreach (var lightController in room.AllLightControllers)
                {
                    lightController.LightsEnabled = false;
                }
            }
        }

    }
}
