using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Tests
{
	public static class Utils
	{
		public static Task FaultedTask(Exception error)
		{
			Contract.Requires<ArgumentNullException>(error != null);
			Contract.Ensures(Contract.Result<Task>() != null);

			return FaultedTask<object>(error);
		}
	}
}
