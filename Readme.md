taskomatic
==========

Taskomatic is a GUI tool to synchronize GitHub issues with a local
[TaskWarrior][taskwarrior] database.

Setup
-----

```console
$ task config uda.taskomatic_ghproject.type string
$ task config uda.taskomatic_ghproject.label "Taskomatic: GitHub project"

$ task config uda.taskomatic_id.type string
$ task config uda.taskomatic_id.label "Taskomatic: GitHub id"
```

Configuration
-------------

Put this into `~/.taskomatic.json`:

```json
{
    "GitHubProjects": ["ForNeVeR/s592", "ForNeVeR/taskomatic"],
    "TaskWarriorPath": "E:\\Programs\\msys2\\usr\\bin\\task.exe"
}
```

[taskwarrior]: https://taskwarrior.org/
