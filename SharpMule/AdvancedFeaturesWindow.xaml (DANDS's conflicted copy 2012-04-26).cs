using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions; 
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using TestManager.DataGridCtrl;
using TestManager.Shared;
using TestManager.TreeViewCtrl;
using System.Collections; 
using ETG.Trip.Framework.Essentials;
using System.ComponentModel;
using ICSharpCode.AvalonEdit; 

namespace TestManager
{
    /// <summary>
    /// Interaction logic for AdvancedFeaturesWindow.xaml
    /// </summary>
    /// 

    public class TestContainer
    {
        List<TaskLibrary> TestList { get; set; }
        int ID { get; set; }

        public TestContainer(List<TaskLibrary> testList,int InstanceID)
        {
            TestList = testList;
            ID = InstanceID; 
        }

    }
    public partial class AdvancedFeaturesWindow : Window
    {
        public ArrayList TestList { get; set; }
        public List<TaskLibrary> TestCaseObjectList { get; set; }

        public delegate void AddSuiteCompletedHandler(object sender, EventArgs e, TaskLibrary tl,int ID);
        public event AddSuiteCompletedHandler TestSuiteCompleted;

        int activeTestCount;
        string[] endpoints;
        Dictionary<int,List<TaskLibrary>> completedSuites;
        List<TaskLibrary> completedTests; 
        List<TestContainer> testContainer;
        List<RichTextBox> logInstances; 

        public AdvancedFeaturesWindow()
        {
            InitializeComponent();
            TestList = new ArrayList();
            TestCaseObjectList = new List<TaskLibrary>();
            testContainer = new List<TestContainer>();
            logInstances = new List<RichTextBox>(); 
           
        }

        private void cbSelectAll_Checked(object sender, RoutedEventArgs e)
        {
                foreach (CheckBox item in lbTestList.Items)
                {
                    item.IsChecked = true;
                }
        }

       

        private void cbSelectAll_Unchecked(object sender, RoutedEventArgs e)
        {
            foreach (CheckBox item in lbTestList.Items)
            {
                item.IsChecked = false;    
            }
        }

        private void btnExecute_Click(object sender, RoutedEventArgs e)
        {

            activeTestCount = 0;
            
            endpoints = txtEndPointsList.Text.Split(','); 
            
            tabctrlLogs.Items.Clear();
            logInstances.Clear(); 

            


            for (int i = 0; i < endpoints.Length;i++ )
            {
                TestSuiteCompleted -= AdvancedFeaturesWindow_TestSuiteCompleted;
                List<TaskLibrary> testList = new List<TaskLibrary>();
                
                for (int j = 0; j < lbTestList.Items.Count; j++)
                {
                    CheckBox item = lbTestList.Items[j] as CheckBox;
                    if (item.IsChecked == true)
                    {
                        TaskLibrary test = new TaskLibrary();
                        test.InstanceID = i; 
                        test.ScriptSetup(TestCaseObjectList[j].TestCasePath, TestCaseObjectList[j].TestCaseFileName);
                        testList.Add(test);
                        activeTestCount++;
                    }

                }

                
                ExecuteTests(testList, endpoints[i], i); 

            }


            btnExecute.IsEnabled = true;
            
        }

      

        private void AddNewLogTab(string tabHeader,out RichTextBox logViewer)
        {
            logViewer = new RichTextBox();
            logViewer.Width = rtbLog.Width; 
            logViewer.Height = rtbLog.Height;
            
           
            
            TabItem log = new TabItem();
            log.Header = tabHeader;
            log.Content = logViewer;

            tabctrlLogs.Items.Add(log);
            
             
        }
        private void ExecuteTests(List<TaskLibrary> testList,string endpoint,int instanceID)
        {

            completedTests = new List<TaskLibrary>();
            completedSuites = new Dictionary<int, List<TaskLibrary>>();
            pbTests.Value = 0;
            rtbLog.Document.Blocks.Clear(); 
            btnExecute.IsEnabled = false; 
            pbTests.Maximum = testList.Count;

            string key = "$TRIP";
            string value = endpoint; 

            TestSuiteCompleted += new AddSuiteCompletedHandler(AdvancedFeaturesWindow_TestSuiteCompleted);

            for (int i = 0; i < testList.Count;i++ )
            {
                TaskLibrary test = testList[i];

                

                if (test.CommandLib.Variables.ContainsKey(key))
                    test.CommandLib.Variables[key] = value;
                else
                    test.CommandLib.Variables.Add(key, value);
                   
                
            


                using (BackgroundWorker _worker = new BackgroundWorker())
                {
                    _worker.DoWork += delegate(object s, System.ComponentModel.DoWorkEventArgs args)
                    {
                        test.ScriptExecute();
                    };
                    _worker.RunWorkerCompleted += delegate(object s, System.ComponentModel.RunWorkerCompletedEventArgs e)
                    {
                        pbTests.Value++;

                        OnTestSuiteCompleted(EventArgs.Empty, test, instanceID);
                        _worker.Dispose();

                    };
                    _worker.RunWorkerAsync(5000);
                }
            }

        }

        void AdvancedFeaturesWindow_TestSuiteCompleted(object sender, EventArgs e, TaskLibrary tl,int ID)
        {
            completedTests.Add(tl); 

            if (completedTests.Count == activeTestCount)
            {
                completedTests = completedTests.OrderBy(x => x.InstanceID).ToList<TaskLibrary>();
                
                LogResult(completedTests);
                completedTests.Clear();
            }
        }

        protected virtual void OnTestSuiteCompleted(EventArgs e, TaskLibrary tl,int id)
        {
            if (TestSuiteCompleted != null)
                TestSuiteCompleted(this, e, tl,id);
        }


        private void LogResult(List<TaskLibrary> completedTests)
        {
            RichTextBox txtResult = new RichTextBox();

            int uniqueCount = 0;
            int curInstID = 0;
            List<TaskLibrary> sameTestInstances = new List<TaskLibrary>();
            Dictionary<int,List<TaskLibrary>> dictInstances= new Dictionary<int,List<TaskLibrary>>();
            foreach (TaskLibrary test in completedTests)
            {

                if (test.InstanceID == curInstID)
                    sameTestInstances.Add(test);
                else
                    dictInstances.Add(test.InstanceID, sameTestInstances);

                curInstID = test.InstanceID;
            }

            for(int i=0;i<dictInstances.Count;i++)
            {
                
                AddNewLogTab(endpoints[i],out txtResult);


                foreach (TaskLibrary test in completedTests)
                {
                    //TextRange testName = new TextRange(txtResult.Document.ContentEnd, txtResult.Document.ContentEnd);
                    //TextRange testResult = new TextRange(txtResult.Document.ContentEnd, txtResult.Document.ContentEnd);
                    //TextRange testLog = new TextRange(txtResult.Document.ContentEnd, txtResult.Document.ContentEnd);
                    txtResult.AppendText(test.TestCaseName);

                    if (SharedTasks.GetTestCaseResult(test))
                    {
                        txtResult.AppendText("[PASS]" + Environment.NewLine);

                        //testResult.Text += " [PASS]"+Environment.NewLine; 
                        //testResult.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Green);

                    }
                    else
                    {
                        txtResult.AppendText("[FAIL]" + Environment.NewLine);
                        //testResult.Text += " [FAIL]" + Environment.NewLine;
                        //testResult.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.DarkRed);
                        txtResult.AppendText("-[LOG]" + SharedTasks.GetFailedTestStepLog(test));
                        //testLog.Text = SharedTasks.GetFailedTestStepLog(test);
                        //testLog.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Red);
                    }


                }
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

        }
       

    }
}
