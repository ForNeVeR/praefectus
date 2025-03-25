<!--
SPDX-FileCopyrightText: 2024-2025 Friedrich von Never <friedrich@fornever.me>

SPDX-License-Identifier: MIT
-->

Contributor Guide
=================

License Automation
------------------
<!-- REUSE-IgnoreStart -->

If the CI asks you to update the file licenses, follow one of these:
1. Update the headers manually (look at the existing files), something like this:
   ```fsharp
   // SPDX-FileCopyrightText: %year% %your name% <%your contact info, e.g. email%>
   //
   // SPDX-License-Identifier: MIT
   ```
   (accommodate to the file's comment style if required).
2. Alternately, use the [REUSE][reuse] tool:
   ```console
   $ reuse annotate --license MIT --copyright '%your name% <%your contact info, e.g. email%>' %file names to annotate%
   ```
(Feel free to attribute the changes to "Praefectus contributors <https://github.com/ForNeVeR/Praefectus>"
instead of your name in a multi-author file,
or if you don't want your name to be mentioned in the project's source: this doesn't mean you'll lose the copyright.)

<!-- REUSE-IgnoreEnd -->

File Encoding Changes
---------------------
If the automation asks you to update the file encoding (line endings or UTF-8 BOM) in certain files, run the following PowerShell script ([PowerShell Core][powershell] is recommended to run this script):
```console
$ pwsh -File scripts/Test-Encoding.ps1 -AutoFix
```

The `-AutoFix` switch will automatically fix the encoding issues, and you'll only need to commit and push the changes.

[powershell]: https://learn.microsoft.com/en-us/powershell/scripting/install/installing-powershell
[reuse]: https://reuse.software/
