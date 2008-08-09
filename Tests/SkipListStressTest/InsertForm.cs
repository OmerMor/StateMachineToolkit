using System;
using System.Collections;
using System.ComponentModel;
using System.Threading;
using System.Windows.Forms;

namespace SkipListStressTest
{
	/// <summary>
	/// Form for inserting items into the dictionary.
	/// </summary>
	public class InsertForm : Form
	{
		private Label label1;
		private Button cancelButton;

		/// <summary>
		/// Required designer variable.
		/// </summary>
		private readonly Container components = new Container();

		// Indicates the type of insertion operation to perform.
		public enum Operation
		{
			Forward,
			Reverse,
			Random
		} ;

		// The dictionary used for insertion.
		private readonly IDictionary dic;

		// For shuffling numbers for random insertion.
		private readonly Shuffler shuffler = new Shuffler();

		// The type of insertion operation to perform.
		private readonly Operation op;

		// The time the operation took place.
		private double time;

		// Number of items to insert.
		private readonly int numItems;

		// Cancel operation flag.
		private bool cancel;

		// Worker thread for performing the insertion operation.
		private Thread workerThread;

		/// <summary>
		/// Initializes an instance of the InsertForm class.
		/// </summary>
		/// <param name="dic">
		/// The dictionary to use for insertion.
		/// </param>
		/// <param name="op">
		/// The type of insertion operation to perform.
		/// </param>
		/// <param name="numItems">
		/// The number of items to insert.
		/// </param>
		public InsertForm(IDictionary dic, Operation op, int numItems)
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			this.dic = dic;
			this.op = op;
			this.numItems = numItems;
			Load += OnLoad;
			CancelButton = cancelButton;
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.label1 = new System.Windows.Forms.Label();
			this.cancelButton = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(32, 8);
			this.label1.Name = "label1";
			this.label1.TabIndex = 0;
			this.label1.Text = "Cancel operation";
			this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// cancelButton
			// 
			this.cancelButton.Location = new System.Drawing.Point(40, 40);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.TabIndex = 1;
			this.cancelButton.Text = "Cancel";
			this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
			// 
			// InsertForm
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(168, 86);
			this.Controls.Add(this.cancelButton);
			this.Controls.Add(this.label1);
			this.Name = "InsertForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Inserting...";
			this.ResumeLayout(false);
		}

		#endregion

		/// <summary>
		/// Event handler for the Cancel button.
		/// </summary>
		private void cancelButton_Click(object sender, EventArgs e)
		{
			// Operation has been cancelled.
			cancel = true;
		}

		/// <summary>
		/// Form is loaded.
		/// </summary>
		private void OnLoad(object sender, EventArgs e)
		{
			// Determine which operation to perform and begin the worker
			// thread accordingly.
			switch (op)
			{
				case Operation.Forward:
					workerThread = new Thread(InsertForward);
					break;

				case Operation.Reverse:
					workerThread = new Thread(InsertReverse);
					break;

				case Operation.Random:
					workerThread = new Thread(InsertRandom);
					break;
			}

			// Begin thread.
			workerThread.Start();
		}

		/// <summary>
		/// Worker thread function for inserting items in forward order.
		/// </summary>
		private void InsertForward()
		{
			// Get current tick count. This will be used to determine the 
			// timing results.
			int startTick = Environment.TickCount;

			// Insert items into the dictionary.
			for (int i = 0; i < numItems && !cancel; i++)
			{
				dic.Add(i, i);
			}

			// If the operation was not cancelled, calculate time results.
			if (!cancel)
			{
				time = (Environment.TickCount - startTick)/1000.0;
				DialogResult = DialogResult.OK;
			}
				// Else indicate that the operation was cancelled.
			else
			{
				DialogResult = DialogResult.Cancel;
			}

			// Close form.
			BeginInvoke((MethodInvoker) Close);
		}

		/// <summary>
		/// Worker thread function for inserting items in reverse order.
		/// </summary>
		private void InsertReverse()
		{
			// Get current tick count. This will be used to determine the 
			// timing results.
			int startTick = Environment.TickCount;

			// Insert items into the dictionary.
			for (int i = numItems; i >= 0 && !cancel; i--)
			{
				dic.Add(i, i);
			}

			// If the operation was not cancelled, calculate time results.
			if (!cancel)
			{
				time = (Environment.TickCount - startTick)/1000.0;
				DialogResult = DialogResult.OK;
			}
				// Else indicate that the operation was cancelled.
			else
			{
				DialogResult = DialogResult.Cancel;
			}

			// Close form.
			BeginInvoke((MethodInvoker) Close);
		}

		/// <summary>
		/// Worker thread function for inserting items in random order.
		/// </summary>
		private void InsertRandom()
		{
			// Create an array to hold numbers.
			int[] array = new int[numItems];

			// Fill array with numbers.
			for (int i = 0; i < array.Length && !cancel; i++)
			{
				array[i] = i;
			}

			// If the operation has not been cancelled, shuffle the numbers.
			if (!cancel)
			{
				shuffler.Shuffle(array);
			}

			// Get current tick count. This will be used to determine the 
			// timing results.
			int startTick = Environment.TickCount;

			// Insert items into the dictionary.
			for (int i = 0; i < array.Length && !cancel; i++)
			{
				dic.Add(array[i], array[i]);
			}

			// If the operation was not cancelled, calculate time results.
			if (!cancel)
			{
				time = (Environment.TickCount - startTick)/1000.0;
				DialogResult = DialogResult.OK;
			}
				// Else indicate that the operation was cancelled.
			else
			{
				DialogResult = DialogResult.Cancel;
			}

			// Close form.
			BeginInvoke((MethodInvoker) Close);
		}

		/// <summary>
		/// Time property - how long the operation took.
		/// </summary>
		public double Time
		{
			get { return time; }
		}
	}
}