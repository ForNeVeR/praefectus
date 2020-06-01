module Praefectus.Tests.ApprovalTestUtils

open ApprovalTests.Namers
open ApprovalTests.Reporters

[<assembly: UseReporter(typeof<DiffReporter>)>]
[<assembly: UseApprovalSubdirectory("TestResults")>]
do ()
