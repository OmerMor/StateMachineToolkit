using System;

namespace Sanford.StateMachineToolkit
{
	[Serializable]
	public class GuardException : Exception
	{
		public GuardException(string message, Exception ex)
			: base(message, ex)
		{
		}
	}

	[Serializable]
	public class EntryException : Exception
	{
		public EntryException(string message, Exception ex)
			: base(message, ex)
		{
		}
	}

	[Serializable]
	public class ActionException : Exception
	{
		public ActionException(string message, Exception ex)
			: base(message, ex)
		{
		}
	}

	[Serializable]
	public class ExitException : Exception
	{
		public ExitException(string message, Exception ex)
			: base(message, ex)
		{
		}
	}
}