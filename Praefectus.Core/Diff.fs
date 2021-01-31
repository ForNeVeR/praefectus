/// Implementation of the Eugene W. Myers diff algorithm [1].
///
/// TODO: Implement the constrained diff algorithm, based on an algorithm by Eugene W. Myers [1].
///
/// The original algorithm by Eugene W. Myers [1] considers a sequence edit graph in the following form (for example,
/// when trying to construct a target sequence ABCD from an initial sequence ACBD):
///
///  0   A  C  B  D
///   ·──·──·──·──·
///   │╲ │  │  │  │
///   │ ╲│  │  │  │
/// A ·──·──·──·──·
///   │  │  │╲ │  │
///   │  │  │ ╲│  │
/// B ·──·──·──·──·
///   │  │╲ │  │  │
///   │  │ ╲│  │  │
/// C ·──·──·──·──·
///   │  │  │  │╲ │
///   │  │  │  │ ╲│
/// D ·──·──·──·──·
///
/// The goal is to get to the bottom right corner, while traversing the edit graph. Horizontal movements (from left to
/// right) correspond to deletions of the corresponding item of the initial sequence; vertical movements (from top to
/// bottom) correspond to insertions of an item from the target sequence; diagonal movements (only available in places
/// where the initial and the target sequences match) correspond to leaving an item of the initial sequence.
///
/// Myers' algorithm helps to find the shortest path in this graph.
///
/// This algorithm only takes into account item sequences, and allows no constraints on their placement. For purpose of
/// this project, this is insufficient. In reality, we could have additional constraints that forbid certain paths in
/// the edit graph.
///
/// Let's consider an example of converting file sequence 1A, 3C, 4B, 5D to order ABCD with a minimal amount of renames.
/// The resulting file sequence has to be numbered, and no number should be repeated twice.
///
///  0   1A 3C 4B 5D
///   ·──·──·──·──·
///   │╲ │  ║  ║  │
///   │ ╲│  ║  ║  │
/// A ·──·──·──·──·
///   │  │  ║╲ ║  │
///   │  │  ║ ╲║  │
/// B ·──·──·──·──·
///   │  │╲ ║  ║  │
///   │  │ ╲║  ║  │
/// C ·──·──·──·──·
///   │  │  ║  ║╲ │
///   │  │  ║  ║ ╲│
/// D ·──·──·──·──·
///
/// In this graph, certain paths (marked as double lines ║) are forbidden, because they would lead to insertion of the
/// file in between of pair of another two subsequent files, which would lead to us having to renumber the latter file
/// anyway, which is essentially the same in complexity as removing a file and inserting a new one.
///
/// In other words, any vertical movements in columns between 3 and 4 are forbidden, because after 3 there's already 4,
/// and after 4 there's already 5, to we cannot insert anything for free, and the graph could be drawn as this:
///
///  0   1A 3C 4B 5D
///   ·──·──·──·──·
///   │╲ │        │
///   │ ╲│        │
/// A ·──·──·──·──·
///   │  │   ╲    │
///   │  │    ╲   │
/// B ·──·──·──·──·
///   │  │╲       │
///   │  │ ╲      │
/// C ·──·──·──·──·
///   │  │      ╲ │
///   │  │       ╲│
/// D ·──·──·──·──·
///
/// But even this is not all. Some of the paths are conditionally forbidden. For example, this path is forbidden,
/// because it would insert more than one item between 1 and 3, where there's a place only for one item:
///
///  0   1A 3C …
///   ·──·  ·
///      │
///      │
/// A ·  ·  ·
///      │
///      │
/// B ·  ·  ·
/// …
///
/// Such conditionality breaks algorithm completely, and there's no easy way to fix it without changing the structure of
/// the graph.
///
/// For such constrained scenarios, the following graph modifications are suggested:
/// 1. The initial sequence is considered with all the empty places within it. So, "1A 3C 4B 5D" becomes "1A 2_ 3C 4B
///    5D".
/// 2. Diagonal movements from any item to an item marked with "_" is allowed (as if it was equal to any item).
/// 3. No vertical movements are allowed except for the rightmost column: insertions to any place should use diagonals
///    left by empty spaces, but after the last item of the initial sequence, unlimited amount of space is available.
///
/// With these considerations, the graph for the initial task will look in the following way:
///
///  0   1A 2_ 3C 4B 5D
///   ·──·──·──·──·──·
///    ╲  ╲          │
///     ╲  ╲         │
/// A ·──·──·──·──·──·
///       ╲     ╲    │
///        ╲     ╲   │
/// B ·──·──·──·──·──·
///       ╲  ╲       │
///        ╲  ╲      │
/// C ·──·──·──·──·──·
///       ╲        ╲ │
///        ╲        ╲│
/// D ·──·──·──·──·──·
///
/// On this graph, the only best trace look like this:
/// - (0, 0) → (1A, A) (leave the file 1A)
/// - (1A, A) → (2_, B) (insert the item B as 2B)
/// - (2_, B) → (3C, C) (leave the file 3C)
/// - (3C, C) → (4B, C) (delete the item 4B)
/// - (4B, C) → (5D, D) (leave the file 5D)
///
/// So, resulting file sequence will be "1A 2B 3C 5D".
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
        abs k <= d && level.GetLowerBound 0 <= k && level.GetUpperBound 0 >= k

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

            let isOnSnake() = x' - x = y' - y

            // Here, (x', y') always points towards the end of a snake. Calculate whether snake starts from point to the
            // right or to the bottom from the current point (x, y).
            //
            // Snake itself is a line y = y_0 + x
            let y_0 = y' - x'
            let snake x = y_0 + x
            let snakeToRight = snake x < y
            let snakeToBottom = snake x > y

            while snakeToRight && not(isOnSnake()) do
                yield DeleteItem
                x <- x + 1
            while snakeToBottom && not(isOnSnake()) do
                yield InsertItem sequenceB.[y]
                y <- y + 1
            while x < x' && y < y' do // snake body itself
                yield LeaveItem
                x <- x + 1
                y <- y + 1
    }
