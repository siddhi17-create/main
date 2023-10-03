namespace DataBaseMigration
{

    using System;
    using DbUp;
    using Microsoft.Extensions.Configuration;

    public static class DatabaseMigrationConfiguration
    {
        public static void ConfigureAndMigrate(IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("SqlDb"); ;
            var scriptsFolder = "DataBaseScripts";

            var upgrader = DeployChanges.To
                .SqlDatabase(connectionString)
                .WithScriptsFromFileSystem(scriptsFolder)
                .LogToConsole()
                .Build();

            var result = upgrader.PerformUpgrade();

            if (!result.Successful)
            {
                // Handle the migration failure
                Console.WriteLine("Database migration failed:");
                Console.WriteLine(result.Error);
            }
            else
            {
                Console.WriteLine("Database migration successful!");
            }
        }
    }

}
