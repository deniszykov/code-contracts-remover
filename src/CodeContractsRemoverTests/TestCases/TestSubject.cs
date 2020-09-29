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
		private static int stat;

		public static Utils(int i)
		{
			Contract.Requires(i > 0);

			_i = i;
		}

        /// <summary> Some comments </summary>
		public int I
		{
			set
			{
				Contract.Requires(value > 0);
				_i = value;
			}
			get
			{
				Contract.Ensures(Contract.Result<int>() > 0);
				return _i;
			}
		}

        /// <summary> Some comments </summary>
        public string s
        {
	        get
	        {
				Contract.Ensures(Contract.Result<string>() != null);
				return "x";
	        }
        }

		public static Task FaultedTask(Exception error)
		{
			Contract.Assert(error != null);
			Contract.Ensures(Contract.Result<Task>() != null);
			Contract.Requires<ArgumentNullException>(error != null);

			var x = 1;
			if (x == 1)
				return FaultedTask<object>(error);
			else if (x == 2)
			{
				return FaultedTask<object>(error);
			}
			else
				return null;
		}

		public static Task FaultedTask2(Exception error)
		{
			Contract.Requires<ArgumentNullException>(error != null);

			return FaultedTask<object>(error);
		}

		public static Task FaultedTask3(Exception error)
		{
			Contract.Ensures(Contract.Result<Task>() != null);
			var x = 1;

			return FaultedTask<object>(error);
		}

        /// <summary> Some comments </summary>
        public static Task FaultedTask4(Exception error)
		{
			Contract.Ensures(Contract.Result<Task>() != null);

			var rrr = FaultedTask<object>(error);

			return rrr;
		}

		public static Task MethodWihoutContracts(Exception error)
		{
			return FaultedTask<object>(error);
		}

		public Task InstanceWihoutContracts(Exception error)
		{
			return FaultedTask<object>(error);
		}

		#region SomeRegion

		public void VoidMethod(string par)
		{
			Contract.Requires<ArgumentNullException>(par != null);
			Contract.Ensures(_i > 10);
			var x = 1;
			if (x == 1)
				return;
		}

		public override string ToString()
		{
			return "x";
		}

		#endregion

		[ContractInvariantMethod]
		private void CheckInvariants()
		{
			Contract.Invariant(_i > 0);
		}

		[ContractInvariantMethod]
		private static void CheckStaticInvariants()
		{
			Contract.Invariant(stat > 0);
		}
	}

	[ContractClassFor(typeof(IFoo))]
	public abstract class FooContracts : IFoo
	{
		public IEnumerable<CultureInfo> GetFooChain(CultureInfo initial)
		{
			Contract.Requires(initial != null);

			throw new NotImplementedException();
		}

		public void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			throw new NotImplementedException();
		}
	}
}
