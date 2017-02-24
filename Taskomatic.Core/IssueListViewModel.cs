using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Octokit;
using ReactiveUI;
using FileMode = System.IO.FileMode;

namespace Taskomatic.Core
{
    public class IssueListViewModel
    {
        private static readonly ProductHeaderValue Product = new ProductHeaderValue(
            "taskomatic",
            Assembly.GetExecutingAssembly().GetName().Version.ToString());

        public List<IssueViewModel> Issues { get; set; }

        public ReactiveCommand<object> LoadIssues { get; } = ReactiveCommand.Create();

        public IssueListViewModel()
        {
            LoadIssues.Subscribe(async _ =>
            {
                var config = await LoadConfig();
                var projectInfo = config.GitHubProject.Split('/');
                var user = projectInfo[0];
                var repo = projectInfo[1];
                var client = new GitHubClient(Product);
                var issues = await client.Issue.GetAllForRepository(user, repo);
                Issues = issues.Select(IssueViewModel.From).ToList();
            });
        }

        private static Task<Config> LoadConfig()
        {
            var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            if (string.IsNullOrEmpty(home))
            {
                throw new Exception("Cannot determine user home directory");
            }

            var path = Path.Combine(home, Config.ConfigFileName);
            return Task.Run(async () =>
            {
                using (var stream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    return await Config.Read(stream);
                }
            });
        }
    }
}
