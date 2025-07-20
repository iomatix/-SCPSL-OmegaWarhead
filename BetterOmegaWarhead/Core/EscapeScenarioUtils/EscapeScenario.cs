namespace BetterOmegaWarhead.Core.EscapeScenarioUtils
{

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using UnityEngine;


    /// <summary>  
    /// Represents different escape scenarios and their configurations.  
    /// </summary>  
    public class EscapeScenario
    {
        public string Name { get; set; }
        public Vector3 EscapeZone { get; set; }
        public Vector3 TeleportPosition { get; set; }
        public string InitialMessage { get; set; }
        public string EscapeMessage { get; set; }
        public float InitialDelay { get; set; }
        public float ProcessingDelay { get; set; }
        public float FinalDelay { get; set; }
        public Action OnEscapeTriggered { get; set; }
    }
}
