Setting Up Your Project
=======================

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

[praefectus.example]: ../Praefectus.Example
