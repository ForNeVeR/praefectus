namespace Praefectus.Core

open System
open System.Collections.Generic

[<RequireQualifiedAccess>]
type TaskStatus =
| Open
| InProgress
| Done
| Deleted

type Task = {
    Order: int option
    Id: string option
    Name: string option
    Title: string option
    Description: string option
    Status: TaskStatus option
    DependsOn: IReadOnlyCollection<string>
}
