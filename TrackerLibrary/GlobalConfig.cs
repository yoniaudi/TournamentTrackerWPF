using System.Configuration;
using TrackerLibrary.DataAccess;

namespace TrackerLibrary
{
    public static class GlobalConfig
    {
        public const string PrizesFileName = "PrizeModels.csv";
        public const string PeopleFileName = "PersonModels.csv";
        public const string TeamsFileName = "TeamModels.csv";
        public const string TournamentsFileName = "TournamentsModels.csv";
        public const string MatchupFileName = "MatchupModels.csv";
        public const string MatchupEntryFileName = "MatchupEntryModels.csv";

        public static IDataConnection Connection { get; private set; }

        public static void InitializeConnections(DatabaseType db)
        {
            if (db == DatabaseType.Sql)
            {
                var sql = new SqlConnector();
                Connection = sql;
            }
            else if (db == DatabaseType.TextFile)
            {
                var text = new TextConnector();
                Connection = text;
            }
        }

        public static string CnnString(string name)
        {
            return ConfigurationManager.ConnectionStrings[name].ConnectionString;
        }

        public static string AppKeyLookup(string key)
        {
            return ConfigurationManager.AppSettings[key];
        }
    }
}
