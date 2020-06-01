namespace Praefectus.Core

open System.Collections.Generic

type Database = {
    Metadata: IReadOnlyCollection<Attribute>
    Tasks: IReadOnlyCollection<Task>
}

module Database =
    let defaultDatabase = {
        Metadata = [|
            DefaultAttributes.DependsOn
            DefaultAttributes.ActuallyDependsOn
        |]
        Tasks = [||]
    }
