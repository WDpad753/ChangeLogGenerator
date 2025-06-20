using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using TestAPI.Model;

namespace TestAPI.Controllers
{
    [ApiController]
    [Route("repos/{owner}/{repo}/commits")]
    public class GitHubCommitsController : ControllerBase
    {
        [HttpGet]
        public IActionResult GetGitHubCommits(string owner, string repo)
        {
            string fileDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string fullpath = Path.Combine(fileDirectory, "Data");

            var commitsJson = System.IO.File.ReadAllText($"{fullpath}\\GithubCommits.json");
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var response = JsonSerializer.Deserialize<List<GitHubCommit>>(commitsJson, options);

            return Ok(response);
        }

        [HttpGet("{sha}")]
        public IActionResult GetGitHubCommitChanges(string sha)
        {
            string fileDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string fullpath = Path.Combine(fileDirectory, "Data");

            var commitsJson = System.IO.File.ReadAllText($"{fullpath}\\GithubCommits.json");
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var response = JsonSerializer.Deserialize<List<GitHubCommit>>(commitsJson, options);

            return Ok(response);
        }
    }
}
