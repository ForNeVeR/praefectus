namespace Praefectus.Core

open System.Collections.Generic

[<RequireQualifiedAccess>]
type ScalarDataType =
| Boolean
| Enum of IReadOnlyCollection<string>
| String
| Integer
| Double
| Timestamp
| TaskReference

[<RequireQualifiedAccess>]
type DataType =
| Scalar of ScalarDataType
| List of ScalarDataType
with
    static member Boolean: DataType = Scalar ScalarDataType.Boolean
    static member Enum(members: IReadOnlyCollection<string>): DataType = Scalar(ScalarDataType.Enum members)
    static member String: DataType = Scalar ScalarDataType.String
    static member Integer: DataType = Scalar ScalarDataType.Integer
    static member Double: DataType = Scalar ScalarDataType.Double
    static member Timestamp: DataType = Scalar ScalarDataType.Timestamp
    static member TaskReference: DataType = Scalar ScalarDataType.TaskReference

[<Struct>]
type AttributeIdentification = {
    Namespace: string
    Id: string
}

type Attribute = {
    Id: AttributeIdentification
    Type: DataType
    Description: string
}

/// Core attribute definitions. Core attributes are different from other ones in that they cannot be changed by the
/// user.
module CoreAttributes =
    let CoreNamespace: string = "praefectus"
    let private coreAttribute id dataType description = {
        Id = {
            Namespace = CoreNamespace
            Id = id
        }
        Type = dataType
        Description = description
    }
    let Id = coreAttribute "id" DataType.String "A task identifier."
    let Title = coreAttribute "title" DataType.String "A short task title description or summary."
    let Created = coreAttribute "created" DataType.Timestamp "Date of task creation."
    let Updated = coreAttribute "updated" DataType.Timestamp "Date of the last task update."
    let Status = coreAttribute "status" (DataType.Enum [|"Open"; "InProgress"; "Done"; "Deleted"|]) "A task status."

/// Default attribute definitions. Default attributes are generated in a database by default; they may be changed by the
/// user afterwards.
module DefaultAttributes =
    let DependsOn = {
        Id = {
            Namespace = CoreAttributes.CoreNamespace
            Id = "depends-on"
        }
        Type = DataType.List ScalarDataType.TaskReference
        Description = "A list of tasks this one depends on."
    }
    let ActuallyDependsOn = {
        Id = {
            Namespace = CoreAttributes.CoreNamespace
            Id = "actually-depends-on"
        }
        Type = DataType.List ScalarDataType.TaskReference
        Description = "A set of unresolved tasks (i.e. not Done or Deleted) this one depends on."
    }
