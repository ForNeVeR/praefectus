namespace Praefectus.Core

open System.Collections.Generic

type Database = {
    Attributes: IReadOnlyCollection<Attribute>
}

module Database =
    let empty = {
        Attributes = [|
            DefaultAttributes.DependsOn
            DefaultAttributes.ActuallyDependsOn
        |]
    }
