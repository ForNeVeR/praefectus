module Praefectus.Tests.Core.DiffTests

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
    let actualSesLength = shortestEditScriptLength (Seq.toArray initial) (Seq.toArray required)
    Assert.Equal(expectedLength, actualSesLength)

[<Fact>]
let ``Shortest edit script test 1``(): unit = doSesLengthTest "" "" 0
[<Fact>]
let ``Shortest edit script test 2``(): unit = doSesLengthTest "" "a" 1
[<Fact>]
let ``Shortest edit script test 3``(): unit = doSesLengthTest "a" "" 1
[<Fact>]
let ``Shortest edit script test 4``(): unit = doSesLengthTest "abc" "b" 2

let private doDiffTest initial required =
    let instructions = diff initial required
    let result = applyInstructions instructions initial
    Assert.Equal<'a>(initial, result)

[<Fact>]
let ``Diff algorithm works on simple cases``(): unit =
    doDiffTest "" ""
    doDiffTest "abcabc" "abc"
    doDiffTest "abcabc" ""
    doDiffTest "" "abcabc"
    doDiffTest "abcdef" "ade"
    doDiffTest "abcdef" "fedcba"
    doDiffTest "abcdef" "afedcbabcd"
