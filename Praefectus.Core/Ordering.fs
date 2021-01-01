module Praefectus.Core.Ordering

type TaskPredicate<'ss> when 'ss : equality = Task<'ss> -> bool

/// Reorders the tasks according to the ordering configured. Returns a collection in the right order, don't change the
/// tasks' Order property.
let reorder<'ss when 'ss : equality>(ordering: TaskPredicate<'ss> seq) (tasks: Task<'ss> seq): Task<'ss> seq =
    failwith "TODO: implement"

let applyOrderInStorage<'ss when 'ss : equality>(orderedTasks: Task<'ss> seq): StorageInstruction<'ss> seq =
    failwith "TODO: implement"
