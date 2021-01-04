/// Implementation of the Eugene W. Myers diff algorithm [1].
///
/// [1]: Eugene W. Myers, An O(ND) Difference Algorithm and Its Variations: Algorithmica (1986), pp. 251-266
/// (http://www.grantjenks.com/wiki/_media/ideas:diffalgorithmlcs.pdf)
module Praefectus.Core.Diff

open System
open System.Collections.Generic

type EditInstruction<'a> =
    | DeleteItem
    | InsertItem of 'a
    | LeaveItem

/// Fig. 2. The greedy LCS/SES algorithm.
let shortestEditScriptTrace<'a when 'a : equality> (a: IReadOnlyList<'a>)
                                                   (b: IReadOnlyList<'a>): int * IReadOnlyList<Array> * int =
    let M = b.Count
    let N = a.Count
    let MAX = M + N

    if MAX < 1 then 0, upcast [| [| 0 |] :> Array |], 0 else

    let mutable V = Array.CreateInstance(typeof<int>, [| MAX * 2 + 1 |], [| -MAX |])
    let V_set (index: int) value = V.SetValue(value, index)
    let V_get (index: int) = V.GetValue(index) :?> int

    let vHistory = ResizeArray()
    vHistory.Add V

    V_set 1 0
    let mutable sesLengthAndLastK = None
    let mutable D = 0
    while D <= MAX && sesLengthAndLastK.IsNone do
        for k in -D .. 2 .. D do
            let mutable x =
                if k = -D || (k <> D && V_get(k - 1) < V_get(k + 1)) then
                    V_get(k + 1)
                else
                    V_get(k - 1) + 1
            let mutable y = x - k
            while x < N && y < M && a.[x] = b.[y] do
                x <- x + 1
                y <- y + 1
            V_set k x
            if x >= N && y >= M then
                sesLengthAndLastK <- Some (D, k)

        if sesLengthAndLastK = None then
            V <- V.Clone() :?> Array
            vHistory.Add V

            D <- D + 1

    match sesLengthAndLastK with
    | Some(sesLength, lastK) -> sesLength, upcast vHistory, lastK
    | None ->
        failwithf "Algorithmic error: length of shortest edit script is greater than %d" MAX

let decypherBacktrace (sequenceA: IReadOnlyList<'a>) (sequenceB: IReadOnlyList<'a>): (int * int) seq =
    let (sesLength, vHistory, lastK) = shortestEditScriptTrace sequenceA sequenceB

    let getXYFromDK d (k: int) =
        let level = vHistory.[d]
        let x = level.GetValue k :?> int
        let y = x - k
        x, y

    let validIndex d k =
        let level = vHistory.[d]
        level.GetLowerBound 0 <= k && level.GetUpperBound 0 >= k

    seq {
        let mutable k = lastK
        for d = sesLength downto 0 do
            let (x, y) = getXYFromDK d k
            yield x, y
            if d <> 0 then
                let possibleCandidates = seq {
                    if validIndex (d - 1) (k - 1) then
                        k - 1, getXYFromDK (d - 1) (k - 1)
                    if validIndex (d - 1) (k + 1) then
                        k + 1, getXYFromDK (d - 1) (k + 1)
                }

                let bestCandidate = possibleCandidates |> Seq.maxBy (fun (_, (x, _)) -> x)
                let (k', _) = bestCandidate
                k <- k'
    }

let diff (sequenceA: IReadOnlyList<'a>) (sequenceB: IReadOnlyList<'a>): EditInstruction<'a> seq =
    let forwardtrace = decypherBacktrace sequenceA sequenceB |> Seq.rev
    seq {
        let mutable x, y = 0, 0
        use enumerator = forwardtrace.GetEnumerator()
        while enumerator.MoveNext() do
            let x', y' = enumerator.Current
            let diagonalAllowed() =
                x' - x = y' - y

            while not(diagonalAllowed()) && y < y' do
                yield InsertItem sequenceB.[y]
                y <- y + 1
            while x < x' && not(diagonalAllowed()) do
                yield DeleteItem
                x <- x + 1
            while x < x' && y < y' do
                yield LeaveItem
                x <- x + 1
                y <- y + 1
    } |> Seq.toArray |> Seq.ofArray
