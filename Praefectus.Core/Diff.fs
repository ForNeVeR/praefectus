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
let shortestEditScriptLength (a: IReadOnlyList<_>) (b: IReadOnlyList<_>) =
    let M = b.Count
    let N = a.Count
    let MAX = M + N

    if MAX < 1 then 0 else

    let mutable V = Array.CreateInstance(typeof<int>, [| MAX * 2 + 1 |], [| -MAX |])
    let V_set (index: int) value = V.SetValue(value, index)
    let V_get (index: int) = V.GetValue(index) :?> int

    V_set 1 0
    let mutable ses = None
    let mutable D = 0
    while D <= MAX && ses.IsNone do
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
                ses <- Some D
        D <- D + 1

    match ses with
    | Some x -> x
    | None ->
        failwithf "Algorithmic error: length of shortest edit script is greater than %d" MAX

let diff (sequenceA: 'a seq) (sequenceB: 'a seq): EditInstruction<'a> seq =
    failwith "TODO: implement"

