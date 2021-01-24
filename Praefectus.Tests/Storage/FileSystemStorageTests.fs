module Praefectus.Tests.Storage.FileSystemStorageTests

open Xunit

open Praefectus.Storage.FileSystemStorage

[<Fact>]
let ``Empty file name should be treated as empty name``(): unit =
    Assert.Equal({ Order = None; Id = None; Name = Some "" }, readFsAttributes(".md"))

[<Fact>]
let ``File name with dot should be treated as empty id and name``(): unit =
    Assert.Equal({ Order = None; Id = Some ""; Name = Some "" }, readFsAttributes("..md"))

[<Fact>]
let ``Integer order should be detected``(): unit =
    Assert.Equal({ Order = Some 300; Id = None; Name = Some "name" }, readFsAttributes("300.name.md"))

[<Fact>]
let ``Non-integer first section should be skipped``(): unit =
    Assert.Equal({ Order = None; Id = Some "id"; Name = Some "test" }, readFsAttributes("id.test.md"))

[<Fact>]
let ``Order only test``(): unit =
    Assert.Equal({ Order = Some 1; Id = None; Name = None }, readFsAttributes("1.md"))

[<Fact>]
let ``Name only test``(): unit =
    Assert.Equal({ Order = None; Id = None; Name = Some "name" }, readFsAttributes("name.md"))

[<Fact>]
let ``Full id test``(): unit =
    Assert.Equal({ Order = Some 1; Id = Some "id"; Name = Some "name" }, readFsAttributes("1.id.name.md"))

[<Fact>]
let ``Only id``(): unit =
    Assert.Equal({ Order = Some 1; Id = Some "id"; Name = Some "" }, readFsAttributes("1.id..md"))

[<Fact>]
let ``Name concatenation``(): unit =
    Assert.Equal({ Order = Some 1; Id = Some "id"; Name = Some "name.test.1" }, readFsAttributes("1.id.name.test.1.md"))
