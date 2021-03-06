module Praefectus.Tests.Core.DiffTests

open System

open System.Collections.Generic
open Xunit

open Praefectus.Core.Diff
open Praefectus.Core.Diff.Algorithms
open Praefectus.Tests.TestFramework.DiffUtils

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

let private toSimplePositionedSequence items =
    let itemArray = Seq.toArray items
    { new IPositionedSequence<_> with
        member _.AllowedToInsertAtArbitraryPlaces = true
        member _.MaxCoord = itemArray.Length
        member _.GetItem coord =
            let index = coord - 1
            if index >= 0 && index < itemArray.Length
            then Some itemArray.[index]
            else None
        member _.AcceptsOn(coord, item) =
            let index = coord - 1
            itemArray.[index] = item
    }

let private doSesLengthTest initial required expectedLength =
    let instructions = myersGeneralized initial (Seq.toArray required)
    let actualLength =
        instructions
        |> Seq.filter(function
            | InsertItem _ -> true
            | DeleteItem -> true
            | _ -> false
        )
        |> Seq.length
    Assert.Equal(expectedLength, actualLength)

let private doSimpleSesLengthTest initial required expectedLength =
    doSesLengthTest (toSimplePositionedSequence initial) required expectedLength

[<Fact>]
let ``Simple shortest edit script test 1``(): unit = doSimpleSesLengthTest "" "" 0
[<Fact>]
let ``Simple shortest edit script test 2``(): unit = doSimpleSesLengthTest "" "a" 1
[<Fact>]
let ``Simple shortest edit script test 3``(): unit = doSimpleSesLengthTest "a" "" 1
[<Fact>]
let ``Simple shortest edit script test 4``(): unit = doSimpleSesLengthTest "abc" "b" 2
[<Fact>]
let ``Simple shortest edit script test 5``(): unit = doSimpleSesLengthTest "abcabc" "abc" 3

let private doConstrainedSesLengthTest initial required expectedLength =
    let positionedSequence = createPositionedSequence initial
    doSesLengthTest positionedSequence required expectedLength

[<Fact>]
let ``Constrained shortest edit script test 0``(): unit =
    doConstrainedSesLengthTest [|
        1, 'A'
        3, 'C'
        4, 'B'
        5, 'D'
    |] "ABCD" 2

[<Fact>]
let ``Constrained shortest edit script test 1``(): unit =
    doConstrainedSesLengthTest [|
        1, 'B'
        2, 'A'
    |] "AB" 2

[<Fact>]
let ``Constrained shortest edit script test 2``(): unit =
    doConstrainedSesLengthTest [|
        2, 'A'
        4, 'D'
        5, 'E'
        6, 'B'
        7, 'C'
    |] "ABCDE" 6

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

let private assertDecypheredBacktraceEqual a b (expectedTrace: (int * int) seq) allowedToInsert =
    let trace = decypherBacktrace a (Seq.toArray b) |> Seq.toArray
    Assert.Equal<int * int>(expectedTrace, trace)

let private assertSimpleDecypheredBacktraceEqual a b expectedTrace =
    assertDecypheredBacktraceEqual (toSimplePositionedSequence a) b expectedTrace (fun _ -> true)

[<Fact>]
let ``Simple decyphered backtrace test 0``(): unit =
    assertSimpleDecypheredBacktraceEqual "ABCABBA" "CBABAC" [|
        7, 6
        7, 5
        5, 3
        5, 2
        4, 1
        3, 1
        2, 0
        1, 0
        0, 0
    |]

[<Fact>]
let ``Simple decyphered backtrace test 1``(): unit =
    assertSimpleDecypheredBacktraceEqual "23" "123" [|
        2, 3
        0, 1
        0, 0
    |]

let assertConditionalDecypheredBacktraceEqual a b expectedTrace =
    let positionedSequence = createPositionedSequence a
    assertDecypheredBacktraceEqual positionedSequence b expectedTrace (fun idx -> idx = positionedSequence.MaxCoord)

