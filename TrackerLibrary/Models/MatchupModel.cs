using System.Collections.Generic;

namespace TrackerLibrary.Models
{
    /// <summary>
    /// Represents one match in the tournament.
    /// </summary>
    public class MatchupModel
    {
        /// <summary>
        /// The unique identifier for the matchup.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Which round this match is a part of.
        /// </summary>
        public int MatchupRound { get; set; }

        /// <summary>
        /// The unique identifier from the database that will identify the winner
        /// </summary>
        public int WinnerId { get; set; }

        /// <summary>
        /// The winner of this match
        /// </summary>
        public TeamModel Winner { get; set; }

        /// <summary>
        /// The set of teams that were involved in this match
        /// </summary>
        public List<MatchupEntryModel> Entries { get; set; } = new List<MatchupEntryModel>();

        public string DisplayName
        {
            get
            {
                var output = "";

                foreach (var entry in Entries)
                {
                    if (entry.TeamCompeting != null)
                    {
                        if (output.Length == 0)
                        {
                            output = entry.TeamCompeting.TeamName;
                        }
                        else
                        {
                            output += $" vs. {entry.TeamCompeting.TeamName}";
                        }
                    }
                    else
                    {
                        output = "Matchup not yet determined";
                        break;
                    }
                }

                return output;
            }
        }
    }
}