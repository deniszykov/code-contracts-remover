using System;
using System.Collections.Generic;
using System.Diagnostics;
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
			if (error == null)
			{
			    throw new ArgumentNullException(nameof(error), "Contract assertion not met: error != null");
			}

			return FaultedTask<object>(error);
		}
	}
}
