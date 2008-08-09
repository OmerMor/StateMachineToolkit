/*
 * Created by: Leslie Sanford
 * 
 * Contact: jabberdabber@hotmail.com
 * 
 * Last modified: 10/01/2005
 */

using System.Collections;

namespace Sanford.StateMachineToolkit.CodeGeneration
{
	/// <summary>
	/// Builds all of the methods responsible for initializing the state 
	/// machine members.
	/// </summary>
	internal class InitializeMethodBuilder
	{
		#region InitializeMethodBuilder Members

		#region Fields

		private string stateMachineName = string.Empty;

		private readonly StateInitializeBuilder stateInitializeBuilder;

		private readonly GuardInitializeBuilder guardInitializeBuilder;

		private readonly ActionInitializeBuilder actionInitializeBuilder;

		private readonly TransitionInitializeBuilder transitionInitializeBuilder;

		private readonly RelationshipInitializeBuilder relationshipInitializeBuilder;

		private readonly HistoryInitializeBuilder historyInitializeBuilder;

		private readonly InitialStateInitializeBuilder initialStateInitializeBuilder;

		private ArrayList result = new ArrayList();

		#endregion

		#region Construction

		/// <summary>
		/// Initializes a new instance of the InitializeMethodBuilder class
		/// with the tables necessary for building the methods.
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
		public InitializeMethodBuilder(ICollection states, ICollection events,
		                               ICollection guards, ICollection actions, IDictionary stateTransitions,
		                               IDictionary stateRelationships, IDictionary stateHistoryTypes,
		                               IDictionary stateInitialStates)
		{
			stateInitializeBuilder = new StateInitializeBuilder(states, events);
			guardInitializeBuilder = new GuardInitializeBuilder(guards);
			actionInitializeBuilder = new ActionInitializeBuilder(actions);
			transitionInitializeBuilder = new TransitionInitializeBuilder(stateTransitions);
			relationshipInitializeBuilder = new RelationshipInitializeBuilder(stateRelationships);
			historyInitializeBuilder = new HistoryInitializeBuilder(stateHistoryTypes);
			initialStateInitializeBuilder = new InitialStateInitializeBuilder(stateInitialStates);
		}

		#endregion

		#region Methods

		/// <summary>
		/// Builds the methods.
		/// </summary>
		public void Build()
		{
			result = new ArrayList();

			stateInitializeBuilder.Build();
			result.Add(stateInitializeBuilder.Result);
			guardInitializeBuilder.Build();
			result.Add(guardInitializeBuilder.Result);
			actionInitializeBuilder.Build();
			result.Add(actionInitializeBuilder.Result);
			transitionInitializeBuilder.StateMachineName = StateMachineName;
			transitionInitializeBuilder.Build();
			result.Add(transitionInitializeBuilder.Result);
			relationshipInitializeBuilder.Build();
			result.Add(relationshipInitializeBuilder.Result);
			historyInitializeBuilder.Build();
			result.Add(historyInitializeBuilder.Result);
			initialStateInitializeBuilder.Build();
			result.Add(initialStateInitializeBuilder.Result);
		}

		#endregion

		#region Properties

		public string StateMachineName
		{
			get { return stateMachineName; }
			set { stateMachineName = value; }
		}

		/// <summary>
		/// Gets the collection of built methods.
		/// </summary>
		public ICollection Result
		{
			get { return result; }
		}

		#endregion

		#endregion
	}
}