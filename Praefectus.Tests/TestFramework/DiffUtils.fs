module Praefectus.Tests.TestFramework.DiffUtils

open System.Collections.Generic

open Praefectus.Core.Diff

let createPositionedSequence(numberedItems: IReadOnlyList<int * char>) =
    let maxPosition =
        if numberedItems.Count = 0 then 0
        else numberedItems |> Seq.map fst |> Seq.max
    let itemsByPosition = Map.ofSeq numberedItems
    { new IPositionedSequence<_> with
        member _.AllowedToInsertAtArbitraryPlaces = false
        member _.MaxCoord = maxPosition
        member _.GetItem coord =
            Map.tryFind coord itemsByPosition
        member _.AcceptsOn(coord, item) =
            match Map.tryFind coord itemsByPosition with
            | None -> true
            | Some existingItem when existingItem = item -> true
            | _ -> false
    }
