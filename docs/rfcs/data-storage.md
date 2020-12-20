Data Storage
============

By default, Praefectus stores its tasks and other information in a set of
Markdown files (according to the [CommonMark specification][commonmark]), each
with an optional [YAML Front Matter][yaml-front-matter] block.

Some tasks attributes may be inferred from the files' contents or names, and
some are stored in a YAML front matter block.

Here's an example task file (say, `42.my-task.md`):

```markdown
---
custom-attribute: 123
---
# Task Title
Some task description.
```

This task has the following explicit attributes:
- `order`: `42`
- `name`: `my-task`
- `title`: `Task Title`
- `custom-attribute`: `123`
- `description`: `Some task description.`

When processing the tasks, Praefectus may change their attributes, which will
then be mirrored by changing the file contents or names.

[commonmark]: https://spec.commonmark.org/
[yaml-front-matter]: https://jekyllrb.com/docs/front-matter/
