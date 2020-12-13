Version Control
===============

Praefectus has multiple facilities that changes the objects within the database:
these activities may either be manually-triggered or automated. It may be
important to know who and when have changed a database object; that's why
Praefectus has an embedded version control system (VCS) that tracks various
objects' history.

Praefectus VCS model is based on SVN one, where every entity (e.g. file in SVN,
a task in Praefectus) has its own history tree and may evolve independently.

Object History
--------------

Any object (i.e. a task) should have its own history, which is a set of _
commits_. A commit is an object with the following properties:

- `id: string`: a unique commit identifier. Any Praefectus node is recommended
  to use its own id combined with an [UUID][uuid] as the commit identifier.
- `author: string`: an name of a change author (e.g. a node identifier).
- `date: timestamp`: a timestamp of a commit.
- `message: string`: an optional commit message (may either be set by user or by
  an entity that makes the commit).
- `parents: list<id>`: a list of parent commits; may only be empty for the first
  commit of a particular object.
- `updated`: a map of an attribute id to attribute value that this commit has
  added (i.e. when adding a new object) or changed.
- `conflicted`: see below. These values are considered to be effectively deleted
  from the object, but Praefectus node may render them in a special way, marking
  a conflict presence.

When an object is created, an initial commit should be prepared with the initial
attribute values. On any further change, a new commit should be created
describing the changes. The object should have a hidden attribute that points to
the current commit.

Every object has a special map of "branch names" to commits. Every node could
create a new branch when it thinks it should. One of the branches in Praefectus
database should be considered as "current", and all the changes goes to that
branch. It is recommended to name this branch the same as the Praefectus node
identifier, so different nodes may distinguish each other's changes.

Commit Merge
------------

Sometimes the same entity may be changed by multiple parties not knowing of each
other. In such cases, Praefectus could _merge_ the object histories. For
example, consider that the object has this history:

```
node1: A -> B -> C
node2: A -> D -> E
```

And now node 1 wants to merge the changes. It passes two _merge heads_, `C` and
`E`, to the merge algorithm (in general, there may be more than two; Praefectus
supports "octopus merge").

Merge algorithm follows these steps:

1. Determine the _base commit_: the last commit where the object histories were
   in consistent state, which is nearest to the merge heads. In case there're
   multiple such commits, perform additional rounds until either the very first
   commit is selected or there're no base commit candidates (e.g. for unrelated
   histories); in such case, choose an imaginary empty base commit.

   In the example, `A` would be the base commit.
2. Create a new commit with parent list filled by the merge heads, and begin to
   fill it with attributes.
3. Now, consider set of differences between each of the merge heads and the base
   commit. If any attribute was _changed_ in multiple heads, add this attribute
   to the _conflict list_. If any attribute was only changed in one of the merge
   heads, then copy this change to the resulting commit.
4. Fill the `conflicted` list in the object (see the list structure below). The
   list gets additional values of every conflicted attribute, marked with the
   origin branch name.

   For example, consider that two nodes have changed the attribute `attr`:
   `node1` set it to the value `X`, and `node2` to the value `Y`. It means that
   the conflict map will look like:

   ```json
    "conflicted": {
        "attr": {
            "node1": "X",
            "node2": "Y"
        }
    }
   ```

   If there was a value in the attribute map (i.e. we were performing a merge
   operation on an object already in a conflicted state), then the conflicts map
   may be updated; new values will replace older ones marked with the same
   branch names.
5. Set a conflict flag for the object that has any conflicts.

In the future, the node may _resolve_ the conflicts: any changes to the
conflicted attributes should clear their conflicted status. If an object has no
conflicted attributes, then it should be marked as conflict-free.

Object History Structure
------------------------

Every versionable Praefectus entity should have a `history` field with the
following structure:

```json
"history": {
    "commits": [{
        "id": "node1.16d44a70-300f-4870-819f-0dfff5588328",
        "author": "node1",
        "date": 123456,
        "message": "init",
        "parents": ["node2.35e978b9-0ceb-439b-b910-8c1887482650"],
        "updated": {
            "attr1": "some string value"
        },
        "deleted": ["attr2"],
        "conflicted": {
            "attr3": {
                "node1": "someval",
                "node2": "someotherval"
            }
        }
    }],
    "heads": {
        "node1": "node1.16d44a70-300f-4870-819f-0dfff5588328",
        "node2": "node2.35e978b9-0ceb-439b-b910-8c1887482650"
    },
    "hasConflicts": true
}
```

Validation
----------

There should be a set of validation steps for the commit history:

1. Check that the object commit history is still a [DAG][dag], i.e. it has no
   cycles.
2. Check that all the commits have unique identifiers across the whole database.
3. Check that any object that has attribute conflicts is in the `Conflicted`
   state.
4. Check that any object that has no attribute conflicts is not in the
   `Conflicted` state.

[dag]: https://en.wikipedia.org/wiki/Directed_acyclic_graph
[uuid]: https://tools.ietf.org/html/rfc4122
