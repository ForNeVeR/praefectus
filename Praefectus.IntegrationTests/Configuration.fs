module Praefectus.IntegrationTests.Configuration

open System
open System.IO
open System.Reflection
open Xunit

let private defaultExecutableUnderTest =
    let thisExecutable = Assembly.GetExecutingAssembly().Location
    let thisExecutableDirectory = Path.GetDirectoryName thisExecutable
    let framework = Path.GetFileName thisExecutableDirectory
    let configuration = Path.Combine(thisExecutableDirectory, "..") |> Path.GetFullPath |> Path.GetFileName
    let solutionDir = Path.Combine(thisExecutableDirectory, "..", "..", "..", "..")
    let slnPath = Path.Combine(solutionDir, "Praefectus.sln") |> Path.GetFullPath
    Assert.True(File.Exists(slnPath), sprintf "Path should exist: %s" slnPath)
    let executableDirectory = Path.Combine(solutionDir, "Praefectus.Console", "bin", configuration, framework) |> Path.GetFullPath
    let executable =
        if Environment.OSVersion.Platform = PlatformID.Win32NT
        then Path.Combine(executableDirectory, "praefectus.exe")
        else Path.Combine(executableDirectory, "praefectus.dll")
    Assert.True(File.Exists(executable), sprintf "Path should exist: %s" executable)
    executable

let executableUnderTest: string =
    Environment.GetEnvironmentVariable "PRAEFECTUS_TEST_EXECUTABLE"
    |> Option.ofObj
    |> Option.defaultValue defaultExecutableUnderTest
