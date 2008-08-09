using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using System.Xml.Serialization;
using System.IO;
using System.CodeDom.Compiler;
using Microsoft.CSharp;
using Microsoft.VisualBasic;
using Sanford.StateMachineToolkit;

namespace StateMachineMaker
{
	/// <summary>
	/// Summary description for Form1.
	/// </summary>
	public class Form1 : System.Windows.Forms.Form
    {
        private IContainer components;
        private System.Windows.Forms.DataGrid stateDataGrid;
        private System.Windows.Forms.TextBox namespaceTextBox;
        private System.Windows.Forms.TextBox stateMachineNameTextBox;
        private System.Windows.Forms.Label namespaceLabel;
        private System.Windows.Forms.Label stateMachineNameLabel;
        private System.Windows.Forms.Label initialStateLabel;
        private System.Windows.Forms.Button buildButton;
        private System.Windows.Forms.TextBox initialStateTextBox;
        private System.Windows.Forms.MainMenu mainMenu1;
        private System.Windows.Forms.MenuItem menuItem6;
        private System.Windows.Forms.MenuItem fileMenuItem;
        private System.Windows.Forms.MenuItem newFileMenuItem;
        private System.Windows.Forms.MenuItem openFileMenuItem;
        private System.Windows.Forms.MenuItem saveFileMenuItem;
        private System.Windows.Forms.MenuItem saveAsFileMenuItem;
        private System.Windows.Forms.MenuItem exitFileMenuItem;
        private System.Windows.Forms.MenuItem helpMenuItem;
        private System.Windows.Forms.MenuItem aboutHelpMenuItem;
        private System.Windows.Forms.OpenFileDialog openStateMachineFileDialog;
        private System.Windows.Forms.SaveFileDialog saveStateMachineFileDialog;

        private bool hasBeenSaved = false;
        private System.Windows.Forms.MenuItem saveAsCSharpFileMenuItem;
        private System.Windows.Forms.MenuItem saveAsVBFileMenuItem;
        private System.Windows.Forms.MenuItem menuItem3;
        private System.Windows.Forms.SaveFileDialog saveCSharpFileDialog;
        private System.Windows.Forms.SaveFileDialog saveVBFileDialog;
        private CheckBox activeCheckBox;

        private StateMachineBuilder builder = new StateMachineBuilder();

		public Form1()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent(); 

            InitializeDataGrid();
		}

        private void InitializeDataGrid()
        {
            stateDataGrid.DataSource = builder.States;
            InitializeStateColumnStyles();
            InitializeTransitionColumnStyles();
            Initialize();
        }

        private void InitializeStateColumnStyles()
        {
            DataGridTableStyle tableStyle = new DataGridTableStyle();

            tableStyle.MappingName = "StateRowCollection";

            DataGridColumnStyle columnStyleName = new DataGridTextBoxColumn();

            columnStyleName.HeaderText = "State Name";
            columnStyleName.MappingName = "Name";
            columnStyleName.NullText = "(null)";
            tableStyle.GridColumnStyles.Add(columnStyleName);

            DataGridColumnStyle columnStyleHistoryType = new DataGridTextBoxColumn();

            columnStyleHistoryType.HeaderText = "History Type";
            columnStyleHistoryType.MappingName = "HistoryType";
            tableStyle.GridColumnStyles.Add(columnStyleHistoryType);

            DataGridColumnStyle columnStyleInitialState = new DataGridTextBoxColumn();

            columnStyleInitialState.HeaderText = "Initial State";
            columnStyleInitialState.MappingName = "InitialState";
            tableStyle.GridColumnStyles.Add(columnStyleInitialState);

            stateDataGrid.TableStyles.Add(tableStyle);
        }

