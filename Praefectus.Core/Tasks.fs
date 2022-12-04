namespace Praefectus.Core

open System
open System.Collections.Generic
open System.Linq

type TaskStatus =
| Open = 0
| InProgress = 1
| Done = 2
| Deleted = 3

[<CustomEquality; NoComparison>]
type Task<'StorageState> when 'StorageState : equality =
    {
        Order: int option
        Id: string option
        Name: string option
        Title: string option
        Description: string option
        Status: TaskStatus option
        DependsOn: IReadOnlyCollection<string>
        StorageState: 'StorageState
    }
    with
        override this.Equals other =
            match other with
            | :? Task<'StorageState> as other when LanguagePrimitives.PhysicalEquality this other -> true
            | :? Task<'StorageState> as other ->
                this.Order = other.Order
                    && this.Id = other.Id
                    && this.Name = other.Name
                    && this.Title = other.Title
                    && this.Description = other.Description
                    && this.Status = other.Status
                    && Enumerable.SequenceEqual(this.DependsOn, other.DependsOn)
                    && this.StorageState = other.StorageState
            | _ -> false
        override this.GetHashCode() =
            HashCode.Combine(this.Order, this.Id, this.Name, this.Title, this.Description, this.Status)
            // NOTE: this.DependsOn is omitted purposefully: it shouldn't break correctness of the implementation, and
            // currently there're no known cases when this matters.

        static member Empty<'ss when 'ss : equality>(ss: 'ss): Task<'ss> = {
            Order = None
            Id = None
            Name = None
            Title = None
            Description = None
            Status = None
            DependsOn = Array.empty
            StorageState = ss
        }
