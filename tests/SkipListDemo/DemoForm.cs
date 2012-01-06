using System;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using Sanford.Collections;

namespace SkipListDemo
{
	/// <summary>
	/// SkipList demo form.
	/// </summary>
	public class DemoForm : Form
	{
		private GroupBox groupBox1;
		private TextBox keyTextBox;
		private Label label1;
		private Label label2;
		private TextBox valueTextBox;
		private RadioButton addRButton;
		private RadioButton containsRButton;
		private RadioButton removeRButton;
		private Button executeButton;
		private RadioButton itemRButton;
		private GroupBox groupBox2;
		private Button idicEnumButton;
		private Button idicEnumMoveNext;
		private Button idicEnumReset;
		private Button clearButton;
		private ListBox keysListBox;
		private Button keysButton;
		private Button valuesButton;
		private ListBox valuesListBox;
		private GroupBox groupBox3;
		private Label label3;
		private Label countLabel;
		private Button copyToButton;
		private ListBox elementsListBox;
		private GroupBox groupBox4;
		private TextBox currentTextBox;

		/// <summary>
		/// Required designer variable.
		/// </summary>
		private readonly Container components = new Container();

		private readonly SkipList list;
		private IDictionaryEnumerator dicEnum;
		private Button icolEnumReset;
		private Button icolEnumMoveNext;
		private Button icolEnumButton;
		private IEnumerator colEnum;

