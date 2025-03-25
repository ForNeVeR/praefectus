// SPDX-FileCopyrightText: 2025 Friedrich von Never <friedrich@fornever.me>
//
// SPDX-License-Identifier: MIT

namespace Praefectus.Console

open System.Collections.Generic

open Praefectus.Core.Ordering

/// Praefectus application configuration.
type Configuration<'StorageState> when 'StorageState : equality = {
    /// Location of the Praefectus storage directory.
    DatabaseLocation: string
    Ordering: IReadOnlyCollection<TaskPredicate<'StorageState>>
}
