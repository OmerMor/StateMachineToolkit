using System;
using System.IO;
using System.CodeDom.Compiler;
using Microsoft.CSharp;
using System.Xml.Serialization;
using Sanford.StateMachineToolkit;

namespace StateMachineBuilderDemo
{
    class Class1
    {
        [STAThread]
        static void Main()
        {
            try
            {
                StateMachineBuilder builder = new StateMachineBuilder();

                builder.NamespaceName = "StateMachineDemo";
                builder.StateMachineName = "TrafficLightBase";
                builder.InitialState = "Off";

                builder.States.Add("Disposed");

                int index = builder.States.Add("Off");
                builder.States[index].Transitions.Add("TurnOn", null, "On");
                builder.States[index].Transitions.Add("Dispose", null, "Disposed");

                index = builder.States.Add("On", "Red", HistoryType.Shallow);
                builder.States[index].Transitions.Add("TurnOff", null, "Off");
                builder.States[index].Transitions.Add("Dispose", null, "Disposed");                

                StateRowCollection substates = builder.States[index].Substates;

                index = substates.Add("Red");
                substates[index].Transitions.Add("TimerElapsed", null, "Green");

                index = substates.Add("Yellow");
                substates[index].Transitions.Add("TimerElapsed", null, "Red"); 

                index = substates.Add("Green");
                substates[index].Transitions.Add("TimerElapsed", null, "Yellow");
                
                builder.Build();
                
                StringWriter writer = new StringWriter();

                CodeDomProvider provider = new CSharpCodeProvider();
                CodeGeneratorOptions options = new CodeGeneratorOptions();

                options.BracingStyle = "C";

                provider.GenerateCodeFromNamespace(builder.Result, writer, options);

                XmlSerializer serializer = new XmlSerializer(typeof(StateMachineBuilder));
                serializer.Serialize(writer, builder);
                Console.WriteLine(writer.ToString());

                writer.Close();

                Console.Read();
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.Read();
            }
        }
    }
}