using Sanford.StateMachineToolkit;

namespace TestExample
{
	public enum Events
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

	public enum States
	{
		S1 = 8,
	}

	public abstract class ExampleBase : PassiveStateMachine<States, Events>
	{
		private State<States, Events> stateS0, stateS1, stateS11, stateS2, stateS21, stateS211;

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
			EntryHandler enS0 = EntryS0;
			ExitHandler exS0 = ExitS0;
			stateS0 = new State<States, Events>(States.S1, enS0, exS0);
			EntryHandler enS1 = EntryS1;
			ExitHandler exS1 = ExitS1;
			stateS1 = new State<States, Events>(States.S1, enS1, exS1);
			EntryHandler enS11 = EntryS11;
			ExitHandler exS11 = ExitS11;
			stateS11 = new State<States, Events>(States.S1, enS11, exS11);
			EntryHandler enS2 = EntryS2;
			ExitHandler exS2 = ExitS2;
			stateS2 = new State<States, Events>(States.S1, enS2, exS2);
			EntryHandler enS21 = EntryS21;
			ExitHandler exS21 = ExitS21;
			stateS21 = new State<States, Events>(States.S1, enS21, exS21);
			EntryHandler enS211 = EntryS211;
			ExitHandler exS211 = ExitS211;
			stateS211 = new State<States, Events>(States.S1, enS211, exS211);
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
			Transition<States, Events> trans;
			trans = Transition.Create(null, stateS211);
			stateS0.Transitions.Add(Events.E, trans);
			trans = Transition.Create(null, stateS0);
			stateS1.Transitions.Add(Events.D, trans);
			trans = Transition.Create(null, stateS2);
			stateS1.Transitions.Add(Events.C, trans);
			trans = Transition.Create(null, stateS1);
			stateS1.Transitions.Add(Events.A, trans);
			trans = Transition.Create(null, stateS211);
			stateS1.Transitions.Add(Events.F, trans);
			trans = Transition.Create(null, stateS11);
			stateS1.Transitions.Add(Events.B, trans);
			trans = Transition.Create(null, stateS211);
			stateS11.Transitions.Add(Events.G, trans);
			trans = Transition.Create<States, Events>(guardFooIsTrue, null);
			trans.Actions.Add(actionSetFooToFalse);
			stateS11.Transitions.Add(Events.H, trans);
			trans = Transition.Create(null, stateS1);
			stateS2.Transitions.Add(Events.C, trans);
			trans = Transition.Create(null, stateS11);
			stateS2.Transitions.Add(Events.F, trans);
			trans = Transition.Create(null, stateS211);
			stateS21.Transitions.Add(Events.B, trans);
			trans = Transition.Create(guardFooIsFalse, stateS21);
			trans.Actions.Add(actionSetFooToTrue);
			stateS21.Transitions.Add(Events.H, trans);
			trans = Transition.Create(null, stateS21);
			stateS211.Transitions.Add(Events.D, trans);
			trans = Transition.Create(null, stateS0);
			stateS211.Transitions.Add(Events.G, trans);
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

		public override void Send(Events eventID, object[] args)
		{
			base.Send(eventID, args);
			Execute();
		}
	}
}