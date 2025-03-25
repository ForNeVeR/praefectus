// SPDX-FileCopyrightText: 2025 Friedrich von Never <friedrich@fornever.me>
//
// SPDX-License-Identifier: MIT

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
    |> append task.Name)
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
    let components = fileName.Split "."
    match components.Length with
    | 0 -> failwith "Impossible"
    | _ ->
        let order, idIndex =
            match Int32.TryParse(components.[0], NumberStyles.None, CultureInfo.InvariantCulture) with
            | true, order -> (Some order, 1)
            | false, _ -> (None, 0)

        let freeComponents = components.Length - idIndex
        let id, name =
            match freeComponents with
            | 0 -> None, None
            | 1 -> Some components.[idIndex], None
            | _ -> Some components.[idIndex], Some <| String.Join('.', Seq.skip (idIndex + 1) components)

        { Order = order; Id = id; Name = name }

