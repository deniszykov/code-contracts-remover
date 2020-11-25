using System.IO;
using System.Text;
using ApprovalTests;
using ApprovalTests.Namers;
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

		[Test]
		public void Process_ProcessManyTimes_ResultIsCorrect()
		{
			//Arrange
			var file = $"{nameof(Process_ProcessManyTimes_ResultIsCorrect)}.received.csproj";
			File.Copy(TestsHelper.GetInitialFileName(".\\TestCases\\Project.csproj"), file);

			new ProjectContractRemover(file, AnnotationsMode.IncludeIntoBinaries).Process();

			//Act
			//Process second time
			new ProjectContractRemover(file, AnnotationsMode.IncludeIntoBinaries).Process();

			//Assert
			Approvals.VerifyFile(file);
		}

		[Test]
		[TestCase(AnnotationsMode.Add)]
		[TestCase(AnnotationsMode.IncludeIntoBinaries)]
		public void Process_ProjectWithoutCc_CsprojNotModified(AnnotationsMode mode)
		{
			//Arrange
			var file = $"{nameof(Process_ProjectWithoutCc_CsprojNotModified)}.{mode}.received.csproj";
			File.Copy(TestsHelper.GetInitialFileName(".\\TestCases\\ProjectNoCc.csproj"), file);

			//Act
			new ProjectContractRemover(file, mode).Process();

			//Assert
			NamerFactory.AdditionalInformation = mode.ToString();
			Approvals.VerifyFile(file);
		}
	}
}
