module Praefectus.Storage.MarkdownDirectory

open System
open System.Globalization
open System.IO
open System.Text

open Markdig
open Markdig.Extensions.Yaml

open Praefectus.Core

module FileName =
    let internal generate task =
        let append (x: string option) (builder: StringBuilder) =
            match x with
            | Some(x: string) -> builder.AppendFormat("{0}.", x)
            | None -> builder

        (StringBuilder()
        |> append(task.Order |> Option.map string)
        |> append task.Id
        |> append task.Name)
            .Append("md")
            .ToString()

    let readAttributes(filePath: string) =
        let fileName = Path.GetFileNameWithoutExtension filePath
        let components = fileName.Split(".")
        match components.Length with
        | 0 -> None, None, None
        | _ ->
            let (order, idIndex) =
                match Int32.TryParse(components.[0], NumberStyles.None, CultureInfo.InvariantCulture) with
                | (true, order) -> (Some order, 1)
                | (false, _) -> (None, 0)

            match components.Length - idIndex with
            | 0 -> order, None, None
            | 1 -> order, None, Some components.[idIndex]
            | _ -> order, Some components.[idIndex], Some <| String.Join('.', Seq.skip (idIndex + 1) components)

module private Markdown =
    let private pipeline =
        MarkdownPipelineBuilder()
            .UseYamlFrontMatter()
            .Build()

    let private parseYamlFrontMatterMetadata _block =
        failwith "TODO: Parse YAML data"

    let read(stream: Stream) = async {
        use reader = new StreamReader(stream, leaveOpen = true)
        let! content = Async.AwaitTask <| reader.ReadToEndAsync()

        let document = Markdown.Parse(content, pipeline)
        return
            match Seq.tryHead document with
            | Some(:? YamlFrontMatterBlock as frontMatter) ->
                let metadata = parseYamlFrontMatterMetadata frontMatter

                document.RemoveAt 0
                let trimmedContent = document.ToString()
                metadata, trimmedContent
            | _ -> None, content
    }

let private emptyTask = {
    Id = None
    Order = None
    Name = None
    Title = None
    Description = None
    Status = None
    DependsOn = Array.empty
}

let private applyMetadata task = function
    | Some metadata ->
        let merge (o1: _ option) o2 = if o1.IsSome then o1 else o2
        { task with
            Order = merge metadata.Order task.Order
            Id = merge metadata.Id task.Id
            Name = merge metadata.Name task.Name
            Title = merge metadata.Title task.Title
            Status = metadata.Status
            DependsOn = metadata.DependsOn
        }
    | None -> task

let readTask (filePath: string) (stream: Stream) = async {
    let (order, id, name) = FileName.readAttributes filePath
    let task =
        { emptyTask with
            Order = order
            Id = id
            Name = name
        }

    let! (metadata, content) = Markdown.read stream
    return { applyMetadata task metadata
                with Description = Some content }
}

let writeTask (task: Task) (stream: Stream) = async {
    use writer = new StreamWriter(stream, leaveOpen = true)
    let text = Option.defaultValue "" task.Description
    do! Async.AwaitTask(writer.WriteAsync(text))
}

let readDatabase (directory: string): Async<Database> = async {
    let! tasks =
        Directory.GetFileSystemEntries(directory, "*.md")
        |> Array.map(fun path -> async {
            use stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read)
            return! readTask path stream
        })
        |> Async.Sequential
    return { Tasks = tasks }
}

let saveDatabase (database: Database) (directory: string): Async<unit> = async {
    for task in database.Tasks do
        let path = Path.Combine(directory, FileName.generate task)
        use stream = File.Open(path, FileMode.Create, FileAccess.Write, FileShare.Read)
        do! writeTask task stream
}
