namespace BetterOmegaWarhead
{
    using System.Collections.Generic;
    using System.Linq;
    using Exiled.API.Enums;
    using Exiled.API.Features;
    using Exiled.Events.EventArgs.Server;
    using Exiled.Events.EventArgs.Warhead;
    using Exiled.Loader;
    using MEC;

    public class EventHandlers
    {
        private readonly Plugin _plugin;
        public EventHandlers(Plugin plugin) => _plugin = plugin;

        public List<CoroutineHandle> Coroutines = new List<CoroutineHandle>();


        public void OnRoundStart()
        {
            _plugin.EventMethods.Init();
            _plugin.CacheShelterLocations();
        }

        public void OnRoundEnd(RoundEndedEventArgs ev)
        {
            _plugin.ResetCache();
            _plugin.EventMethods.Disable();
        }
        public void OnWaitingForPlayers()
        {
            _plugin.EventMethods.Disable();
        }
        public void OnWarheadStart(StartingEventArgs ev)
        {

            if (!_plugin.EventMethods.isOmegaActive())
            {
                float realTimeToDetonation = _plugin.Config.TimeToDetonation;
                float eventTimeToDetonation = realTimeToDetonation - 0.45f; //- (1.5f * 10.0f);

                int engagedCount = Generator.List.Count(generator =>
                       (generator.State & GeneratorState.Engaged) == GeneratorState.Engaged);
                if (engagedCount >= _plugin.Config.generatorsNumGuaranteeOmega ||
                    (float)Loader.Random.NextDouble() * 100 < _plugin.Config.ReplaceAlphaChance)
                {

                    ev.IsAllowed = true;
                    _plugin.EventMethods.ActivateOmegaWarhead(eventTimeToDetonation);
                    // The event is called before starting warhead, so we need to delay our activation to overwrite the main event
                    _plugin.EventHandlers.Coroutines.Add(Timing.RunCoroutine(DelayedWarheadActivation(realTimeToDetonation, 0.75f)));
                }
            }
        }

        public IEnumerator<float> DelayedWarheadActivation(float timeToDetonation, float delay)
        {
            yield return Timing.WaitForSeconds(delay);
            Warhead.Status = WarheadStatus.Armed;
            Warhead.DetonationTimer = timeToDetonation - delay;
            Warhead.Controller.IsLocked = !Plugin.Singleton.Config.isStopAllowed;
        }

        public void OnWarheadStop(StoppingEventArgs ev)
        {
            if (_plugin.EventMethods.isOmegaActive() && _plugin.Config.isStopAllowed)
            {
                _plugin.EventMethods.StopOmega();

                Warhead.Status = WarheadStatus.NotArmed;
                Warhead.Controller.IsLocked = false;
            }

        }
        public void OnWarheadDetonate(DetonatingEventArgs ev)
        {
            if (_plugin.EventMethods.isOmegaActive())
            {

            }
        }
    }
}