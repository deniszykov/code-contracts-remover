Introduction
============
Tool for removing .NET Code Contracts source code

Installation
============
```
Install-Package CodeContractsRemover
```

Usage
============
```bash
code_contracts_remover.exe <Convert|Remove> <directoryPath> [searchPattern=*.cs] [encoding=utf-8]
```
Example
```bash
code_contracts_remover.exe Convert ./myproject
```

#### Convert
- Converts all Contract.Requires to "if throw ArgumentException" pattern.
- Converts all Contract.Assert to Debug.Assert
- Removes any other Contract invocations.

#### Remove
- Removes any Contract invocations.
