namespace OmegaWarhead.Core.RoundScenarioUtils
{
    using OmegaWarhead.Core.LoggingUtils;
    using LabApi.Features.Wrappers;
    using MEC;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    public class RoundController
    {
        private bool _autoRoundEndLocked = false;
        private Plugin _plugin;
        private readonly HashSet<RoundScenario> _endingScenarios;
        private DetonationEndingScenario _detonationScenario;

        /// <summary>
        /// Initializes a new instance of the RoundController class.
        /// </summary>
        public RoundController(Plugin plugin)
        {
            _plugin = plugin;
            _detonationScenario = new DetonationEndingScenario(this);

            _endingScenarios = new HashSet<RoundScenario>();
            _endingScenarios.Add(_detonationScenario);
        }

        /// <summary>  
        /// Gets whether the round end is currently locked  
        /// </summary>  
        public bool IsRoundEndLocked => _autoRoundEndLocked;

        /// <summary>
        /// Gets all registered ending scenarios.
        /// </summary>
        public IEnumerable<RoundScenario> EndingScenarios => _endingScenarios;

        /// <summary>  
        /// Locks or unlocks automatic round ending to allow custom round management  
        /// </summary>  
        public void SetAutoRoundEndLock(bool locked)
        {
            _autoRoundEndLocked = locked;
            Round.IsLocked = locked; // Uses LabAPI's round locking  
        }

        /// <summary>  
        /// Executes a specific round scenario  
        /// </summary>  
        public void ExecuteScenario(RoundScenario scenario)
        {
            SetAutoRoundEndLock(true);
            scenario.Execute();
        }

        /// <summary>  
        /// Gracefully ends the round with proper cleanup  
        /// </summary>  
        public void EndRoundGracefully(float delay = 0f)
        {
            Timing.CallDelayed(delay, () =>
            {
                SetAutoRoundEndLock(false);
                Round.End(force: true);
            });
        }

        public T GetScenario<T>() where T : RoundScenario
        {
            return _endingScenarios.OfType<T>().FirstOrDefault();
        }
    }
}
