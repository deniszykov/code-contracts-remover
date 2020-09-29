Introduction
============
Tool for removing .NET Code Contracts from source code. 
Currently only C# is supported. I will gladly accept PR with VB support.

.NET 4.7.1 is required to run this application.

Installation
============
```
Install-Package CodeContractsRemover
```
or
```
Download and un-zip from https://www.nuget.org/api/v2/package/CodeContractsRemover 
```

Usage
============
Tool is located at "PROJECT_DIR/packages/CodeContractsRemover.VERSION/tools/".

```bash
CodeContractsRemover.exe <Convert|Remove|Stats> <directoryPath> [--searchPattern *.cs *.csproj] [--encoding utf-8] [--ignorePattern .svn/ ]
```
Example
```bash
CodeContractsRemover.exe Convert ./myproject
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
- Converts all [Contract.Requires](https://msdn.microsoft.com/en-us/library/system.diagnostics.contracts.contract.requires(v=vs.110).aspx) to "if(!x) throw new ArgumentException()" pattern at the beginning of method/property/constructor.
- Converts all [Contract.Ensures](https://docs.microsoft.com/en-us/dotnet/api/system.diagnostics.contracts.contract.ensures?view=netcore-3.1) to "if(!x) throw new InvalidOperationException()" pattern before each return incide method/property/constructor.
- Converts all [Contract.Assert](https://msdn.microsoft.com/en-us/library/system.diagnostics.contracts.contract.assert(v=vs.110).aspx) and [Contract.Assume](https://docs.microsoft.com/en-us/dotnet/api/system.diagnostics.contracts.contract.assume?view=netcore-3.1) to "if(!x) throw new ArgumentException()" pattern.
- Invokes invariant methods before each return incide method/property/constructor.
- Preserves all other [Contract](https://msdn.microsoft.com/en-us/library/system.diagnostics.contracts.contract(v=vs.110).aspx) invocations (including Attributes and Contract classes).
- Removes CodeContract properties and constants from project files

#### Mode - Remove
- Removes any [Contract](https://msdn.microsoft.com/en-us/library/system.diagnostics.contracts.contract(v=vs.110).aspx) invocations.
- Invariant methods are preserved
- Attributes and Contract classes are removed
- Removes CodeContract properties and constants from project files

#### Mode - Stats
- Collects statistics about how [Contracts](https://msdn.microsoft.com/en-us/library/system.diagnostics.contracts.contract(v=vs.110).aspx) are used. Example of result:
```
[ContractClassFor]                                	  1
[ContractInvariantMethod]                         	  2
Contract.Assert                                   	  1
Contract.Ensures                                  	  6
Contract.Invariant                                	  2
Contract.Requires                                 	  6
Contract.Result                                   	  5
```

## Annotation modes

In Convert mode [Jet Brains annotations](https://blog.jetbrains.com/dotnet/2018/05/02/improving-rider-resharper-code-analysis-using-jetbrains-annotations/) ([NotNull]) can be added to class members. Jet Brains annotations are added using [JetBrains.Annotations NuGet package](https://www.nuget.org/packages/JetBrains.Annotations/).

#### Mode - None
Don't add annotations

#### Mode - Add
Include annotations only for work in Visual Studio/Rider before compilation. When project is compiled, annotations would be removed. When dll is referenced into other project, Rider/Re# wouldn't show hints. This mode is recommended for applications. Read more on [Jet Brains site](https://blog.jetbrains.com/dotnet/2015/08/12/how-to-use-jetbrains-annotations-to-improve-resharper-inspections/).

#### Mode - IncludeIntoBinaries
Annotations will be included into binaries, so Rider/Re# would show hints before and after compilation (when binary is referenced into other project). This mode is recommended for packages. Read more on [Jet Brains site](https://blog.jetbrains.com/dotnet/2015/08/12/how-to-use-jetbrains-annotations-to-improve-resharper-inspections/).
		
# Contributors
 * @ishatalkin
 * @mgaffigan

