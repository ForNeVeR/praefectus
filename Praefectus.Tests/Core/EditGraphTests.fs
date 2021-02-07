module Praefectus.Tests.Core.EditGraphTests

open Xunit

open Praefectus.Core.Diff
open Praefectus.Tests.TestFramework.DiffUtils

let doRouteStepAllTest sequenceADef sequenceB steps expectedBacktraces =
    let sequenceA = createPositionedSequence sequenceADef
    let graph = EditGraph(sequenceA, Seq.toArray sequenceB)

    let mutable currentBacktraces = graph.InitialBacktraces()
    for i in 1..steps do
        currentBacktraces <- graph.StepAll currentBacktraces |> Seq.toArray

    Assert.Equal<list<int * int>>(expectedBacktraces, currentBacktraces)

let sequence = [|
    2, 'A'
    4, 'D'
    5, 'E'
    6, 'F'
    7, 'G'
    8, 'B'
    9, 'C'
|]

[<Fact>]
let ``EditGraph traverse test 0``() =
    doRouteStepAllTest sequence "ABCDEFG" 0 [|
        [1, 1; 0, 0]
    |]

[<Fact>]
let ``EditGraph traverse test 1``() =
    doRouteStepAllTest sequence "ABCDEFG" 1 [|
        [      2, 1; 1, 1; 0, 0]
        [3, 2; 2, 1; 1, 1; 0, 0]
    |]

[<Fact>]
let ``EditGraph traverse test 2``() =
    doRouteStepAllTest sequence "ABCDEFG" 2 [|
        [3, 2; 2, 1; 1, 1; 0, 0]
        [2, 2; 2, 1; 1, 1; 0, 0]
        [7, 7; 2, 2; 2, 1; 1, 1; 0, 0]
        [4, 2; 3, 2; 2, 1; 1, 1; 0, 0]
    |]

let doRouteOneStepTest sequenceADef sequenceB route expectedBacktraces =
    let sequenceA = createPositionedSequence sequenceADef
    let graph = EditGraph(sequenceA, Seq.toArray sequenceB)

    let newBacktraces = graph.StepAll(Seq.singleton route) |> Seq.toArray
    Assert.Equal<list<int * int>>(expectedBacktraces, newBacktraces)

[<Fact>]
let ``EditGraph traverse step test 0``() =
    doRouteOneStepTest sequence "ABCDEFG" [2, 1; 1, 1; 0, 0] [|
        [3, 2; 2, 1; 1, 1; 0, 0]
        [2, 2; 2, 1; 1, 1; 0, 0]
        [7, 7; 2, 2; 2, 1; 1, 1; 0, 0]
    |]
