Introduction
============
tool for removing .net code contracts from project

Installation
============
```
Install-Package CodeContractsRemover
```

Usage
============
```xml
code_contracts_remover.exe <Convert|Remove> <directorPath> [searchPattern=*.cs] [encoding=utf-8]
```

#### Convert
Converts all Contract.Requires to "if throw ArgumentException" pattern.
Converts all Contract.Assert to Debug.Assert
Removes any other Contract invocations.

#### Remove
Removes any Contract invocations.