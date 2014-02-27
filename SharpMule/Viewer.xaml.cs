using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Collections; 
using SharpMule.Automation.Framework.Essentials;
using TestManager.Shared; 

namespace TestManager
{
    /// <summary>
    /// Interaction logic for Viewer.xaml
    /// </summary>
    public partial class Viewer : Window
    {
        int CurrentTestStepIndex { get; set; }
        TestInterfaceEngine CurrentTestObject { get; set; }
        public Viewer()
        {
            InitializeComponent();
            EnableFindTextControl();
            ViewerContextMenu();
        }

        public void SetText(TestInterfaceEngine ts,int index)
        {
            try
            {
                txtViewer.Clear(); 
                CurrentTestStepIndex = index;
                CurrentTestObject = ts; 
                List<Command> cmdList = new List<Command>();

                // Remove Ignored
                foreach (Command cmd in  ts.CommandLib.Commands.ToList<Command>())
                {
                    if (!cmd.Ignore)
                        cmdList.Add(cmd); 
                }


                Command key = cmdList[index];
              

                if (key.Proc.Contains("httprequest"))
                    EnableEventLogs(true);
                else
                    EnableEventLogs(false);


                string errorString = String.Empty;

                //if (String.Compare(ts.CommandLib.Log.LogStorage[key].Info, String.Empty) != 0)
                //    errorString +=  ts.CommandLib.Log.LogStorage[key].Info+Environment.NewLine;
                //if (String.Compare(ts.CommandLib.Log.LogStorage[key].Error, String.Empty) != 0)
                //    errorString += ts.CommandLib.Log.LogStorage[key].Error + Environment.NewLine;
                //if (String.Compare(ts.CommandLib.Log.LogStorage[key].Warning, String.Empty) != 0)
                //    errorString += ts.CommandLib.Log.LogStorage[key].Warning + Environment.NewLine;
                if (String.Compare(ts.CommandLib.Log.LogStorage[key].Debug, String.Empty) != 0)
                    errorString += ts.CommandLib.Log.LogStorage[key].Debug + Environment.NewLine;

                this.txtViewer.Text = errorString; 
            }
            catch
            {
                this.txtViewer.Text = "Nothing to show...";
            }

        }

        private void ViewerContextMenu()
        {
            MenuItem miCopy = new MenuItem();
            MenuItem miPate = new MenuItem();
            MenuItem miCut = new MenuItem();


            miCopy.Header = "Copy";
            miPate.Header = "Paste";
            miCut.Header = "Cut";

            miCopy.Click +=new RoutedEventHandler(miCopy_Click);
            miPate.Click += new RoutedEventHandler(miPate_Click);
            miCut.Click += new RoutedEventHandler(miCut_Click);


            cmResultsViewer.Items.Add(miCopy);
            cmResultsViewer.Items.Add(miPate);
            cmResultsViewer.Items.Add(miCut);



        }

        public void SetText(string text)
        {
            txtViewer.Clear(); 
            txtViewer.Text = text;
              
        }

        private void EnableEventLogs(bool enable)
        {
            btnGetEvents.IsEnabled = enable;
            btnSendRequest.IsEnabled = enable; 
            
        }
        public void EnableFindTextControl()
        {

            FindReplace.FindReplaceMgr FRM = new FindReplace.FindReplaceMgr();
            FRM.CurrentEditor = new FindReplace.TextEditorAdapter(txtViewer);
            FRM.ShowSearchIn = false;
            FRM.OwnerWindow = this;

            CommandBindings.Add(FRM.FindBinding);
            CommandBindings.Add(FRM.ReplaceBinding);
            CommandBindings.Add(FRM.FindNextBinding);
        }

        void miCut_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Clipboard.SetText(txtViewer.SelectedText);
                txtViewer.SelectedText = String.Empty;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message); 
            }
        }

        void miPate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string text = Clipboard.GetText();
                txtViewer.SelectedText = text;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        void miCopy_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Clipboard.SetText(txtViewer.SelectedText);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }

        private void btnGetEvents_Click(object sender, RoutedEventArgs e)
        {
            string envName = SharedTasks.Environments.DefaultEnvironment; 
            string endPoint = SharedTasks.Environments.GetEnvironmentByName(envName).DefaultEndpoint.Split(':')[0]; 

            Utilities.EventLogs eventLog = new Utilities.EventLogs(endPoint);

            string log = eventLog.GetEventLogs(10);

   

            if (!log.Equals(string.Empty))
                txtViewer.Text = log ; 
            else
                txtViewer.Text = "Unable to find any errors logged"; 


        }
        private void btnSendRequest_Click(object sender, RoutedEventArgs e)
        {
            string text=String.Empty; 
            Command cmd = CurrentTestObject.CommandLib.Commands.ToList<Command>()[CurrentTestStepIndex];
            CurrentTestObject.CommandLib.DoCommand(cmd, CurrentTestObject.CommandLib.GetParameters(cmd.Param)); 
            text = CurrentTestObject.CommandLib.Log.LogStorage[cmd].Debug; 
            SetText(text); 
             
        }
        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            txtViewer.Margin = this.Margin;
        }
        
        


    }
}
