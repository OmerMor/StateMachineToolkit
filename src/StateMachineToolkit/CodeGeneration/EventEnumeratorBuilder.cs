/*
 * Created by: Leslie Sanford
 * 
 * Contact: jabberdabber@hotmail.com
 * 
 * Last modified: 10/13/2005
 */

using System.CodeDom;
using System.Collections;

namespace Sanford.StateMachineToolkit.CodeGeneration
{
    /// <summary>
    /// Builds the event identifiers.
    /// </summary>
    internal class EventEnumeratorBuilder
    {
        #region EventEnumeratorBuilder Members

        #region Fields

        private readonly ICollection events;

        private CodeTypeDeclaration result = new CodeTypeDeclaration();

        #endregion

        #region Construction

        public EventEnumeratorBuilder(ICollection events)
        {
            this.events = events;
        }

        #endregion

        #region Methods

        public void Build()
        {
            result = new CodeTypeDeclaration("EventID");

            result.IsEnum = true;

            foreach (string e in events)
            {
                result.Members.Add(new CodeMemberField(typeof (int), e));
            }
        }

        #endregion

        #region Properties

        public CodeTypeDeclaration Result
        {
            get { return result; }
        }

        #endregion

        #endregion
    }
}