Praefectus [![Status Zero][status-zero]][andivionian-status-classifier]
==========

Praefectus is a Getting Things Done (GTD) application that manages your tasks.

**⚠ Currently the project is in the planning state, nothing is ready yet.**

General Description
-------------------

Praefectus is a modern GTD application that allows you to store the information
on all sorts of your current tasks, manage them, view their change history,
import or export from/into various sources.

It is also a task planning application: it allows to schedule the tasks and
predict their real schedule.

It has a terminal UI (inspired by [Taskwarrior][taskwarrior]) and an API.

Documentation
-------------

1. [Development Process][docs.1.development-process]

### RFCs

- [Command Line Interface][docs.rfcs.command-line-interface]: an RFC for [issue
  #13: Simple UI/UX to navigate the database][issue-13]

Prerequisites
-------------

Praefectus is a .NET Core 3.1 application, that is published in a
[self-contained deployment mode][dotnet-publish.self-contained]. It means that
to run the released binaries, you'll need to install [.NET Core
prerequisites][dotnet-prerequisites] for your operating system.

For developers, [.NET Core 3.1 SDK][dotnet-download] is required.

Build
-----

To build the application, run the following command in the terminal:

```console
$ dotnet build --configuration Release
```

To publish the application (prepare a distributable copy independent of
installed runtime), run the following command:

```console
$ dotnet publish --self-contained true --configuration Release --runtime <RUNTIME_IDENTIFIER> --output publish Praefectus.Console
```

Here `<RUNTIME_IDENTIFIER>` is a [RID][dotnet-rid] for the target platform.
Currently used RIDs are:
- `linux-x64`
- `osx-x64`
- `win-x64`

This will create a redistributable set of application files in the `publish`
directory. `praefectus` (or `praefectus.exe`) binary is a console entry point.

Run
---

To start the application in the development mode, run the following command in
the terminal:

```console
$ dotnet run --project Praefectus.Console -- [arguments…]
```

See the [command line interface][docs.rfcs.command-line-interface] documentation
on possible arguments, or pass `--help` to see the option list.

Test
----

To execute the automatic test suite, run the following command in the terminal:

```console
$ dotnet test --configuration Release
```

Questions
---------

### Why not Taskwarrior?

Praefectus is very much inspired of Taskwarrior in many of its features. But,
while it's very good, Taskwarrior lacks some main features I'd like to see in a
GTD application:

1. It should be cross-platform. I'm sorry, but "POSIX-only" is not
   cross-platform. It is hard to use on Windows, while it shouldn't be.
2. I would like to add thorough notes for every task. Taskwarrior allows to add
   annotations, but these aren't very convenient.
3. Repeated task functionality in Taskwarrior is a bit messy (in how it handles
   dates: e.g. `wait` date will move together with the new tasks, but the
   `scheduled` one won't).
4. Taskwarrior offers a very centralized way to manage the tasks. If you have
   multiple devices, then you'll have to either use a file synchronization
   software to manage these or set up the Taskserver.
5. Taskserver is agressively non-crossplatform, I wasn't able to run it on my
   computers at all.
6. Taskwarrior has no convention of task progress: you can't say to it that this
   task is 90% done so probably it'll take less time to finish it.
7. Taskwarrior is not a planning application. It has no good schedule view to
   show the upcoming tasks.
8. The task constraints (e.g. `due`) are non-transitive: if my task with
   `due=tomorrow` depends on some other tasks with no constrains, Taskwarrior
   won't tell me about it (aside from slightly increasing these tasks' urgency).
9. Taskwarrior doesn't offer an embedded periodic import/export capabilities
   (e.g. to sync with GitHub or some other task/issue lists).
10. Taskwarrior "API" (the command line one) is not the best: e.g. it has no
    easy way to deal with the escapes in the Windows' command line (this
    significantly complicates its usage from other programs on that particular
    platform).
11. Taskwarrior doesn't perform automatic backups (believe or not, I have
    developed a habit of backing up all of my task files every morning, and it
    saved me a lot of times).