		public DemoForm()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			list = new SkipList();
			countLabel.Text = list.Count.ToString();
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
			this.valuesButton = new System.Windows.Forms.Button();
			this.valuesListBox = new System.Windows.Forms.ListBox();
			this.keysButton = new System.Windows.Forms.Button();
			this.keysListBox = new System.Windows.Forms.ListBox();
			this.clearButton = new System.Windows.Forms.Button();
			this.groupBox2 = new System.Windows.Forms.GroupBox();
			this.idicEnumReset = new System.Windows.Forms.Button();
			this.idicEnumMoveNext = new System.Windows.Forms.Button();
			this.idicEnumButton = new System.Windows.Forms.Button();
			this.itemRButton = new System.Windows.Forms.RadioButton();
			this.executeButton = new System.Windows.Forms.Button();
			this.removeRButton = new System.Windows.Forms.RadioButton();
			this.containsRButton = new System.Windows.Forms.RadioButton();
			this.addRButton = new System.Windows.Forms.RadioButton();
			this.label2 = new System.Windows.Forms.Label();
			this.valueTextBox = new System.Windows.Forms.TextBox();
			this.label1 = new System.Windows.Forms.Label();
			this.keyTextBox = new System.Windows.Forms.TextBox();
			this.groupBox3 = new System.Windows.Forms.GroupBox();
			this.groupBox4 = new System.Windows.Forms.GroupBox();
			this.currentTextBox = new System.Windows.Forms.TextBox();
			this.icolEnumReset = new System.Windows.Forms.Button();
			this.icolEnumMoveNext = new System.Windows.Forms.Button();
			this.icolEnumButton = new System.Windows.Forms.Button();
			this.copyToButton = new System.Windows.Forms.Button();
			this.elementsListBox = new System.Windows.Forms.ListBox();
			this.countLabel = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.groupBox1.SuspendLayout();
			this.groupBox2.SuspendLayout();
			this.groupBox3.SuspendLayout();
			this.groupBox4.SuspendLayout();
			this.SuspendLayout();
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.Add(this.valuesButton);
			this.groupBox1.Controls.Add(this.valuesListBox);
			this.groupBox1.Controls.Add(this.keysButton);
			this.groupBox1.Controls.Add(this.keysListBox);
			this.groupBox1.Controls.Add(this.clearButton);
			this.groupBox1.Controls.Add(this.groupBox2);
			this.groupBox1.Controls.Add(this.itemRButton);
			this.groupBox1.Controls.Add(this.executeButton);
			this.groupBox1.Controls.Add(this.removeRButton);
			this.groupBox1.Controls.Add(this.containsRButton);
			this.groupBox1.Controls.Add(this.addRButton);
			this.groupBox1.Controls.Add(this.label2);
			this.groupBox1.Controls.Add(this.valueTextBox);
			this.groupBox1.Controls.Add(this.label1);
			this.groupBox1.Controls.Add(this.keyTextBox);
			this.groupBox1.Location = new System.Drawing.Point(8, 8);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(520, 232);
			this.groupBox1.TabIndex = 0;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "IDictionary ";
			// 
			// valuesButton
			// 
			this.valuesButton.Location = new System.Drawing.Point(400, 192);
			this.valuesButton.Name = "valuesButton";
			this.valuesButton.TabIndex = 9;
			this.valuesButton.Text = "Values";
			this.valuesButton.Click += new System.EventHandler(this.valuesButton_Click);
			// 
			// valuesListBox
			// 
			this.valuesListBox.Location = new System.Drawing.Point(376, 128);
			this.valuesListBox.Name = "valuesListBox";
			this.valuesListBox.SelectionMode = System.Windows.Forms.SelectionMode.None;
			this.valuesListBox.Size = new System.Drawing.Size(120, 56);
			this.valuesListBox.TabIndex = 10;
			// 
			// keysButton
			// 
			this.keysButton.Location = new System.Drawing.Point(400, 88);
			this.keysButton.Name = "keysButton";
			this.keysButton.TabIndex = 8;
			this.keysButton.Text = "Keys";
			this.keysButton.Click += new System.EventHandler(this.keysButton_Click);
			// 
			// keysListBox
			// 
			this.keysListBox.Location = new System.Drawing.Point(376, 24);
			this.keysListBox.Name = "keysListBox";
			this.keysListBox.SelectionMode = System.Windows.Forms.SelectionMode.None;
			this.keysListBox.Size = new System.Drawing.Size(120, 56);
			this.keysListBox.TabIndex = 12;
			// 
			// clearButton
			// 
			this.clearButton.Location = new System.Drawing.Point(264, 192);
			this.clearButton.Name = "clearButton";
			this.clearButton.TabIndex = 7;
			this.clearButton.Text = "Clear";
			this.clearButton.Click += new System.EventHandler(this.clearButton_Click);
			// 
			// groupBox2
			// 
			this.groupBox2.Controls.Add(this.idicEnumReset);
			this.groupBox2.Controls.Add(this.idicEnumMoveNext);
			this.groupBox2.Controls.Add(this.idicEnumButton);
			this.groupBox2.Location = new System.Drawing.Point(16, 96);
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.Size = new System.Drawing.Size(232, 120);
			this.groupBox2.TabIndex = 9;
			this.groupBox2.TabStop = false;
			this.groupBox2.Text = "IDictionaryEnumerator";
			// 
			// idicEnumReset
			// 
			this.idicEnumReset.Location = new System.Drawing.Point(72, 80);
			this.idicEnumReset.Name = "idicEnumReset";
			this.idicEnumReset.Size = new System.Drawing.Size(88, 23);
			this.idicEnumReset.TabIndex = 2;
			this.idicEnumReset.Text = "Reset";
			this.idicEnumReset.Click += new System.EventHandler(this.idicEnumReset_Click);
			// 
			// idicEnumMoveNext
			// 
			this.idicEnumMoveNext.Location = new System.Drawing.Point(120, 32);
			this.idicEnumMoveNext.Name = "idicEnumMoveNext";
			this.idicEnumMoveNext.Size = new System.Drawing.Size(96, 23);
			this.idicEnumMoveNext.TabIndex = 1;
			this.idicEnumMoveNext.Text = "Move Next";
			this.idicEnumMoveNext.Click += new System.EventHandler(this.idicEnumMoveNext_Click);
			// 
			// idicEnumButton
			// 
			this.idicEnumButton.Location = new System.Drawing.Point(16, 32);
			this.idicEnumButton.Name = "idicEnumButton";
			this.idicEnumButton.Size = new System.Drawing.Size(96, 23);
			this.idicEnumButton.TabIndex = 0;
			this.idicEnumButton.Text = "Get Enumerator";
			this.idicEnumButton.Click += new System.EventHandler(this.idicEnumButton_Click);
			// 
			// itemRButton
			// 
			this.itemRButton.Location = new System.Drawing.Point(272, 120);
			this.itemRButton.Name = "itemRButton";
			this.itemRButton.Size = new System.Drawing.Size(48, 24);
			this.itemRButton.TabIndex = 5;
			this.itemRButton.Text = "Item";
			// 
			// executeButton
			// 
			this.executeButton.Location = new System.Drawing.Point(264, 152);
			this.executeButton.Name = "executeButton";
			this.executeButton.TabIndex = 6;
			this.executeButton.Text = "Execute";
			this.executeButton.Click += new System.EventHandler(this.executeButton_Click);
			// 
			// removeRButton
			// 
			this.removeRButton.Location = new System.Drawing.Point(272, 88);
			this.removeRButton.Name = "removeRButton";
			this.removeRButton.Size = new System.Drawing.Size(88, 24);
			this.removeRButton.TabIndex = 4;
			this.removeRButton.Text = "Remove";
			// 
			// containsRButton
			// 
			this.containsRButton.Location = new System.Drawing.Point(272, 56);
			this.containsRButton.Name = "containsRButton";
			this.containsRButton.Size = new System.Drawing.Size(88, 24);
			this.containsRButton.TabIndex = 3;
			this.containsRButton.Text = "Contains";
			// 
			// addRButton
			// 
			this.addRButton.Checked = true;
			this.addRButton.Location = new System.Drawing.Point(272, 24);
			this.addRButton.Name = "addRButton";
			this.addRButton.Size = new System.Drawing.Size(56, 24);
			this.addRButton.TabIndex = 2;
			this.addRButton.TabStop = true;
			this.addRButton.Text = "Add";
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(176, 32);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(40, 16);
			this.label2.TabIndex = 3;
			this.label2.Text = "Value";
			// 
			// valueTextBox
			// 
			this.valueTextBox.Location = new System.Drawing.Point(144, 56);
			this.valueTextBox.Name = "valueTextBox";
			this.valueTextBox.TabIndex = 1;
			this.valueTextBox.Text = "";
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(56, 32);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(32, 16);
			this.label1.TabIndex = 1;
			this.label1.Text = "Key";
			// 
			// keyTextBox
			// 
			this.keyTextBox.Location = new System.Drawing.Point(16, 56);
			this.keyTextBox.Name = "keyTextBox";
			this.keyTextBox.TabIndex = 0;
			this.keyTextBox.Text = "";
			// 
			// groupBox3
			// 
			this.groupBox3.Controls.Add(this.groupBox4);
			this.groupBox3.Controls.Add(this.copyToButton);
			this.groupBox3.Controls.Add(this.elementsListBox);
			this.groupBox3.Controls.Add(this.countLabel);
			this.groupBox3.Controls.Add(this.label3);
			this.groupBox3.Location = new System.Drawing.Point(16, 248);
			this.groupBox3.Name = "groupBox3";
			this.groupBox3.Size = new System.Drawing.Size(512, 160);
			this.groupBox3.TabIndex = 0;
			this.groupBox3.TabStop = false;
			this.groupBox3.Text = "ICollection";
			// 
			// groupBox4
			// 
			this.groupBox4.Controls.Add(this.currentTextBox);
			this.groupBox4.Controls.Add(this.icolEnumReset);
			this.groupBox4.Controls.Add(this.icolEnumMoveNext);
			this.groupBox4.Controls.Add(this.icolEnumButton);
			this.groupBox4.Location = new System.Drawing.Point(152, 24);
			this.groupBox4.Name = "groupBox4";
			this.groupBox4.Size = new System.Drawing.Size(232, 120);
			this.groupBox4.TabIndex = 17;
			this.groupBox4.TabStop = false;
			this.groupBox4.Text = "ICollectionEnumerator";
			// 
			// currentTextBox
			// 
			this.currentTextBox.Location = new System.Drawing.Point(120, 80);
			this.currentTextBox.Name = "currentTextBox";
			this.currentTextBox.ReadOnly = true;
			this.currentTextBox.Size = new System.Drawing.Size(96, 20);
			this.currentTextBox.TabIndex = 3;
			this.currentTextBox.Text = "";
			// 
			// icolEnumReset
			// 
			this.icolEnumReset.Location = new System.Drawing.Point(16, 80);
			this.icolEnumReset.Name = "icolEnumReset";
			this.icolEnumReset.Size = new System.Drawing.Size(88, 23);
			this.icolEnumReset.TabIndex = 2;
			this.icolEnumReset.Text = "Reset";
			this.icolEnumReset.Click += new System.EventHandler(this.icolEnumReset_Click);
			// 
			// icolEnumMoveNext
			// 
			this.icolEnumMoveNext.Location = new System.Drawing.Point(120, 32);
			this.icolEnumMoveNext.Name = "icolEnumMoveNext";
			this.icolEnumMoveNext.Size = new System.Drawing.Size(96, 23);
			this.icolEnumMoveNext.TabIndex = 1;
			this.icolEnumMoveNext.Text = "Move Next";
			this.icolEnumMoveNext.Click += new System.EventHandler(this.icolEnumMoveNext_Click);
			// 
			// icolEnumButton
			// 
			this.icolEnumButton.Location = new System.Drawing.Point(16, 32);
			this.icolEnumButton.Name = "icolEnumButton";
			this.icolEnumButton.Size = new System.Drawing.Size(96, 23);
			this.icolEnumButton.TabIndex = 0;
			this.icolEnumButton.Text = "Get Enumerator";
			this.icolEnumButton.Click += new System.EventHandler(this.icolEnumButton_Click);
			// 
			// copyToButton
			// 
			this.copyToButton.Location = new System.Drawing.Point(40, 112);
			this.copyToButton.Name = "copyToButton";
			this.copyToButton.TabIndex = 0;
			this.copyToButton.Text = "Copy To";
			this.copyToButton.Click += new System.EventHandler(this.copyToButton_Click);
			// 
			// elementsListBox
			// 
			this.elementsListBox.Location = new System.Drawing.Point(16, 32);
			this.elementsListBox.Name = "elementsListBox";
			this.elementsListBox.SelectionMode = System.Windows.Forms.SelectionMode.None;
			this.elementsListBox.Size = new System.Drawing.Size(120, 69);
			this.elementsListBox.TabIndex = 0;
			// 
			// countLabel
			// 
			this.countLabel.Location = new System.Drawing.Point(440, 32);
			this.countLabel.Name = "countLabel";
			this.countLabel.Size = new System.Drawing.Size(64, 16);
			this.countLabel.TabIndex = 1;
			// 
			// label3
			// 
			this.label3.Location = new System.Drawing.Point(392, 32);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(40, 16);
			this.label3.TabIndex = 0;
			this.label3.Text = "Count:";
			// 
			// Form1
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(536, 430);
			this.Controls.Add(this.groupBox3);
			this.Controls.Add(this.groupBox1);
			this.Name = "Form1";
			this.Text = "SkipList Demo";
			this.groupBox1.ResumeLayout(false);
			this.groupBox2.ResumeLayout(false);
			this.groupBox3.ResumeLayout(false);
			this.groupBox4.ResumeLayout(false);
			this.ResumeLayout(false);
		}

