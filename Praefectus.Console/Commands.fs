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

let doList (app: Application) (isJson: bool) (output: Stream): System.Threading.Tasks.Task<unit> = task {
    let! database = MarkdownDirectory.readDatabase app.Config.DatabaseLocation
    let tasks = database.Tasks
    if isJson then
        do! Json.serializeData tasks output
    else
        tasks |> orderTasks |> Seq.iter printTask
}
