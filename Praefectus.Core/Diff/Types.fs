namespace Praefectus.Core.Diff

type EditInstruction<'a> =
    | DeleteItem
    | InsertItem of 'a
    | LeaveItem

type IPositionedSequence<'a> =
    // TODO: Drop this flag, since it is the only mode we use in production code
    abstract AllowedToInsertAtArbitraryPlaces: bool
    abstract MaxOrder: int
    abstract GetItem: index: int -> 'a option
    abstract AcceptsOn: index: int * item: 'a -> bool
