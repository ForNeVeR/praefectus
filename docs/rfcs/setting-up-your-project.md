Setting Up Your Project
=======================

_This is an RFC for the issue [#43: Praefectus Prime][issue-43]._

Technically, Praefectus is a .NET library that should be used in an application
to make it work.

So, to use Praefectus, you should create a .NET application, reference
Praefectus library, and then pass the configuration data to its entry point.

See an example F# application in the [`Praefectus.Example`
project][praefectus.example].

Creating a new Praefectus application should be as simple as

```console
$ dotnet new --type console --language F# -o MyTodoList
$ cd MyTodoList
$ dotnet add package Praefectus
$ dotnet run -- --help # to run the app and pass the --help option to Praefectus
```

The application is started via `Praefectus.Console.EntryPoint.run` static
method. You may pass the following parameters:

- `baseConfigDirectory`: configuration directory where `praefectus.json` file
  will be loaded.
- `args`: command line arguments passed to the Praefectus.
- `environment`: an object used to describe the application environment. If you
  don't want to override any specifics, use
  `Praefectus.Console.Environment.OpenDefault()`.

[issue-43]: https://github.com/ForNeVeR/praefectus/issues/43
[praefectus.example]: ../../Praefectus.Example
