/*
 * Created by: Leslie Sanford
 * 
 * Contact: jabberdabber@hotmail.com
 * 
 * Last modified: 10/04/2005
 */

using System;

namespace Sanford.StateMachineToolkit.StateMachineBuilder
{
    /// <summary>
    /// The exception that is thrown when the StateMachineBuilder encounters
    /// an error while building a state machine.
    /// </summary>
    public class StateMachineBuilderException : ApplicationException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StateMachineBuilderException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        public StateMachineBuilderException(string message) : base(message)
        {
        }
    }
}