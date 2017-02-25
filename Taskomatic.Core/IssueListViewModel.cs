using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Octokit;
using ReactiveUI;
using FileMode = System.IO.FileMode;

namespace Taskomatic.Core
{
    public class IssueListViewModel : ReactiveObject
    {
        private static readonly ProductHeaderValue Product = new ProductHeaderValue(
            "taskomatic",
            Assembly.GetExecutingAssembly().GetName().Version.ToString());

        private IssueViewModel _selectedIssue;

        public ObservableCollection<IssueViewModel> Issues { get; } = new ObservableCollection<IssueViewModel>();

        public IssueViewModel SelectedIssue
        {
            get { return _selectedIssue; }
            set
            {
                _selectedIssue = value;
                this.RaisePropertyChanged();
            }
        }

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

                Issues.Clear();
                foreach (var model in issues.Select(i => IssueViewModel.From(config, i)))
                {
                    Issues.Add(model);
                }
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
