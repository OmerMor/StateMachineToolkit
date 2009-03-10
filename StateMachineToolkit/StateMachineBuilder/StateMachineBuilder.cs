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

using System.CodeDom;
using System.Collections;
using System.Reflection;
using System.Xml.Serialization;
using Sanford.StateMachineToolkit.CodeGeneration;

namespace Sanford.StateMachineToolkit.StateMachineBuilder
{
    /// <summary>
    /// Generates code for state machine base classes.
    /// </summary>
    [XmlRoot("stateMachine")]
    public class StateMachineBuilder
    {
        #region StateMachineBuilder Members

        #region Fields

        // The default namespace.
        private const string DefaultNamespaceName = "YourNamespace";

        // The default state machine name.
        private const string DefaultStateMachineName = "YourStateMachine";

        // The name of the namespace in which the state machine resides.
        private string namespaceName = DefaultNamespaceName;

        // The name of the state machine.
        private string stateMachineName = DefaultStateMachineName;

        // The initial state of the state machine.
        private string initialState = string.Empty;

        // The root of the state hierarchy.
        private readonly StateRowCollection stateRowCollection = new StateRowCollection();

        private StateMachineType stateMachineType = StateMachineType.Active;

        // The result of building the state machine.
        private CodeNamespace result = new CodeNamespace();

        #endregion

        #region Construction

        #endregion

        #region Methods

        /// <summary>
        /// Builds the state machine.
        /// </summary>
        public void Build()
        {
            ArrayList states = new ArrayList();
            ArrayList events = new ArrayList();
            ArrayList guards = new ArrayList();
            ArrayList actions = new ArrayList();
            IDictionary stateTransitions = new SortedList();
            IDictionary stateRelationships = new SortedList();
            IDictionary stateHistoryTypes = new SortedList();
            IDictionary stateInitialStates = new SortedList();

            readStates(States, states);
            readEvents(States, events);
            readGuards(States, guards);
            readActions(States, actions);
            readStateTransitions(States, stateTransitions);
            readStateRelationships(States, stateRelationships);
            readStateHistoryTypes(States, stateHistoryTypes);
            readStateInitialStates(States, stateInitialStates);

            verifyTargets(States, states);
            verifyInitialState(States);
            verifyInitialStates(States);

            result = new CodeNamespace(NamespaceName);

            CodeTypeDeclaration stateMachineClass =
                new CodeTypeDeclaration(StateMachineName);

            if (StateMachineType == StateMachineType.Active)
            {
                stateMachineClass.BaseTypes.Add(typeof (ActiveStateMachine));
            }
            else
            {
                stateMachineClass.BaseTypes.Add(typeof (PassiveStateMachine));
            }

            stateMachineClass.IsClass = true;
            stateMachineClass.TypeAttributes =
                TypeAttributes.Abstract |
                TypeAttributes.Public;

            EventEnumeratorBuilder eventEnumBuilder = new EventEnumeratorBuilder(events);
            eventEnumBuilder.Build();
            stateMachineClass.Members.Add(eventEnumBuilder.Result);

            StateEnumeratorBuilder stateEnumBuilder = new StateEnumeratorBuilder(states);
            stateEnumBuilder.Build();
            stateMachineClass.Members.Add(stateEnumBuilder.Result);

            ConstructorBuilder constructorBuilder = new ConstructorBuilder();
            constructorBuilder.InitialState = InitialState;
            constructorBuilder.Build();

            foreach (CodeConstructor constructor in constructorBuilder.Result)
            {
                stateMachineClass.Members.Add(constructor);
            }

            FieldBuilder fieldBuilder = new FieldBuilder(states, events, guards, actions);
            fieldBuilder.Build();

            foreach (CodeMemberField field in fieldBuilder.Result)
            {
                stateMachineClass.Members.Add(field);
            }

            MethodBuilder methodBuilder = new MethodBuilder(states, events,
                                                            guards, actions, stateTransitions, stateRelationships,
                                                            stateHistoryTypes, stateInitialStates);
            methodBuilder.StateMachineName = StateMachineName;
            methodBuilder.InitialState = InitialState;
            methodBuilder.Build();

            foreach (CodeMemberMethod method in methodBuilder.Result)
            {
                stateMachineClass.Members.Add(method);
            }

            result.Types.Add(stateMachineClass);
        }

        /// <summary>
        /// Clears the builder of all states and transitions, and resets its 
        /// properties to their default values. 
        /// </summary>
        public void Clear()
        {
            States.Clear();
            NamespaceName = DefaultNamespaceName;
            StateMachineName = DefaultStateMachineName;
            InitialState = string.Empty;
        }

        // Reads all of the state machine's states.
        private static void readStates(StateRowCollection col, IList states)
        {
            foreach (StateRow row in col)
            {
                if (string.IsNullOrEmpty(row.Name))
                {
                    throw new StateMachineBuilderException(
                        "State name cannot be null or empty.");
                }
                if (states.Contains(row.Name))
                {
                    throw new StateMachineBuilderException("Duplicate states not allowed.");
                }

                states.Add(row.Name);

                readStates(row.Substates, states);
            }
        }

        // Reads all of the state machine's events.
        private static void readEvents(StateRowCollection col, IList events)
        {
            foreach (StateRow row in col)
            {
                foreach (TransitionRow transRow in row.Transitions)
                {
                    if (string.IsNullOrEmpty(transRow.Event))
                    {
                        throw new StateMachineBuilderException(
                            "Event cannot be null or empty.");
                    }

                    if (!events.Contains(transRow.Event))
                    {
                        events.Add(transRow.Event);
                    }
                }

                readEvents(row.Substates, events);
            }
        }

