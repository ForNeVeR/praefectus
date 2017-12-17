using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Reflection;
using Octokit;
using Octokit.Internal;
using ReactiveUI;

namespace Taskomatic.Core
{
    public class ApplicationViewModel : ReactiveObject
    {
        private static readonly ProductHeaderValue Product = new ProductHeaderValue(
            "taskomatic",
            Assembly.GetExecutingAssembly().GetName().Version.ToString());

        private IssueViewModel _selectedIssue;
        private ItemState? _filterState;

        private List<IssueViewModel> _allIssues = new List<IssueViewModel>();
        private string _filterAssignee;
        private string _selectedProject;

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

        public List<string> AllProjects { get; } = new List<string>();

        public string SelectedProject
        {
            get => _selectedProject;
            set
            {
                _selectedProject = value;
                this.RaisePropertyChanged();
            }
        }

        public ReactiveCommand<object> LoadIssues { get; }

        private static GitHubClient InitializeGitHubClient(Config config) =>
            string.IsNullOrWhiteSpace(config.GitHubAccessToken)
                ? new GitHubClient(Product)
                : new GitHubClient(Product, new InMemoryCredentialStore(new Credentials(config.GitHubAccessToken)));

        public ApplicationViewModel(Config config)
        {
            AllProjects = config.GitHubProjects.ToList();

            LoadIssues = ReactiveCommand.Create(this.ObservableForProperty(m => m.SelectedProject).Select(p => p != null));
            LoadIssues.Subscribe(async _ =>
            {
                var projectInfo = SelectedProject.Split('/');
                var user = projectInfo[0];
                var repo = projectInfo[1];
                var client = InitializeGitHubClient(config);
                var issues = await client.Issue.GetAllForRepository(user, repo, new RepositoryIssueRequest
                {
                    State = ItemStateFilter.All
                });

                _allIssues.Clear();
                _allIssues.AddRange(issues.Select(i => new IssueViewModel(config, SelectedProject, i)));

                FillCollection(States, _allIssues.Select(i => i.Status).Distinct());
                FillCollection(Assignees, _allIssues.SelectMany(i => i.Assignees).Distinct());

                FilterState = null;
                FilterAssignee = null;

                Filter();
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
