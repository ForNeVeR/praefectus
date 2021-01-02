/// Implementation of the Eugene W. Myers diff algorithm [1].
///
/// [1]: Eugene W. Myers, An O(ND) Difference Algorithm and Its Variations: Algorithmica (1986), pp. 251-266
/// (http://www.grantjenks.com/wiki/_media/ideas:diffalgorithmlcs.pdf)
module Praefectus.Core.Diff

type EditInstruction<'a> =
    | DeleteItem
    | InsertItem of 'a
    | LeaveItem

let diff (sequenceA: 'a seq) (sequenceB: 'a seq): EditInstruction<'a> seq =
    failwith "TODO: implement"
