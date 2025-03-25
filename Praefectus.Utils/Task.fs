// SPDX-FileCopyrightText: 2025 Friedrich von Never <friedrich@fornever.me>
//
// SPDX-License-Identifier: MIT

module Praefectus.Utils.Task

open System.Threading.Tasks

let RunSynchronously(task: Task<'a>): 'a =
    task.GetAwaiter().GetResult()
