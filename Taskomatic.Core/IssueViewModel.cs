using Octokit;

namespace Taskomatic.Core
{
    public class IssueViewModel
    {
        public int Id { get; private set; }
        public string Name { get; private set; }

        public static IssueViewModel From(Issue issue)
        {
            return new IssueViewModel
            {
                Id = issue.Number,
                Name = issue.Title
            };
        }
    }
}
