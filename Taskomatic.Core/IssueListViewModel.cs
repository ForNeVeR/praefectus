using System;
using System.Collections.Generic;
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
        private ItemState? _filterState;

        private List<IssueViewModel> _allIssues = new List<IssueViewModel>();
        private string _filterAssignee;

        public ObservableCollection<IssueViewModel> Issues { get; } = new ObservableCollection<IssueViewModel>();
        public ObservableCollection<ItemState> States { get; } = new ObservableCollection<ItemState>();
        public ObservableCollection<string> Assignees { get; } = new ObservableCollection<string>();

        public ItemState? FilterState
        {
            get { return _filterState; }
            set
            {
                if (_filterState != value)
                {
                    _filterState = value;
                    this.RaisePropertyChanged();

                    Filter();
                }
            }
        }

        public string FilterAssignee
        {
            get { return _filterAssignee; }
            set
            {
                if (_filterAssignee != value)
                {
                    _filterAssignee = value;
                    this.RaisePropertyChanged();

                    Filter();
                }
            }
        }

        private void Filter()
        {
            IEnumerable<IssueViewModel> issues = _allIssues;
            if (FilterState != null)
            {
                issues = issues.Where(i => i.Status == FilterState.Value);
            }

            if (FilterAssignee != null)
            {
                issues = issues.Where(i => i.Assignees.Contains(FilterAssignee));
            }

            FillCollection(Issues, issues.OrderBy(i => i.FullInfo));
        }

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
                var issues = await client.Issue.GetAllForRepository(user, repo, new RepositoryIssueRequest
                {
                    State = ItemStateFilter.All
                });

                _allIssues.Clear();
                _allIssues.AddRange(issues.Select(i => new IssueViewModel(config, i)));

                FillCollection(States, _allIssues.Select(i => i.Status).Distinct());
                FillCollection(Assignees, _allIssues.SelectMany(i => i.Assignees).Distinct());

                FilterState = null;
                FilterAssignee = null;

                Filter();
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

        private static void FillCollection<T>(ObservableCollection<T> collection, IEnumerable<T> items)
        {
            collection.Clear();
            foreach (var item in items)
            {
                collection.Add(item);
            }
        }
    }
}
