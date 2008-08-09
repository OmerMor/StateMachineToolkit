using System;
using System.CodeDom.Compiler;
using System.IO;
using System.Windows.Forms;
using System.Xml.Serialization;
using Microsoft.CSharp;
using Microsoft.VisualBasic;
using Sanford.StateMachineToolkit;

namespace StateMachineMaker
{
	public partial class MainForm : Form
	{

        private bool hasBeenSaved;

        private StateMachineBuilder builder = new StateMachineBuilder();

		public MainForm()
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

            activeCheckBox.Checked = builder.StateMachineType == StateMachineType.Active;
        }

        private static void InitializeRows(StateRowCollection rows)
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
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main() 
		{
			Application.Run(new MainForm());
		}

        private void buildButton_Click(object sender, EventArgs e)
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

        private void newFileMenuItem_Click(object sender, EventArgs e)
        {
            Initialize();  
            hasBeenSaved = false;
        }

        private void openFileMenuItem_Click(object sender, EventArgs e)
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

        private void saveFileMenuItem_Click(object sender, EventArgs e)
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

        private void saveAsFileMenuItem_Click(object sender, EventArgs e)
        {
            if(saveStateMachineFileDialog.ShowDialog() == DialogResult.OK)
            {
                Save();
            }        
        }

        private void saveAsCSharpFileMenuItem_Click(object sender, EventArgs e)
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

        private void saveAsVBFileMenuItem_Click(object sender, EventArgs e)
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

        private void exitFileMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void aboutHelpMenuItem_Click(object sender, EventArgs e)
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
        	builder.StateMachineType = activeCheckBox.Checked 
				? StateMachineType.Active 
				: StateMachineType.Passive;
        }
	}
}
