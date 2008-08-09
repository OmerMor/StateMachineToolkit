using System;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using Sanford.Collections;

namespace SkipListStressTest
{
	/// <summary>
	/// The main form for the SkipList stress test program.
	/// </summary>
	public class StressTestForm : Form
	{
		private GroupBox groupBox1;
		private RadioButton forwardRButton;
		private RadioButton reverseRButton;
		private RadioButton randomRButton;
		private Label label1;
		private Button insertButton;
		private GroupBox groupBox2;
		private Label label2;
		private NumericTextBox numItemsTxtBox;
		private NumericTextBox searchKeyTxtBox;
		private Button searchButton;
		private readonly IContainer components = new Container();
		private GroupBox groupBox3;
		private RadioButton skipListRButton;
		private RadioButton sortedListRButton;

		private IDictionary dic = new SkipList();

		public StressTestForm()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();
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
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.numItemsTxtBox = new SkipListStressTest.NumericTextBox();
			this.insertButton = new System.Windows.Forms.Button();
			this.label1 = new System.Windows.Forms.Label();
			this.randomRButton = new System.Windows.Forms.RadioButton();
			this.reverseRButton = new System.Windows.Forms.RadioButton();
			this.forwardRButton = new System.Windows.Forms.RadioButton();
			this.groupBox2 = new System.Windows.Forms.GroupBox();
			this.searchButton = new System.Windows.Forms.Button();
			this.label2 = new System.Windows.Forms.Label();
			this.searchKeyTxtBox = new SkipListStressTest.NumericTextBox();
			this.groupBox3 = new System.Windows.Forms.GroupBox();
			this.skipListRButton = new System.Windows.Forms.RadioButton();
			this.sortedListRButton = new System.Windows.Forms.RadioButton();
			this.groupBox1.SuspendLayout();
			this.groupBox2.SuspendLayout();
			this.groupBox3.SuspendLayout();
			this.SuspendLayout();
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.Add(this.numItemsTxtBox);
			this.groupBox1.Controls.Add(this.insertButton);
			this.groupBox1.Controls.Add(this.label1);
			this.groupBox1.Controls.Add(this.randomRButton);
			this.groupBox1.Controls.Add(this.reverseRButton);
			this.groupBox1.Controls.Add(this.forwardRButton);
			this.groupBox1.Location = new System.Drawing.Point(16, 16);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(224, 128);
			this.groupBox1.TabIndex = 2;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Insertion";
			// 
			// numItemsTxtBox
			// 
			this.numItemsTxtBox.Location = new System.Drawing.Point(136, 56);
			this.numItemsTxtBox.MaxLength = 6;
			this.numItemsTxtBox.Name = "numItemsTxtBox";
			this.numItemsTxtBox.Size = new System.Drawing.Size(48, 20);
			this.numItemsTxtBox.TabIndex = 6;
			this.numItemsTxtBox.Text = "";
			// 
			// insertButton
			// 
			this.insertButton.Location = new System.Drawing.Point(120, 88);
			this.insertButton.Name = "insertButton";
			this.insertButton.TabIndex = 5;
			this.insertButton.Text = "Insert";
			this.insertButton.Click += new System.EventHandler(this.insertButton_Click);
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(120, 24);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(88, 23);
			this.label1.TabIndex = 3;
			this.label1.Text = "Number of items";
			this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// randomRButton
			// 
			this.randomRButton.Location = new System.Drawing.Point(8, 88);
			this.randomRButton.Name = "randomRButton";
			this.randomRButton.Size = new System.Drawing.Size(72, 24);
			this.randomRButton.TabIndex = 2;
			this.randomRButton.Text = "Random";
			// 
			// reverseRButton
			// 
			this.reverseRButton.Location = new System.Drawing.Point(8, 56);
			this.reverseRButton.Name = "reverseRButton";
			this.reverseRButton.Size = new System.Drawing.Size(64, 24);
			this.reverseRButton.TabIndex = 1;
			this.reverseRButton.Text = "Reverse";
			// 
			// forwardRButton
			// 
			this.forwardRButton.Checked = true;
			this.forwardRButton.Location = new System.Drawing.Point(8, 24);
			this.forwardRButton.Name = "forwardRButton";
			this.forwardRButton.Size = new System.Drawing.Size(64, 24);
			this.forwardRButton.TabIndex = 0;
			this.forwardRButton.TabStop = true;
			this.forwardRButton.Text = "Forward";
			// 
			// groupBox2
			// 
			this.groupBox2.Controls.Add(this.searchButton);
			this.groupBox2.Controls.Add(this.label2);
			this.groupBox2.Controls.Add(this.searchKeyTxtBox);
			this.groupBox2.Location = new System.Drawing.Point(16, 160);
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.Size = new System.Drawing.Size(112, 128);
			this.groupBox2.TabIndex = 3;
			this.groupBox2.TabStop = false;
			this.groupBox2.Text = "Search";
			// 
			// searchButton
			// 
			this.searchButton.Location = new System.Drawing.Point(16, 88);
			this.searchButton.Name = "searchButton";
			this.searchButton.TabIndex = 8;
			this.searchButton.Text = "Search";
			this.searchButton.Click += new System.EventHandler(this.searchButton_Click);
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(32, 24);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(48, 23);
			this.label2.TabIndex = 0;
			this.label2.Text = "Key";
			this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// searchKeyTxtBox
			// 
			this.searchKeyTxtBox.Location = new System.Drawing.Point(32, 56);
			this.searchKeyTxtBox.MaxLength = 6;
			this.searchKeyTxtBox.Name = "searchKeyTxtBox";
			this.searchKeyTxtBox.Size = new System.Drawing.Size(48, 20);
			this.searchKeyTxtBox.TabIndex = 7;
			this.searchKeyTxtBox.Text = "";
			// 
			// groupBox3
			// 
			this.groupBox3.Controls.Add(this.sortedListRButton);
			this.groupBox3.Controls.Add(this.skipListRButton);
			this.groupBox3.Location = new System.Drawing.Point(144, 160);
			this.groupBox3.Name = "groupBox3";
			this.groupBox3.Size = new System.Drawing.Size(96, 128);
			this.groupBox3.TabIndex = 4;
			this.groupBox3.TabStop = false;
			this.groupBox3.Text = "Type";
			// 
			// skipListRButton
			// 
			this.skipListRButton.Checked = true;
			this.skipListRButton.Location = new System.Drawing.Point(8, 32);
			this.skipListRButton.Name = "skipListRButton";
			this.skipListRButton.Size = new System.Drawing.Size(63, 24);
			this.skipListRButton.TabIndex = 0;
			this.skipListRButton.TabStop = true;
			this.skipListRButton.Text = "SkipList";
			this.skipListRButton.CheckedChanged += new System.EventHandler(this.skipListRButton_CheckedChanged);
			// 
			// sortedListRButton
			// 
			this.sortedListRButton.Location = new System.Drawing.Point(8, 80);
			this.sortedListRButton.Name = "sortedListRButton";
			this.sortedListRButton.Size = new System.Drawing.Size(80, 24);
			this.sortedListRButton.TabIndex = 1;
			this.sortedListRButton.Text = "SortedList";
			this.sortedListRButton.CheckedChanged += new System.EventHandler(this.sortedListRButton_CheckedChanged);
			// 
			// Form1
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(256, 302);
			this.Controls.Add(this.groupBox3);
			this.Controls.Add(this.groupBox2);
			this.Controls.Add(this.groupBox1);
			this.Name = "Form1";
			this.Text = "Skip List Stress Test";
			this.groupBox1.ResumeLayout(false);
			this.groupBox2.ResumeLayout(false);
			this.groupBox3.ResumeLayout(false);
			this.ResumeLayout(false);
		}

