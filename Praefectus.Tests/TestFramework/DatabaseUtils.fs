module Praefectus.Tests.TestFramework.DatabaseUtils

open System.IO

open Praefectus.Core
open Praefectus.Storage
open Praefectus.Storage.FileSystemStorage

let saveDatabaseToTempDirectory (database: Database<FileSystemTaskState>): Async<string> = async {
    let databasePath = Path.GetTempFileName()
    File.Delete databasePath
    Directory.CreateDirectory databasePath |> ignore
    do! MarkdownDirectory.saveDatabase database databasePath
    return databasePath
}
