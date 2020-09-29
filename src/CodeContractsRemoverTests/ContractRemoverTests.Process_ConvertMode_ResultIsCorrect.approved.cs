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
		private static int stat;

		public static Utils(int i)
		{
			if (!(i > 0)) throw new ArgumentOutOfRangeException(nameof(i), "Contract assertion not met: i > 0");

			_i = i;
		    CheckStaticInvariants();
		}

        /// <summary> Some comments </summary>
		public int I
		{
			set
			{
				if (!(value > 0)) throw new ArgumentOutOfRangeException(nameof(value), "Contract assertion not met: value > 0");
				_i = value;
			    CheckInvariants();
			}
			get
			{
			    if (!(_i > 0)) throw new InvalidOperationException("Contract assertion not met: Contract.Result<int>() > 0");
			    CheckInvariants();
			    return _i;
			}
		}

        /// <summary> Some comments </summary>
        public string s
        {
	        get
	        {
	            var result = "x";
	            if (result == null) throw new InvalidOperationException("Contract assertion not met: Contract.Result<string>() != null");
	            CheckInvariants();
	            return result;
	        }
        }

		public static Task FaultedTask([NotNull] Exception error)
		{
			if (error == null) throw new ArgumentNullException(nameof(error), "Contract assertion not met: error != null");
			if (error == null) throw new ArgumentNullException(nameof(error), "Contract assertion not met: error != null");

			var x = 1;
			if (x == 1)
			{
			    var result = FaultedTask<object>(error);
			    if (result == null) throw new InvalidOperationException("Contract assertion not met: Contract.Result<Task>() != null");
			    CheckStaticInvariants();
			    return result;
			}
			else if (x == 2)
			{
				{
				    var result = FaultedTask<object>(error);
				    if (result == null) throw new InvalidOperationException("Contract assertion not met: Contract.Result<Task>() != null");
				    CheckStaticInvariants();
				    return result;
				}
			}
			else
			{
			    var result = null;
			    if (result == null) throw new InvalidOperationException("Contract assertion not met: Contract.Result<Task>() != null");
			    CheckStaticInvariants();
			    return result;
			}
		}

		public static Task FaultedTask2([NotNull] Exception error)
		{
			if (error == null) throw new ArgumentNullException(nameof(error), "Contract assertion not met: error != null");

		    var result = FaultedTask<object>(error);
		    CheckStaticInvariants();
		    return result;
		}

		public static Task FaultedTask3(Exception error)
		{
			var x = 1;

		    var result = FaultedTask<object>(error);
		    if (result == null) throw new InvalidOperationException("Contract assertion not met: Contract.Result<Task>() != null");
		    CheckStaticInvariants();
		    return result;
		}

        /// <summary> Some comments </summary>
        public static Task FaultedTask4(Exception error)
		{

			var rrr = FaultedTask<object>(error);

		    if (rrr == null) throw new InvalidOperationException("Contract assertion not met: Contract.Result<Task>() != null");
		    CheckStaticInvariants();
		    return rrr;
		}

		public static Task MethodWihoutContracts(Exception error)
		{
		    var result = FaultedTask<object>(error);
		    CheckStaticInvariants();
		    return result;
		}

		public Task InstanceWihoutContracts(Exception error)
		{
		    var result = FaultedTask<object>(error);
		    CheckInvariants();
		    return result;
		}

		#region SomeRegion

		public void VoidMethod([NotNull] string par)
		{
			if (par == null) throw new ArgumentNullException(nameof(par), "Contract assertion not met: par != null");
			var x = 1;
			if (x == 1)
			{
			    if (!(_i > 10)) throw new InvalidOperationException("Contract assertion not met: _i > 10");
			    CheckInvariants();
			    return;
			}
		    if (!(_i > 10)) throw new InvalidOperationException("Contract assertion not met: _i > 10");
		    CheckInvariants();
		}

		public override string ToString()
		{
			return "x";
		}
		
		#endregion

		private void CheckInvariants()
		{
			if (!(_i > 0)) throw new ArgumentOutOfRangeException("_i", "Contract assertion not met: _i > 0");
		}
		
		private static void CheckStaticInvariants()
		{
			if (!(stat > 0)) throw new ArgumentOutOfRangeException("stat", "Contract assertion not met: stat > 0");
		}
	}

	[ContractClassFor(typeof(IFoo))]
	public abstract class FooContracts : IFoo
	{
		public IEnumerable<CultureInfo> GetFooChain([NotNull] CultureInfo initial)
		{
			if (initial == null) throw new ArgumentNullException(nameof(initial), "Contract assertion not met: initial != null");

			throw new NotImplementedException();
		}

		public void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			throw new NotImplementedException();
		}
	}
}
