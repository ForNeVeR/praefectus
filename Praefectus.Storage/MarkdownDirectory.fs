module Praefectus.Storage.MarkdownDirectory

open System
open System.Collections.Generic
open System.IO
open System.Text

open Markdig
open Markdig.Extensions.Yaml
open Markdig.Renderers.Normalize
open Markdig.Syntax
open YamlDotNet.Serialization

open Praefectus.Core
open Praefectus.Storage.FileSystemStorage

module private Yaml =
    let private deserializer = DeserializerBuilder().Build()
    let read(text: string) =
        deserializer.Deserialize<Dictionary<string, obj>> text

    let private serializer = SerializerBuilder().Build()
    let write value =
        use writer = new StringWriter(NewLine = "\n")
        serializer.Serialize(writer, value)
        writer.ToString()

module private Markdown =
    let private pipeline =
        MarkdownPipelineBuilder()
            .UseYamlFrontMatter()
            .Build()

    let private parseYamlFrontMatterMetadata (block: YamlFrontMatterBlock) =
        let yamlText = block.Lines.ToString()
        let metadata = Yaml.read yamlText

        let readValue key mapping =
            match metadata.TryGetValue key with
            | (true, v) -> Some(mapping (v :?> 'a))
            | (false, _) -> None

        let readInt32 key = readValue key (fun (s: string) -> int s)
        let readString key: string option = readValue key id
        let readEnum key = readValue key (fun s -> Enum.Parse(s, ignoreCase = true))
        let readList key =
            readValue key Seq.cast<string>
            |> Option.defaultValue Seq.empty
            |> ResizeArray

        {
            Order = readInt32 "order"
            Id = readString "id"
            Name = readString "name"
            Title = readString "title"
            Description = readString "description"
            Status = readEnum "status"
            DependsOn = readList "depends-on"
            StorageState = ()
        }

    let private toText document =
        use writer = new StringWriter(NewLine = "\n")
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
                Some metadata
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
    | Some (metadata: Task<_>) ->
        let pick (a: _ option) (b: _ option) = Seq.tryPick id (seq { a; b })
        { task with
            Order = pick metadata.Order task.Order
            Id = pick metadata.Id task.Id
            Name = pick metadata.Name task.Name
            Title = pick metadata.Title task.Title
            Description = pick metadata.Description task.Description
            Status = metadata.Status
            DependsOn = metadata.DependsOn }

let readTask (filePath: string) (stream: Stream): Async<Task<FileSystemTaskState>> = async {
    let { FsAttributes.Order = order; Id = id; Name = name } = readFsAttributes filePath
    let! (metadata, title, content) = Markdown.read stream
    let task = {
        Order = order
        Id = id
        Name = name
        Title = title
        Description = Some content
        Status = None
        DependsOn = Array.empty
        StorageState = { FileName = Path.GetFileName filePath }
    }

    return applyMetadata task metadata
}

let writeTask (task: Task<FileSystemTaskState>) (stream: Stream): Async<unit> = async {
    use writer = new StreamWriter(stream, leaveOpen = true)

    let writeStatus = Option.map (fun s -> box(Enum.GetName(s).ToLowerInvariant()))
    let writeDependsOn: IReadOnlyCollection<_> -> _ = function
    | collection when collection.Count = 0 -> None
    | collection -> Some(box collection)

    let metadata =
        seq {
            "status", writeStatus task.Status
            "depends-on", writeDependsOn task.DependsOn
        }
        |> Seq.choose(fun(k, v) ->
            v |> Option.map (fun v -> KeyValuePair(k, v))
        )
        |> Dictionary

    let text = StringBuilder()

    if metadata.Count > 0 then
        let frontMatter = Yaml.write metadata
        text.AppendFormat("---\n{0}---\n", frontMatter) |> ignore

    task.Title |> Option.iter (fun title ->
        text.AppendFormat("# {0}\n", title) |> ignore
    )

    task.Description |> Option.iter (fun description ->
        if description <> "" then
            if Option.isSome task.Title then
                text.Append '\n' |> ignore
            text.AppendFormat("{0}\n", description) |> ignore
    )

    let! ct = Async.CancellationToken
    do! Async.AwaitTask(writer.WriteAsync(text, ct))
}

let readDatabase (directory: string): Async<Database<FileSystemTaskState>> = async {
    let! tasks =
        Directory.GetFileSystemEntries(directory, "*.md")
        |> Array.map(fun path -> async {
            use stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read)
            return! readTask path stream
        })
        |> Async.Sequential
    return { Tasks = tasks }
}

let private applyInstruction databaseDirectory { Task = { StorageState = oldState }; NewState = newState } = async {
    do! Async.SwitchToThreadPool()
    let getPath state = Path.Combine(databaseDirectory, state.FileName)
    File.Move(getPath oldState, getPath newState)
}

let applyStorageInstructions (databaseDirectory: string)
                             (instructions: seq<StorageInstruction<FileSystemTaskState>>): Async<unit> = async {
    for instruction in instructions do
        do! applyInstruction databaseDirectory instruction
}

let saveDatabase (database: Database<FileSystemTaskState>) (directory: string): Async<unit> = async {
    for task in database.Tasks do
        let path = Path.Combine(directory, generateFileName task)
        use stream = File.Open(path, FileMode.Create, FileAccess.Write, FileShare.Read)
        do! writeTask task stream
}
