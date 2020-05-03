Configuration
-------------

_This is an RFC for the issue [#10: Configuration system][issue-10]._

Praefectus is configured via a configuration file, which is named
`praefectus.json` by default. See the [command line interface
RFC][docs.rfcs.command-line-interface] for details on changing the configuration
file location.

Currently, the only settings read from the configuration file are Serilog
settings from the `"Serilog"` section. See [the Serilog
documentation][serilog-settings-configuration] for details, and a configuration
example below.

```json
{
  "Serilog": {
    "Using":  ["Serilog.Sinks.Console"],
    "MinimumLevel": "Debug",
    "WriteTo": [
      { "Name": "Console" },
      { "Name": "File", "Args": { "path": "%TEMP%\\praefectus\\praefectus.log" } }
    ]
  }
}
```

Praefectus has `Serilog.Sinks.Console` and `Serilog.Sinks.File` packages
installed and ready to be used in the logging configuration.

Default Configuration File
--------------------------

Default configuration file for Praefectus configures the logs to be written to
the console, no other settings are introduced.

[docs.rfcs.command-line-interface]: docs/rfcs/command-line-interface.md
[issue-10]: https://github.com/ForNeVeR/praefectus/issues/10
[serilog-settings-configuration]: https://github.com/serilog/serilog-settings-configuration
