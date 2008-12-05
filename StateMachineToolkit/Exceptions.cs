using System;

namespace Sanford.StateMachineToolkit
{
	/// <summary>
	/// Represents errors that occur during <see cref="StateMachine{TState,TEvent}.Transition"/>'s
	/// <see cref="StateMachine{TState,TEvent}.Transition.Guard"/> execution.
	/// </summary>
	[Serializable]
	public class GuardException : Exception
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="GuardException"/> class.
		/// </summary>
		/// <param name="message">The message.</param>
		/// <param name="ex">The ex.</param>
		public GuardException(string message, Exception ex)
			: base(message, ex)
		{
		}
	}

	/// <summary>
	/// Represents errors that occur during <see cref="StateMachine{TState,TEvent}.State"/>'s 
	/// <see cref="StateMachine{TState,TEvent}.State.Entry"/> execution.
	/// </summary>
	[Serializable]
	public class EntryException : Exception
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="EntryException"/> class.
		/// </summary>
		/// <param name="message">The message.</param>
		/// <param name="ex">The ex.</param>
		public EntryException(string message, Exception ex)
			: base(message, ex)
		{
		}
	}

	/// <summary>
	/// Represents errors that occur during <see cref="StateMachine{TState,TEvent}.Transition"/>'s 
	/// <see cref="StateMachine{TState,TEvent}.Transition.Actions"/> execution.
	/// </summary>
	[Serializable]
	public class ActionException : Exception
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="ActionException"/> class.
		/// </summary>
		/// <param name="message">The message.</param>
		/// <param name="ex">The ex.</param>
		public ActionException(string message, Exception ex)
			: base(message, ex)
		{
		}
	}

	/// <summary>
	/// Represents errors that occur during <see cref="StateMachine{TState,TEvent}.State"/>'s 
	/// <see cref="StateMachine{TState,TEvent}.State.Exit"/> execution.
	/// </summary>
	[Serializable]
	public class ExitException : Exception
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="ExitException"/> class.
		/// </summary>
		/// <param name="message">The message.</param>
		/// <param name="ex">The ex.</param>
		public ExitException(string message, Exception ex)
			: base(message, ex)
		{
		}
	}
}