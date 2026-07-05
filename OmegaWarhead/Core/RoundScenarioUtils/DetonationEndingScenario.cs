using LabApi.Extensions;
using LabApi.Extensions.RoundManagement;
using LabApi.Features.Wrappers;
using OmegaWarhead.Shared;
using UnityEngine;
using Logger = LabApi.Extensions.Misc.iLogger;

namespace OmegaWarhead.Core.RoundScenarioUtils
{
    /// <summary>
    /// Specific feature-content scenario handling the site vaporization aftermath sequence.
    /// Inherits cleanly from the generic native framework base scenario class.
    /// </summary>
    public class DetonationEndingScenario : RoundScenario
    {
        public DetonationEndingScenario(RoundController roundController) : base(roundController) { }

        public override void Execute()
        {
            ExecuteWarheadDetonation();
            ExecuteFacilityEffects();
            ExecuteLightSequence();
            ScheduleRoundEnd();
        }

        private static void ExecuteWarheadDetonation()
        {
            Warhead.Shake();
            Plugin.Singleton.PlayerMethods?.HandlePlayersOnNuke();
        }

        private static void ExecuteFacilityEffects()
        {
            // Utilizing global batch commands straight out of the shared API assembly
            MapExtensions.BreakAllFacilityDoors();
            MapExtensions.SetAllLightsEnabled(false);
        }

        private void ExecuteLightSequence()
        {
            // Building the red strobe timeline cleanly via the shared framework ActionChain engine
            var strobeChain = this.CreateChain();

            for (int i = 0; i < 10; i++)
            {
                strobeChain.Then(_ => Room.List.SetLightsColor(Color.red))
                           .Wait(0.5f)
                           .Then(_ => MapExtensions.SetAllLightsEnabled(false))
                           .Wait(0.77f)
                           .Then(_ =>
                           {
                               MapExtensions.SetAllLightsEnabled(true);
                               Room.List.SetLightsColor(Color.black);
                           })
                           .Wait(0.73f);
            }

            strobeChain.Run(CoroutineTags.Scenario);
        }

        private void ScheduleRoundEnd()
        {
            this.CreateChain()
                .Wait(30f)
                .Then(_ =>
                {
                    MapExtensions.SetAllLightsEnabled(true);
                    Room.List.SetLightsColor(Color.gray);
                    Plugin.Singleton.PlayerMethods?.HandleEndingByFate();
                })
                .Run(CoroutineTags.Scenario);

            // Accessing the native framework controller's delayed termination loop
            Controller.EndRoundGracefully(60.0f, coroutineTag: CoroutineTags.Scenario);
        }

        protected override void OnComplete()
        {
            Logger.Info(Plugin.Singleton.Name, "Detonation ending scenario completed successfully.");
            base.OnComplete();
        }
    }
}