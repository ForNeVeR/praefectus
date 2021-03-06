Command Line Interface
======================

_This is an RFC for the issue [#13: Simple UI/UX to navigate the
database][issue-13]._

Praefectus offers a command line interface to operate the application. This
command application is distributed along the main Praefectus release package.

The application communicates with user via a console interface. User may pass
command line arguments, and the application will print output as required.

Exit Codes
----------

Praefectus will set exit code on termination, and follows the standard
convention for exit codes: code `0` means successful termination (i.e.
Praefectus has successfully done whatever user asked), other codes mean various
kinds of errors:

- exit code `1` means unspecified error (which has no its own specific code),
  usually a runtime exception
- exit code `2` means that Praefectus wasn't able to parse the command line
  arguments passed by the user

Command Line Arguments
----------------------

Praefectus supports the following command line options:

- `--help` to print a quick help message
- `--version` to print Praefectus version in format `Praefectus v{version}`

By default (without any arguments), it will do nothing (but will still parse the
configuration file).

Commands
--------

Praefectus has certain commands with their own arguments:

- `list [--json]`: will list all the tasks in the current database (optionally
  in a JSON format)
- `order [--whatif]`: will bring rasks to the specified order,
  see [Task Ordering][docs.4.task-ordering] for details.

[docs.4.task-ordering]: ../4.task-ordering.md
[issue-13]: https://github.com/ForNeVeR/praefectus/issues/13
