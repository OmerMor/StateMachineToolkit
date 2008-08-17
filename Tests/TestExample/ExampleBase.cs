using Sanford.StateMachineToolkit;

namespace TestExample
{
	public enum EventID
	{
		E,
		D,
		C,
		A,
		F,
		B,
		G,
		H,
	}

	public enum StateID
	{
		S1 = 8,
	}

	public abstract class ExampleBase : PassiveStateMachine<StateID, EventID>
	{
		private State stateS0, stateS1, stateS11, stateS2, stateS21, stateS211;

		private GuardHandler guardFooIsTrue, guardFooIsFalse;

		private ActionHandler actionSetFooToFalse, actionSetFooToTrue;

		protected ExampleBase()
		{
			Initialize();
		}

		private void Initialize()
		{
			InitializeStates();
			InitializeGuards();
			InitializeActions();
			InitializeTransitions();
			InitializeRelationships();
			InitializeHistoryTypes();
			InitializeInitialStates();
			Initialize(stateS0);
		}

		private void InitializeStates()
		{
			stateS0 = CreateState(StateID.S1, EntryS0, ExitS0);
			stateS1 = CreateState(StateID.S1, EntryS1, ExitS1);
			stateS11 = CreateState(StateID.S1, EntryS11, ExitS11);
			stateS2 = CreateState(StateID.S1, EntryS2, ExitS2);
			stateS21 = CreateState(StateID.S1, EntryS21, ExitS21);
			stateS211 = CreateState(StateID.S1, EntryS211, ExitS211);
		}

		private void InitializeGuards()
		{
			guardFooIsTrue = FooIsTrue;
			guardFooIsFalse = FooIsFalse;
		}

		private void InitializeActions()
		{
			actionSetFooToFalse = SetFooToFalse;
			actionSetFooToTrue = SetFooToTrue;
		}

		private void InitializeTransitions()
		{
			stateS0.Transitions.Add(EventID.E, null, stateS211);
			stateS1.Transitions.Add(EventID.D, null, stateS0);
			stateS1.Transitions.Add(EventID.C, null, stateS2);
			stateS1.Transitions.Add(EventID.A, null, stateS1);
			stateS1.Transitions.Add(EventID.F, null, stateS211);
			stateS1.Transitions.Add(EventID.B, null, stateS11);
			stateS11.Transitions.Add(EventID.G, null, stateS211);
			stateS11.Transitions.Add(EventID.H, guardFooIsTrue, null, actionSetFooToFalse);
			stateS2.Transitions.Add(EventID.C, null, stateS1);
			stateS2.Transitions.Add(EventID.F, null, stateS11);
			stateS21.Transitions.Add(EventID.B, null, stateS211);
			stateS21.Transitions.Add(EventID.H, guardFooIsFalse, stateS21, actionSetFooToTrue);
			stateS211.Transitions.Add(EventID.D, null, stateS21);
			stateS211.Transitions.Add(EventID.G, null, stateS0);
		}

		private void InitializeRelationships()
		{
			stateS0.Substates.Add(stateS1);
			stateS1.Substates.Add(stateS11);
			stateS0.Substates.Add(stateS2);
			stateS2.Substates.Add(stateS21);
			stateS21.Substates.Add(stateS211);
		}

		private void InitializeHistoryTypes()
		{
			stateS0.HistoryType = HistoryType.None;
			stateS1.HistoryType = HistoryType.None;
			stateS11.HistoryType = HistoryType.None;
			stateS2.HistoryType = HistoryType.None;
			stateS21.HistoryType = HistoryType.None;
			stateS211.HistoryType = HistoryType.None;
		}

		private void InitializeInitialStates()
		{
			stateS0.InitialState = stateS1;
			stateS1.InitialState = stateS11;
			stateS2.InitialState = stateS21;
			stateS21.InitialState = stateS211;
		}

		protected virtual void EntryS0()
		{
		}

		protected virtual void EntryS1()
		{
		}

		protected virtual void EntryS11()
		{
		}

		protected virtual void EntryS2()
		{
		}

		protected virtual void EntryS21()
		{
		}

		protected virtual void EntryS211()
		{
		}

		protected virtual void ExitS0()
		{
		}

		protected virtual void ExitS1()
		{
		}

		protected virtual void ExitS11()
		{
		}

		protected virtual void ExitS2()
		{
		}

		protected virtual void ExitS21()
		{
		}

		protected virtual void ExitS211()
		{
		}

		protected abstract bool FooIsTrue(object[] args);

		protected abstract bool FooIsFalse(object[] args);

		protected abstract void SetFooToFalse(object[] args);

		protected abstract void SetFooToTrue(object[] args);

		public override void Send(EventID eventID, object[] args)
		{
			base.Send(eventID, args);
			Execute();
		}
	}
}