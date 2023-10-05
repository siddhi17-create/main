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
            var executedScriptsFolder = "ExecutedScripts";

            var executedScripts = new List<string>();

            try
            {
                var scriptFiles = Directory.GetFiles(scriptsFolder, "*.sql");

                foreach (var scriptFile in scriptFiles)
                {
                    var executedScriptPath = Path.Combine(executedScriptsFolder, Path.GetFileName(scriptFile));
                   
                    if (executedScripts.Contains(scriptFile) || File.Exists(executedScriptPath))
                    {
                        Console.WriteLine($"Script {scriptFile} has already been executed. Skipping.");
                        continue;
                    }

                    var scriptContent = System.IO.File.ReadAllText(scriptFile);

                    var upgrader = DeployChanges.To
                        .SqlDatabase(connectionString)
                        .WithScripts(new SqlScript(scriptFile, scriptContent))
                        .LogToConsole()
                        .Build();

                    var result = upgrader.PerformUpgrade();

                    if (!result.Successful)
                    {
                      
                        Console.WriteLine($"Script {scriptFile} failed to execute:");
                        Console.WriteLine(result.Error);

                        var rollbackScriptName = $"RollBack_{Path.GetFileName(scriptFile)}";
                   
                        RollbackScript(connectionString, rollbackScriptsFolder, executedScripts, rollbackScriptName);

                        Console.WriteLine($"Rollback for script {scriptFile} completed.");
                    }
                    else
                    {
                        Console.WriteLine($"Script {scriptFile} executed successfully.");
                        executedScripts.Add(scriptFile);
                        File.Move(scriptFile, executedScriptPath);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during migration: {ex.Message}");
            }
        }

        private static void RollbackScript(string connectionString, string rollbackScriptsFolder, List<string> executedScripts, string rollbackScriptName)
        {
            try
            {
              
                    var rollbackScriptPath = Path.Combine(rollbackScriptsFolder, rollbackScriptName);

                    // Read the content of the rollback script
                    var rollbackScriptContent = System.IO.File.ReadAllText(rollbackScriptPath);

                    // Attempt to execute the rollback script
                    var rollbackUpgrader = DeployChanges.To
                        .SqlDatabase(connectionString)
                        .WithScripts(new SqlScript(rollbackScriptName, rollbackScriptContent))
                        .LogToConsole()
                        .Build();

                    var rollbackResult = rollbackUpgrader.PerformUpgrade();

                    if (!rollbackResult.Successful)
                    {
                        Console.WriteLine($"Rollback for script {rollbackScriptName} failed:");
                        Console.WriteLine(rollbackResult.Error);
                    }
                    else
                    {
                        Console.WriteLine($"Rollback for script {rollbackScriptName} successful.");
                    }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during rollback: {ex.Message}");
            }
        }


    }

}
