/*
 * Created by: Leslie Sanford
 * 
 * Contact: jabberdabber@hotmail.com
 * 
 * Last modified: 11/05/2005
 */

using System;
using System.CodeDom;
using System.Collections;

namespace Sanford.StateMachineToolkit.CodeGeneration
{
	/// <summary>
	/// Builds the methods that make up the state machine.
	/// </summary>
	internal class MethodBuilder
	{
		#region MethodBuilder Members

		#region Fields

		private string stateMachineName = string.Empty;

		private string initialState = string.Empty;

		// The state machine's states.
		private readonly ICollection states;

		// The state machine's guards.
		private readonly ICollection guards;

		// The state machine's actions.
		private readonly ICollection actions;

		// Builds initializing methods.
		private readonly InitializeMethodBuilder initializeMethodBuilder;

		// The collection of built methods.
		private ArrayList methods = new ArrayList();

		#endregion

		#region Construction

		/// <summary>
		/// Initializes a new instance of the MethodBuilder class with all
		/// of the tables necessary to build the state machine methods.
		/// </summary>
		/// <param name="states">
		/// The state machine's states.
		/// </param>
		/// <param name="events">
		/// The state machine's events.
		/// </param>
		/// <param name="guards">
		/// The state machine's guards.
		/// </param>
		/// <param name="actions">
		/// The state machine's actions.
		/// </param>
		/// <param name="stateTransitions">
		/// The state transitions.
		/// </param>
		/// <param name="stateRelationships">
		/// The substate/superstate relationships.
		/// </param>
		/// <param name="stateHistoryTypes">
		/// The state history types.
		/// </param>
		/// <param name="stateInitialStates">
		/// The states' initial states.
		/// </param>
		public MethodBuilder(ICollection states, ICollection events,
		                     ICollection guards, ICollection actions, IDictionary stateTransitions,
		                     IDictionary stateRelationships, IDictionary stateHistoryTypes,
		                     IDictionary stateInitialStates)
		{
			this.states = states;
			this.guards = guards;
			this.actions = actions;

			initializeMethodBuilder = new InitializeMethodBuilder(states, events,
			                                                      guards, actions, stateTransitions, stateRelationships,
			                                                      stateHistoryTypes, stateInitialStates);
		}

		#endregion

		#region Methods

		/// <summary>
		/// Builds the methods for the state machine.
		/// </summary>
		public void Build()
		{
			methods = new ArrayList();

			// 
			// Builds initializing methods.
			//

			BuildInitializeMethod();
			initializeMethodBuilder.StateMachineName = StateMachineName;
			initializeMethodBuilder.Build();
			methods.AddRange(initializeMethodBuilder.Result);

			//
			// Build entry and exit actions.
			//

			BuildMethods(states, "Entry", MemberAttributes.Family, null,
			             typeof (void));
			BuildMethods(states, "Exit", MemberAttributes.Family, null,
			             typeof (void));

			CodeParameterDeclarationExpression args =
				new CodeParameterDeclarationExpression(typeof (object[]), "args");

			// Build guard methods.
// ReSharper disable BitwiseOperatorOnEnumWihtoutFlags
			const MemberAttributes memberAttributes = MemberAttributes.Family | MemberAttributes.Abstract;
// ReSharper restore BitwiseOperatorOnEnumWihtoutFlags
			BuildMethods(guards, string.Empty, memberAttributes, args, typeof (bool));

			// Build action methods.
			BuildMethods(actions, string.Empty, memberAttributes, args, typeof (void));
		}

		// Builds methods.
		private void BuildMethods(ICollection col, string methodPrefix,
		                          MemberAttributes attributes,
		                          CodeParameterDeclarationExpression args, Type returnType)
		{
			CodeMemberMethod method;

			foreach (string name in col)
			{
				method = new CodeMemberMethod();
				method.Name = methodPrefix + name;
				method.Attributes = attributes;
				method.ReturnType = new CodeTypeReference(returnType);

				if (args != null)
				{
					method.Parameters.Add(args);
				}

				methods.Add(method);
			}
		}

		// Builds the initialize method.
		private void BuildInitializeMethod()
		{
			CodeThisReferenceExpression thisReference =
				new CodeThisReferenceExpression();
			CodeMemberMethod initializeMethod = new CodeMemberMethod();

			initializeMethod.Name = "Initialize";
			initializeMethod.Attributes = MemberAttributes.Private;

			CodeMethodInvokeExpression methodInvoke = new CodeMethodInvokeExpression();

			methodInvoke.Method.TargetObject = thisReference;
			methodInvoke.Method.MethodName = "InitializeStates";

			initializeMethod.Statements.Add(methodInvoke);

			methodInvoke = new CodeMethodInvokeExpression();

			methodInvoke.Method.TargetObject = thisReference;
			methodInvoke.Method.MethodName = "InitializeGuards";

			initializeMethod.Statements.Add(methodInvoke);

			methodInvoke = new CodeMethodInvokeExpression();

			methodInvoke.Method.TargetObject = thisReference;
			methodInvoke.Method.MethodName = "InitializeActions";

			initializeMethod.Statements.Add(methodInvoke);

			methodInvoke = new CodeMethodInvokeExpression();

			methodInvoke.Method.TargetObject = thisReference;
			methodInvoke.Method.MethodName = "InitializeTransitions";

			initializeMethod.Statements.Add(methodInvoke);

			methodInvoke = new CodeMethodInvokeExpression();

			methodInvoke.Method.TargetObject = thisReference;
			methodInvoke.Method.MethodName = "InitializeRelationships";

			initializeMethod.Statements.Add(methodInvoke);

			methodInvoke = new CodeMethodInvokeExpression();

			methodInvoke.Method.TargetObject = thisReference;
			methodInvoke.Method.MethodName = "InitializeHistoryTypes";

			initializeMethod.Statements.Add(methodInvoke);

			methodInvoke = new CodeMethodInvokeExpression();

			methodInvoke.Method.TargetObject = thisReference;
			methodInvoke.Method.MethodName = "InitializeInitialStates";

			initializeMethod.Statements.Add(methodInvoke);

			CodeExpression[] parameters =
				{
					new CodeFieldReferenceExpression(thisReference,
					                                 "state" + InitialState)
				};

			CodeMethodInvokeExpression initializeInvoke =
				new CodeMethodInvokeExpression(thisReference,
				                               "Initialize", parameters);

			initializeMethod.Statements.Add(initializeInvoke);

			methods.Add(initializeMethod);
		}

		#endregion

		#region Properties

		/// <summary>
		/// Gets the collection of built methods.
		/// </summary>
		public ICollection Result
		{
			get { return methods; }
		}

		public string StateMachineName
		{
			get { return stateMachineName; }
			set { stateMachineName = value; }
		}

		/// <summary>
		/// Gets or sets the state machine's initial state.
		/// </summary>
		public string InitialState
		{
			get { return initialState; }
			set { initialState = value; }
		}

		#endregion

		#endregion
	}
}