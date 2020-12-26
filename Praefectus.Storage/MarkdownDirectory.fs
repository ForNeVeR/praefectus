module Praefectus.Storage.MarkdownDirectory

open System
open System.Globalization
open System.IO
open System.Text

open Markdig
open Markdig.Extensions.Yaml

open Markdig.Renderers.Normalize
open Markdig.Syntax
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

    let private toText document =
        use writer = new StringWriter()
        let renderer = NormalizeRenderer writer
        renderer.Render document |> ignore
        writer.ToString()

    let read(stream: Stream) = async {
        use reader = new StreamReader(stream, leaveOpen = true)
        let! content = Async.AwaitTask <| reader.ReadToEndAsync()

        let document = Markdown.Parse(content, pipeline)
        let metadata =
            match Seq.tryHead document with
            | Some(:? YamlFrontMatterBlock as frontMatter) ->
                let metadata = parseYamlFrontMatterMetadata frontMatter

                document.RemoveAt 0
                metadata
            | _ -> None
        let title =
            match Seq.tryHead document with
            | Some(:? HeadingBlock as heading) when heading.Level = 1 ->
                document.RemoveAt 0
                Some(toText heading.Inline)
            | _ -> None
        let content = toText document
        return metadata, title, content
    }

let private applyMetadata task = function
    | None -> task
    | Some metadata ->
        let pick (a: _ option) (b: _ option) = Seq.tryPick id (seq { a; b })
        { task with
            Order = pick metadata.Order task.Order
            Id = pick metadata.Id task.Id
            Name = pick metadata.Name task.Name
            Title = pick metadata.Title task.Title
            Description = pick metadata.Description task.Description
            Status = metadata.Status
            DependsOn = metadata.DependsOn }

let readTask (filePath: string) (stream: Stream) = async {
    let (order, id, name) = FileName.readAttributes filePath
    let! (metadata, title, content) = Markdown.read stream
    let task = {
        Order = order
        Id = id
        Name = name
        Title = title
        Description = Some content
        Status = None
        DependsOn = Array.empty
    }

    return applyMetadata task metadata
}

let writeTask (task: Task) (stream: Stream) = async {
    use writer = new StreamWriter(stream, leaveOpen = true)

    let text =
        match (task.Title, task.Description) with
        | (Some title, Some description) -> String.Format("# {0}\n\n{1}\n", title, description)
        | (Some title, None) -> String.Format("# {0}\n", title)
        | (None, Some description) -> String.Format("{0}\n", description)
        | (None, None) -> ""

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
