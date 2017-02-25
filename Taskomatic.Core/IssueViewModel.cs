using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Octokit;

namespace Taskomatic.Core
{
    public class IssueViewModel
    {
        public string Project { get; }
        public int Id { get; }
        public string Name { get; }

        public string FullInfo => $"{Project}#{Id}: {Name}";

        public LazyAsync<string> LocalStatus { get; }

        private IssueViewModel(Config config, int id, string name)
        {
            Project = config.GitHubProject;
            Id = id;
            Name = name;
            LocalStatus = new LazyAsync<string>(() => GetLocalStatus(config, id), "Loading…");
        }

        public static IssueViewModel From(Config config, Issue issue)
        {
            return new IssueViewModel(config, issue.Number, issue.Title);
        }

        private class TaskwarriorTaskInfo { }

        private Task<string> GetLocalStatus(Config config, int id)
        {
            var project = config.GitHubProject;
            var startInfo = new ProcessStartInfo(
                config.TaskWarriorPath,
                $"taskomatic_ghproject:{project} taskomatic_id:{id} export")
            {
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            };
            return Task.Run(() =>
            {
                using (var process = Process.Start(startInfo))
                {
                    process.WaitForExit();
                    var serializer = new JsonSerializer();
                    using (var streamReader = process.StandardOutput)
                    using (var reader = new JsonTextReader(streamReader))
                    {
                        var list = serializer.Deserialize<List<TaskwarriorTaskInfo>>(reader);
                        return list.Count == 0 ? "Not imported" : "Imported";
                    }
                }
            });
        }
    }
}
