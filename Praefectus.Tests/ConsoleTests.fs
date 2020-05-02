module Praefectus.Tests.ConsoleTests

open Xunit

[<Fact>]
let ``Console should return 0`` () =
    let exitCode = Praefectus.EntryPoint.main [||]
    Assert.Equal(0, exitCode)
