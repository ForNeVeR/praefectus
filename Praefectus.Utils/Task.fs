module Praefectus.Utils.Task

open System.Threading.Tasks

let RunSynchronously(task: Task<'a>): 'a =
    task.GetAwaiter().GetResult()