[<Fact>]
let ``Constrained decyphered backtrace test 0``(): unit =
    assertConditionalDecypheredBacktraceEqual [|
        1, 'A'
        3, 'C'
        4, 'B'
        5, 'D'
    |] "ABCD" [|
        5, 4
        4, 3
        3, 3
        0, 0
    |]

[<Fact>]
let ``Constrained decyphered backtrace test 1``(): unit =
    assertConditionalDecypheredBacktraceEqual [|
        1, 'B'
        2, 'A'
    |] "AB" [|
        2, 2
        2, 1
        1, 0
        0, 0
    |]

[<Fact>]
let ``Constrained decyphered backtrace test 2``(): unit =
    assertConditionalDecypheredBacktraceEqual [|
        2, 'A'
        4, 'D'
        5, 'E'
        6, 'F'
        7, 'G'
        8, 'B'
        9, 'C'
    |] "ABCDEFG" [|
        9, 7
        8, 7
        7, 7
        2, 2
        2, 1
        1, 1
        0, 0
    |]

let private doDiffAndAssert initial target (initialString: string) allowedToInsert =
    let instructions = myersGeneralized initial (Seq.toArray target) |> Seq.cache
    let result = applyInstructions instructions initialString |> Seq.toArray |> String
    Assert.Equal(target, result)
    instructions

let private doSimpleDiffTest (initial: string) required =
    let instructions: EditInstruction<_> seq =
        doDiffAndAssert (toSimplePositionedSequence initial) required initial (fun _ -> true)
    ignore instructions

[<Fact>]
let ``Simple diff test 0``(): unit = doSimpleDiffTest "ABCABBA" "CBABAC"
[<Fact>]
let ``Simple diff test 1``(): unit = doSimpleDiffTest "" ""
[<Fact>]
let ``Simple diff test 2``(): unit = doSimpleDiffTest "abcabc" "abc"
[<Fact>]
let ``Simple diff test 3``(): unit = doSimpleDiffTest "abcabc" ""
[<Fact>]
let ``Simple diff test 4``(): unit = doSimpleDiffTest "" "abcabc"
[<Fact>]
let ``Simple diff test 5``(): unit = doSimpleDiffTest "abcdef" "ade"
[<Fact>]
let ``Simple diff test 6``(): unit = doSimpleDiffTest "abcdef" "fedcba"
[<Fact>]
let ``Simple diff test 7``(): unit = doSimpleDiffTest "abcdef" "afedcbabcd"

let private constrainedSequenceToString sequence =
    sequence |> Seq.map snd |> Seq.toArray |> String

let private doConstrainedDiffTest (initial: IReadOnlyList<_>) required =
    let initialString = constrainedSequenceToString initial
    let positionedSequence = createPositionedSequence initial
    let instructions: EditInstruction<_> seq =
        doDiffAndAssert positionedSequence required initialString (fun idx -> idx = positionedSequence.MaxCoord)
    ignore instructions

[<Fact>]
let ``Constrained diff test 0``(): unit =
    doConstrainedDiffTest [|
        1, 'A'
        2, 'B'
        3, 'C'
        4, 'D'
    |] "ABCD"

[<Fact>]
let ``Constrained diff test 1``(): unit =
    doConstrainedDiffTest [|
        1, 'A'
        2, 'B'
        3, 'C'
        4, 'D'
    |] "ABCDE"

[<Fact>]
let ``Constrained diff test 2``(): unit =
    doConstrainedDiffTest Array.empty "ABCD"

[<Fact>]
let ``Constrained diff test 3``(): unit =
    doConstrainedDiffTest [|
        1, 'A'
        3, 'C'
        4, 'B'
        5, 'D'
    |] "ABCD"

[<Fact>]
let ``Constrained diff test 4``(): unit =
    doConstrainedDiffTest [|
        1, 'A'
        2, 'C'
        3, 'B'
        4, 'D'
    |] "ABCDE"

