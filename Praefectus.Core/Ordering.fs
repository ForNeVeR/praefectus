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

let applyOrderInStorage<'ss when 'ss : equality>(orderedTasks: Task<'ss> seq): StorageInstruction<'ss> seq =
    failwith "TODO: implement"
