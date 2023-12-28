using System.Collections.Generic;

namespace TrackerLibrary.Models
{
    public class TournamentModel
    {
        public EventHandler<DateTime> OnTournamentComplete;

        /// <summary>
        /// The unique identifier for the tournament.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// The name given to this tournament.
        /// </summary>
        public string TournamentName { get; set; }

        /// <summary>
        /// The entry fee each team has to pay in order to participate in this tournament.
        /// </summary>
        public decimal EntryFee { get; set; }

        /// <summary>
        /// The teams that are participating in this tournament.
        /// </summary>
        public List<TeamModel> EnteredTeams { get; set; } = new List<TeamModel>();

        /// <summary>
        /// The list of prizes for this tournament.
        /// </summary>
        public List<PrizeModel> Prizes { get; set; } = new List<PrizeModel>();

        /// <summary>
        /// The number of matches per round in this tournament.
        /// </summary>
        public List<List<MatchupModel>> Rounds { get; set; } = new List<List<MatchupModel>>();

        public void CompleteTournament()
        {
            OnTournamentComplete?.Invoke(this, DateTime.Now);
        }
    }
}
