namespace BetterOmegaWarhead
{
    using System.Collections.Generic;
    using System.Linq;
    using Exiled.API.Enums;
    using Exiled.API.Extensions;
    using Exiled.API.Features;
    using Exiled.Events.EventArgs.Server;
    using Exiled.Events.EventArgs.Warhead;
    using Exiled.Events.EventArgs.Player;
    using Exiled.Loader;
    using MEC;
    using PlayerRoles;

    public class EventHandlers
    {
        private readonly Plugin _plugin;
        public EventHandlers(Plugin plugin) => _plugin = plugin;

        public List<CoroutineHandle> Coroutines = new List<CoroutineHandle>();

        public void OnRoundStart()
        {
            Log.Debug("OnRoundStart triggered, resetting cache and initializing event methods.");
            _plugin.CacheHandlers.ResetCache();
            _plugin.EventMethods.Init();
        }

        public void OnRoundEnd(RoundEndedEventArgs ev)
        {
            Log.Debug($"OnRoundEnd triggered with leading team: {ev.LeadingTeam}. Disabling event methods.");
            _plugin.CacheHandlers.ResetCache();
            _plugin.EventMethods.Disable();
        }

        public void OnWaitingForPlayers()
        {
            Log.Debug("OnWaitingForPlayers triggered, disabling event methods.");
            _plugin.CacheHandlers.ResetCache();
            _plugin.EventMethods.Disable();
        }

        public void OnWarheadStart(StartingEventArgs ev)
        {
            Log.Debug($"OnWarheadStart triggered, Omega active: {_plugin.EventMethods.isOmegaActive()}");
            if (!_plugin.EventMethods.isOmegaActive())
            {
                float realTimeToDetonation = _plugin.Config.TimeToDetonation;
                float eventTimeToDetonation = realTimeToDetonation - 0.45f;
                int engagedCount = Generator.List.Count(generator =>
                       (generator.State & GeneratorState.Engaged) == GeneratorState.Engaged);
                Log.Debug($"Warhead start check: {engagedCount}/{_plugin.Config.generatorsNumGuaranteeOmega} generators engaged, ReplaceAlphaChance: {_plugin.Config.ReplaceAlphaChance}%");

                if (engagedCount >= _plugin.Config.generatorsNumGuaranteeOmega ||
                    (float)Loader.Random.NextDouble() * 100 < _plugin.Config.ReplaceAlphaChance)
                {
                    Log.Debug($"Activating Omega Warhead with detonation time: {eventTimeToDetonation}s");
                    ev.IsAllowed = true;
                    _plugin.EventMethods.ActivateOmegaWarhead(eventTimeToDetonation);
                    _plugin.EventHandlers.Coroutines.Add(Timing.RunCoroutine(DelayedWarheadActivation(realTimeToDetonation, 0.75f)));
                }
                else
                {
                    Log.Debug("Omega Warhead not activated: conditions not met.");
                }
            }
            else
            {
                Log.Debug("Omega Warhead already active, skipping activation.");
            }
        }

        public IEnumerator<float> DelayedWarheadActivation(float timeToDetonation, float delay)
        {
            Log.Debug($"Starting DelayedWarheadActivation with detonation time: {timeToDetonation}s, delay: {delay}s");
            yield return Timing.WaitForSeconds(delay);
            Log.Debug("Activating warhead: setting status to Armed and updating timer.");
            Warhead.Status = WarheadStatus.Armed;
            Warhead.DetonationTimer = timeToDetonation - delay;
            Warhead.Controller.IsLocked = !Plugin.Singleton.Config.isStopAllowed;
            Log.Debug($"Warhead armed, timer set to {Warhead.DetonationTimer}s, locked: {Warhead.Controller.IsLocked}");
        }

        public void OnWarheadStop(StoppingEventArgs ev)
        {
            Log.Debug($"OnWarheadStop triggered, Omega active: {_plugin.EventMethods.isOmegaActive()}, stop allowed: {_plugin.Config.isStopAllowed}");
            if (_plugin.EventMethods.isOmegaActive() && _plugin.Config.isStopAllowed)
            {
                Log.Debug("Stopping Omega Warhead and resetting warhead status.");
                _plugin.EventMethods.StopOmega();
                Warhead.Status = WarheadStatus.NotArmed;
                Warhead.Controller.IsLocked = false;
            }
            else
            {
                Log.Debug("Warhead stop skipped: Omega not active or stopping not allowed.");
            }
        }

        public void OnWarheadDetonate(DetonatingEventArgs ev)
        {
            Log.Debug($"OnWarheadDetonate triggered, Omega active: {_plugin.EventMethods.isOmegaActive()}");
            if (_plugin.EventMethods.isOmegaActive())
            {
                Log.Debug("Omega Warhead detonating.");
            }
        }

        public void OnChangingRole(ChangingRoleEventArgs ev)
        {
            Faction faction = ev.NewRole.GetFaction();
            Log.Debug($"OnChangingRole triggered for {ev.Player.Nickname}, new role: {ev.NewRole}, faction: {faction}");
            if (_plugin.CacheHandlers.IsFactionDisabled(faction))
            {
                Log.Debug($"Blocking role assignment for {ev.Player.Nickname} due to disabled faction: {faction}");
                ev.IsAllowed = false;
            }
        }
    }
}