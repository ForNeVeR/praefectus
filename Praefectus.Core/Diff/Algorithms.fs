// SPDX-FileCopyrightText: 2025 Friedrich von Never <friedrich@fornever.me>
//
// SPDX-License-Identifier: MIT

/// Implementation of the diff algorithm for constrained conditions [1], based on Eugene W. Myers diff algorithm [2].
///
/// Implementation of the algorithm in this file works both for its constrained and the original variants. The selection
/// is chosen based on the provided implementation of IPositionedSequence function.
///
/// [1]: Friedrich von Never, A variant of a diff algorithm for constrained conditions
/// (https://fornever.me/en/posts/2021-02-12.constrained-diff-algorithm.html)
/// [2]: Eugene W. Myers, An O(ND) Difference Algorithm and Its Variations: Algorithmica (1986), pp. 251-266
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
