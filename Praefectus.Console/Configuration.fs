namespace Praefectus.Console

open Microsoft.Extensions.Configuration

type Configuration = {
    DatabaseLocation: string
}

module Configuration =
    let parse(config: IConfigurationRoot): Configuration = {
        DatabaseLocation = config.["databaseLocation"] |> Option.ofObj |> Option.defaultValue "praefectus.data.json"
    }
