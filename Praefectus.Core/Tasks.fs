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
    Id: string
    Title: string
    Created: DateTimeOffset
    Updated: DateTimeOffset
    Status: TaskStatus
    AttributeValues: IReadOnlyDictionary<AttributeIdentification, AttributeValue>
}