        private void InitializeTransitionColumnStyles()
        {
            DataGridTableStyle tableStyle = new DataGridTableStyle();

            tableStyle.MappingName = "TransitionRowCollection";

            DataGridColumnStyle columnStyleEvent = new DataGridTextBoxColumn();

            columnStyleEvent.HeaderText = "Event";
            columnStyleEvent.MappingName = "Event";
            tableStyle.GridColumnStyles.Add(columnStyleEvent);

            DataGridColumnStyle columnStyleGuard = new DataGridTextBoxColumn();

            columnStyleGuard.HeaderText = "Guard";
            columnStyleGuard.MappingName = "Guard";
            tableStyle.GridColumnStyles.Add(columnStyleGuard);

            DataGridColumnStyle columnStyleTarget = new DataGridTextBoxColumn();

            columnStyleTarget.HeaderText = "Target";
            columnStyleTarget.MappingName = "Target";
            tableStyle.GridColumnStyles.Add(columnStyleTarget);

            stateDataGrid.TableStyles.Add(tableStyle);
        }

        private void InitializeControls()
        {
            namespaceTextBox.Text = builder.NamespaceName;
            stateMachineNameTextBox.Text = builder.StateMachineName;
            initialStateTextBox.Text = builder.InitialState;

            if(builder.StateMachineType == StateMachineType.Active)
            {
                activeCheckBox.Checked = true;
            }
            else
            {
                activeCheckBox.Checked = false;
            }
        }

        private void InitializeRows(StateRowCollection rows)
        {
            foreach(StateRow row in rows)
            {
                row.BeginEdit();
                row.EndEdit();

                foreach(TransitionRow tRow in row.Transitions)
                {
                    tRow.BeginEdit();
                    tRow.EndEdit();
                }

                InitializeRows(row.Substates);
            }
        }

