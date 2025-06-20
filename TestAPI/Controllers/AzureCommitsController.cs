using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using TestAPI.Model;

namespace TestAPI.Controllers
{

    [ApiController]
    [Route("{organization}/{project}/_apis/git/repositories/{repositoryId}/commits")]
    public class AzureCommitsController : ControllerBase
    {
        [HttpGet]
        public IActionResult GetAzureCommits(
            string organization,
            string project,
            string repositoryId,
            [FromQuery(Name = "api-version")] string apiVersion = "7.1")
        {

            string fileDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string fullpath = Path.Combine(fileDirectory, "Data");

            var commitsJson = System.IO.File.ReadAllText($"{fullpath}\\AzureCommits.json");
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var response = JsonSerializer.Deserialize<AzureCommit>(commitsJson, options);

            return Ok(response);
        }
    }
}
