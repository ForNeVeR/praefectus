/// Implementation of the Eugene W. Myers diff algorithm [1].
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
/// The goal is to get from the top left corner to the bottom right corner of the graph. Horizontal movements (from left
/// to right) correspond to deletions of the corresponding item of the initial sequence; vertical movements (from top to
/// bottom) correspond to insertions of an item from the target sequence; diagonal movements (only available in places
/// where the initial and the target sequences match) correspond to leaving an item of the initial sequence.
///
/// Myers' algorithm helps to find the shortest path in this graph.
///
/// This algorithm only takes into account item sequences, and allows no constraints on their placement. For purpose of
/// this project, this is insufficient. In reality, we could have additional constraints that forbid certain paths in
/// the edit graph.
///
/// Let's consider an example of applying the order ABCD to the initial file sequence "1A 3C 4B 5D" with a minimal
/// amount of renames. The resulting file sequence has to be numbered in the target order, and no number should be
/// repeated twice.
///
///  0   1A 3C 4B 5D
///   ·──·──·──·──·
///   ║╲ │  ║  ║  │
///   ║ ╲│  ║  ║  │
/// A ·──·──·──·──·
///   ║  │  ║╲ ║  │
///   ║  │  ║ ╲║  │
/// B ·──·──·──·──·
///   ║  │╲ ║  ║  │
///   ║  │ ╲║  ║  │
/// C ·──·──·──·──·
///   ║  │  ║  ║╲ │
///   ║  │  ║  ║ ╲│
/// D ·──·──·──·──·
///
/// In this graph, certain paths (marked as double lines ║) are forbidden, because they would lead to insertion of the
/// file in between of two existing subsequent files (or with a number below 1), which would lead to us having to
/// renumber the latter file anyway, which is essentially the same in complexity as removing a file and inserting a new
/// one.
///
/// In other words, any vertical movements in columns 0, 3 and 4 are forbidden, because 1 is occupied, after 3 there's
/// already 4, and after 4 there's already 5, and we cannot insert anything there for free, so the graph could be drawn
/// as this:
///
///  0   1A 3C 4B 5D
///   ·──·──·──·──·
///    ╲ │        │
///     ╲│        │
/// A ·──·──·──·──·
///      │   ╲    │
///      │    ╲   │
/// B ·──·──·──·──·
///      │╲       │
///      │ ╲      │
/// C ·──·──·──·──·
///      │      ╲ │
///      │       ╲│
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
/// ⋮
///
/// But any separate step of this path could freely be taken in other routes, if necessary, if no two steps are taken in
/// this column across a route.
///
/// Such conditionality breaks algorithm completely, and there's no easy way to fix it without changing the structure of
/// the graph.
///
/// For such constrained scenarios, the following graph modifications are proposed:
///
/// 1. The initial sequence is considered with all the empty places within it. So, "1A 3C 4B 5D" becomes "1A 2_ 3C 4B
///    5D".
/// 2. Diagonal movement from any item to an item marked with "_" is allowed (as if such item was equal to any other
///    item).
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
/// In addition to that, sometimes it may be necessary to remove an item and insert something else instead of it, which
/// allows to take the following movement:
///
///  0   1A
///   ·──·
///      │
///      │
/// A ·  ·
///
/// This movement only allows to insert a character to an occupied position when the corresponding one was deleted
/// beforehand.
///
/// Implementation of the algorithm in this file works both for its constrained and the original variants. The selection
/// is chosen based on the provided implementation of IPositionedSequence function.
///
/// [1]: Eugene W. Myers, An O(ND) Difference Algorithm and Its Variations: Algorithmica (1986), pp. 251-266
/// (http://www.grantjenks.com/wiki/_media/ideas:diffalgorithmlcs.pdf)
module Praefectus.Core.Diff.Algorithms

open System.Collections.Generic

let shortestEditBacktrace(graph: EditGraph<'a>) =
    let rec traverse currentRoutes =
        if Array.isEmpty currentRoutes then failwithf "No routes"
        match graph.GetFinished currentRoutes with
        | Some route -> route
        | None ->
            let newRoutes = graph.StepAll currentRoutes |> Seq.toArray
            traverse newRoutes

    traverse(graph.InitialBacktraces() |> Seq.toArray)

let decypherBacktrace (sequenceA: IPositionedSequence<'a>) (sequenceB: IReadOnlyList<'a>): seq<(int * int)> =
    let graph = EditGraph(sequenceA, sequenceB)
    upcast shortestEditBacktrace graph

let myersGeneralized' (sequenceA: IPositionedSequence<'a>) (sequenceB: IReadOnlyList<'a>): EditInstruction<'a> seq =
    let graph = EditGraph(sequenceA, sequenceB)
    let forwardtrace = shortestEditBacktrace graph |> Seq.rev
    seq {
        let mutable x, y = 0, 0
        for x', y' in forwardtrace do
            let isOnSnake() = x' - x = y' - y

            // Here, (x', y') always points towards the end of a snake. Calculate whether snake starts from point to the
            // right or to the bottom from the current point (x, y).
            //
            // Snake itself is a line y = y_0 + x
            let y_0 = y' - x'
            let snake x = y_0 + x
            let snakeToRight = snake x < y
            let snakeToBottom = sequenceA.AllowedToInsertAtArbitraryPlaces && snake x > y

            while snakeToRight && not(isOnSnake()) do
                yield DeleteItem
                x <- x + 1
            while snakeToBottom && not(isOnSnake()) do
                yield InsertItem sequenceB.[y]
                y <- y + 1
            while x < x' && y < y' do // snake body itself
                yield
                    match sequenceA.GetItem x with
                    | Some _ -> LeaveItem
                    | None -> InsertItem sequenceB.[y]

                x <- x + 1
                y <- y + 1
    }
let myersGeneralized (sequenceA: IPositionedSequence<'a>) (sequenceB: IReadOnlyList<'a>): EditInstruction<'a> seq =
    let graph = EditGraph(sequenceA, sequenceB)
    let rec traverse currentRoutes =
        if Array.isEmpty currentRoutes then failwithf "No routes"
        match graph.GetFinished currentRoutes with
        | Some route -> route
        | None ->
            let newRoutes = graph.StepAll currentRoutes |> Seq.toArray
            traverse newRoutes

    let initialRoute = [ (0, 0) ]
    let route = traverse [| initialRoute |]
    graph.GetInstructions route
