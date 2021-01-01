module Praefectus.Console.Commands

open System.IO

open FSharp.Control.Tasks

open Praefectus.Core
open Praefectus.Storage

let private orderTasks =
    Seq.sortBy (fun t -> t.Order, t.Id, t.Name)

let private printTask task =
    let print key = function
        | Some value -> string value
        | None -> sprintf "[NO %s]" key

    printfn "%s : %s : %s" (print "ORDER" task.Order) (print "ID" task.Id) (print "NAME" task.Name)

let doList (app: Application<FileSystemStorage.FileSystemTaskState>)
           (isJson: bool)
           (output: Stream): System.Threading.Tasks.Task<unit> = task {
    let! database = MarkdownDirectory.readDatabase app.Config.DatabaseLocation
    let tasks = database.Tasks
    if isJson then
        do! Json.serializeData tasks output
    else
        tasks |> orderTasks |> Seq.iter printTask
}

let private printInstruction { Task = task; NewState = { FileSystemStorage.FileName = newFileName } } =
    printfn "%s -> %s" task.StorageState.FileName newFileName

let doSort (config: Configuration<FileSystemStorage.FileSystemTaskState>) (whatIf: bool): Async<unit> = async {
    let! database = MarkdownDirectory.readDatabase config.DatabaseLocation
    let reorderedTasks = Ordering.reorder config.Ordering database.Tasks
    let instructions = Ordering.applyOrderInStorage reorderedTasks
    if whatIf then
        instructions |> Seq.iter printInstruction
    else
        do! MarkdownDirectory.applyStorageInstructions config.DatabaseLocation instructions
}
