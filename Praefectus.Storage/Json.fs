// SPDX-FileCopyrightText: 2025 Friedrich von Never <friedrich@fornever.me>
//
// SPDX-License-Identifier: MIT

module Praefectus.Storage.Json

open System.IO

open FSharp.Control.Tasks
open Newtonsoft.Json

open Praefectus.Core

let serializeData (data: 'a) (output: Stream): System.Threading.Tasks.Task<unit> = task {
    let serializer = JsonSerializer(Formatting = Formatting.Indented)
    use writer = new StreamWriter(output, leaveOpen = true)
    serializer.Serialize(writer, data)
}

let save (database: Database<_>) (target: Stream): System.Threading.Tasks.Task<unit> =
    serializeData database target
