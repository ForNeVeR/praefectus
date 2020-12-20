module Praefectus.Console.Commands

open System.IO
open System.Threading.Tasks

open FSharp.Control.Tasks

open Praefectus.Storage

let doList (app: Application) (isJson: bool) (output: Stream): Task<unit> = task {
    let! database = MarkdownDirectory.readDatabase app.Config.DatabaseLocation
    let tasks = database.Tasks
    if isJson then
        do! Json.serializeData tasks output
    else
        tasks |> Seq.iter (printfn "%A")
}
