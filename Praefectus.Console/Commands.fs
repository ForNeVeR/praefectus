module Praefectus.Console.Commands

open System
open System.IO
open System.Threading.Tasks

open FSharp.Control.Tasks

open Praefectus.Storage

let doList (app: Application) (isJson: bool): Task<unit> = task {
    use source = new FileStream(app.Config.DatabaseLocation, FileMode.Open)
    let! database = Json.load source
    let tasks = database.Tasks
    if isJson then
        use output = Console.OpenStandardOutput()
        do! Json.serializeData tasks output
    else
        tasks |> Seq.iter (printfn "%A")
}
