using System;
using System.IO;
using System.Linq;
using System.Text;
using ApprovalTests;
using ApprovalTests.Reporters;
using CodeContractsRemover;
using CodeContractsRemover.CS;
using NUnit.Framework;

namespace CodeContractsRemoverTests
{
	[UseReporter(typeof(DiffReporter))]
	public class ContractRemoverTests
    {
	    private static string InitialFileName => TestsHelper.GetInitialFileName(".\\TestCases\\TestSubject.cs");

	    [Test]
        public void Process_ConvertMode_ResultIsCorrect()
        {
			//Arrange
			var file = $"{nameof(Process_ConvertMode_ResultIsCorrect)}.received.cs";
			File.Copy(InitialFileName, file);

			//Act
			ContractRemover.Process(file, ContractReplacementMode.Convert, Encoding.UTF8);

			//Assert
			Approvals.VerifyFile(file);
        }

        [Test]
        public void Process_FileWithLeadingComments_LeadingCommentsArePreserved()
        {
	        //Arrange
	        var file = $"{nameof(Process_FileWithLeadingComments_LeadingCommentsArePreserved)}.received.cs";
	        File.Copy(TestsHelper.GetInitialFileName(".\\TestCases\\FileWithLeadingComments.cs"), file);

	        //Act
	        ContractRemover.Process(file, ContractReplacementMode.Convert, Encoding.UTF8);

	        //Assert
	        Approvals.VerifyFile(file);
        }

        [Test]
        public void Process_ExtensionsCase_ResultIsCorrect()
        {
	        //Arrange
	        var file = $"{nameof(Process_ExtensionsCase_ResultIsCorrect)}.received.cs";
	        File.Copy(TestsHelper.GetInitialFileName(".\\TestCases\\ExtensionsCase.cs"), file);

	        //Act
	        ContractRemover.Process(file, ContractReplacementMode.ConvertAndAddAnnotations, Encoding.UTF8);

	        //Assert
	        Approvals.VerifyFile(file);
        }

		[Test]
        public void Process_NothingToModify_FileNotModified()
        {
	        //Arrange
	        var file = $"{nameof(Process_NothingToModify_FileNotModified)}.received.cs";
	        File.Copy(TestsHelper.GetInitialFileName(".\\TestCases\\NothingToModify.cs"), file);

	        //Act
	        var changed = ContractRemover.Process(file, ContractReplacementMode.Convert, Encoding.UTF8);

	        //Assert
	        Assert.False(changed);
        }

		[Test]
        public void Process_StatisticMode_ResultIsCorrect()
        {
	        //Arrange
	        var file = $"{nameof(Process_StatisticMode_ResultIsCorrect)}.received.cs";
	        File.Copy(InitialFileName, file);

	        //Act
	        ContractRemover.Process(file, ContractReplacementMode.Stats, Encoding.UTF8);

	        //Assert
	        Approvals.Verify(CSharpStatsCollector.GetStats());
        }
    }
}
