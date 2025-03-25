// SPDX-FileCopyrightText: 2025 Friedrich von Never <friedrich@fornever.me>
//
// SPDX-License-Identifier: MIT

module Praefectus.Tests.Core.OrderingTests

open System

open Xunit

open Praefectus.Core
open Praefectus.Storage

let private generateTasksById = Array.mapi (fun i id ->
    let order = i + 1
    { Task.Empty<_> { FileSystemStorage.FileName = sprintf "%d.%s.md" order id } with
        Order = Some order
        Id = Some id }
)

let private generateTaskByAttributes fileName (attributes: FileSystemStorage.FsAttributes) =
    { Task.Empty<_> { FileSystemStorage.FileName = fileName } with
       Order = attributes.Order
       Id = attributes.Id
       Name = attributes.Name }

[<Fact>]
let ``Ordering should be stable``(): unit =
    let tasks = generateTasksById [| "b"; "ab"; "aa" |]
    let predicates = [|
        fun task -> task.Id.Value.[0] = 'a'
    |]
    let reordered = Ordering.reorder predicates tasks
    Assert.Collection(
        reordered,
        (fun t -> Assert.Equal("ab", t.Id.Value)),
        (fun t -> Assert.Equal("aa", t.Id.Value)),
        fun t -> Assert.Equal("b", t.Id.Value)
    )

[<Fact>]
let ``reorder should order items``(): unit =
    let tasks = generateTasksById [| "aa"; "ca"; "ab"; "ba" |]
    let predicates = [|
        fun task -> task.Id.Value.[0] = 'a'
        fun task -> task.Id.Value.[0] = 'b'
        fun task -> task.Id.Value.[0] = 'c'
    |]
    let reordered = Ordering.reorder predicates tasks
    let sorted = tasks |> Seq.sortBy (fun t -> t.Id)
    Assert.Equal<Task<_>>(sorted, reordered)

[<Fact>]
let ``reorder should move unordered items to the end``(): unit =
    let tasks = generateTasksById [| "aa"; "ca"; "ab"; "ba" |]
    let predicates = [|
        fun task -> task.Id.Value.[0] = 'a'
        fun task -> task.Id.Value.[0] = 'b'
    |]
    let reordered = Ordering.reorder predicates tasks
    let lastTask = Seq.last reordered
    Assert.Equal(Some "ca", lastTask.Id)

[<Fact>]
let ``applyOrderInStorage should do nothing if order is already right``(): unit =
    let tasks = generateTasksById [| "a"; "b"; "c"; "d"; "e"; "f" |]
    let instructions = Ordering.applyOrderInStorage FileSystemStorage.getNewState tasks
    Assert.Equal(Array.empty, instructions)

let private isTask fileName additionalChecks =
    let attributes = FileSystemStorage.readFsAttributes fileName
    Action<_>(fun (instruction: StorageInstruction<FileSystemStorage.FileSystemTaskState>) ->
        Assert.Equal(attributes.Id, instruction.Task.Id)
        Assert.Equal(fileName, instruction.NewState.FileName)
        additionalChecks instruction
    )

let private generateRenameRequirements initialFileNames requiredFileNames =
    let fileNamesByTaskId =
        initialFileNames
        |> Seq.map FileSystemStorage.readFsAttributes
        |> Seq.map(fun ({ Id = id } as attrs) -> id, attrs)
        |> Map.ofSeq

    requiredFileNames
    |> Seq.choose(fun fileName ->
        let attrs = FileSystemStorage.readFsAttributes fileName
        let oldAttrs = fileNamesByTaskId.[attrs.Id]
        if oldAttrs.Order <> attrs.Order then
            Some(isTask fileName ignore)
        else
            None
    )
    |> Seq.toArray

let private testTaskOrdering initialFileNames orderedFileNames =
    let tasks =
        initialFileNames
        |> Seq.map (fun fileName ->
            let attributes = FileSystemStorage.readFsAttributes fileName
            generateTaskByAttributes fileName attributes
        )
    let tasksByIdMap =
        tasks
        |> Seq.map (fun t -> t.Id, t)
        |> Map.ofSeq
    let requiredOrder =
        orderedFileNames
        |> Seq.map FileSystemStorage.readFsAttributes
        |> Seq.map (fun { Id = id } -> tasksByIdMap.[id])
        |> Seq.toArray

    let instructions = Ordering.applyOrderInStorage FileSystemStorage.getNewState requiredOrder
    let requirements = generateRenameRequirements initialFileNames orderedFileNames

    Assert.Collection(instructions, requirements)

[<Fact>]
let ``applyOrderInStorage order test case 0``(): unit =
    testTaskOrdering [| "1.a.md"; "2.b.md"; "3.c.md"; "4.d.md" |]
                     [| "1.a.md"; "2.b.md"; "3.c.md"; "4.d.md" |]

[<Fact>]
let ``applyOrderInStorage order test case 1``(): unit =
    testTaskOrdering [| "1.b.md"; "2.c.md"; "3.d.md"; "4.a.md" |]
                     [| "4.a.md"; "5.b.md"; "6.c.md"; "7.d.md" |]

[<Fact>]
let ``applyOrderInStorage order test case 2``(): unit =
    testTaskOrdering [| "1.a.md"; "2.c.md"; "3.b.md"; "4.d.md" |]
                     [| "1.a.md"; "3.b.md"; "4.c.md"; "5.d.md" |]

[<Fact>]
let ``applyOrderInStorage order test case 3``(): unit =
    testTaskOrdering [| "1.a.md"; "3.c.md"; "4.b.md"; "5.d.md" |]
                     [| "1.a.md"; "2.b.md"; "3.c.md"; "5.d.md" |]

[<Fact>]
let ``applyOrderInStorage order test case 4``(): unit =
    testTaskOrdering [| "1.a.md";                     "4.d.md"; "5.e.md"; "6.b.md"; "7.c.md" |]
                     [| "1.a.md"; "2.b.md"; "3.c.md"; "4.d.md"; "5.e.md" |]

[<Fact>]
let ``applyOrderInStorage order test case 5``(): unit =
    testTaskOrdering [| "1.a.md";           "3.d.md"; "4.e.md";           "6.b.md"; "7.c.md" |]
                     [| "1.a.md";                                         "6.b.md"; "7.c.md"; "8.d.md"; "9.e.md" |]

[<Fact>]
let ``applyOrderInStorage order test case 6``(): unit =
    testTaskOrdering [|           "2.a.md";           "4.d.md"; "5.e.md"; "6.f.md"; "7.g.md"; "8.b.md"; "9.c.md" |]
                     [| "1.a.md"; "2.b.md"; "3.c.md"; "4.d.md"; "5.e.md"; "6.f.md"; "7.g.md" |]

[<Fact>]
let ``applyOrderInStorage order test case 7``(): unit =
    testTaskOrdering
     <| [| "2.a.md"; "3.d.md"; "4.e.md"; "5.f.md"; "6.g.md"; "7.b.md"; "8.c.md" |]
     <| [| "2.a.md";                                         "7.b.md"; "8.c.md"; "9.d.md"; "10.e.md"; "11.f.md"; "12.g.md" |]
