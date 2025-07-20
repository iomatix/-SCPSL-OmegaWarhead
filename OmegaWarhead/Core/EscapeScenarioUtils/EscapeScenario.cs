namespace OmegaWarhead.Core.EscapeScenarioUtils
{
    using System;
    using UnityEngine;

    /// <summary>  
    /// Represents the configuration for a specific escape scenario, including messages, timing, and positions.  
    /// </summary>  
    #region EscapeScenario Class
    public class EscapeScenario
    {
        #region Identification
        /// <summary>
        /// Gets or sets the internal name of the escape scenario.
        /// </summary>
        /// <value>The display or identifier name of the scenario.</value>
        public string Name { get; set; }
        #endregion

        #region Positioning
        /// <summary>
        /// Gets or sets the world-space location that defines the escape zone.
        /// </summary>
        /// <value>A <see cref="Vector3"/> representing the position where the escape trigger occurs.</value>
        public Vector3 EscapeZone { get; set; }

        /// <summary>
        /// Gets or sets the position to teleport a player or object to upon triggering the escape.
        /// </summary>
        /// <value>A <see cref="Vector3"/> representing the teleport destination.</value>
        public Vector3 TeleportPosition { get; set; }
        #endregion

        #region Messaging
        /// <summary>
        /// Gets or sets the message to broadcast or show at the start of the escape sequence.
        /// </summary>
        /// <value>A string message shown before processing the escape.</value>
        public string InitialMessage { get; set; }

        /// <summary>
        /// Gets or sets the message to display or broadcast when the escape event is completed.
        /// </summary>
        /// <value>A string shown upon successful escape.</value>
        public string EscapeMessage { get; set; }
        #endregion

        #region Timing
        /// <summary>
        /// Gets or sets the delay in seconds before processing the escape logic after the scenario is triggered.
        /// </summary>
        /// <value>Time in seconds to wait before beginning escape handling.</value>
        public float InitialDelay { get; set; }

        /// <summary>
        /// Gets or sets the delay in seconds to wait during processing of the escape event.
        /// </summary>
        /// <value>Time in seconds to simulate or delay escape operations.</value>
        public float ProcessingDelay { get; set; }

        /// <summary>
        /// Gets or sets the final delay in seconds after the escape logic completes.
        /// </summary>
        /// <value>Time in seconds to wait before finalizing the scenario.</value>
        public float FinalDelay { get; set; }
        #endregion

        #region Actions
        /// <summary>
        /// Gets or sets the action to invoke when the escape is triggered.
        /// </summary>
        /// <value>An <see cref="Action"/> that is executed upon escape trigger.</value>
        public Action OnEscapeTriggered { get; set; }
        #endregion
    }
    #endregion
}