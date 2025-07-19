namespace BetterOmegaWarhead
{
    using BetterOmegaWarhead.Core.LoggingUtils;
    using LabApi.Features.Wrappers;
    using MEC;
    using System;
    using System.Linq;

    public class WarheadEventMethods : IDisposable
    {
        private readonly Plugin _plugin;
        private readonly OmegaWarheadManager _omegaWarheadManager;

        public bool isOmegaActive => _plugin.OmegaManager.IsOmegaActive;

        public WarheadEventMethods(Plugin plugin)
        {
            _plugin = plugin;
            _omegaWarheadManager = new OmegaWarheadManager(plugin);
        }


        public void OnWarheadStart(LabApi.Events.Arguments.WarheadEvents.WarheadStartingEventArgs ev)
        {
            if (!ev.IsAllowed || !ev.IsAutomatic) // TODO: !ev.IsDetonationLocked 
                return;

            int activeGenerators = Generator.List.Count(g => g.Engaged);
            LogHelper.Debug($"Active Generators: {activeGenerators} (Required: {_plugin.Config.GeneratorsNumGuaranteeOmega})");

            if (activeGenerators < _plugin.Config.GeneratorsNumGuaranteeOmega)
            {
                var chance = _plugin.Config.ReplaceAlphaChance;
                if (chance < 100 && UnityEngine.Random.Range(0, 100) >= chance)
                {
                    LogHelper.Debug($"Omega replacement skipped due to random chance roll. (Chance: {chance}%)");
                    return;
                }
            }
            else
            {
                LogHelper.Debug("Omega forced by generator threshold.");
            }

            LogHelper.Debug("WarheadStart triggered. Initiating Omega sequence...");

            Timing.CallDelayed(_plugin.Config.DelayBeforeOmegaSequence, () =>
            {
                _omegaWarheadManager.StartOmegaSequence(_plugin.Config.TimeToDetonation);
            });
        }

        public void OnWarheadStop(LabApi.Events.Arguments.WarheadEvents.WarheadStoppingEventArgs ev)
        {
            if (!ev.IsAllowed)
                return;

            if (_plugin.Config.ResetOmegaOnWarheadStop && Warhead.IsDetonated)
            {
                LogHelper.Debug("WarheadStop triggered. Resetting Omega sequence...");
                _omegaWarheadManager.ResetOmegaState();
            }
        }

        public void OnWarheadDetonate()
        {
            LogHelper.Debug("WarheadDetonate triggered. Executing post-detonation effects...");
            _omegaWarheadManager.OnDetonation();
        }

        public void OnWaitingForPlayers()
        {
            LogHelper.Debug("WaitingForPlayers triggered. Cleaning up from previous round.");
            _omegaWarheadManager.OnRoundStartCleanup();
        }

        public void Dispose()
        {
            LogHelper.Debug("Disposing WarheadEventMethods and stopping all Omega-related coroutines.");
            _omegaWarheadManager.Dispose();
        }

        public void ActivateOmegaWarhead(float timeToDetonation)
        {
            _plugin.OmegaManager.StartOmegaSequence(timeToDetonation);
        }




    }
}
