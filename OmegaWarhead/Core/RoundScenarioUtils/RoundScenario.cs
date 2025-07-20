namespace BetterOmegaWarhead.Core.RoundScenarioUtils
{
    using BetterOmegaWarhead.Core.RoundScenarioUtils;
    public abstract class RoundScenario
    {


        protected RoundController _roundController;

        protected RoundScenario(RoundController roundController)
        {
            _roundController = roundController;
        }

        /// <summary>  
        /// Executes the scenario logic  
        /// </summary>  
        public abstract void Execute();

        /// <summary>  
        /// Called when scenario completes  
        /// </summary>  
        protected virtual void OnComplete()
        {
            Plugin.Singleton.OmegaManager.Cleanup();

        }

    }
}