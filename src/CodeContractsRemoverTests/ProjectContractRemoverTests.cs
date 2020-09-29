using System.IO;
using System.Text;
using ApprovalTests;
using ApprovalTests.Reporters;
using CodeContractsRemover;
using NUnit.Framework;

namespace CodeContractsRemoverTests
{
	[UseReporter(typeof(DiffReporter))]
	public class ProjectContractRemoverTests
	{
		[Test]
		public void Process_CsProj_CodeContractsRemoved()
		{
			//Arrange
			var file = $"{nameof(Process_CsProj_CodeContractsRemoved)}.received.csproj";
			File.Copy(TestsHelper.GetInitialFileName(".\\TestCases\\Project.csproj"), file);

			//Act
			new ProjectContractRemover(file, AnnotationsMode.None).Process();

			//Assert
			Approvals.VerifyFile(file);
		}

		[Test]
		public void Process_AddAnnotations_ReferenceToJJAAdded()
		{
			//Arrange
			var file = $"{nameof(Process_AddAnnotations_ReferenceToJJAAdded)}.received.csproj";
			File.Copy(TestsHelper.GetInitialFileName(".\\TestCases\\Project.csproj"), file);

			//Act
			new ProjectContractRemover(file, AnnotationsMode.Add).Process();

			//Assert
			Approvals.VerifyFile(file);
		}

		[Test]
		public void Process_IncludeAnnotationsIntoBinaries_ResultIsCorrect()
		{
			//Arrange
			var file = $"{nameof(Process_IncludeAnnotationsIntoBinaries_ResultIsCorrect)}.received.csproj";
			File.Copy(TestsHelper.GetInitialFileName(".\\TestCases\\Project.csproj"), file);

			//Act
			new ProjectContractRemover(file, AnnotationsMode.IncludeIntoBinaries).Process();

			//Assert
			Approvals.VerifyFile(file);
		}
	}
}
