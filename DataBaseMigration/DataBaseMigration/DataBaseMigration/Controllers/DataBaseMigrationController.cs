using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using Dapper;

namespace DataBaseMigration.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DataBaseMigrationController : ControllerBase
    {
        private readonly string _connectionString;
        private readonly string _scriptsFolder = "DatabaseScripts";

        public DataBaseMigrationController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DbCon");
        }
        [HttpPost]
        [Route("migrate")]
        public IActionResult Migrate()
        {
            try
            {
                using (IDbConnection dbConnection = new SqlConnection(_connectionString))
                {
                    dbConnection.Open();

                    // Get a list of all .sql files in the scripts folder
                    var scriptFiles = Directory.GetFiles(_scriptsFolder, "*.sql");

                    foreach (var scriptFile in scriptFiles)
                    {
                        ExecuteScript(dbConnection, scriptFile);
                    }

                    dbConnection.Close();
                }

                return Ok("Stored procedures successfully applied.");
            }
            catch (Exception ex)
            {
                return BadRequest($"Error: {ex.Message}");
            }
        }

        private void ExecuteScript(IDbConnection connection, string scriptFilePath)
        {
            var sqlScript = System.IO.File.ReadAllText(scriptFilePath);
            connection.Execute(sqlScript);
        }
    }
}
