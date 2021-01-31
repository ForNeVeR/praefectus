module Praefectus.Tests.Core.OrderingTests

open System

open Xunit

open Praefectus.Core
open Praefectus.Storage

let private generateTasksByName = Array.mapi (fun i id ->
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
    let tasks = generateTasksByName [| "b"; "ab"; "aa" |]
    let predicates = [|
        fun task -> task.Name.Value.[0] = 'a'
    |]
    let reordered = Ordering.reorder predicates tasks
    Assert.Collection(
        reordered,
        (fun t -> Assert.Equal("ab", t.Name.Value)),
        (fun t -> Assert.Equal("aa", t.Name.Value)),
        fun t -> Assert.Equal("b", t.Name.Value)
    )

[<Fact>]
let ``reorder should order items``(): unit =
    let tasks = generateTasksByName [| "aa"; "ca"; "ab"; "ba" |]
    let predicates = [|
        fun task -> task.Name.Value.[0] = 'a'
        fun task -> task.Name.Value.[0] = 'b'
        fun task -> task.Name.Value.[0] = 'c'
    |]
    let reordered = Ordering.reorder predicates tasks
    let sorted = tasks |> Seq.sortBy (fun t -> t.Name)
    Assert.Equal<Task<_>>(sorted, reordered)

[<Fact>]
let ``reorder should move unordered items to the end``(): unit =
    let tasks = generateTasksByName [| "aa"; "ca"; "ab"; "ba" |]
    let predicates = [|
        fun task -> task.Name.Value.[0] = 'a'
        fun task -> task.Name.Value.[0] = 'b'
    |]
    let reordered = Ordering.reorder predicates tasks
    let lastTask = Seq.last reordered
    Assert.Equal(Some "ca", lastTask.Name)

[<Fact>]
let ``applyOrderInStorage should do nothing if order is already right``(): unit =
    let tasks = generateTasksByName [| "a"; "b"; "c"; "d"; "e"; "f" |]
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
    let fileNamesByTaskName =
        initialFileNames
        |> Seq.map FileSystemStorage.readFsAttributes
        |> Seq.map(fun ({ Name = name } as attrs) -> name, attrs)
        |> Map.ofSeq

    requiredFileNames
    |> Seq.choose(fun fileName ->
        let attrs = FileSystemStorage.readFsAttributes fileName
        let oldAttrs = fileNamesByTaskName.[attrs.Name]
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
            let attributes =  FileSystemStorage.readFsAttributes fileName
            generateTaskByAttributes fileName attributes
        )
    let tasksByNameMap =
        tasks
        |> Seq.map (fun t -> t.Name, t)
        |> Map.ofSeq
    let requiredOrder =
        orderedFileNames
        |> Seq.map FileSystemStorage.readFsAttributes
        |> Seq.map (fun { Name = name } -> tasksByNameMap.[name])
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
                     [| "1.a.md"; "2.b.md"; "3.c.md"; "4.d.md" |]

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
    testTaskOrdering [| "1.a.md"; "4.d.md"; "5.e.md"; "6.b.md"; "7.c.md" |]
                     [| "1.a.md"; "2.b.md"; "3.c.md"; "5.d.md"; "6.e.md" |]

[<Fact>]
let ``applyOrderInStorage order test case 5``(): unit =
    testTaskOrdering [| "1.a.md"; "3.d.md"; "4.e.md"; "6.b.md"; "7.c.md" |]
                     [| "1.a.md"; "2.b.md"; "3.c.md"; "5.d.md"; "6.e.md" |]

[<Fact>]
let ``applyOrderInStorage order test case 6``(): unit =
    testTaskOrdering [| "2.a.md"; "4.d.md"; "5.e.md"; "6.f.md"; "7.g.md"; "8.b.md"; "9.c.md" |]
                     [| "1.a.md"; "2.b.md"; "3.c.md"; "4.d.md"; "5.e.md"; "6.f.md"; "7.g.md" |]

[<Fact>]
let ``applyOrderInStorage order test case 7``(): unit =
    testTaskOrdering [| "2.a.md"; "3.d.md"; "4.e.md"; "5.f.md"; "6.g.md"; "7.b.md"; "8.c.md" |]
                     [| "2.a.md"; "3.b.md"; "4.c.md"; "5.d.md"; "6.e.md"; "7.f.md"; "8.g.md" |]

[<Fact>]
let ``applyOrderInStorage order test case with same name``(): unit =
    // 1.a.md ; 2.b.md ; 3.b.md ; 4.d.md
    // -> 1.a.md ; 3.b.md ; 4.b.md ; 5.d.md (change order of 2.b.md and 3.b.md)
    let tasks = generateTasksByName [| "a"; "b"; "b"; "d" |]
    let reorderedTasks = [|
        tasks.[0]
        tasks.[2]
        tasks.[1]
        tasks.[3]
    |]

    let instructions = Ordering.applyOrderInStorage FileSystemStorage.getNewState reorderedTasks

    Assert.Collection(instructions,
                      isTask "4.b.md" (fun instruction ->
                          Assert.Equal("2.b.md", instruction.Task.StorageState.FileName)
                      ),
                      isTask "5.d.md" ignore)

[<Fact>]
let ``applyOrderInStorage should rename minimal amount of items``(): unit =
    let tasks = generateTasksByName [| "a"; "a" |] |> Seq.rev |> Seq.toArray
    let instructions = Ordering.applyOrderInStorage FileSystemStorage.getNewState tasks
    Assert.Collection(instructions, fun i ->
        Assert.Equal(tasks.[0], i.Task)
        Assert.Equal("3.a.md", i.NewState.FileName)
    )

[<Fact>]
let ``applyOrderInStorage should work for duplicated orders``(): unit =
    Assert.True false
