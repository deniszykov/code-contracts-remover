Introduction
============
Tool for removing .NET Code Contracts from source code. 
Currently only C# is supported. I will gladly accept PR with VB support.

Installation
============
```
Install-Package CodeContractsRemover
```

Usage
============
Tool is located at "PROJECT_DIR/packages/CodeContractsRemover.VERSION/tools/".

```bash
CodeContractsRemover.exe <Convert|Remove> <directoryPath> [--searchPattern *.cs *.csproj] [--encoding utf-8] [--ignorePattern .svn/ ]
```
Example
```bash
code_contracts_remover.exe Convert ./myproject
```

To run using [mono](http://www.mono-project.com/download/#download-mac) on Mac OS X
```bash
/Library/Frameworks/Mono.framework/Commands/mono code_contracts_remover.exe Convert ./myproject
```

To run using [mono](http://www.mono-project.com/download/#download-lin) on Linux
```bash
/usr/bin/mono code_contracts_remover.exe Convert ./myproject
```

## Modes

#### Mode - Convert
- Converts all [Contract.Requires](https://msdn.microsoft.com/en-us/library/system.diagnostics.contracts.contract.requires(v=vs.110).aspx) to "if(!x) throw new ArgumentException()" pattern.
- Converts all [Contract.Assert](https://msdn.microsoft.com/en-us/library/system.diagnostics.contracts.contract.assert(v=vs.110).aspx) to [Debug.Assert](https://msdn.microsoft.com/en-us/library/system.diagnostics.debug.assert(v=vs.110).aspx)
- Removes any other [Contract](https://msdn.microsoft.com/en-us/library/system.diagnostics.contracts.contract(v=vs.110).aspx) invocations.
- Invariant methods are preserved
- Attributes and Contract classes are removed

#### Mode - Remove
- Removes any [Contract](https://msdn.microsoft.com/en-us/library/system.diagnostics.contracts.contract(v=vs.110).aspx) invocations.
- Invariant methods are preserved
- Attributes and Contract classes are removed
