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
        public List<Player> heliSurvivors = new List<Player>();


        public void OnRoundStart()
        {
            _plugin.EventMethods.Init();
        }

        public void OnRoundEnd(RoundEndedEventArgs ev)
        {
            _plugin.EventMethods.Disable();
        }
        public void OnWaitingForPlayers()
        {
            _plugin.EventMethods.Disable();
        }
        public void OnWarheadStart(StartingEventArgs ev)
        {
            if (_plugin.EventMethods.isOmegaActive())
            {
                ev.IsAllowed = _plugin.Config.isStopAllowed;
            }
            else if (!_plugin.EventMethods.isOmegaActive())
            {
                int engagedCount = Generator.List.Count(generator =>
                       (generator.State & GeneratorState.Engaged) == GeneratorState.Engaged);
                if (engagedCount >= _plugin.Config.generatorsNumGuaranteeOmega ||
                    (float)Loader.Random.NextDouble() * 100 < _plugin.Config.ReplaceAlphaChance)
                {
                    float realTimeToDetonation = _plugin.Config.TimeToDetonation;
                    float eventTimeToDetonation = realTimeToDetonation - 0.45f; //- (1.5f * 10.0f);
                    _plugin.EventMethods.ActivateOmegaWarhead(eventTimeToDetonation);

                    Warhead.DetonationTimer = realTimeToDetonation;

                    ev.IsAllowed = _plugin.Config.isStopAllowed;

                }
            }
        }

        public void OnWarheadStop(StoppingEventArgs ev)
        {
            if (_plugin.EventMethods.isOmegaActive() && ev.IsAllowed)
            {
                _plugin.EventMethods.StopOmega();
            }

        }
        public void OnWarheadDetonate(DetonatingEventArgs ev)
        {
            if (_plugin.EventMethods.isOmegaActive()) {

            }
        }
    }
}