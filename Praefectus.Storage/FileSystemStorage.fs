module Praefectus.Storage.FileSystemStorage

open System
open System.Globalization
open System.IO
open System.Text

open Praefectus.Core

let generateFileName(task: Task<_>): string =
    let append (x: string option) (builder: StringBuilder) =
        match x with
        | Some(x: string) -> builder.AppendFormat("{0}.", x)
        | None -> builder

    (StringBuilder()
    |> append(task.Order |> Option.map string)
    |> append task.Id
    |> append (task.Name |> Option.orElse(Some "")))
        .Append("md")
        .ToString()

type FileSystemTaskState = {
    FileName: string
}

let readMetadata(path: string): FileSystemTaskState = {
    FileName = Path.GetFileName path
}

let getNewState(order: int) (task: Task<FileSystemTaskState>): FileSystemTaskState =
    let newTask = { task with Order = Some order }
    { FileName = generateFileName newTask }

type FsAttributes = { Order: int option; Name: string option; Id: string option }

let readFsAttributes(filePath: string): FsAttributes =
    let fileName = Path.GetFileNameWithoutExtension filePath
    let components = fileName.Split(".")
    match components.Length with
    | 0 -> { Order = None; Id = None; Name = None }
    | _ ->
        let (order, nextIndex) =
            match Int32.TryParse(components.[0], NumberStyles.None, CultureInfo.InvariantCulture) with
            | (true, order) -> (Some order, 1)
            | (false, _) -> (None, 0)

        let (id, name) =
            match components.Length - nextIndex with
            | 0 -> None, None
            | 1 -> None, Some components.[nextIndex]
            | _ -> Some components.[nextIndex], Some <| String.Join('.', Seq.skip (nextIndex + 1) components)

        { Order = order; Id = id; Name = name }

