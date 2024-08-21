namespace BetterOmegaWarhead
{
    using System.Collections.Generic;
    using Exiled.API.Features;
    using Exiled.CustomModules.API.Enums;
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
            _plugin.Methods.Init();
        }

        public void OnRoundEnd(RoundEndedEventArgs ev)
        {
            foreach (CoroutineHandle handle in Coroutines) Timing.KillCoroutines(handle);

            Coroutines.Clear();
            _plugin.Methods.Disable();
        }
        public void OnWaitingForPlayers()
        {

            foreach (CoroutineHandle handle in Coroutines) Timing.KillCoroutines(handle);
            heliSurvivors.Clear();
            Coroutines.Clear();
        }
        public void OnWarheadStart(StartingEventArgs ev)
        {
            if (_plugin.Methods.isOmegaActivated())
            {
                ev.IsAllowed = _plugin.Config.isStopAllowed;
            }
            else if (!_plugin.Methods.isOmegaActivated() && (float)Loader.Random.NextDouble() * 100 < _plugin.Config.ReplaceAlphaChance)
            {
                ev.IsAllowed = _plugin.Config.isStopAllowed;
                Warhead.Controller.CurScenario.TimeToDetonate = (int)_plugin.Config.TimeToDetonation;
                _plugin.Methods.ActivateOmegaWarhead();
            }

        }

        public void OnWarheadStop(StartingEventArgs ev)
        {
            if (_plugin.Methods.isOmegaActivated() && ev.IsAllowed)
            {
                _plugin.Methods.StopOmega();
            }

        }
        public void OnWarheadDetonate(StartingEventArgs ev)
        {

        }
    }
}