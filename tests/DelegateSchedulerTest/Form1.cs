using System;
using System.Diagnostics;
using System.Threading;
using System.Windows.Forms;
using Sanford.Threading;

namespace DelegateSchedulerTest
{
	public partial class Form1 : Form
	{
		public Form1()
		{
			InitializeComponent();

			delegateScheduler1.PollingInterval = (int) pollNumericUpDown.Value;
		}

		private void startButton_Click(object sender, EventArgs e)
		{
			try
			{
				delegateScheduler1.Start();
				toolStripStatusLabel1.Text = "Running...";
			}
			catch (Exception ex)
			{
				MessageBox.Show("Error!", ex.Message, MessageBoxButtons.OK, MessageBoxIcon.Stop);
			}
		}

		private void stopButton_Click(object sender, EventArgs e)
		{
			try
			{
				delegateScheduler1.Stop();
				toolStripStatusLabel1.Text = "Stopped.";
			}
			catch (Exception ex)
			{
				MessageBox.Show("Error!", ex.Message, MessageBoxButtons.OK, MessageBoxIcon.Stop);
			}
		}

		private void clearButton_Click(object sender, EventArgs e)
		{
			try
			{
				delegateScheduler1.Clear();
				logListBox.Items.Clear();
			}
			catch (Exception ex)
			{
				MessageBox.Show("Error!", ex.Message, MessageBoxButtons.OK, MessageBoxIcon.Stop);
			}
		}

		private void pollNumericUpDown_ValueChanged(object sender, EventArgs e)
		{
			try
			{
				delegateScheduler1.PollingInterval = (int) pollNumericUpDown.Value;
			}
			catch (Exception ex)
			{
				MessageBox.Show("Error!", ex.Message, MessageBoxButtons.OK, MessageBoxIcon.Stop);
			}
		}

		private void addButton_Click(object sender, EventArgs e)
		{
			int millisecondsTimeout = (int) timeNumericUpDown.Value;
			object id = idTextBox.Text;
			int count = (int) countNumericUpDown.Value;

			try
			{
				delegateScheduler1.Add(count, millisecondsTimeout, new SendOrPostCallback(SchedulerCallback), id);
			}
			catch (Exception ex)
			{
				MessageBox.Show("Error!", ex.Message, MessageBoxButtons.OK, MessageBoxIcon.Stop);
			}
		}

		private void SchedulerCallback(object state)
		{
			// Do nothing...
		}

		private void delegateScheduler1_InvokeCompleted(object sender, InvokeCompletedEventArgs e)
		{
			object[] args = e.GetArgs();

			Debug.Assert(args != null);
			Debug.Assert(args.Length == 1);
			Debug.Assert(args[0] != null);

			if (e.Error == null)
			{
				logListBox.Items.Add(args[0].ToString());
			}
			else
			{
				MessageBox.Show("Error!", e.Error.Message, MessageBoxButtons.OK, MessageBoxIcon.Stop);
			}
		}
	}
}