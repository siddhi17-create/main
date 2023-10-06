namespace DataBaseMigration
{
    using System;
    using System.Data;
    using System.Data.SqlClient;
    using System.Linq;
    using System.Transactions;
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
            return null;
            //using (var connection = new SqlConnection(_connectionString))
            //{
            //    connection.Open();
            //    var scripts = connection.Query<string>(
            //        $"SELECT MigrationName FROM {_journalTableName}"
            //    ).ToArray();
            //    return scripts;
            //}
        }


        public void StoreExecutedScript(SqlScript script, Func<IDbCommand> dbCommandFactory)
        {
            //using (var connection = new SqlConnection(_connectionString))
            //{
            //    connection.Open();
            //    connection.Execute(
            //                 "EXEC InsertMigrationHistory @MigrationName, @AppliedOn",
            //                 new { MigrationName = script.Name, AppliedOn = DateTime.UtcNow }                           
            //             );
            //}
        }
    }

}
