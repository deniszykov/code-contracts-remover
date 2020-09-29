using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CodeContractsRemover.CS
{
	public class CSharpStatsCollector : SyntaxWalker
	{
		public static Dictionary<string, int> Stats = new Dictionary<string, int>();

		public static string GetStats()
		{
			var sb = new StringBuilder();
			foreach (var stat in CSharpStatsCollector.Stats.OrderBy(s => s.Key))
			{
				sb.AppendLine($"{stat.Key,-50}\t{stat.Value,3}");
			}

			return sb.ToString();
		}

		/// <summary>
		/// Called when the walker visits a node.  This method may be overridden if subclasses want
		/// to handle the node.  Overrides should call back into this base method if they want the
		/// children of this node to be visited.
		/// </summary>
		/// <param name="node">The current node that the walker is visiting.</param>
		public override void Visit(SyntaxNode node)
		{
			switch (node)
			{
				case InvocationExpressionSyntax invNode:
					var invInfo = new InvocationInfo(invNode);

					if (invInfo.Class == "Contract")
					{
						IncrementStats($"{invInfo.Class}.{invInfo.Method}");
					}

					break;

				case AttributeSyntax attrNode:
					var attrName = attrNode.Name.ToString().Replace("Attribute", string.Empty);
					if (ContractRemover.ContractAttributes.Contains(attrName)
					    || attrName.StartsWith(ContractRemover.ContractClassForAttributeName))
					{
						IncrementStats($"[{attrName}]");
					}
					break;
			}

			base.Visit(node);
		}

		private static void IncrementStats(string key)
		{
			if (Stats.ContainsKey(key))
			{
				Stats[key]++;
			}
			else
			{
				Stats[key] = 1;
			}
		}
	}
}
