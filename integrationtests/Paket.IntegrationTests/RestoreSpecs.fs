﻿module Paket.IntegrationTests.RestoreSpec

open System
open System.IO
open Fake
open NUnit.Framework
open FsUnit
open Paket
open Paket.Utils

[<Test>]
let ``#3608 dotnet build should work with unparsable cache``() = 
    let project = "console"
    let scenario = "i003608-invalid-cache"
    use __ = prepareSdk scenario

    let wd = (scenarioTempPath scenario) @@ project
    // Build should work immediately (and call 'paket restore')
    directDotnet true (sprintf "build %s.fsproj" project) wd
        |> ignore

[<Test>]
let ``#2684 Paket should not be called the second time in msbuild (Restore Performance)``() =
    // NOTE: This test also ensure that FAKE can be used without paket on the CI server, see https://github.com/fsharp/FAKE/issues/2348
    let project = "console"
    let scenario = "i002684-fast-restore"
    use __ = prepareSdk scenario

    let wd = (scenarioTempPath scenario) @@ project
    // first call paket restore (to restore and to extract the targets file as well as emulate a "full" restore)
    directPaket "restore" scenario |> ignore
    // second time no more paket calls should be required (as we already did a full restore)
    directDotnetEx [ "PAKET_ERROR_ON_MSBUILD_EXEC", "true" ] true (sprintf "restore %s.fsproj" project) wd
        |> ignore
    // make sure it builds as well (checks if restore-targets contains syntax errors)
    directDotnet true (sprintf "build %s.fsproj" project) wd
        |> ignore

[<Test>]
let ``#2496 Paket fails on projects that target multiple frameworks``() = 
    let project = "EmptyTarget"
    let scenario = "i002496"
    use __ = prepareSdk scenario

    let wd = (scenarioTempPath scenario) @@ project
    directDotnet true (sprintf "restore %s.csproj" project) wd
        |> ignore

[<Test>]
let ``#3527 BaseIntermediateOutputPath``() =
    let project = "project"
    let scenario = "i003527"
    use __ = prepareSdk scenario

    let wd = (scenarioTempPath scenario) @@ project
    directDotnet true (sprintf "restore %s.fsproj" project) wd
        |> ignore

    let defaultObjDir = DirectoryInfo (Path.Combine (scenarioTempPath scenario, project, "obj"))
    let customObjDir = DirectoryInfo (Path.Combine (scenarioTempPath scenario, project, "obj", "custom"))

    defaultObjDir.GetFiles() |> shouldBeEmpty
    customObjDir.GetFiles().Length |> shouldBeGreaterThan 0

[<Test>]
let ``#3000-a dotnet restore``() =
    let scenario = "i003000-netcoreapp2"
    let projectName = "c1"
    let packageName = "AutoMapper"
    let workingDir = scenarioTempPath scenario
    let projectDir = workingDir @@ projectName

    [ packageName; (packageName.ToLower()) ] |> Seq.iter clearPackage
    
    use __ = prepareSdk scenario
    directDotnet false "restore" projectDir |> ignore
    directDotnet false "build --no-restore" projectDir |> ignore

[<Test>]
let ``#3012 Paket restore silently fails when TargetFramework(s) are specified in Directory.Build.props and not csproj`` () =
    let scenario = "i003012"
    let projectName = "dotnet"
    let packageName = "AutoMapper"
    let workingDir = scenarioTempPath scenario
    let projectDir = workingDir @@ projectName

    [ packageName; (packageName.ToLower()) ] |> Seq.iter clearPackage
    
    use __ = prepareSdk scenario
    directPaket "install" scenario |> ignore
    directDotnet false "build" projectDir |> ignore