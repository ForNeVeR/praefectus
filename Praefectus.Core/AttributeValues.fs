namespace Praefectus.Core

open System
open System.Collections.Generic

[<RequireQualifiedAccess>]
type ScalarAttributeValue =
| Boolean of bool
| Enum of string
| String of string
| Integer of int32
| Double of double
| Timestamp of DateTimeOffset
| TaskReference of string

[<RequireQualifiedAccess>]
type AttributeValue =
| Scalar of ScalarAttributeValue
| List of IReadOnlyCollection<ScalarAttributeValue>
with
    static member Boolean(value: bool): AttributeValue = Scalar(ScalarAttributeValue.Boolean value)
    static member Enum(value: string): AttributeValue = Scalar(ScalarAttributeValue.Enum value)
    static member String(value: string): AttributeValue = Scalar(ScalarAttributeValue.String value)
    static member Integer(value: int): AttributeValue = Scalar(ScalarAttributeValue.Integer value)
    static member Double(value: double): AttributeValue = Scalar(ScalarAttributeValue.Double value)
    static member Timestamp(value: DateTimeOffset): AttributeValue = Scalar(ScalarAttributeValue.Timestamp value)
    static member TaskReference(value: string): AttributeValue = Scalar(ScalarAttributeValue.TaskReference value)
