module Praefectus.Tests.Core.DiffTests

open System

open System.Collections.Generic
open Xunit

open Praefectus.Core.Diff

let applyInstructions (instructions: EditInstruction<'a> seq) (sequence: 'a seq): 'a seq =
    seq {
        use sequenceEnumerator = sequence.GetEnumerator()
        use instructionEnumerator = instructions.GetEnumerator()

        while instructionEnumerator.MoveNext() do
            let command = instructionEnumerator.Current
            match command with
            | DeleteItem -> sequenceEnumerator.MoveNext() |> ignore
            | InsertItem x -> yield x
            | LeaveItem ->
                sequenceEnumerator.MoveNext() |> ignore
                yield sequenceEnumerator.Current
    }

let private doSesLengthTest initial required expectedLength =
    let actualSesLength, _, _ = shortestEditScriptTrace (Seq.toArray initial) (Seq.toArray required)
    Assert.Equal(expectedLength, actualSesLength)

[<Fact>]
let ``Shortest edit script test 1``(): unit = doSesLengthTest "" "" 0
[<Fact>]
let ``Shortest edit script test 2``(): unit = doSesLengthTest "" "a" 1
[<Fact>]
let ``Shortest edit script test 3``(): unit = doSesLengthTest "a" "" 1
[<Fact>]
let ``Shortest edit script test 4``(): unit = doSesLengthTest "abc" "b" 2
[<Fact>]
let ``Shortest edit script test 5``(): unit = doSesLengthTest "abcabc" "abc" 3

let private convertHistory a b array2d =
    let max = Array.length a + Array.length b
    array2d
    |> Array.map (fun sampleArray ->
        let realArray = Array.CreateInstance(typeof<int>, [| max * 2 + 1 |], [| -max |])
        let offset = 1 - Array.length sampleArray
        for i = -max to max do
            realArray.SetValue(-1, i)
        sampleArray |> Array.iteri (fun i x ->
            realArray.SetValue(x, i * 2 + offset)
        )
        realArray
    )

let private assertHistoryEqual (expected: Array[]) (actual: IReadOnlyList<Array>) =
    Assert.Equal(expected.Length, actual.Count)

    Seq.zip expected actual |> Seq.iteri (fun i (expectedStep, actualStep) ->
        let lower = expectedStep.GetLowerBound 0
        let upper = expectedStep.GetUpperBound 0
        let length = expectedStep.GetLength 0
        Assert.Equal(lower, actualStep.GetLowerBound 0)
        Assert.Equal(upper, actualStep.GetUpperBound 0)
        Assert.Equal(length, actualStep.GetLength 0)

        for k = lower to upper do
            let expectedItem = expectedStep.GetValue k :?> int
            let actualItem = actualStep.GetValue k :?> int
            if expectedItem <> -1 && expectedItem <> actualItem then
                let expectedStringified =
                     expectedStep
                     |> Seq.cast<int>
                     |> Seq.map (fun x -> if x = -1 then "_" else string x)

                let expectedString = String.Join("; ", expectedStringified)
                let actualString = String.Join("; ", Seq.cast<int> actualStep)
                let message = $"Historical arrays aren't equal at step {i}.\nExpected: [{expectedString}],\nactual:   [{actualString}]"
                Assert.True(false, message)
    )

[<Fact>]
let ``Trace array test from the paper``(): unit =
    let seqA = Seq.toArray "ABCABBA"
    let seqB = Seq.toArray "CBABAC"
    let sesLength, history, k = shortestEditScriptTrace seqA seqB

    let expectedHistory = convertHistory seqA seqB [|
        //          0 1 2 3 4
        [|          0         |]
        [|        0 ; 1       |]
        [|      2 ; 2 ; 3     |]
        [|    3 ; 4 ; 5 ; 5   |]
        [| -1 ; 4 ; 5 ; 7 ; 7 |]
        [|        5 ; 7       |]
    |]

    Assert.Equal(sesLength, 5)
    Assert.Equal(k, 1)
    assertHistoryEqual expectedHistory history

let private assertDecypheredBacktraceEqual a b (expectedTrace: (int * int) seq) =
    let trace = decypherBacktrace a b
    Assert.Equal<int * int>(expectedTrace, trace)

[<Fact>]
let ``Decyphered backtrace for diff from paper``(): unit =
    let a = Seq.toArray "ABCABBA"
    let b = Seq.toArray "CBABAC"
    assertDecypheredBacktraceEqual a b [|
        7, 6
        7, 5
        5, 4
        3, 1
        1, 0
        0, 0
    |]

[<Fact>]
let ``Decyphered backtrace test 0``(): unit =
    let a = Seq.toArray "23"
    let b = Seq.toArray "123"
    assertDecypheredBacktraceEqual a b [|
        2, 3
        0, 0
    |]

let private doDiffAndAssert initial required =
    let instructions = diff (Seq.toArray initial) (Seq.toArray required) |> Seq.cache
    let result = applyInstructions instructions initial |> Seq.toArray |> String
    Assert.Equal(required, result)
    instructions

let private doDiffTest initial required =
    doDiffAndAssert initial required |> ignore

[<Fact>]
let ``Diff test 0``(): unit = doDiffTest "ABCABBA" "CBABAC"
[<Fact>]
let ``Diff test 1``(): unit = doDiffTest "" ""
[<Fact>]
let ``Diff test 2``(): unit = doDiffTest "abcabc" "abc"
[<Fact>]
let ``Diff test 3``(): unit = doDiffTest "abcabc" ""
[<Fact>]
let ``Diff test 4``(): unit = doDiffTest "" "abcabc"
[<Fact>]
let ``Diff test 5``(): unit = doDiffTest "abcdef" "ade"
[<Fact>]
let ``Diff test 6``(): unit = doDiffTest "abcdef" "fedcba"
[<Fact>]
let ``Diff test 7``(): unit = doDiffTest "abcdef" "afedcbabcd"

let private doDiffInstructionTest initial required expectedInstructions =
    let instructions = doDiffAndAssert initial required
    Assert.Equal<EditInstruction<_>>(expectedInstructions, instructions)

[<Fact>]
let ``Diff instruction test 0``(): unit =
    doDiffInstructionTest "21" "12" [|
        DeleteItem
        LeaveItem
        InsertItem '2'
    |]


[<Fact>]
let ``Diff instruction test 1``(): unit =
    doDiffInstructionTest "23" "123" [|
        InsertItem '1'
        LeaveItem
        LeaveItem
    |]
