/*
 * Created by: Leslie Sanford
 * 
 * Contact: jabberdabber@hotmail.com
 * 
 * Last modified: 10/04/2005
 */

using System;

namespace Sanford.StateMachineToolkit
{
	/// <summary>
	/// The exception that is thrown when the StateMachineBuilder encounters
	/// an error while building a state machine.
	/// </summary>
	public class StateMachineBuilderException : ApplicationException
	{
		public StateMachineBuilderException(string message) : base(message)
		{
		}
	}
}