        // Reads all of the state machine's guards.
        private static void readGuards(StateRowCollection col, IList guards)
        {
            foreach (StateRow row in col)
            {
                foreach (TransitionRow transRow in row.Transitions)
                {
                    if (!string.IsNullOrEmpty(transRow.Guard) &&
                        !guards.Contains(transRow.Guard))
                    {
                        guards.Add(transRow.Guard);
                    }
                }

                readGuards(row.Substates, guards);
            }
        }

        // Reads all of the state machine's actions.
        private static void readActions(StateRowCollection col, IList actions)
        {
            foreach (StateRow row in col)
            {
                foreach (TransitionRow transRow in row.Transitions)
                {
                    foreach (ActionRow actionRow in transRow.Actions)
                    {
                        if (!actions.Contains(actionRow.Name))
                        {
                            actions.Add(actionRow.Name);
                        }
                    }
                }

                readActions(row.Substates, actions);
            }
        }

        // Reads all of the state machine's state transitions.
        private static void readStateTransitions(StateRowCollection col, IDictionary stateTransitions)
        {
            foreach (StateRow row in col)
            {
                if (row.Transitions.Count > 0)
                {
                    stateTransitions.Add(row.Name, row.Transitions);
                }

                readStateTransitions(row.Substates, stateTransitions);
            }
        }

        // Reads all of the state machine's substate/superstate relationships.
        private static void readStateRelationships(StateRowCollection col, IDictionary stateRelationships)
        {
            foreach (StateRow row in col)
            {
                foreach (StateRow childRow in row.Substates)
                {
                    stateRelationships.Add(childRow.Name, row.Name);
                }

                readStateRelationships(row.Substates, stateRelationships);
            }
        }

        // Reads all of the state machine's state history types.
        private static void readStateHistoryTypes(StateRowCollection col, IDictionary stateHistoryTypes)
        {
            foreach (StateRow row in col)
            {
                stateHistoryTypes.Add(row.Name, row.HistoryType);

                readStateHistoryTypes(row.Substates, stateHistoryTypes);
            }
        }

        // Reads all of the state machine's states' initial state.
        private static void readStateInitialStates(StateRowCollection col, IDictionary stateInitialStates)
        {
            foreach (StateRow row in col)
            {
                if (!string.IsNullOrEmpty(row.InitialState))
                {
                    stateInitialStates.Add(row.Name, row.InitialState);
                }

                readStateInitialStates(row.Substates, stateInitialStates);
            }
        }

        // Verifies that all of the targets are known.
        private static void verifyTargets(StateRowCollection col, IList states)
        {
            foreach (StateRow row in col)
            {
                foreach (TransitionRow transRow in row.Transitions)
                {
                    if (!string.IsNullOrEmpty(transRow.Target) &&
                        !states.Contains(transRow.Target))
                    {
                        throw new StateMachineBuilderException(
                            "Unknown target state: " + transRow.Target);
                    }
                }

                verifyTargets(row.Substates, states);
            }
        }

        // Verifies that the initial state is known.
        private void verifyInitialState(StateRowCollection col)
        {
            bool found = false;

            foreach (StateRow row in col)
            {
                if (row.Name != InitialState) continue;
                found = true;
                break;
            }

            if (!found)
            {
                throw new StateMachineBuilderException(
                    "Initial state is missing or is not a top state.");
            }
        }

        // Verifies that each superstate has an initial state and that its 
        // initial state is known.
        private static void verifyInitialStates(StateRowCollection col)
        {
            foreach (StateRow row in col)
            {
                if (string.IsNullOrEmpty(row.InitialState))
                {
                    if (row.Substates.Count > 0)
                    {
                        throw new StateMachineBuilderException(
                            "Initial state missing from " + row.Name + " state.");
                    }
                }
                else
                {
                    bool found = false;

                    foreach (StateRow childRow in row.Substates)
                    {
                        if (row.InitialState != childRow.Name) continue;
                        found = true;
                        break;
                    }

                    if (!found)
                    {
                        throw new StateMachineBuilderException(
                            "No substate match for initial state.");
                    }
                }

                verifyInitialStates(row.Substates);
            }
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the result of building the state machine.
        /// </summary>
        public CodeNamespace Result
        {
            get { return result; }
        }

        /// <summary>
        /// Gets the collection of StateRows that represent the state machine's
        /// top level states.
        /// </summary>
        [XmlElement("state", typeof (StateRow))]
        public StateRowCollection States
        {
            get { return stateRowCollection; }
        }

        /// <summary>
        /// Gets or sets the name of the namespace in which the state machine
        /// resides.
        /// </summary>
        [XmlAttribute("namespace")]
        public string NamespaceName
        {
            get { return namespaceName; }
            set { namespaceName = value; }
        }

        /// <summary>
        /// Gets or sets the name of the state machine.
        /// </summary>
        [XmlAttribute("name")]
        public string StateMachineName
        {
            get { return stateMachineName; }
            set { stateMachineName = value; }
        }

        /// <summary>
        /// Gets or sets the initial state of the state machine.
        /// </summary>
        [XmlAttribute("initialState")]
        public string InitialState
        {
            get { return initialState; }
            set { initialState = value; }
        }

        /// <summary>
        /// Gets or sets the type of the state machine.
        /// </summary>
        /// <value>The type of the state machine.</value>
        [XmlAttribute("stateMachineType")]
        public StateMachineType StateMachineType
        {
            get { return stateMachineType; }
            set { stateMachineType = value; }
        }

        #endregion

        #endregion
    }
}