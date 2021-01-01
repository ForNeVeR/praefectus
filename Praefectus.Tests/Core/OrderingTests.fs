module Praefectus.Tests.Core.OrderingTests

open Xunit

open Praefectus.Core
open Praefectus.Storage

let private generateTasksByName = Seq.map (fun name ->
    { Task.Empty<_> { FileSystemStorage.FileName = sprintf "%s.md" name } with
        Name = Some name }
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
let ``applyOrderInStorage should rename minimal amount of items``(): unit =
    let tasks = generateTasksByName [| "a"; "a" |] |> Seq.rev |> Seq.toArray
    let instructions = Ordering.applyOrderInStorage tasks
    Assert.Collection(instructions, fun i ->
        Assert.Equal(tasks.[0], i.Task)
        Assert.Equal("3.a.md", i.NewState.FileName)
    )
