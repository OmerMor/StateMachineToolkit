#region License

/* Copyright (c) 2006 Leslie Sanford
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy 
 * of this software and associated documentation files (the "Software"), to 
 * deal in the Software without restriction, including without limitation the 
 * rights to use, copy, modify, merge, publish, distribute, sublicense, and/or 
 * sell copies of the Software, and to permit persons to whom the Software is 
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in 
 * all copies or substantial portions of the Software. 
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE 
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, 
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN 
 * THE SOFTWARE.
 */

#endregion

#region Contact

/*
 * Leslie Sanford
 * Email: jabberdabber@hotmail.com
 */

#endregion

using System;
using System.Diagnostics;

namespace Sanford.StateMachineToolkit
{
	/// <summary>
	/// Event data for transition events.
	/// </summary>
	/// <typeparam name="TState">The type of the state.</typeparam>
	/// <typeparam name="TEvent">The type of the event.</typeparam>
	public class TransitionEventArgs<TState, TEvent> : EventArgs
		where TState : struct, IComparable, IFormattable
		where TEvent : struct, IComparable, IFormattable
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="TransitionEventArgs{TState, TEvent}"/> class.
		/// </summary>
		/// <param name="eventContext">The event context.</param>
		public TransitionEventArgs(StateMachine<TState, TEvent>.EventContext eventContext)
		{
			m_eventContext = eventContext;
		}

		/// <summary>
		/// The event context.
		/// </summary>
		protected StateMachine<TState, TEvent>.EventContext m_eventContext;

		/// <summary>
		/// Gets the event ID.
		/// </summary>
		/// <value>The event ID.</value>
		public TEvent EventID
		{
			[DebuggerStepThrough]
			get { return m_eventContext.CurrentEvent; }
		}

		/// <summary>
		/// Gets the source state ID.
		/// </summary>
		/// <value>The source state ID.</value>
		public TState SourceStateID
		{
			[DebuggerStepThrough]
			get { return m_eventContext.SourceState; }
		}

		/// <summary>
		/// Gets the event arguments.
		/// </summary>
		/// <value>The event arguments.</value>
		public object[] EventArgs
		{
			[DebuggerStepThrough]
			get { return m_eventContext.Args; }
		}
	}

	/// <summary>
	/// Event data for the <see cref="StateMachine{TState,TEvent}.ExceptionThrown"/> event.
	/// </summary>
	/// <typeparam name="TState">The type of the state.</typeparam>
	/// <typeparam name="TEvent">The type of the event.</typeparam>
	public class TransitionErrorEventArgs<TState, TEvent> : TransitionEventArgs<TState, TEvent>
		where TState : struct, IComparable, IFormattable /*, IConvertible*/
		where TEvent : struct, IComparable, IFormattable /*, IConvertible*/
	{
		private readonly Exception m_error;

		private static readonly TransitionErrorEventArgs<TState, TEvent> empty =
			new TransitionErrorEventArgs<TState, TEvent>(null, null);

		/// <summary>
		/// Initializes a new instance of the <see cref="TransitionErrorEventArgs{TState, TEvent}"/> class.
		/// </summary>
		/// <param name="eventContext">The event context.</param>
		/// <param name="error">The error.</param>
		public TransitionErrorEventArgs(StateMachine<TState, TEvent>.EventContext eventContext, Exception error)
			: base(eventContext)
		{
			m_error = error;
		}

		/// <summary>
		/// Gets the error.
		/// </summary>
		/// <value>The error.</value>
		public Exception Error
		{
			[DebuggerStepThrough]
			get { return m_error; }
		}

		/// <summary>
		/// Represents an event with no event data.
		/// </summary>
		public new static TransitionErrorEventArgs<TState, TEvent> Empty
		{
			[DebuggerStepThrough]
			get { return empty; }
		}

		/// <summary>
		/// Gets a value indicating whether the state machine was initialized.
		/// </summary>
		/// <value><c>true</c> if the state machine was initialized; otherwise, <c>false</c>.</value>
		public bool MachineInitialized
		{
			get { return m_eventContext != null; }
		}
	}

	/// <summary>
	/// Event data for the <see cref="StateMachine{TState,TEvent}.TransitionCompleted"/> event.
	/// </summary>
	public class TransitionCompletedEventArgs<TState, TEvent> : TransitionErrorEventArgs<TState, TEvent>
		where TState : struct, IComparable, IFormattable /*, IConvertible*/
		where TEvent : struct, IComparable, IFormattable /*, IConvertible*/
	{
		private readonly TState m_targetStateID;

		private readonly object m_actionResult;

		/// <summary>
		/// Initializes a new instance of the <see cref="TransitionCompletedEventArgs{TState, TEvent}"/> class.
		/// </summary>
		/// <param name="targetStateID">The target state ID.</param>
		/// <param name="eventContext">The event context.</param>
		/// <param name="actionResult">The action result.</param>
		/// <param name="error">The error.</param>
		public TransitionCompletedEventArgs(TState targetStateID, StateMachine<TState, TEvent>.EventContext eventContext,
			object actionResult, Exception error)
			: base(eventContext, error)
		{
			m_targetStateID = targetStateID;
			m_actionResult = actionResult;
		}

		/// <summary>
		/// Gets the target state ID.
		/// </summary>
		/// <value>The target state ID.</value>
		public TState TargetStateID
		{
			get { return m_targetStateID; }
		}

		/// <summary>
		/// Gets the action result.
		/// </summary>
		/// <value>The action result.</value>
		public object ActionResult
		{
			get { return m_actionResult; }
		}
	}
}