module Praefectus.Core.Ordering

open System.Collections.Generic

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

let applyOrderInStorage<'ss when 'ss : equality>(getNewState: int -> Task<'ss> -> 'ss)
                                                (orderedTasks: IReadOnlyList<Task<'ss>>): StorageInstruction<'ss> seq =
    let initialTasks = orderedTasks |> Seq.sortBy(fun t -> t.Order) |> Seq.toArray
    let instructions = Diff.diff initialTasks orderedTasks
    seq {
        use initialTaskEnumerator = (initialTasks :> IEnumerable<_>).GetEnumerator()
        initialTaskEnumerator.MoveNext() |> ignore

        let mutable currentFreeOrder = 1
        for instruction in instructions do
            match instruction with
            | Diff.DeleteItem -> initialTaskEnumerator.MoveNext() |> ignore
            | Diff.LeaveItem ->
                initialTaskEnumerator.MoveNext() |> ignore
                let currentTask = initialTaskEnumerator.Current
                currentFreeOrder <- currentTask.Order.Value + 1
            | Diff.InsertItem(newTask) ->
                let newOrder = currentFreeOrder
                let newState = getNewState newOrder newTask
                yield {
                    Task = { newTask with Order = Some newOrder }
                    NewState = newState
                }
                currentFreeOrder <- currentFreeOrder + 1
    }
