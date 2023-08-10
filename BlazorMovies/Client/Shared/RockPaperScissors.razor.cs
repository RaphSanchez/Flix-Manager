using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlazorMovies.Client.Shared
{
    public partial class RockPaperScissors
    {
        /// <summary>
        /// Options to choose by the player (or consumer) of the
        /// RockPaperScissors component. 
        /// </summary>
        public enum Options
        {
            Rock, Paper, Scissors
        }

        /// <summary>
        /// Possible results of a hand of Rock, Paper, Scissors.
        /// </summary>
        public enum GameResult
        {
            Victory, Loss, Draw
        }

        /// <summary>
        /// Model can represent each available option. Both the system 
        /// and the user can choose from three available options.
        /// </summary>
        public class Hand
        {
            /// <summary>
            /// One of the three available options (Rock, Paper,
            /// Scissors).
            /// </summary>
            public Options OptionSelected { get; set; }

            /// <summary>
            /// Stores the option to which the OptionSelected
            /// wins against.
            /// </summary>
            public Options WinsAgainst { get; set; }

            /// <summary>
            /// Stores the option to which the OptionSelected
            /// loses against.
            /// </summary>
            public Options LosesAgainst { get; set; }

            /// <summary>
            /// The path where the image for the OptionSelected resides.
            /// </summary>
            public string? ImagePath { get; set; }

            /// <summary>
            /// Evaluates a condition with the OptionSelected property of
            /// the current Hand object with the OptionSelected property of
            /// the Hand object passed to satisfy its formal input parameter. 
            /// </summary>
            /// <param name="systemHand">The Hand object that the System is
            /// holding at any given time.</param>
            /// <returns>One of the three options contained in the GameResult
            /// enumeration: Victory, Loss, or Draw.</returns>
            public GameResult GetGameResult(Hand systemHand)
            {
                if (OptionSelected == systemHand.OptionSelected)
                {
                    return GameResult.Draw;
                }

                if (LosesAgainst == systemHand.OptionSelected)
                {
                    return GameResult.Loss;
                }

                return GameResult.Victory;
            }
        }
    }
}

