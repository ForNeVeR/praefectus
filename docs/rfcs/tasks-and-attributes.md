<!--
SPDX-FileCopyrightText: 2025 Friedrich von Never <friedrich@fornever.me>

SPDX-License-Identifier: MIT
-->

Tasks and Attributes
====================

_Task_ is the main piece of information used by Praefectus. Conceptually, the
only requirement for something being a task is that it has to have some
_definition of done_: it should be clear (to the one who manages the task at
least) what does it mean for the task to be _done_; which particular conditions
allow to consider the task as _closed_, i.e. not requiring any additional
actions.

Attributes
----------

In Praefectus, a _task_ is characterized by a set of attribute values. A _task
attribute_ is a piece of information of which a _task_ may consist of.

Attribute itself may be described by the following properties:

- `name`: a name of a particular attribute.

- `type`: a data type of the attribute values. Currently, the following simple
  data types are used:

  - `boolean`: a boolean value, either true of false.
  - `enum`: a value from a finite set of named entities, declared by the type.
    One particular example is `enum[A, B, None]`.
  - `string`: a (reasonably-unlimited) sequence of Unicode characters.
  - `integer`: a 32-bit signed integer value
  - `double`: a 64-bit floating-point value, called as `binary64` in the IEEE
    754-2008 standard.
  - `timestamp`: a 64-bit UTC timestamp (count of milliseconds since
    `1970-01-01T00:00:00.000Z`) with local time zone offset.
  - `task`: a reference to another (or the same) task, represented by task
    identifier. Could be checked automatically to determine health of the
    database.

  Also, there's a complex data type supported: `list<T>`, where `T` is any of
  the simple data types. List is a finite sequence of values of the
  corresponding type.

- `description`: an attribute description in text format.

Data Attributes
---------------

Basic task in Praefectus has the following attributes (all of them are
optional):

- `id: string`: a task identifier.
- `order: int`: a task order (ideally, tasks with lower order should be done
  first).
- `name: string`: the short task name, appropriate to store it in a file name.
- `title: string`: a short task title description or summary; will be shown in
  most of the default Praefectus views, so should be concise.
- `description: string`: the task description, may be longer text.
- `status: enum[Open, InProgress, Done, Deleted]`: a task status. While `Open`
  and `InProgress` are self-explanatory, `Done` and `Deleted` are differ in how
  the corresponding tasks are treated: `Deleted` ones shouldn't be considered by
  most reports, and any links from regular tasks to deleted ones are reported
  during diagnostics.
- `depends-on: list<task>`: a list of tasks this one depends on.

Calculated Attributes
---------------------

Not all the attributes are stored in the Praefectus data storage. Some are
calculated on the fly, because they depend on the calculation time.

- `actually-depends-on: list<task>`: a set of unresolved tasks (i.e. not `Done`
  or `Deleted`) this one depends on.
