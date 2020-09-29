using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CodeContractsRemover.CS.Class
{
	/// <summary> Info about class's contract </summary>
	public class ClassContractInfo
	{
		/// <summary> Class </summary>
		public ClassDeclarationSyntax Class { get; set; }

		/// <summary> Invariant methods declared in class </summary>
		public List<string> InstanceInvariants { get; } = new List<string>();

		/// <summary> Static invariant methods declared in class </summary>
		public List<string> StaticInvariants { get; } = new List<string>();

		/// <summary> All invariant methods (static and instance) </summary>
		public List<string> AllInvariants => InstanceInvariants.Union(StaticInvariants).ToList();
	}
}
