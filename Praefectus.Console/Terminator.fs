// SPDX-FileCopyrightText: 2025 Friedrich von Never <friedrich@fornever.me>
//
// SPDX-License-Identifier: MIT

namespace Praefectus.Console

type ITerminator =
    abstract Terminate: exitCode: int -> 'a

type ProgramTerminator() =
    interface ITerminator with
        member _.Terminate code = exit code
