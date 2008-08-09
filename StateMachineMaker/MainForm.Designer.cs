namespace StateMachineMaker
{
	partial class MainForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
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
			this.components = new System.ComponentModel.Container();
			this.stateDataGrid = new System.Windows.Forms.DataGrid();
			this.menuItem3 = new System.Windows.Forms.MenuItem();
			this.saveAsVBFileMenuItem = new System.Windows.Forms.MenuItem();
			this.saveAsCSharpFileMenuItem = new System.Windows.Forms.MenuItem();
			this.menuItem6 = new System.Windows.Forms.MenuItem();
			this.exitFileMenuItem = new System.Windows.Forms.MenuItem();
			this.helpMenuItem = new System.Windows.Forms.MenuItem();
			this.aboutHelpMenuItem = new System.Windows.Forms.MenuItem();
			this.saveVBFileDialog = new System.Windows.Forms.SaveFileDialog();
			this.saveCSharpFileDialog = new System.Windows.Forms.SaveFileDialog();
			this.saveStateMachineFileDialog = new System.Windows.Forms.SaveFileDialog();
			this.openStateMachineFileDialog = new System.Windows.Forms.OpenFileDialog();
			this.saveAsFileMenuItem = new System.Windows.Forms.MenuItem();
			this.activeCheckBox = new System.Windows.Forms.CheckBox();
			this.initialStateLabel = new System.Windows.Forms.Label();
			this.buildButton = new System.Windows.Forms.Button();
			this.saveFileMenuItem = new System.Windows.Forms.MenuItem();
			this.mainMenu1 = new System.Windows.Forms.MainMenu(this.components);
			this.fileMenuItem = new System.Windows.Forms.MenuItem();
			this.newFileMenuItem = new System.Windows.Forms.MenuItem();
			this.openFileMenuItem = new System.Windows.Forms.MenuItem();
			this.stateMachineNameLabel = new System.Windows.Forms.Label();
			this.namespaceLabel = new System.Windows.Forms.Label();
			this.initialStateTextBox = new System.Windows.Forms.TextBox();
			this.stateMachineNameTextBox = new System.Windows.Forms.TextBox();
			this.namespaceTextBox = new System.Windows.Forms.TextBox();
			((System.ComponentModel.ISupportInitialize)(this.stateDataGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// stateDataGrid
			// 
			this.stateDataGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.stateDataGrid.DataMember = "";
			this.stateDataGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.stateDataGrid.Location = new System.Drawing.Point(7, 7);
			this.stateDataGrid.Name = "stateDataGrid";
			this.stateDataGrid.Size = new System.Drawing.Size(518, 271);
			this.stateDataGrid.TabIndex = 9;
			// 
			// menuItem3
			// 
			this.menuItem3.Index = 7;
			this.menuItem3.Text = "-";
			// 
			// saveAsVBFileMenuItem
			// 
			this.saveAsVBFileMenuItem.Index = 6;
			this.saveAsVBFileMenuItem.Text = "Save As &VB...";
			this.saveAsVBFileMenuItem.Click += new System.EventHandler(this.saveAsVBFileMenuItem_Click);
			// 
			// saveAsCSharpFileMenuItem
			// 
			this.saveAsCSharpFileMenuItem.Index = 5;
			this.saveAsCSharpFileMenuItem.Text = "Save As &C#...";
			this.saveAsCSharpFileMenuItem.Click += new System.EventHandler(this.saveAsCSharpFileMenuItem_Click);
			// 
			// menuItem6
			// 
			this.menuItem6.Index = 4;
			this.menuItem6.Text = "-";
			// 
			// exitFileMenuItem
			// 
			this.exitFileMenuItem.Index = 8;
			this.exitFileMenuItem.Text = "E&xit";
			this.exitFileMenuItem.Click += new System.EventHandler(this.exitFileMenuItem_Click);
			// 
			// helpMenuItem
			// 
			this.helpMenuItem.Index = 1;
			this.helpMenuItem.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.aboutHelpMenuItem});
			this.helpMenuItem.Text = "&Help";
			// 
			// aboutHelpMenuItem
			// 
			this.aboutHelpMenuItem.Index = 0;
			this.aboutHelpMenuItem.Text = "&About...";
			this.aboutHelpMenuItem.Click += new System.EventHandler(this.aboutHelpMenuItem_Click);
			// 
			// saveVBFileDialog
			// 
			this.saveVBFileDialog.DefaultExt = "vb";
			this.saveVBFileDialog.Filter = "VB files|*.vb|All files|*.*";
			this.saveVBFileDialog.Title = "Save As VB...";
			// 
			// saveCSharpFileDialog
			// 
			this.saveCSharpFileDialog.DefaultExt = "cs";
			this.saveCSharpFileDialog.Filter = "C# files|*.cs|All files|*.*";
			this.saveCSharpFileDialog.Title = "Save As CSharp...";
			// 
			// saveStateMachineFileDialog
			// 
			this.saveStateMachineFileDialog.DefaultExt = "xml";
			this.saveStateMachineFileDialog.Filter = "XML files|*.xml|All files|*.*";
			this.saveStateMachineFileDialog.Title = "Save State Machine...";
			// 
			// openStateMachineFileDialog
			// 
			this.openStateMachineFileDialog.DefaultExt = "xml";
			this.openStateMachineFileDialog.Filter = "XML files|*.xml|All files|*.*";
			this.openStateMachineFileDialog.Title = "Open State Machine...";
			// 
			// saveAsFileMenuItem
			// 
			this.saveAsFileMenuItem.Index = 3;
			this.saveAsFileMenuItem.Text = "Save &As...";
			this.saveAsFileMenuItem.Click += new System.EventHandler(this.saveAsFileMenuItem_Click);
			// 
			// activeCheckBox
			// 
			this.activeCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.activeCheckBox.AutoSize = true;
			this.activeCheckBox.Checked = true;
			this.activeCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
			this.activeCheckBox.Location = new System.Drawing.Point(579, 182);
			this.activeCheckBox.Name = "activeCheckBox";
			this.activeCheckBox.Size = new System.Drawing.Size(56, 17);
			this.activeCheckBox.TabIndex = 17;
			this.activeCheckBox.Text = "Active";
			this.activeCheckBox.UseVisualStyleBackColor = true;
			this.activeCheckBox.CheckedChanged += new System.EventHandler(this.activeCheckBox_CheckedChanged);
			// 
			// initialStateLabel
			// 
			this.initialStateLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.initialStateLabel.Location = new System.Drawing.Point(538, 119);
			this.initialStateLabel.Name = "initialStateLabel";
			this.initialStateLabel.Size = new System.Drawing.Size(136, 23);
			this.initialStateLabel.TabIndex = 15;
			this.initialStateLabel.Text = "Initial State";
			this.initialStateLabel.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
			// 
			// buildButton
			// 
			this.buildButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.buildButton.Location = new System.Drawing.Point(570, 223);
			this.buildButton.Name = "buildButton";
			this.buildButton.Size = new System.Drawing.Size(75, 23);
			this.buildButton.TabIndex = 16;
			this.buildButton.Text = "Build";
			this.buildButton.Click += new System.EventHandler(this.buildButton_Click);
			// 
			// saveFileMenuItem
			// 
			this.saveFileMenuItem.Index = 2;
			this.saveFileMenuItem.Text = "&Save";
			this.saveFileMenuItem.Click += new System.EventHandler(this.saveFileMenuItem_Click);
			// 
			// mainMenu1
			// 
			this.mainMenu1.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.fileMenuItem,
            this.helpMenuItem});
			// 
			// fileMenuItem
			// 
			this.fileMenuItem.Index = 0;
			this.fileMenuItem.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.newFileMenuItem,
            this.openFileMenuItem,
            this.saveFileMenuItem,
            this.saveAsFileMenuItem,
            this.menuItem6,
            this.saveAsCSharpFileMenuItem,
            this.saveAsVBFileMenuItem,
            this.menuItem3,
            this.exitFileMenuItem});
			this.fileMenuItem.Text = "&File";
			// 
			// newFileMenuItem
			// 
			this.newFileMenuItem.Index = 0;
			this.newFileMenuItem.Text = "&New";
			this.newFileMenuItem.Click += new System.EventHandler(this.newFileMenuItem_Click);
			// 
			// openFileMenuItem
			// 
			this.openFileMenuItem.Index = 1;
			this.openFileMenuItem.Text = "&Open...";
			this.openFileMenuItem.Click += new System.EventHandler(this.openFileMenuItem_Click);
			// 
			// stateMachineNameLabel
			// 
			this.stateMachineNameLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.stateMachineNameLabel.Location = new System.Drawing.Point(538, 70);
			this.stateMachineNameLabel.Name = "stateMachineNameLabel";
			this.stateMachineNameLabel.Size = new System.Drawing.Size(136, 23);
			this.stateMachineNameLabel.TabIndex = 14;
			this.stateMachineNameLabel.Text = "State Machine Name";
			this.stateMachineNameLabel.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
			// 
			// namespaceLabel
			// 
			this.namespaceLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.namespaceLabel.Location = new System.Drawing.Point(538, 15);
			this.namespaceLabel.Name = "namespaceLabel";
			this.namespaceLabel.Size = new System.Drawing.Size(136, 23);
			this.namespaceLabel.TabIndex = 13;
			this.namespaceLabel.Text = "Namespace";
			this.namespaceLabel.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
			// 
			// initialStateTextBox
			// 
			this.initialStateTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.initialStateTextBox.Location = new System.Drawing.Point(538, 145);
			this.initialStateTextBox.Name = "initialStateTextBox";
			this.initialStateTextBox.Size = new System.Drawing.Size(136, 20);
			this.initialStateTextBox.TabIndex = 12;
			// 
			// stateMachineNameTextBox
			// 
			this.stateMachineNameTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.stateMachineNameTextBox.Location = new System.Drawing.Point(538, 96);
			this.stateMachineNameTextBox.Name = "stateMachineNameTextBox";
			this.stateMachineNameTextBox.Size = new System.Drawing.Size(136, 20);
			this.stateMachineNameTextBox.TabIndex = 11;
			// 
			// namespaceTextBox
			// 
			this.namespaceTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.namespaceTextBox.Location = new System.Drawing.Point(538, 41);
			this.namespaceTextBox.Name = "namespaceTextBox";
			this.namespaceTextBox.Size = new System.Drawing.Size(136, 20);
			this.namespaceTextBox.TabIndex = 10;
			// 
			// MainForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
			this.ClientSize = new System.Drawing.Size(686, 285);
			this.Controls.Add(this.stateDataGrid);
			this.Controls.Add(this.activeCheckBox);
			this.Controls.Add(this.initialStateLabel);
			this.Controls.Add(this.buildButton);
			this.Controls.Add(this.stateMachineNameLabel);
			this.Controls.Add(this.namespaceLabel);
			this.Controls.Add(this.initialStateTextBox);
			this.Controls.Add(this.stateMachineNameTextBox);
			this.Controls.Add(this.namespaceTextBox);
			this.Menu = this.mainMenu1;
			this.Name = "MainForm";
			this.Text = "State Machine Maker";
			((System.ComponentModel.ISupportInitialize)(this.stateDataGrid)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.DataGrid stateDataGrid;
		private System.Windows.Forms.MenuItem menuItem3;
		private System.Windows.Forms.MenuItem saveAsVBFileMenuItem;
		private System.Windows.Forms.MenuItem saveAsCSharpFileMenuItem;
		private System.Windows.Forms.MenuItem menuItem6;
		private System.Windows.Forms.MenuItem exitFileMenuItem;
		private System.Windows.Forms.MenuItem helpMenuItem;
		private System.Windows.Forms.MenuItem aboutHelpMenuItem;
		private System.Windows.Forms.SaveFileDialog saveVBFileDialog;
		private System.Windows.Forms.SaveFileDialog saveCSharpFileDialog;
		private System.Windows.Forms.SaveFileDialog saveStateMachineFileDialog;
		private System.Windows.Forms.OpenFileDialog openStateMachineFileDialog;
		private System.Windows.Forms.MenuItem saveAsFileMenuItem;
		private System.Windows.Forms.CheckBox activeCheckBox;
		private System.Windows.Forms.Label initialStateLabel;
		private System.Windows.Forms.Button buildButton;
		private System.Windows.Forms.MenuItem saveFileMenuItem;
		private System.Windows.Forms.MainMenu mainMenu1;
		private System.Windows.Forms.MenuItem fileMenuItem;
		private System.Windows.Forms.MenuItem newFileMenuItem;
		private System.Windows.Forms.MenuItem openFileMenuItem;
		private System.Windows.Forms.Label stateMachineNameLabel;
		private System.Windows.Forms.Label namespaceLabel;
		private System.Windows.Forms.TextBox initialStateTextBox;
		private System.Windows.Forms.TextBox stateMachineNameTextBox;
		private System.Windows.Forms.TextBox namespaceTextBox;

	}
}