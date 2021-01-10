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

[<Fact>]
let ``applyOrderInStorage order test case 1``(): unit =
    let tasks = generateTasksByName [| "b"; "c"; "d"; "a" |] |> Seq.cache
    let a = Seq.last tasks
    let reorderedTasks = [|
        a
        yield! Seq.take 3 tasks
    |]

    let instructions = Ordering.applyOrderInStorage FileSystemStorage.getNewState reorderedTasks |> Seq.toArray
    Assert.Equal(4, instructions.Length)

    let isTask fileName =
        let attributes = FileSystemStorage.readFsAttributes fileName
        Action<_>(fun (instruction: StorageInstruction<FileSystemStorage.FileSystemTaskState>) ->
            Assert.Equal(attributes.Id, instruction.Task.Id)
            Assert.Equal(fileName, instruction.NewState.FileName)
        )

    Assert.Collection(instructions,
                      isTask "1.a.md",
                      isTask "2.b.md",
                      isTask "3.c.md",
                      isTask "4.d.md")

[<Fact>]
let ``applyOrderInStorage order test case 2``(): unit =
    // TODO: Test case for:
    //   1.a.md ; 2.c.md ; 3.b.md ; 4.d.md
    // → 1.a.md ; 2.b.md ; 3.c.md ; 4.d.md
    Assert.True false

[<Fact>]
let ``applyOrderInStorage order test case 3``(): unit =
    // TODO: Test case for:
    //   1.a.md ; 2.b.md ; 3.b.md ; 4.d.md (switch order for two files named *.b.md)
    // → 1.a.md ; 3.b.md ; 4.b.md ; 5.d.md
    Assert.True false

[<Fact>]
let ``applyOrderInStorage order test case 4``(): unit =
    // TODO: Test case for:
    //   1.a.md ; 3.c.md ; 4.b.md ; 5.d.md
    // → 1.a.md ; 2.b.md ; 3.c.md ; 5.d.md
    Assert.True false

[<Fact>]
let ``applyOrderInStorage should rename minimal amount of items``(): unit =
    let tasks = generateTasksByName [| "a"; "a" |] |> Seq.rev |> Seq.toArray
    let instructions = Ordering.applyOrderInStorage FileSystemStorage.getNewState tasks
    Assert.Collection(instructions, fun i ->
        Assert.Equal(tasks.[0], i.Task)
        Assert.Equal("3.a.md", i.NewState.FileName)
    )