Overall, Taskwarrior is a very good application and I recommend it to you. But
it doesn't solve **all** of my needs, and thus I've decided to develop a GTD
application that will try to do it.

Many of these issues may be solved by developing additional habits, writing
scripts, shell aliases or via various automation, I agree with that. But the
fact is that Taskwarrior doesn't offer these tools out-of-the-box — and it is
arguable whether it should or not; we'll see!

Features
--------

### Cross-platform

Praefectus is written in .NET Core and runs on any platform .NET Core is
supported on.

### User interface

Praefectus has a query tool (Taskwarrior-like) and a Text UI dashboard
application that runs in any terminal.

### Decentralized

Praefectus supports various decentralized update scenarios via the concept of
_nodes_: these nodes may not all be directly connected, but will still deliver
tasks to the user.

### Automated import

While most of Praefectus _nodes_ are Praefectus application instances running
over various devices or over the network, some of them are _programmatic_ ones:
for example, all GitHub issues for one particular user may be described as one
_node_, your internal homegrown bugtracker is another one, a directory with a
bunch of markdown notes, a Git repository with backups, literally any API could
be a Praefectus node.

Praefectus knows a task history for each node, and it will allow multiple nodes
to manage single issue. Conflict resolution is sometimes performed automatically
and is sometimes up to the user. For example, you may import a GitHub issue,
then add some annotations to it, change its status and then export it back:
depending on your settings, the status may be synchronized back to GitHub (e.g.
an issue will be closed), and your annotations may be leaved on a Praefectus
node that authored them.

### Automated export

Some nodes may support Praefectus export, so it will automatically flow the
changes to these nodes.

### Task history

Each node has its own history for every task. Conflicts are inevitable in a
decentralized world, so Praefectus allows you to manage them in case they occur.
Out-of-band conflict resolution (either in Praefectus itself or in any external
merge tool) is supported.

### Task management

Start tasks, merge tasks from different sources, annotate your tasks: anything
you expect to see in a good task management application is here in Praefectus.

### (A bit of) artificial intelligence

Praefectus tries to be useful with some small AI components in it: it will try
to derive the tasks completion percents and task estimation based on simple
machine learning algorithms. Everything it presumes then should be approved by
a human, so don't worry, machines aren't (yet) going to take over your tasks.

### Task progress

Praefectus is aware of the task progress: task may be 50% done — and Praefectus
known about your cognitive bias in estimating the progress, thanks to AI!

Praefectus also allows you to specify the task _value_, and it will try to
maximize the value you gather when making your schedule.

### Programmable

Praefectus has good APIs for both external programs and internal scripts.

### Schedules

Praefectus uses some small achievements of the modern theory of economy to
manage the schedules: it shows critical paths in solving the tasks,
automatically applies transitive constraints, has an ability to show Gantt(-ish)
charts.

### Delegation

Praefectus supports some small bits of multi-actor task management: you may mark
your tasks as delegated to someone else, manage and control their work load and
their schedules. Praefectus is still a personal task management application, not
an enterprise one. But in our daily work/life, we often have to delegate some of
the work, and Praefectus is made be aware of it.

### Fun

It is a mandatory detail that working on the Praefectus project should be fun.
And I hope it will be fun to work _with_ Praefectus for the users.

[status-zero]: https://img.shields.io/badge/status-zero-lightgrey.svg

[andivionian-status-classifier]: https://github.com/ForNeVeR/andivionian-status-classifier#status-zero-
[docs.1.development-process]: docs/1.development-process.md
[docs.rfcs.command-line-interface]: docs/rfcs/command-line-interface.md
[dotnet-download]: https://dotnet.microsoft.com/download
[dotnet-prerequisites]: https://docs.microsoft.com/en-us/dotnet/core/install/dependencies?tabs=netcore31
[dotnet-publish.self-contained]: https://docs.microsoft.com/en-us/dotnet/core/deploying/deploy-with-cli#self-contained-deployment
[dotnet-rid]: https://docs.microsoft.com/en-us/dotnet/core/rid-catalog
[issue-13]: https://github.com/ForNeVeR/praefectus/issues/13
[taskwarrior]: https://taskwarrior.org/
