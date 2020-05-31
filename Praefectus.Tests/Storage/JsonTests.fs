module Praefectus.Tests.Storage.JsonTests

open System.IO
open System.Text

open Quibble.Xunit
open Xunit

open Praefectus.Core
open Praefectus.Storage

[<Fact>]
let ``Json should serialize the database successfully``(): unit =
    Async.RunSynchronously <| async {
        let database = Database.empty
        use stream = new MemoryStream()
        do! Json.save database stream
        Assert.NotEqual(0L, stream.Length)
    }

let private rewindStream(stream: Stream) =
    stream.Position <- 0L

let private assertEqual expected actual: Async<unit> =
    let serialize x = async {
        use stream = new MemoryStream()
        do! Json.save x stream
        return Encoding.UTF8.GetString(stream.ToArray())
    }

    async {
        let! expected = serialize expected
        let! actual = serialize actual
        JsonAssert.Equal(expected, actual)
    }

let ``Json should be able to load the database after save``(): unit =
    Async.RunSynchronously <| async {
        let database = Database.empty
        use stream = new MemoryStream()
        do! Json.save database stream
        rewindStream stream

        let! newDatabase = Json.load stream
        do! assertEqual database newDatabase
    }
