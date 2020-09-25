using System;
using System.IO;
using System.Reflection;
using System.Text;
using ApprovalTests;
using ApprovalTests.Reporters;
using CodeContractsRemover;
using NUnit.Framework;

namespace CodeContractsRemoverTests
{
	[UseReporter(typeof(DiffReporter))]
	public class ContractRemoverTests
    {
	    private static string InitialFileName
	    {
		    get
		    {
			    var location = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			    return Path.Combine(location, ".\\TestCases\\TestSubject.cs");
		    }
	    }

	    [Test]
        public void Process_ConvertMode_ResultIsCorrect()
        {
			//Arrange
			File.Copy(InitialFileName, $"{nameof(Process_ConvertMode_ResultIsCorrect)}.cs");

			//Act
			ContractRemover.Process($"{nameof(Process_ConvertMode_ResultIsCorrect)}.cs", ContractReplacementMode.Convert, Encoding.UTF8);

			//Assert
			Approvals.VerifyFile($"{nameof(Process_ConvertMode_ResultIsCorrect)}.cs");
        }
    }
}
