namespace Praefectus.Core.Diff

open System.Collections.Generic

type EditGraphRoute = seq<struct (int * int)>

type EditGraph<'a>(sequenceA: IPositionedSequence<'a>, sequenceB: IReadOnlyList<'a>) =
    class end
