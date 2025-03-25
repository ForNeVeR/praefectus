// SPDX-FileCopyrightText: 2025 Friedrich von Never <friedrich@fornever.me>
//
// SPDX-License-Identifier: MIT

namespace Praefectus.Console

open Serilog

type Application<'StorageState> when 'StorageState : equality = {
    Config: Configuration<'StorageState>
    Logger: ILogger
}
