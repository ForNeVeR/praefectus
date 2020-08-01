module Praefectus.Tests.Storage.JsonTests

open System
open System.IO
open System.Text

open Quibble.Xunit
open VerifyXunit
open Xunit

open Praefectus.Core
open Praefectus.Storage

let private createTask id title av = {
    Id = id
    Title = title
    Created = DateTimeOffset.UnixEpoch
    Updated = DateTimeOffset.UnixEpoch
    Status = TaskStatus.Open
    AttributeValues = Map.ofArray av
}

let private testDatabase =
    { Database.defaultDatabase with
        Tasks = [|
            createTask "Task1" "Perform tests" [|
                DefaultAttributes.DependsOn.Id, AttributeValue.TaskReference "Task1"
            |]
        |]
    }
[<Fact>]
let ``Json should serialize the database successfully``(): unit =
    Async.RunSynchronously <| async {
        let database = testDatabase
        use stream = new MemoryStream()
        do! Json.save database stream
        Assert.NotEqual(0L, stream.Length)
    }

let private rewindStream(stream: Stream) =
    stream.Position <- 0L

let private streamToString(stream: MemoryStream) =
    Encoding.UTF8.GetString(stream.ToArray())

let private assertEqual expected actual: Async<unit> =
    let serialize x = async {
        use stream = new MemoryStream()
        do! Json.save x stream
        return streamToString stream
    }

    async {
        let! expected = serialize expected
        let! actual = serialize actual
        JsonAssert.Equal(expected, actual)
    }

let ``Json should be able to load the database after save``(): unit =
    Async.RunSynchronously <| async {
        let database = testDatabase
        use stream = new MemoryStream()
        do! Json.save database stream
        rewindStream stream

        let! newDatabase = Json.load stream
        do! assertEqual database newDatabase
    }

[<UsesVerify>]
module DeserializationTests =
    let private createId x = { Namespace = "praefectus.tests"; Id = x }
    let private createAttribute id dataType = {
        Id = createId id
        Type = dataType
        Description = ""
    }

    let private allAttributes = [|
        createAttribute "boolean" DataType.Boolean
        createAttribute "enum" (DataType.Enum [| "A"; "B"; "C" |])
        createAttribute "string" DataType.String
        createAttribute "integer" DataType.Integer
        createAttribute "double" DataType.Double
        createAttribute "timestamp" DataType.Timestamp
        createAttribute "taskReference" DataType.TaskReference
        createAttribute "taskReferenceList" (DataType.List ScalarDataType.TaskReference)
    |]

    let private databaseWithEveryAttribute = {
        Metadata = allAttributes
        Tasks = [||]
    }

    let private verifyDatabase database =
        Async.RunSynchronously <| async {
            use stream = new MemoryStream()
            do! Json.save database stream
            let string = streamToString stream

            return! Async.AwaitTask(Verifier.Verify string)
        }

    [<Fact>]
    let everyAttributeType(): unit =
        verifyDatabase databaseWithEveryAttribute

    let private allAttributeValues = [|
        createId "boolean", AttributeValue.Boolean true
        createId "enum", AttributeValue.Enum "X"
        createId "string", AttributeValue.String "Z"
        createId "integer", AttributeValue.Integer 123
        createId "double.nan", AttributeValue.Double Double.NaN
        createId "double.maxValue", AttributeValue.Double Double.MaxValue
        createId "double.inf", AttributeValue.Double Double.PositiveInfinity
        createId "double.epsilon", AttributeValue.Double Double.Epsilon
        createId "timestamp", AttributeValue.Timestamp DateTimeOffset.UnixEpoch
        createId "taskReference", AttributeValue.TaskReference "Task1"
        createId "taskReferenceList", AttributeValue.List [|
            ScalarAttributeValue.TaskReference "Task1"
            ScalarAttributeValue.TaskReference "Task2"
        |]
    |]

    let private databaseWithEveryAttributeValue =
        { databaseWithEveryAttribute with
            Tasks = [|
                createTask "Task1" "Use all the attributes" allAttributeValues
            |]
        }

    [<Fact>]
    let everyAttributeValue(): unit =
        verifyDatabase databaseWithEveryAttributeValue