[<Fact>]
let ``Constrained diff test 5``(): unit =
    doConstrainedDiffTest [|
        3, 'B'
        5, 'D'
    |] "ABCDE"

[<Fact>]
let ``Constrained diff test 6``(): unit =
    doConstrainedDiffTest [|
        1, 'B'
        2, 'A'
    |] "AB"

[<Fact>]
let ``Constrained diff test 7``(): unit =
    doConstrainedDiffTest [|
        1, 'B'
        3, 'C'
        5, 'D'
        7, 'A'
    |] "ABCD"

[<Fact>]
let ``Constrained diff test 8``(): unit =
    doConstrainedDiffTest [|
        1, '0'
        3, 'B'
        5, 'C'
        7, 'D'
        9, 'A'
    |] "0ABCD"

[<Fact>]
let ``Constrained diff test 9``(): unit =
    doConstrainedDiffTest [|
        2, 'A'
        4, 'D'
        5, 'E'
        6, 'F'
        7, 'G'
        8, 'B'
        9, 'C'
    |] "ABCDEFG"

[<Fact>]
let ``Constrained diff test 10``(): unit =
    doConstrainedDiffTest [|
        1, 'A'
        3, 'D'
        4, 'E'
        6, 'B'
        7, 'C'
    |] "ABCDE"

let private doDiffInstructionTest initial required (initialString: string) expectedInstructions allowedToInsert =
    let instructions = doDiffAndAssert initial required initialString allowedToInsert
    Assert.Equal<EditInstruction<_>>(expectedInstructions, instructions)

let private doSimpleDiffInstructionTest initial required expectedInstructions =
    doDiffInstructionTest (toSimplePositionedSequence initial) required initial expectedInstructions (fun _ -> true)

[<Fact>]
let ``Simple diff instruction test 0``(): unit =
    doSimpleDiffInstructionTest "21" "12" [|
        DeleteItem
        LeaveItem
        InsertItem '2'
    |]

[<Fact>]
let ``Simple diff instruction test 1``(): unit =
    doSimpleDiffInstructionTest "23" "123" [|
        InsertItem '1'
        LeaveItem
        LeaveItem
    |]

let private doConstrainedDiffInstructionTest (initial: IReadOnlyList<_>) order expectedInstructions =
    let initialString = constrainedSequenceToString initial
    let positionedSequence = createPositionedSequence initial
    doDiffInstructionTest positionedSequence order initialString expectedInstructions (fun idx ->
        idx = positionedSequence.MaxCoord
    )

[<Fact>]
let ``Constrained diff instruction test 0``(): unit =
    doConstrainedDiffInstructionTest [|
        1, 'A'
        3, 'C'
        4, 'B'
        5, 'D'
    |] "ABCD" [|
        LeaveItem
        InsertItem 'B'
        LeaveItem
        DeleteItem
        LeaveItem
    |]

[<Fact>]
let ``Constrained diff instruction test 1``(): unit =
    doConstrainedDiffInstructionTest [|
        1, 'B'
        2, 'A'
    |] "AB" [|
        DeleteItem
        LeaveItem
        InsertItem 'B'
    |]

[<Fact>]
let ``Constrained diff instruction test 2``(): unit =
    doConstrainedDiffInstructionTest [|
        2, 'A'
        4, 'D'
        5, 'E'
        6, 'B'
        7, 'C'
    |] "ABCDE" [|
        InsertItem 'A'
        DeleteItem
        InsertItem 'B'
        InsertItem 'C'
        LeaveItem
        LeaveItem
        DeleteItem
        DeleteItem
    |]

[<Fact>]
let ``Constrained diff instruction test 3``(): unit =
    doConstrainedDiffInstructionTest [|
        1, 'A'
        3, 'D'
        4, 'E'
        6, 'B'
        7, 'C'
    |] "ABCDE" [|
        DeleteItem
        DeleteItem
        DeleteItem
        InsertItem 'A'
        LeaveItem
        LeaveItem
        InsertItem 'D'
        InsertItem 'E'
    |]
