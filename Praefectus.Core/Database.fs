namespace Praefectus.Core

open System.Collections.Generic

type Database<'StorageState> when 'StorageState : equality = {
    Tasks: IReadOnlyCollection<Task<'StorageState>>
}

type StorageInstruction<'StorageState> when 'StorageState : equality = {
    Task: Task<'StorageState>
    NewState: 'StorageState
}
