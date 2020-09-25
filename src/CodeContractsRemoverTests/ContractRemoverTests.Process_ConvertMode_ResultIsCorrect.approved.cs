using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace Tests
{
	public static class Utils
	{
		private int _i;

		public static Task FaultedTask(Exception error)
		{
			if (error == null)
			{
			    throw new ArgumentNullException(nameof(error), "Contract assertion not met: error != null");
			}
			if (error == null)
			{
			    throw new ArgumentNullException(nameof(error), "Contract assertion not met: error != null");
			}

			var x = 1;
			if (x == 1)
				{
				    var resultOfFaultedTask = FaultedTask<object>(error);
				    if (resultOfFaultedTask == null)
				    {
				        throw new InvalidOperationException("Contract assertion not met: Contract.Result<Task>() != null");
				    }
				    return resultOfFaultedTask;
				}
			else
				{
				    var resultOfFaultedTask = null;
				    if (resultOfFaultedTask == null)
				    {
				        throw new InvalidOperationException("Contract assertion not met: Contract.Result<Task>() != null");
				    }
				    return resultOfFaultedTask;
				}
		}

		public static Task MethodWihoutContracts(Exception error)
		{
			return FaultedTask<object>(error);
		}

		public void VoidMethod(string par)
		{
			if (par == null)
			{
			    throw new ArgumentNullException(nameof(par), "Contract assertion not met: par != null");
			}
			var x = 1;
			if (x == 1)
				{
				    if (!(_i > 10))
				    {
				        throw new InvalidOperationException("Contract assertion not met: _i > 10");
				    }
				    return;
				}
		    if (!(_i > 10))
		    {
		        throw new InvalidOperationException("Contract assertion not met: _i > 10");
		    }
		}
	}
}
