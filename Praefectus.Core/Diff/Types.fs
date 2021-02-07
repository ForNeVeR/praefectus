namespace Praefectus.Core.Diff

type EditInstruction<'a> =
    | DeleteItem
    | InsertItem of 'a
    | LeaveItem

type IPositionedSequence<'a> =
    // TODO: Drop this flag, since it is the only mode we use in production code
    abstract AllowedToInsertAtArbitraryPlaces: bool
    abstract MaxCoord: int
    abstract GetItem: coord: int -> 'a option
    abstract AcceptsOn: coord: int * item: 'a -> bool