		#endregion

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		private static void Main()
		{
			Application.Run(new DemoForm());
		}

		/// <summary>
		/// Event handler for the Execute button.
		/// </summary>
		private void executeButton_Click(object sender, EventArgs e)
		{
			string key = keyTextBox.Text;
			string val = valueTextBox.Text;

			try
			{
				// If the add radio button is checked.
				if (addRButton.Checked)
				{
					// Add item to skip list.
					list.Add(key, val);
					// Show the number of items in the skip list.
					countLabel.Text = list.Count.ToString();
				}
					// Else if the contains radio button is checked.
				else if (containsRButton.Checked)
				{
					//
					// Check to see if it contains the key and display a 
					// message showing the results.
					//

					if (list.Contains(key))
					{
						MessageBox.Show("SkipList contains " + key);
					}
					else
					{
						MessageBox.Show("SkipList does not contain " + key);
					}
				}
					// Else if the remove radio button is checked.
				else if (removeRButton.Checked)
				{
					// Remove item from the skip list.
					list.Remove(key);
					// Show the number of items in the skip list.
					countLabel.Text = list.Count.ToString();
				}
					// Else the item radio button is checked.
				else
				{
					// Insert value into skip list.
					list[key] = val;
					// Show the number of items in the skip list.
					countLabel.Text = list.Count.ToString();
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
			}
		}

		/// <summary>
		/// Event handler for the Clear button.
		/// </summary>
		private void clearButton_Click(object sender, EventArgs e)
		{
			//
			// Clear the skip list and the form.
			//

			list.Clear();
			countLabel.Text = list.Count.ToString();
			keysListBox.Items.Clear();
			valuesListBox.Items.Clear();
			elementsListBox.Items.Clear();
			keyTextBox.Text = "";
			valueTextBox.Text = "";
			currentTextBox.Text = "";
		}

		/// <summary>
		/// Event handler for the dictionary enumerator button.
		/// </summary>
		private void idicEnumButton_Click(object sender, EventArgs e)
		{
			// Get dictionary enumerator.
			dicEnum = list.GetEnumerator();
		}

		/// <summary>
		/// Event handler for the dictionary enumerator move next button.
		/// </summary>
		private void idicEnumMoveNext_Click(object sender, EventArgs e)
		{
			// If a dictionary enumerator exists.
			if (dicEnum == null)
			{
				return;
			}
			try
			{
				// Move to next element and display it.
				if (dicEnum.MoveNext())
				{
					keyTextBox.Text = dicEnum.Entry.Key.ToString();
					valueTextBox.Text = dicEnum.Entry.Value.ToString();
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
			}
		}

		/// <summary>
		/// Event handler for the dictionary enumerator reset button.
		/// </summary>
		private void idicEnumReset_Click(object sender, EventArgs e)
		{
			// If the dictionary enumerator exists.
			if (dicEnum == null)
			{
				return;
			}
			try
			{
				// Reset the enumerator.
				dicEnum.Reset();
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
			}
		}

		/// <summary>
		/// Event handler for the Keys button.
		/// </summary>
		private void keysButton_Click(object sender, EventArgs e)
		{
			// Get the collection of keys from the skip list.
			ICollection col = list.Keys;

			// Get enumerator from collection.
			IEnumerator en = col.GetEnumerator();

			// Clear the keys list box.
			keysListBox.Items.Clear();

			// Fill keys list box with the keys from the collection.
			while (en.MoveNext())
			{
				keysListBox.Items.Add(en.Current);
			}
		}

		/// <summary>
		/// Event handler for the Values button.
		/// </summary>
		private void valuesButton_Click(object sender, EventArgs e)
		{
			// Get the collection of values from the skip list.
			ICollection col = list.Values;

			// Get enumerator from the collection.
			IEnumerator en = col.GetEnumerator();

			// Clear values list box.
			valuesListBox.Items.Clear();

			// Fill values list box with the values from the collection.
			while (en.MoveNext())
			{
				valuesListBox.Items.Add(en.Current);
			}
		}

		/// <summary>
		/// Event handler for the collection enumerator button.
		/// </summary>
		private void icolEnumButton_Click(object sender, EventArgs e)
		{
			// Get enumerator from the collection.
			ICollection col = list;
			colEnum = col.GetEnumerator();
		}

		/// <summary>
		/// Event handler for the collection move next button.
		/// </summary>
		private void icolEnumMoveNext_Click(object sender, EventArgs e)
		{
			// If collection enumerator exists.
			if (colEnum == null)
			{
				return;
			}
			try
			{
				// Move to next element and display it.
				if (colEnum.MoveNext())
				{
					currentTextBox.Text = colEnum.Current.ToString();
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
			}
		}

		/// <summary>
		/// Event handler for resetting the collection enumerator button.
		/// </summary>
		private void icolEnumReset_Click(object sender, EventArgs e)
		{
			// If a collection enumerator exists.
			if (colEnum == null)
			{
				return;
			}
			try
			{
				// Reset enumerator.
				colEnum.Reset();
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
			}
		}

		/// <summary>
		/// Event handler for the CopyTo button.
		/// </summary>
		private void copyToButton_Click(object sender, EventArgs e)
		{
			// Array for holding the objects to be copied from the collection.
			object[] elements = new object[list.Count];

			// The collection for the CopyTo operation.
			ICollection col = list;

			// Clear list box for displaying the copied elements.
			elementsListBox.Items.Clear();

			try
			{
				// Copy objects to the array.
				col.CopyTo(elements, 0);

				// Add copied elements to the list box.
				for (int i = 0; i < elements.Length; i++)
				{
					elementsListBox.Items.Add(elements[i]);
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
			}
		}
	}
}