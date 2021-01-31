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

let private toSimplePositionedSequence items =
    let itemArray = Seq.toArray items
    { new IPositionedSequence<_> with
        member _.MaxPosition = itemArray.Length
        member _.AcceptsOn(index, item) =
            itemArray.[index] = item
    }

let private doSesLengthTest initial required expectedLength =
    let actualSesLength, _, _ = shortestEditScriptTrace initial (Seq.toArray required)
    Assert.Equal(expectedLength, actualSesLength)

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

let private createPositionedSequence(numberedItems: IReadOnlyList<_>) =
    let maxPosition = numberedItems |> Seq.map fst |> Seq.max
    let itemsByPosition = Map.ofSeq numberedItems
    { new IPositionedSequence<_> with
        member _.MaxPosition = maxPosition
        member _.AcceptsOn(index, item) =
            let position = index + 1
            match Map.tryFind position itemsByPosition with
            | None -> true
            | Some existingItem when existingItem = item -> true
            | _ -> false
    }

let private doConstrainedSesLengthTest initial required expectedLength =
    doSesLengthTest (createPositionedSequence initial) required expectedLength

[<Fact>]
let ``Constrained shortest edit script test 0``(): unit =
    doConstrainedSesLengthTest [|
        1, 'A'
        3, 'C'
        4, 'B'
        5, 'D'
    |] "ABCD" 1

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
    let sesLength, history, k = shortestEditScriptTrace (toSimplePositionedSequence seqA) seqB

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
    let trace = decypherBacktrace a (Seq.toArray b)
    Assert.Equal<int * int>(expectedTrace, trace)

let private assertSimpleDecypheredBacktraceEqual a b expectedTrace =
    assertDecypheredBacktraceEqual (toSimplePositionedSequence a) b expectedTrace

[<Fact>]
let ``Simple decyphered backtrace test 0``(): unit =
    assertSimpleDecypheredBacktraceEqual "ABCABBA" "CBABAC" [|
        7, 6
        7, 5
        5, 4
        3, 1
        1, 0
        0, 0
    |]

[<Fact>]
let ``Simple decyphered backtrace test 1``(): unit =
    assertSimpleDecypheredBacktraceEqual "23" "123" [|
        2, 3
        0, 0
    |]

let assertConditionalDecypheredBacktraceEqual a b expectedTrace =
    assertDecypheredBacktraceEqual (createPositionedSequence a) b expectedTrace

[<Fact>]
let ``Constrained decyphered backtrace test 0``(): unit =
    assertConditionalDecypheredBacktraceEqual [|
        1, 'A'
        3, 'C'
        4, 'B'
        5, 'D'
    |] "ABCD" [|
        4, 4
        4, 3
        3, 3
        0, 0
    |]

let private doDiffAndAssert initialSequence expectedResult required =
    let instructions = diff initialSequence (Seq.toArray required) |> Seq.cache
    let result = applyInstructions instructions expectedResult |> Seq.toArray |> String
    Assert.Equal(required, result)
    instructions

let private doSimpleDiffTest (initial: string) required =
    doDiffAndAssert (toSimplePositionedSequence initial) required initial |> ignore

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
    let expectedSequence = constrainedSequenceToString initial
    doDiffAndAssert (createPositionedSequence initial) required expectedSequence |> ignore

[<Fact>]
let ``Constrained diff test 0``(): unit =
    doConstrainedDiffTest [|
        1, 'A'
        3, 'C'
        4, 'B'
        5, 'D'
    |] "ABCD"

let private doDiffInstructionTest initial required expectedSequence expectedInstructions =
    let instructions = doDiffAndAssert initial required expectedSequence
    Assert.Equal<EditInstruction<_>>(expectedInstructions, instructions)

let private doSimpleDiffInstructionTest initial required expectedInstructions =
    doDiffInstructionTest (toSimplePositionedSequence initial) required initial expectedInstructions

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
    let requiredSequence = constrainedSequenceToString initial
    doDiffInstructionTest (createPositionedSequence initial) order requiredSequence expectedInstructions

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
