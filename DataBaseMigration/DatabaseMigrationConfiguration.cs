using DbUp.Engine;

namespace DataBaseMigration
{

    using System;
    using DbUp;
    using Microsoft.Extensions.Configuration;

    public static class DatabaseMigrationConfiguration
    {
        public static void ConfigureAndMigrate(IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("SqlDb"); 
            var scriptsFolder =  "DataBaseScripts";
            var rollbackScriptsFolder = "RollBackScripts";

            var executedScripts = new List<string>();

            try
            {
                // Get a list of script files in the migration folder
                var scriptFiles = Directory.GetFiles(scriptsFolder, "*.sql");

                foreach (var scriptFile in scriptFiles)
                {
                    var scriptContent = System.IO.File.ReadAllText(scriptFile);
                    // Attempt to execute the migration script
                    var upgrader = DeployChanges.To
                        .SqlDatabase(connectionString)
                        .WithScripts(new SqlScript(scriptFile, scriptContent))
                        .LogToConsole()
                        .Build();

                    var result = upgrader.PerformUpgrade();

                    if (!result.Successful)
                    {
                        // Handle the migration failure
                        Console.WriteLine($"Script {scriptFile} failed to execute:");
                        Console.WriteLine(result.Error);

                        // Rollback the failed script
                        RollbackScript(connectionString, rollbackScriptsFolder, executedScripts, scriptFile);

                        Console.WriteLine($"Rollback for script {scriptFile} completed.");
                    }
                    else
                    {
                        Console.WriteLine($"Script {scriptFile} executed successfully.");
                        executedScripts.Add(scriptFile);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during migration: {ex.Message}");
            }
        }

        private static void RollbackScript(string connectionString, string rollbackScriptsFolder, List<string> executedScripts, string failedScript)
        {
            try
            {
                // Check if the failed script was previously executed
                if (executedScripts.Contains(failedScript))
                {
                    // Construct the path to the rollback script
                    var rollbackScriptName = $"RollBack_{Path.GetFileName(failedScript)}";
                    var rollbackScriptPath = Path.Combine(rollbackScriptsFolder, rollbackScriptName);

                    if (File.Exists(rollbackScriptPath))
                    {
                        // Attempt to execute the corresponding rollback script
                        var rollbackUpgrader = DeployChanges.To
                            .SqlDatabase(connectionString)
                            .WithScriptsFromFileSystem(rollbackScriptPath)
                            .LogToConsole()
                            .Build();

                        var rollbackResult = rollbackUpgrader.PerformUpgrade();

                        if (!rollbackResult.Successful)
                        {
                            Console.WriteLine($"Rollback for script {failedScript} failed:");
                            Console.WriteLine(rollbackResult.Error);
                        }
                        else
                        {
                            Console.WriteLine($"Rollback for script {failedScript} successful.");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Rollback script {rollbackScriptPath} not found.");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during rollback: {ex.Message}");
            }
        }

    }

}
