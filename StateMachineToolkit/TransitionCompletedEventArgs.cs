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
	public class TransitionEventArgs<TState, TEvent> : EventArgs where TState : struct, IComparable, IFormattable where TEvent : struct, IComparable, IFormattable
	{
		public TransitionEventArgs(EventContext<TState, TEvent> eventContext)
		{
			this.eventContext = eventContext;
		}

		protected EventContext<TState, TEvent> eventContext;

		public TEvent EventID
		{
			[DebuggerStepThrough]
			get { return eventContext.CurrentEvent; }
		}

		public TState SourceStateID
		{
			[DebuggerStepThrough]
			get { return eventContext.SourceState; }
		}

		public object[] EventArgs
		{
			[DebuggerStepThrough]
			get { return eventContext.Args; }
		}
	}

	/// <summary>
	/// Summary description for TransitionCompletedEventArgs.
	/// </summary>
	public class TransitionCompletedEventArgs<TState, TEvent> : TransitionErrorEventArgs<TState, TEvent> 
		where TState : struct, IComparable, IFormattable /*, IConvertible*/
		where TEvent : struct, IComparable, IFormattable /*, IConvertible*/
	{
		private readonly TState targetStateID;

		private readonly object actionResult;

		public TransitionCompletedEventArgs(TState targetStateID, EventContext<TState, TEvent> eventContext, 
			object actionResult, Exception error) : base(eventContext, error)
		{
			this.targetStateID = targetStateID;
			this.actionResult = actionResult;
		}

		public TState TargetStateID
		{
			get { return targetStateID; }
		}

		public object ActionResult
		{
			get { return actionResult; }
		}
	}
}