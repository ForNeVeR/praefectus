namespace Praefectus.Core.Diff

open System.Collections.Generic

type EditGraphBacktrace = list<int * int>

type EditGraph<'a>(sequenceA: IPositionedSequence<'a>, sequenceB: IReadOnlyList<'a>) =
    let maxX = sequenceA.MaxCoord
    let maxY = sequenceB.Count

    let getItemA coordX = sequenceA.GetItem coordX
    let getItemB coordY = sequenceB.[coordY - 1]

    let allowedStepRight = function
    | [] -> failwith "Empty route detected"
    | (x, y) :: _ -> (getItemA(x + 1)).IsSome

    let allowedStepDown = function
    | [] -> failwith "Empty route detected"
    | (x, y) :: _ when sequenceA.AllowedToInsertAtArbitraryPlaces -> x <= maxX && y < maxY
    | (x_1, y_1) :: (x_0, y_0) :: _ when not sequenceA.AllowedToInsertAtArbitraryPlaces && x_1 = x_0 + 1 && y_1 = y_0 ->
        // only in case of previous step was a step right
        x_1 <= maxX && y_1 < maxY
    | (x, y) :: _ -> x = maxX && y < maxY
    | _ -> false

    let allowedDiagonalStep = function
    | [] -> failwith "Empty route detected"
    | (x, y) :: _ -> x < maxX && y < maxY && sequenceA.AcceptsOn(x + 1, getItemB(y + 1))

    let makeDiagonalSteps = function
    | [] -> failwith "Empty route detected"
    | ((x, y) :: _) as route when allowedDiagonalStep route ->
        let mutable newRoute = route
        let mutable x, y = x, y
        while allowedDiagonalStep newRoute do
            x <- x + 1
            y <- y + 1
            newRoute <- (x, y) :: route
        newRoute
    | route -> route

    let processDiagonalSteps initialRoute derivedRoutes =
        seq {
            if allowedDiagonalStep initialRoute then
                yield makeDiagonalSteps initialRoute
            yield! (derivedRoutes |> Seq.collect(fun route -> seq {
                yield route // choose not to take diagonal step
                if allowedDiagonalStep route then
                    yield makeDiagonalSteps route
            }))
        }

    let appendPossibleSteps = function
    | [] -> failwith "Empty route detected"
    | ((x, y) :: _) as route ->
        seq {
            if allowedStepRight route then
                (x + 1, y) :: route
            if allowedStepDown route then
                (x, y + 1) :: route
        } |> processDiagonalSteps route

    let minifyBacktraces routes =
        Seq.groupBy List.head routes
        |> Seq.map(fun (_, group) -> Seq.head group)

    let isFinishedRoute = function
    | [] -> failwith "Empty route detected"
    | (x, y) :: _ -> x = maxX && y = maxY

    member _.InitialBacktraces(): EditGraphBacktrace seq =
        let trace = [0, 0]
        Seq.singleton(makeDiagonalSteps trace)

    member _.StepAll(routes: EditGraphBacktrace seq): EditGraphBacktrace seq =
        Seq.collect appendPossibleSteps routes
        |> minifyBacktraces

    member _.GetFinished(routes: EditGraphBacktrace seq): EditGraphBacktrace option =
        Seq.tryFind isFinishedRoute routes

    member _.GetInstructions(route: EditGraphBacktrace): EditInstruction<'a> seq =
        let route = Seq.rev route
        seq {
            let mutable x, y = 0, 0
            for targetX, targetY in route do
                while x < targetX && y = targetY do
                    yield DeleteItem
                    x <- x + 1
                while x = targetX && y < targetY do
                    yield InsertItem sequenceB.[y]
                    y <- y + 1
                while x < targetX && y < targetY do
                    x <- x + 1
                    y <- y + 1
                    yield
                        match getItemA x with
                        | Some _ -> LeaveItem
                        | None -> InsertItem(getItemB y)
        }
