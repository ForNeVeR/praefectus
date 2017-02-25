using Octokit;

namespace Taskomatic.Core
{
    public class IssueViewModel
    {
        public string Project { get; set; }
        public int Id { get; private set; }
        public string Name { get; private set; }

        public string FullInfo => $"{Project}#{Id}: {Name}";

        public static IssueViewModel From(string project, Issue issue)
        {
            return new IssueViewModel
            {
                Project = project,
                Id = issue.Number,
                Name = issue.Title
            };
        }
    }
}