		#endregion

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		private static void Main()
		{
			Application.Run(new StressTestForm());
		}

		/// <summary>
		/// Event handler for the Insert button.
		/// </summary>
		private void insertButton_Click(object sender, EventArgs e)
		{
			InsertForm form;
			bool cancel = false;

			// If there are items to insert.
			if (numItemsTxtBox.Text == "") return;
			// Clear any previous items in the dictionary.
			dic.Clear();

			// Get the number of items to insert.
			int numItems = int.Parse(numItemsTxtBox.Text);

			// If the dictionary is a SortedList, change its capacity
			// to match that of the number of items to be inserted.
			if (dic is SortedList)
			{
				SortedList list = (SortedList) dic;
				list.Capacity = numItems;
				dic = list;
			}

			// If the Forward radio button is checked insert items in 
			// forward order.
			if (forwardRButton.Checked)
			{
				form = new InsertForm(dic,
				                      InsertForm.Operation.Forward, numItems);
			}
				// Else if the Reverse radio button is checked, insert items
				// in reverse order.
			else if (reverseRButton.Checked)
			{
				form = new InsertForm(dic,
				                      InsertForm.Operation.Reverse, numItems);
			}
				// Else insert items in random order.
			else
			{
				form = new InsertForm(dic,
				                      InsertForm.Operation.Random, numItems);
			}

			// Run insert dialog.
			if (form.ShowDialog() == DialogResult.Cancel)
			{
				cancel = true;
			}

			// If the operation was not cancelled, display results.
			if (!cancel)
			{
				MessageBox.Show("Operation took " + form.Time + " in seconds",
				                "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
			}
		}

		/// <summary>
		/// Event handler for the Search button.
		/// </summary>
		private void searchButton_Click(object sender, EventArgs e)
		{
			// If there is an item to search for.
			if (searchKeyTxtBox.Text == "") return;
			// Parse search string into an integer.
			int searchKey = int.Parse(searchKeyTxtBox.Text);

			// Get the current tick count. This will be used later to 
			// determine how long the search operation took.
			int startTick = Environment.TickCount;

			// Search for the key.
			bool found = dic.Contains(searchKey);

			// Determine how much time has elapsed.
			double time = (Environment.TickCount - startTick)/1000.0;

			// String message for the results.
			string msg;

			// If the search key was found.
			if (found)
			{
				// Create message.
				msg = String.Format("Key {0} was found in {1} seconds",
				                    searchKey, time);
			}
				// Else the search key was not found.
			else
			{
				// Create message.
				msg = String.Format("Key {0} was not found in {1} seconds",
				                    searchKey, time);
			}

			// Display message.
			MessageBox.Show(msg, "Information", MessageBoxButtons.OK,
			                MessageBoxIcon.Information);
		}

		/// <summary>
		/// Event handler for the SkipList radio button.
		/// </summary>
		private void skipListRButton_CheckedChanged(object sender, EventArgs e)
		{
			// Dictionary is now a SkipList.
			dic = new SkipList();
		}

		/// <summary>
		/// Event handler for the SortedList radio button.
		/// </summary>
		private void sortedListRButton_CheckedChanged(object sender, EventArgs e)
		{
			// Dictionary is now a SortedList.
			dic = new SortedList();
		}
	}
}