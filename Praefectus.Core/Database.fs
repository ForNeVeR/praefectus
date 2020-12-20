namespace Praefectus.Core

open System.Collections.Generic

type Database = {
    Tasks: IReadOnlyCollection<Task>
}

module Database =
    let defaultDatabase = {
        Tasks = [||]
    }
