using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Reflection;
using Dapper;
using DbUp;
using DbUp.Engine;

public static class DatabaseMigrationConfiguration
{
    public static void ConfigureAndMigrate(IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("SqlDb");

        // Ensure the database is created (if not exists) before applying migrations
        EnsureDatabase.For.SqlDatabase(connectionString);
        EnsureMigrationHistoryTableExists(connectionString);

        // Initialize DbUp upgrader
        var upgrader = DeployChanges.To
            .SqlDatabase(connectionString)
            .LogToConsole()
            .WithScripts(GetMigrationScriptsFromDirectory())
            .JournalTo(new DataBaseMigration.CustomJournal(connectionString, "MigrationHistory"))
            .Build();

        // Execute the migrations
        var result = upgrader.PerformUpgrade();

        if (!result.Successful)
        {
            Console.WriteLine("Database migration failed:");
            Console.WriteLine(result.Error);
            ExecuteRollbackScripts(connectionString, result.Scripts);
        }
        else
        {
            Console.WriteLine("Database migration successful!");
            InsertMigrationHistory(connectionString, result.Scripts);
        }
    }
    private static void EnsureMigrationHistoryTableExists(string connectionString)
    {
        using (var connection = new SqlConnection(connectionString))
        {
            connection.Open();
            var sqlCommand = @"
            IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'MigrationHistory')
            BEGIN
                CREATE TABLE MigrationHistory
                (
                    Id INT PRIMARY KEY IDENTITY(1,1),
                    MigrationName NVARCHAR(255) NOT NULL,
                    AppliedOn DATETIME2 NOT NULL
                );
            END";
            connection.Execute(sqlCommand);
        }
    }
    private static IEnumerable<SqlScript> GetMigrationScriptsFromDirectory()
    {
        var migrationDirectory = GetMigrationDirectory();
        return new DirectoryInfo(migrationDirectory)
            .GetDirectories("Migration_*")
            .OrderBy(d => d.Name)
            .SelectMany(GetScriptsFromDirectory)
            .ToList();
    }

    private static string GetMigrationDirectory()
    {
        var projectDirectory = Directory.GetCurrentDirectory();

        return Path.Combine(projectDirectory, "Migrations");
    }

    private static IEnumerable<SqlScript> GetScriptsFromDirectory(DirectoryInfo directory)
    {
        return directory
        .GetFiles("*.sql")
        .OrderBy(f => f.Name)
        .Select(file => new SqlScript(directory.Name, File.ReadAllText(file.FullName)))
        .ToList();
    }
    private static void ExecuteRollbackScripts(string connectionString, IEnumerable<SqlScript> appliedMigrations = null)
    {
        var rollbackScripts = GetRollbackScriptsFromDirectory();
        if (rollbackScripts.Any())
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // Execute the rollback scripts in reverse order
                        foreach (var rollbackScript in rollbackScripts.Reverse())
                        {
                            connection.Execute(rollbackScript.Contents, null, transaction);
                        }

                        transaction.Commit();
                        Console.WriteLine("Rollback successful.");
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        Console.WriteLine("Rollback failed:");
                        Console.WriteLine(ex.Message);
                    }
                }
            }
        }
    }

    private static IEnumerable<SqlScript> GetRollbackScriptsFromDirectory()
    {
        var migrationDirectory = GetMigrationDirectory();
        var migrationFolders = new DirectoryInfo(migrationDirectory)
            .GetDirectories("Migration_*");

        var rollbackScripts = migrationFolders
            .SelectMany(migrationFolder =>
                Directory.GetFiles(migrationFolder.FullName, "Rollback_*.sql")
                    .Select(file => new SqlScript(Path.GetFileName(file), File.ReadAllText(file)))
            )
            .ToList();

        return rollbackScripts;
    }

    private static void InsertMigrationHistory(string connectionString, IEnumerable<SqlScript> appliedMigrations)
    {
        using (var connection = new SqlConnection(connectionString))
        {
            connection.Open();
            using (var transaction = connection.BeginTransaction())
            {
                foreach (var appliedMigration in appliedMigrations)
                {
                    // Check if the migration name already exists in the table
                    var existingMigration = connection.ExecuteScalar<string>(
                        "SELECT MigrationName FROM MigrationHistory WHERE MigrationName = @MigrationName",
                        new { MigrationName = appliedMigration.Name },
                        transaction
                    );

                    if (string.IsNullOrEmpty(existingMigration))
                    {
                        // Insert the migration history into the table
                        connection.Execute(
                            "INSERT INTO MigrationHistory (MigrationName, AppliedOn) VALUES (@MigrationName, @AppliedOn)",
                            new { MigrationName = appliedMigration.Name, AppliedOn = DateTime.UtcNow },
                            transaction
                        );
                    }
                }

                transaction.Commit();
            }
        }
    }

}
