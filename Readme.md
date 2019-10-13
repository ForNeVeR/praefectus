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
4. Taskwarrior offers very centralized way to manage the tasks. If you have
   multiple devices, then you'll have to either use a file synchronization
   software to manage these or set up the Taskserver.
5. Taskserver is extremely non-crossplatform, I wasn't able to run it on my
   computers at all.
6. Taskwarrior has no convention of task progress: you can't say to it that this
   task is 90% done so probably it'll take less time to finish it.
7. Taskwarrior is not a planning application. It has no good schedule view to
   show the upcoming tasks.
8. The task constraints (e.g. `due`) are non-transitive: if my task with
   `due=tomorrow` depends on some other tasks with no constrains, Taskwarrior
   won't tell me about it (aside from slightly increasing these tasks' urgency).
9. Taskwarrior has no "scopes" (e.g. for home and office work).
10. Taskwarrior doesn't offer an embedded periodic import/export capabilities
    (e.g. to sync with GitHub or some other task/issue lists).
11. Taskwarrior "API" (the command line one) is not the best: e.g. it has no
    easy way yo deal with the escapes in the Windows' command line (this
    significantly complicates its usage from other programs on that particular
    platform).
12. Taskwarrior doesn't perform automatic backups (believe or not, I have
    developed a habit of backing up all of my task files every morning, and it
    saved me a lot of times).

Overall, Taskwarrior is a very good application and I recommend it to you. But
it doesn't solve **all** of my needs, and thus I've decided to develop a GTD
application that will try to do it.

Many of these issues may be solved by developing additional habits, writing
scripts, shell aliases or via various automation, I agree with that. But the
fact is that Taskwarrior doesn't offer these tools out-of-the-box — and it is
arguable whether it should or not; we'll see!

[andivionian-status-classifier]: https://github.com/ForNeVeR/andivionian-status-classifier#status-zero-
[status-zero]: https://img.shields.io/badge/status-zero-lightgrey.svg
[taskwarrior]: https://taskwarrior.org/