        private void Initialize()
        {            
            builder.Clear();
            InitializeControls();
            openStateMachineFileDialog.FileName = builder.StateMachineName + ".xml";
            saveStateMachineFileDialog.FileName = builder.StateMachineName + ".xml";
            saveCSharpFileDialog.FileName = builder.StateMachineName + ".cs";
            saveVBFileDialog.FileName = builder.StateMachineName + ".vb";
        }

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if (components != null) 
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
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
            this.namespaceTextBox = new System.Windows.Forms.TextBox();
            this.stateMachineNameTextBox = new System.Windows.Forms.TextBox();
            this.initialStateTextBox = new System.Windows.Forms.TextBox();
            this.namespaceLabel = new System.Windows.Forms.Label();
            this.stateMachineNameLabel = new System.Windows.Forms.Label();
            this.initialStateLabel = new System.Windows.Forms.Label();
            this.buildButton = new System.Windows.Forms.Button();
            this.mainMenu1 = new System.Windows.Forms.MainMenu(this.components);
            this.fileMenuItem = new System.Windows.Forms.MenuItem();
            this.newFileMenuItem = new System.Windows.Forms.MenuItem();
            this.openFileMenuItem = new System.Windows.Forms.MenuItem();
            this.saveFileMenuItem = new System.Windows.Forms.MenuItem();
            this.saveAsFileMenuItem = new System.Windows.Forms.MenuItem();
            this.menuItem6 = new System.Windows.Forms.MenuItem();
            this.saveAsCSharpFileMenuItem = new System.Windows.Forms.MenuItem();
            this.saveAsVBFileMenuItem = new System.Windows.Forms.MenuItem();
            this.menuItem3 = new System.Windows.Forms.MenuItem();
            this.exitFileMenuItem = new System.Windows.Forms.MenuItem();
            this.helpMenuItem = new System.Windows.Forms.MenuItem();
            this.aboutHelpMenuItem = new System.Windows.Forms.MenuItem();
            this.openStateMachineFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.saveStateMachineFileDialog = new System.Windows.Forms.SaveFileDialog();
            this.saveCSharpFileDialog = new System.Windows.Forms.SaveFileDialog();
            this.saveVBFileDialog = new System.Windows.Forms.SaveFileDialog();
            this.activeCheckBox = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.stateDataGrid)).BeginInit();
            this.SuspendLayout();
            // 
            // stateDataGrid
            // 
            this.stateDataGrid.DataMember = "";
            this.stateDataGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
            this.stateDataGrid.Location = new System.Drawing.Point(8, 8);
            this.stateDataGrid.Name = "stateDataGrid";
            this.stateDataGrid.Size = new System.Drawing.Size(504, 264);
            this.stateDataGrid.TabIndex = 0;
            // 
            // namespaceTextBox
            // 
            this.namespaceTextBox.Location = new System.Drawing.Point(528, 42);
            this.namespaceTextBox.Name = "namespaceTextBox";
            this.namespaceTextBox.Size = new System.Drawing.Size(136, 20);
            this.namespaceTextBox.TabIndex = 1;
            // 
            // stateMachineNameTextBox
            // 
            this.stateMachineNameTextBox.Location = new System.Drawing.Point(528, 97);
            this.stateMachineNameTextBox.Name = "stateMachineNameTextBox";
            this.stateMachineNameTextBox.Size = new System.Drawing.Size(136, 20);
            this.stateMachineNameTextBox.TabIndex = 2;
            // 
            // initialStateTextBox
            // 
            this.initialStateTextBox.Location = new System.Drawing.Point(528, 146);
            this.initialStateTextBox.Name = "initialStateTextBox";
            this.initialStateTextBox.Size = new System.Drawing.Size(136, 20);
            this.initialStateTextBox.TabIndex = 3;
            // 
            // namespaceLabel
            // 
            this.namespaceLabel.Location = new System.Drawing.Point(528, 16);
            this.namespaceLabel.Name = "namespaceLabel";
            this.namespaceLabel.Size = new System.Drawing.Size(136, 23);
            this.namespaceLabel.TabIndex = 4;
            this.namespaceLabel.Text = "Namespace";
            this.namespaceLabel.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            // 
            // stateMachineNameLabel
            // 
            this.stateMachineNameLabel.Location = new System.Drawing.Point(528, 71);
            this.stateMachineNameLabel.Name = "stateMachineNameLabel";
            this.stateMachineNameLabel.Size = new System.Drawing.Size(136, 23);
            this.stateMachineNameLabel.TabIndex = 5;
            this.stateMachineNameLabel.Text = "State Machine Name";
            this.stateMachineNameLabel.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            // 
            // initialStateLabel
            // 
            this.initialStateLabel.Location = new System.Drawing.Point(528, 120);
            this.initialStateLabel.Name = "initialStateLabel";
            this.initialStateLabel.Size = new System.Drawing.Size(136, 23);
            this.initialStateLabel.TabIndex = 6;
            this.initialStateLabel.Text = "Initial State";
            this.initialStateLabel.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            // 
            // buildButton
            // 
            this.buildButton.Location = new System.Drawing.Point(560, 224);
            this.buildButton.Name = "buildButton";
            this.buildButton.Size = new System.Drawing.Size(75, 23);
            this.buildButton.TabIndex = 7;
            this.buildButton.Text = "Build";
            this.buildButton.Click += new System.EventHandler(this.buildButton_Click);
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
            // saveFileMenuItem
            // 
            this.saveFileMenuItem.Index = 2;
            this.saveFileMenuItem.Text = "&Save";
            this.saveFileMenuItem.Click += new System.EventHandler(this.saveFileMenuItem_Click);
            // 
            // saveAsFileMenuItem
            // 
            this.saveAsFileMenuItem.Index = 3;
            this.saveAsFileMenuItem.Text = "Save &As...";
            this.saveAsFileMenuItem.Click += new System.EventHandler(this.saveAsFileMenuItem_Click);
            // 
            // menuItem6
            // 
            this.menuItem6.Index = 4;
            this.menuItem6.Text = "-";
            // 
            // saveAsCSharpFileMenuItem
            // 
            this.saveAsCSharpFileMenuItem.Index = 5;
            this.saveAsCSharpFileMenuItem.Text = "Save As &C#...";
            this.saveAsCSharpFileMenuItem.Click += new System.EventHandler(this.saveAsCSharpFileMenuItem_Click);
            // 
            // saveAsVBFileMenuItem
            // 
            this.saveAsVBFileMenuItem.Index = 6;
            this.saveAsVBFileMenuItem.Text = "Save As &VB...";
            this.saveAsVBFileMenuItem.Click += new System.EventHandler(this.saveAsVBFileMenuItem_Click);
            // 
            // menuItem3
            // 
            this.menuItem3.Index = 7;
            this.menuItem3.Text = "-";
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
            // openStateMachineFileDialog
            // 
            this.openStateMachineFileDialog.DefaultExt = "xml";
            this.openStateMachineFileDialog.Filter = "XML files|*.xml|All files|*.*";
            this.openStateMachineFileDialog.Title = "Open State Machine...";
            // 
            // saveStateMachineFileDialog
            // 
            this.saveStateMachineFileDialog.DefaultExt = "xml";
            this.saveStateMachineFileDialog.Filter = "XML files|*.xml|All files|*.*";
            this.saveStateMachineFileDialog.Title = "Save State Machine...";
            // 
            // saveCSharpFileDialog
            // 
            this.saveCSharpFileDialog.DefaultExt = "cs";
            this.saveCSharpFileDialog.Filter = "C# files|*.cs|All files|*.*";
            this.saveCSharpFileDialog.Title = "Save As CSharp...";
            // 
            // saveVBFileDialog
            // 
            this.saveVBFileDialog.DefaultExt = "vb";
            this.saveVBFileDialog.Filter = "VB files|*.vb|All files|*.*";
            this.saveVBFileDialog.Title = "Save As VB...";
            // 
            // activeCheckBox
            // 
            this.activeCheckBox.AutoSize = true;
            this.activeCheckBox.Checked = true;
            this.activeCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.activeCheckBox.Location = new System.Drawing.Point(569, 183);
            this.activeCheckBox.Name = "activeCheckBox";
            this.activeCheckBox.Size = new System.Drawing.Size(56, 17);
            this.activeCheckBox.TabIndex = 8;
            this.activeCheckBox.Text = "Active";
            this.activeCheckBox.UseVisualStyleBackColor = true;
            this.activeCheckBox.CheckedChanged += new System.EventHandler(this.activeCheckBox_CheckedChanged);
            // 
            // Form1
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.ClientSize = new System.Drawing.Size(680, 278);
            this.Controls.Add(this.activeCheckBox);
            this.Controls.Add(this.buildButton);
            this.Controls.Add(this.initialStateLabel);
            this.Controls.Add(this.stateMachineNameLabel);
            this.Controls.Add(this.namespaceLabel);
            this.Controls.Add(this.initialStateTextBox);
            this.Controls.Add(this.stateMachineNameTextBox);
            this.Controls.Add(this.namespaceTextBox);
            this.Controls.Add(this.stateDataGrid);
            this.Menu = this.mainMenu1;
            this.Name = "Form1";
            this.Text = "State Machine Maker";
            ((System.ComponentModel.ISupportInitialize)(this.stateDataGrid)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }
		#endregion

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main() 
		{
			Application.Run(new Form1());
		}

        private void buildButton_Click(object sender, System.EventArgs e)
        {
            try
            {
                builder.NamespaceName = namespaceTextBox.Text;
                builder.StateMachineName = stateMachineNameTextBox.Text;
                builder.InitialState = initialStateTextBox.Text;
                builder.Build();

                MessageBox.Show("Build successful.", "Build Results", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "Build Failed", 
                    MessageBoxButtons.OK, MessageBoxIcon.Stop);                
            }
        }

        private void newFileMenuItem_Click(object sender, System.EventArgs e)
        {
            Initialize();  
            hasBeenSaved = false;
        }

        private void openFileMenuItem_Click(object sender, System.EventArgs e)
        {
            if(openStateMachineFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    string fileName = openStateMachineFileDialog.FileName;                    
                    XmlSerializer serializer = new XmlSerializer(typeof(StateMachineBuilder));

                    using(FileStream stream = new FileStream(fileName, FileMode.Open, 
                              FileAccess.Read, FileShare.Read))
                    {
                        builder = (StateMachineBuilder)serializer.Deserialize(stream);
                        stream.Close();
                        stateDataGrid.DataSource = builder.States;

                        InitializeRows(builder.States);                        

                        InitializeControls();
                        fileName = Path.GetFileNameWithoutExtension(fileName);
                        saveStateMachineFileDialog.FileName = fileName + ".xml";
                        saveCSharpFileDialog.FileName = fileName + ".cs";
                        saveVBFileDialog.FileName = fileName + ".vb";
                        hasBeenSaved = true;
                    }
                }
                catch(Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error!", 
                        MessageBoxButtons.OK, MessageBoxIcon.Stop);
                }
            }
        }

        private void saveFileMenuItem_Click(object sender, System.EventArgs e)
        {
            if(!hasBeenSaved)
            {
                saveAsFileMenuItem_Click(sender, e);
            }
            else
            {
                Save();
            }
        }

        private void saveAsFileMenuItem_Click(object sender, System.EventArgs e)
        {
            if(saveStateMachineFileDialog.ShowDialog() == DialogResult.OK)
            {
                Save();
            }        
        }

        private void saveAsCSharpFileMenuItem_Click(object sender, System.EventArgs e)
        {
            if(saveCSharpFileDialog.ShowDialog() == DialogResult.OK)
            {
                CodeDomProvider provider = new CSharpCodeProvider();
                CodeGeneratorOptions options = new CodeGeneratorOptions();                

                options.BracingStyle = "C";

                string fileName = saveCSharpFileDialog.FileName;

                using(StreamWriter writer = new StreamWriter(fileName))
                {
                    provider.GenerateCodeFromNamespace(builder.Result, 
                        writer, options);

                    writer.Close();
                }
            }        
        }

        private void saveAsVBFileMenuItem_Click(object sender, System.EventArgs e)
        {
            if(saveVBFileDialog.ShowDialog() == DialogResult.OK)
            {
                CodeDomProvider provider = new VBCodeProvider();
                CodeGeneratorOptions options = new CodeGeneratorOptions();

                string fileName = saveVBFileDialog.FileName;

                using(StreamWriter writer = new StreamWriter(fileName))
                {
                    provider.GenerateCodeFromNamespace(builder.Result, 
                        writer, options);

                    writer.Close();
                }
            }              
        }

        private void exitFileMenuItem_Click(object sender, System.EventArgs e)
        {
            Close();
        }

        private void aboutHelpMenuItem_Click(object sender, System.EventArgs e)
        {
            AboutDialog dlg = new AboutDialog();

            dlg.ShowDialog();
        }

        private void Save()
        {
            try
            {
                string fileName = saveStateMachineFileDialog.FileName;
                XmlSerializer serializer = new XmlSerializer(typeof(StateMachineBuilder));

                using(FileStream stream = new FileStream(fileName, FileMode.Create, 
                          FileAccess.Write, FileShare.None))
                {
                    builder.NamespaceName = namespaceTextBox.Text;
                    builder.StateMachineName = stateMachineNameTextBox.Text;
                    serializer.Serialize(stream, builder);
                    stream.Close();
                    hasBeenSaved = true;
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "Error!", 
                    MessageBoxButtons.OK, MessageBoxIcon.Stop);
            }        
        }

        private void activeCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if(activeCheckBox.Checked)
            {
                builder.StateMachineType = StateMachineType.Active;
            }
            else
            {
                builder.StateMachineType = StateMachineType.Passive;
            }
        }        
	}
}
