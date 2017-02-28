using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Octokit;
using ReactiveUI;

namespace Taskomatic.Core
{
    public class IssueViewModel
    {
        public string Project { get; }
        public int Id { get; }
        public string Name { get; }

        public ItemState Status { get; }
        public IReadOnlyList<string> Assignees { get; }

        public string FullInfo => $"{Project}#{Id}: {Name}";

        public string AssigneeNames => string.Join(",", Assignees);

        public LazyAsync<string> LocalStatus { get; }

        public ReactiveCommand<object> SyncCommand { get; }

        public IssueViewModel(Config config, Issue issue)
        {
            Project = config.GitHubProject;
            Id = issue.Number;
            Name = issue.Title;
            Status = issue.State;
            Assignees = issue.Assignees.Select(u => u.Login).ToList();
            
            LocalStatus = new LazyAsync<string>(() => GetLocalStatus(config, Id), "Loading…");
            SyncCommand = ReactiveCommand.Create(
                LocalStatus.ObservableForProperty(ls => ls.Value).Select(p => p.Value == "Not imported"));
            SyncCommand.Subscribe(async _ =>
            {
                await SyncTask(config, Id, Name);
                LocalStatus.Reset();
            });
        }

        private class TaskwarriorTaskInfo { }

        private Task<string> GetLocalStatus(Config config, int id)
        {
            var project = config.GitHubProject;
            var startInfo = new ProcessStartInfo(
                config.TaskWarriorPath,
                $"taskomatic_ghproject:\"{project}\" taskomatic_id:{id} export")
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

        private async Task SyncTask(Config config, int id, string name)
        {
            var status = await GetLocalStatus(config, id);
            if (status != "Not imported")
            {
                return;
            }

            var project = config.GitHubProject;
            var startInfo = new ProcessStartInfo(
                config.TaskWarriorPath,
                $"add \"{project}#{id}: {name.Replace("\"", "\\\"")}\" taskomatic_ghproject:\"{project}\" taskomatic_id:{id}")
            {
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            };

            await Task.Run(() =>
            {
                using (var process = Process.Start(startInfo))
                {
                    process.WaitForExit();
                    if (process.ExitCode != 0)
                    {
                        throw new Exception("TaskWarrior process returned error exit code: " + process.ExitCode);
                    }
                }
            });
        }
    }
}
