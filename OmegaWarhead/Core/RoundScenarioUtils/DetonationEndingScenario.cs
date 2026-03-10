namespace OmegaWarhead.Core.RoundScenarioUtils
{
    using OmegaWarhead.Core.LoggingUtils;
    using OmegaWarhead.NotificationUtils;
    using LabApi.Features.Wrappers;
    using MEC;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using OmegaWarhead.Shared;

    public class DetonationEndingScenario : RoundScenario
    {
        public DetonationEndingScenario(RoundController roundController) : base(roundController) { }

        public override void Execute()
        {
            // Warhead detonation  
            ExecuteWarheadDetonation();

            // Facility effects  
            ExecuteFacilityEffects();

            // Light sequence  
            ExecuteLightSequence();

            // Final cleanup and round end  
            ScheduleRoundEnd();
        }

        private void ExecuteWarheadDetonation()
        {
            Warhead.Shake();

            Plugin.Singleton.PlayerMethods.HandlePlayersOnNuke();
        }

        private void ExecuteFacilityEffects()
        {
            foreach (Room room in Room.List)
            {
                foreach (var door in room.Doors)
                {
                    if (door is BreakableDoor breakable && !breakable.IsBroken)
                        breakable.TryBreak();
                }

                foreach (var lightController in room.AllLightControllers)
                {
                    lightController.LightsEnabled = false;
                }
            }
        }

        private void ExecuteLightSequence()
        {
            for (float t = 0f; t < 20f; t += 2f)
            {
                float delay = t;
                var coroutine = Timing.CallDelayed(delay, () =>
                {
                    Map.SetColorOfLights(UnityEngine.Color.red);

                    var coroutine_d1 = Timing.CallDelayed(0.5f, () => Map.TurnOffLights());
                    coroutine_d1.Tag = "Omega-Scenario";

                    var couroutine_d2 = Timing.CallDelayed(1.27f, () =>
                    {
                        Map.TurnOnLights();
                        Map.SetColorOfLights(UnityEngine.Color.black);
                    });
                    couroutine_d2.Tag = "Omega-Scenario";
                });
                coroutine.Tag = CoroutineTags.Scenario;
            }
        }

        private void ScheduleRoundEnd()
        {
            var coroutine = Timing.CallDelayed(30f, () =>
            {
                Map.TurnOnLights();
                Map.SetColorOfLights(UnityEngine.Color.gray);
                Plugin.Singleton.PlayerMethods.HandleEndingByFate();
            });
            coroutine.Tag = CoroutineTags.Scenario;

            _roundController.EndRoundGracefully(60f);
        }

        protected override void OnComplete()
        {
            LogHelper.Info("Detonation ending scenario completed successfully");


            base.OnComplete();
        }
    }
}
