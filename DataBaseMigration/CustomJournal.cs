namespace DataBaseMigration
{
    using System;
    using System.Data;
    using System.Data.SqlClient;
    using System.Linq;
    using Dapper;
    using DbUp;
    using DbUp.Engine;
    using DbUp.Engine.Transactions;
    using DbUp.SqlServer;

    public class CustomJournal : IJournal
    {
        private readonly string _connectionString;
        private readonly string _journalTableName;

        public CustomJournal(string connectionString, string journalTableName)
        {
            _connectionString = connectionString;
            _journalTableName = journalTableName;
        }

        public void EnsureTableExistsAndIsLatestVersion(Func<IDbCommand> dbCommandFactory)
        {
            
        }

        public string[] GetExecutedScripts()
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var scripts = connection.Query<string>(
                    $"SELECT MigrationName FROM {_journalTableName}"
                ).ToArray();
                return scripts;
            }
        }

        public void StoreExecutedScript(SqlScript script)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                connection.Execute(
                    $"INSERT INTO {_journalTableName} (MigrationName, AppliedOn) VALUES (@MigrationName, @AppliedOn)",
                    new { ScriptName = script.Name, AppliedOn = DateTime.UtcNow }
                );
            }
        }

        public void StoreExecutedScript(SqlScript script, Func<IDbCommand> dbCommandFactory)
        {

        }
    }

}
