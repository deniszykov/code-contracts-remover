using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
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
			Contract.Requires<ArgumentNullException>(error != null);
			Contract.Assert(error != null);
			Contract.Ensures(Contract.Result<Task>() != null);

			var x = 1;
			if (x == 1)
				return FaultedTask<object>(error);
			else
				return null;
		}

		public static Task MethodWihoutContracts(Exception error)
		{
			return FaultedTask<object>(error);
		}

		public void VoidMethod(string par)
		{
			Contract.Requires<ArgumentNullException>(par != null);
			Contract.Ensures(_i > 10);
			var x = 1;
			if (x == 1)
				return;
		}
	}
}
