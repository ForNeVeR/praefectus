module Praefectus.Core.Ordering

open System.Collections.Generic
open System.Linq

open Praefectus.Core.Diff.Algorithms

type TaskPredicate<'ss> when 'ss : equality = Task<'ss> -> bool

/// Reorders the tasks according to the ordering configured. Returns a collection in the right order, don't change the
/// tasks' Order property.
let reorder<'ss when 'ss : equality>(ordering: IReadOnlyCollection<TaskPredicate<'ss>>)
                                    (tasks: Task<'ss> seq): Task<'ss> seq =
    let getOrder task =
        ordering
        |> Seq.tryFindIndex (fun p -> p task)
        |> Option.defaultValue ordering.Count // move any unmatched items to the end
    Seq.sortBy getOrder tasks

let private createPositionedSequence (tasks: IReadOnlyList<Task<'a>>) =
    let itemDictionary =
        tasks
        |> Seq.map(fun t -> Option.defaultValue 0 t.Order, t)
        |> fun seq -> Enumerable.ToDictionary(seq, fst, snd)
    let orderDictionary =
        tasks
        |> Seq.map(fun t -> t, Option.defaultValue 0 t.Order)
        |> fun seq -> Enumerable.ToDictionary(seq, fst, snd)
    let occupiedIndices = Set.ofSeq orderDictionary.Values
    let maxOrder = Seq.max orderDictionary.Values

    let indexToOrder i = i + 1

    { new Diff.IPositionedSequence<_> with
        member _.AllowedToInsertAtArbitraryPlaces = false
        member _.MaxOrder = maxOrder
        member _.GetItem index =
            match itemDictionary.TryGetValue(indexToOrder index) with
            | true, item -> Some item
            | false, _ -> None
        member _.AcceptsOn(index, item) =
            let position = indexToOrder index
            if not(Set.contains position occupiedIndices) then true
            else
                match orderDictionary.TryGetValue item with
                | true, order -> order = position
                | false, _ -> false
    }

let applyOrderInStorage<'ss when 'ss : equality>(getNewState: int -> Task<'ss> -> 'ss)
                                                (orderedTasks: IReadOnlyList<Task<'ss>>): StorageInstruction<'ss> seq =
    let initialTasks = orderedTasks |> Seq.sortBy(fun t -> t.Order) |> Seq.toArray
    let positionedSequence = createPositionedSequence initialTasks
    let instructions = myersGeneralized positionedSequence orderedTasks
    seq {
        use initialTaskEnumerator = (initialTasks :> IEnumerable<_>).GetEnumerator()

        let mutable currentFreeOrder = 1
        for instruction in instructions do
            match instruction with
            | Diff.DeleteItem -> initialTaskEnumerator.MoveNext() |> ignore
            | Diff.LeaveItem ->
                initialTaskEnumerator.MoveNext() |> ignore
                let currentTask = initialTaskEnumerator.Current
                if currentTask.Order.Value < currentFreeOrder then
                    let newOrder = currentFreeOrder
                    yield {
                        Task = { currentTask with Order = Some newOrder }
                        NewState = getNewState newOrder currentTask
                    }
                    currentFreeOrder <- newOrder + 1
                else
                    currentFreeOrder <- currentTask.Order.Value + 1
            | Diff.InsertItem(newTask) ->
                let newOrder = currentFreeOrder
                let newState = getNewState newOrder newTask
                yield {
                    Task = { newTask with Order = Some newOrder }
                    NewState = newState
                }
                currentFreeOrder <- newOrder + 1
    }
