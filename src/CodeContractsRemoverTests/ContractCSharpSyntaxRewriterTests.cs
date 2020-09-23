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
	public class ContractCSharpSyntaxRewriterTests
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
        public void Convert_IntegrationTest()
        {
			//Arrange
			File.Copy(InitialFileName, $"{nameof(Convert_IntegrationTest)}.cs");

			//Act
			ContractRemover.Process($"{nameof(Convert_IntegrationTest)}.cs", ContractReplacementMode.Convert, Encoding.UTF8);

			//Assert
			Approvals.VerifyFile($"{nameof(Convert_IntegrationTest)}.cs");
        }
    }
}
