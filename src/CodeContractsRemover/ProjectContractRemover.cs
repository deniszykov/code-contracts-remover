using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Build.Construction;

namespace CodeContractsRemover
{
	public class ProjectContractRemover
	{
		private readonly string _filePath;
		private readonly AnnotationsMode _mode;
		private readonly ProjectRootElement _project;
		private bool _changed;

		/// <summary>Initializes a new instance of the <see cref="T:System.Object" /> class.</summary>
		public ProjectContractRemover(string filePath, AnnotationsMode mode)
		{
			if (!filePath.EndsWith(".csproj", StringComparison.OrdinalIgnoreCase) &&
			    !filePath.EndsWith(".vbproj", StringComparison.OrdinalIgnoreCase))
			{
				throw new ArgumentOutOfRangeException(
					nameof(filePath),
					filePath,
					$"Unknown project file extension {Path.GetExtension(filePath)}. Only .csproj and .vbproj are supported.");
			}

			_project = ProjectRootElement.Open(filePath);
			_filePath = filePath;
			_mode = mode;
		}

		public void Process()
		{
			Console.Write($"Processing file {Path.GetFileName(_filePath)}...");

			RemoveCodeContracts();

			if (_mode == AnnotationsMode.Add || _mode == AnnotationsMode.IncludeIntoBinaries)
			{
				AddAnnotations();
			}

			if (_changed)
			{
				_project.Save();
			}

			if (_changed)
				Console.WriteLine("Modified.");
			else
				Console.WriteLine("Not Modified.");
		}

		private void AddAnnotations()
		{
			var include = _project.ItemGroups
				.SelectMany(g => g.Items)
				.FirstOrDefault(i => i.ElementName == "PackageReference" && i.Include == "JetBrains.Annotations");
			if (include == null)
			{
				var item = _project.AddItem("PackageReference", "JetBrains.Annotations");
				var metadata = item.AddMetadata("Version", "2020.1.0");
				metadata.ExpressedAsAttribute = true;

				_changed = true;
			}

			if (_mode == AnnotationsMode.IncludeIntoBinaries)
			{
				var consts = _project.PropertyGroups.SelectMany(g => g.Properties)
					.Where(p => p.Name.Equals("DefineConstants", StringComparison.OrdinalIgnoreCase))
					.ToArray();
				var addDefaultConst = true;
				foreach (var element in consts)
				{
					element.Value = $"{element.Value};JETBRAINS_ANNOTATIONS";
					if (string.IsNullOrEmpty(element.Parent.Condition))
					{
						addDefaultConst = false;
					}
				}

				if (addDefaultConst)
				{
					//add const without conditions
					var gr = _project.PropertyGroups.Where(g => string.IsNullOrEmpty(g.Condition)).First();
					gr.AddProperty("DefineConstants", "JETBRAINS_ANNOTATIONS");
				}

				_changed = true;
			}
		}

		private void RemoveCodeContracts()
		{
			foreach (var pg in _project.PropertyGroups)
			{
				var propsToRemove = pg.Properties
					.Where(p => p.Name.StartsWith("CodeContracts", StringComparison.OrdinalIgnoreCase))
					.ToList();

				var constDefinitions =
					pg.Properties.Where(p => p.Name.Equals("DefineConstants", StringComparison.OrdinalIgnoreCase));
				foreach (var constDefinition in constDefinitions)
				{
					if (constDefinition.Value.Equals("CONTRACTS_FULL", StringComparison.OrdinalIgnoreCase))
					{
						propsToRemove.Add(constDefinition);
					}
					else if (constDefinition.Value.Contains("CONTRACTS_FULL;"))
					{
						_changed = true;
						constDefinition.Value = constDefinition.Value.Replace("CONTRACTS_FULL;", string.Empty);
					}
					else if (constDefinition.Value.Contains(";CONTRACTS_FULL"))
					{
						_changed = true;
						constDefinition.Value = constDefinition.Value.Replace(";CONTRACTS_FULL", string.Empty);
					}
				}

				foreach (var prop in propsToRemove)
				{
					_changed = true;
					pg.RemoveChild(prop);
				}
			}
		}
	}
}
