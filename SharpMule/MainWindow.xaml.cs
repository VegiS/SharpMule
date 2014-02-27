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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ComponentModel;
using System.IO;
using TestManager.TreeViewCtrl;
using TestManager.DataGridCtrl;
using TestManager.Shared;
using System.Threading;
using System.Windows.Threading;
using System.Collections;
using System.Xml;
using System.Diagnostics;
using FindReplace;
using ICSharpCode.AvalonEdit;
using System.Windows.Controls.DataVisualization.Charting;
using SharpMule.Automation.Framework.Essentials;
using SharpMule.Automation.Framework.Network; 



namespace TestManager
{

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>

    public partial class MainWindow : Window
    {
        ArrayList testCases;
        const string DEFAULT_SCRIPT = "main.tc";
        const int ENVIRONMENT_STATUS_TIMECHECK = 60;  // Seconds it will take for timer to check Environment status; 
        const double SUITE_TIMEOUT_PERIOUD = 600000; // In Miliseconds (10 min)
        public const string CONFIG_PATH = "../Properties/configuration.xml";

        public bool reRunTestSuites { get; set; }
        public bool suiteCompleted { get; set; }
        public bool IsTestRunning { get; set; }
        public int TestSuiteCount { get; set; }
        public int FailedTestSuiteCount { get; set; }
        public int Counter { get; set; }
        public TestInterfaceEngine currentInstance { get; set; }
        List<TestInterfaceEngine> tlList;
        // A delegate type for hooking up added log notifications.
        public delegate void AddSuiteCompletedHandler(object sender, EventArgs e,TestInterfaceEngine tl);
        public event AddSuiteCompletedHandler TestSuiteCompleted;
        public static XmlDocument Config { get; set; }
        public static MainWindow MainWindowInstance { get; set; }
        public bool IsSharpMuleLoaded { get; set; }
        // Actions
        List<string> actionsList { get; set; }
        // Dialogs
        public static Window dbCommitDialog; 
        // Auto Run Vars
        bool isSuiteAutoRun { get; set; }
        const int AUTORUN_TRIES = 3;
        int autoCounter = 0; 

        // Startup
        public static String[] mArgs; 


        public MainWindow()
        {
            InitializeStartUpArgs(); 
            InitializeComponent();
            InitializeControls();
            InitializeUsageLogging();
            InitializeActionsAutoComplete(); 
            
        }


        public static void MessageBx(string text)
        {
            
            MessageBox.Show(text);
        }

        public void InitializeTestContext()
        {
            if (mArgs.Length >= 2)
            {
                string project = mArgs.ElementAtOrDefault(0);
                string test = mArgs.ElementAtOrDefault(1).TrimEnd('"')+"\\";
                string run = mArgs.ElementAtOrDefault(2);

                cmbProjects.SelectedValue = project;

                TreeViewItem items = (tvNavigation.Items[0] as TreeViewItem);
                TreeViewItem found = new TreeViewItem();
                if (Find(items, test, out found))
                {

                    found.IsSelected = true;

                    if(run.ToLower().Equals("run"))
                        btnRunSave_Click(found, null);
                }


                
            }


        }

        public bool Find(TreeViewItem item,string tag,out TreeViewItem foundItem)
        {
            foundItem = item; 
            if (item.Tag.ToString().Equals(tag))
                return true;

               
                foreach (TreeViewItem i in item.Items)
                {
                    if (Find(i, tag, out foundItem))
                        return true;
                }


               return false;
        }

        public void InitializeStartUpArgs()
        {
            mArgs = App.mArgs; 
        }
        public void InitializeUsageLogging()
        {
            try
            {
                TestUtilities.EmailClient email = new TestUtilities.EmailClient("sslugic@expedia.com", new TestInterfaceEngine());
                email.SmtpHost = SharedTasks.Email.SmtpHost;

                string stats = "\nUser:" + Environment.UserDomainName + "\\" + Environment.UserName +
                    "\nOS:" + Environment.OSVersion +
                    "\nMachine Name:" + Environment.MachineName +
                    "\nIs64BitOS:" + Environment.Is64BitOperatingSystem;




                email.SendCustomEmail(stats);
            }
            catch
            {
               
            }


        }
        public void InitializeControls()
        {
            MainWindowInstance = this; 
            LoadConfiguration();
            tlList = new List<TestInterfaceEngine>();
            ChangeRunButtonState("Run", true);
            EnableFindTextControl();
            IsSharpMuleLoaded = false;      
        }

        public void InitializeActionsAutoComplete()
        {
            try
            {
                actionsList = File.ReadAllLines(@"..\Actions\ActionsCollection.txt").ToList<string>();
                actionsList.Sort(); 
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message); 
            }
        }

        public void LoadConfiguration()
        {
            XmlDocument config = new XmlDocument();
            Config = config;

            try
            {
                config.LoadXml(File.ReadAllText(CONFIG_PATH));

                try
                {
                    SharedTasks.Script.Root = config.SelectSingleNode("/configuration/script/root").InnerXml;
                    //SharedTasks.Editor.External.Name = config.SelectSingleNode("/configuration/editor/name").InnerXml;
                    //SharedTasks.Editor.External.Path = config.SelectSingleNode("/configuration/editor/exepath").InnerXml;
                    SharedTasks.LoadAllEnvironments(config);
                    SharedTasks.LoadEmailConfig(config);
                    SharedTasks.LoadDbLoggingConfig(config);



                    if (SharedTasks.LoadAllConnections(config))
                    {
                        foreach (SharedTasks.Connections.Connection connection in SharedTasks.Connections.ConnectionList)
                        {
                            MenuItem miConnectionItem = new MenuItem();
                            miConnectionItem.Header = connection.Name;
                            miConnectionItem.ToolTip = connection.Env;
                            miConnectionItem.IsCheckable = true;
                            //miConnectionItem.Checked += new RoutedEventHandler(miConnectionItem_Checked);
                            //miConnectionItem.Unchecked += new RoutedEventHandler(miConnectionItem_Unchecked);

                            //Uncomment once you enable miConnection MenuItem
                            //miConnections.Items.Add(miConnectionItem); 

                            
                        }


                    }

                    //if (SharedTasks.LoadAllTools(config))
                    //{
                    //    foreach (SharedTasks.Tools.Tool tool in SharedTasks.Tools.ToolsList)
                    //    {
                    //        MenuItem miTool = new MenuItem();
                    //        miTool.Header = tool.ToolName;
                    //        miTool.ToolTip = tool.ToolPath;
                    //        miTool.Click += new RoutedEventHandler(miTool_Click);

                    //        // Add new tool to the menu item
                    //        miTools.Items.Add(miTool);

                    //    }


                    //}

                    if (SharedTasks.LoadAllProjects(config))
                    {
                        SharedTasks.Projects.Project project = SharedTasks.Projects.ProjectList[0] as SharedTasks.Projects.Project;
                        foreach (SharedTasks.Projects.Project p in SharedTasks.Projects.ProjectList)
                        {
                            cmbProjects.Items.Add(p.Path.Trim('.')); // Trim in case its relative path
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }

            }
            catch (Exception ex)
            {
                //MessageBox.Show("Unable to load configuration.xml. Check if the file exists. Exception:" + ex.Message);
                rtbErrorMessage.Visibility = System.Windows.Visibility.Visible; 
                rtbErrorMessage.AppendText("Unable to load configuration.xml. Check if the file exists. Exception:"+ex.Message); 
            }

           

        }

        void miTool_Click(object sender, RoutedEventArgs e)
        {
            MenuItem item = sender as MenuItem;
            string ExecutablePath = item.ToolTip.ToString();


            try
            {
                Utilities.StartNewProcess(ExecutablePath, String.Empty);
            }
            catch(Exception ex)
            {
                MessageBox.Show("Unable to start the tool:\n"+ex.Message,"Exception",MessageBoxButton.OK,MessageBoxImage.Error); 
            }
        }

       



        // Connection Name can be found in configuration.xml under connections
        bool IsConnectToJumpBox(string connectionName)
        {
            SharedTasks.Connections.ActiveConnection = SharedTasks.Connections.GetConnectionByName(connectionName).Host;
            SharedTasks.Connections.ActivePort = Convert.ToInt32(SharedTasks.Connections.GetConnectionByName(connectionName).Port);

            Client client = new Client(SharedTasks.Connections.ActiveConnection, SharedTasks.Connections.ActivePort);

            return client.CanConnect(); 
        }


        void miConnectionItem_Checked(object sender, RoutedEventArgs e)
        {
            // Uncomment once you enable miConnection MenuItem

            //MenuItem selectedItem = sender as MenuItem;
            //foreach (MenuItem item in miConnections.Items)
            //{
            //    if (item.IsChecked && item != selectedItem)
            //        item.IsChecked = false;
            //}



            //if (IsConnectToJumpBox(selectedItem.Header.ToString()))
            //{

            //    lblConnection.Content = "Connected to " + selectedItem.Header;
            //    lblConnection.Foreground = Brushes.DarkGreen;


            //    // Fill in the Other Combo box with available endpoints
            //    //MenuItem menuItem = sender as MenuItem;
            //    //SharedTasks.Environments.Environment env = null;
            //    //SharedTasks.GetEnvironmentByName(menuItem.ToolTip.ToString().ToLower(), out env);
            //    //var epItem = mainGrid.FindName("cmbOTHER") as ComboBox;
            //    //var envItem = mainGrid.FindName("rdbOTHER") as RadioButton;
            //    //epItem.ItemsSource = env.EndPoints;
            //    //epItem.SelectedIndex = env.DefaultEndpointIndex;
            //    //envItem.Content = env.EnvName;
            //    //envItem.IsEnabled = true;
            //    //envItem.IsChecked = true;


            //    //var rbVersion = mainGrid.FindName("rdbV" + SharedTasks.Environments.HotelVersion) as RadioButton;
            //    //rbVersion.IsChecked = true;
            //}
            //else
            //{
            //    selectedItem.IsChecked = false;
            //    MessageBox.Show("Unable to talk to connect");
            //}
                        

        }


        private void GetAllTests(TreeViewItem tItem)
        {

            foreach (TreeViewItem item in tItem.Items)
            {
                if (item.Items.Count == 0)
                    testCases.Add(item.Tag.ToString());
                else
                    GetAllTests(item);

            }

        }


        private void treeView1_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.NewValue != null)
            {
                InitializeTreeView((TreeViewItem)e.NewValue);
            }

        }
        
        
        private void InitializeTreeView(TreeViewItem tvItem)
        {
            
            TreeViewItem tvi = tvItem;
            TreeViewTasks.TestPath = tvi.Tag as string;
            SharedTasks.CurrentTestPath = tvi.Tag as string; 
            testCases = new ArrayList();


            DisposeObjects();
            CloseAllOpenTabs(tcScriptView);  

            //Load up all the test paths
            GetAllTests(tvi);
            
            // Set Test Case/Suite Title
            SharedTasks.SetTestCaseTitle();
            lblTestName.Content = SharedTasks.CurrentTestTitle;

            // Reset Progress Values
            prbStatus.Value = 0;
            lblTestNameProgress.Content = String.Empty; 

            ChangeRunButtonState("Run", true);
    

            //Hide rerun checkbox
            cbRerun.IsChecked = false;
            cbRerun.Visibility = System.Windows.Visibility.Hidden;

            

            if (TreeViewTasks.IsScriptSelected())
            {
                UpdateTestGridList();

            }
            else
            {
                UpdateSuiteGridList();
            }
        }

        private void LoadRtfText(RichTextBox rtbControl,string text)
        {
            MemoryStream stream = new MemoryStream(ASCIIEncoding.Default.GetBytes(text));
            rtbControl.Selection.Load(stream, DataFormats.Rtf);

        }
        private void UpdateSuiteGridList()
        {
            EnableEditTabItem(false);
            DataGridTasks.FillDataGridWithTests(dataGrid1, testCases);
            DataGridTasks.FillDataGridTestSuiteHistory(dgHistory, (new List<Chart>() { null, chartB }), tabItemHistory); 
            SetProgressBar(testCases.Count);
           
        }
        private void UpdateTestGridList()
        {

            SharedTasks.SetNewInstance();
            EnableEditTabItem(true);
            DataGridTasks.FillDataGrid(dataGrid1);
            DataGridTasks.FillDataGridTestCaseHistory(dgHistory, (new List<Chart>() { null, chartB }), tabItemHistory); 
            TreeViewTasks.GetScriptContent();
            txtScriptEditor.Text = SharedTasks.Editor.Internal.Text;
            txtScriptEditor.TextChanged += new EventHandler(txtScriptEditor_TextChanged);
            CreateAllScriptDocuments(SharedTasks.Editor.Internal.Text);
          
        }

        public void CreateAllScriptDocuments(string content)
        {
            (tcScriptView.Items[0] as TabItem).ToolTip = SharedTasks.CurrentTestPath+"main"; // ToolTip for the default tab
            for (int i = 1; i < tcScriptView.Items.Count; i++)
                tcScriptView.Items.RemoveAt(i); 

            string pattern = @"!attach\s*<(.*?)>";
            string varpattern = @"\$[a-zA-Z0-9_]*";
            string extension = ".tc";
            string sname = String.Empty; 
           
            MatchCollection scripts = Regex.Matches(content, pattern);

            //If script has content, then do foreach loop on each included file and process the content
            if (content.Length > 0)
            {
                foreach (Match sc in scripts)
                {
                    string filepattern = "<(.*?)>";
                    string file = String.Empty;

                    if (Regex.IsMatch(sc.Value, pattern))
                        file = Regex.Match(sc.Value, filepattern).Value.TrimStart('<').TrimEnd('>');

                    if (Regex.IsMatch(file, varpattern))
                    {
                        string key = Regex.Match(file, varpattern).Value;
                        string path = SharedTasks.TaskLibInstance.CommandLib.Variables[key];
                        file = file.Replace(key, path);
                        sname = file.Split('\\','/').Last();

                    }
                    else
                    {
                        
                        file = System.IO.Path.GetFullPath(SharedTasks.CurrentTestPath + "\\" + file);
                        sname = file.Split('\\', '/').Last();
                    }


                    try
                    {
                        string stext = File.ReadAllText(file + extension); 
                        GenerateNewScriptEditorTab(file,sname,stext);
                        //CreateAllScriptDocuments(stext); Need to figure out the path to other scripts
                    }
                    catch(Exception ex)
                    {
                        MessageBox.Show(ex.Message); 
                    }

                    
                }
            }

        }

        private void SetProgressBar(int max)
        {
            prbStatus.Value = 0;
            prbStatus.Maximum = max;
            prbStatus.Visibility = System.Windows.Visibility.Visible;
        }
        private void SetSuiteInstances()
        {
            int id = 0;
            foreach (string testCase in testCases)
            {
                SharedTasks.SetNewInstance(new TestCaseViewer(), testCase, id);
                id++;
            }
        }

        private void dataGrid1_AutoGeneratedColumns(object sender, EventArgs e)
        {
         
            dataGrid1.Columns[0].Width = dataGrid1.Width - 220; //Nr
            dataGrid1.Columns[1].Width = 100; //Lp
            dataGrid1.Columns[2].Width = 100; //Lp
        }

        private void dataGrid1_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
          
            DataGridColumn col = ((DataGrid)sender).CurrentColumn;

            int row = ((DataGrid)sender).SelectedIndex;

            if (col != null) // col.DisplayIndex == 1 if you want only second column
            {
                if (TreeViewTasks.IsScriptSelected())
                {
                    Viewer viewer = new Viewer();
                    viewer.SetText(SharedTasks.TaskLibInstance, row);
                    viewer.Show();

                }
                else
                {
                    try
                    {
                        TestCaseViewer tcViewer = new TestCaseViewer();
                        

                        if (SharedTasks.TestCaseViewerInstances.Count >= row)
                        {
                            tcViewer = SharedTasks.TestCaseViewerInstances[row];

                        }
                        else
                        {
                            tcViewer = new TestCaseViewer();
                        }

                        tcViewer.dgTestViewer.MouseDoubleClick += tcViewer.dgTestViewer_MouseDoubleClick;
                       

                        SharedTasks.TaskLibInstance = SharedTasks.TestSuitInstances[row];
                        SharedTasks.CurrentTestPath = testCases[row].ToString();

                        SharedTasks.Editor.Internal.FilePath = testCases[row].ToString();
                        SharedTasks.Editor.Internal.Text = File.ReadAllText(testCases[row].ToString() + DEFAULT_SCRIPT);

                        tcViewer.SetEditorText = SharedTasks.Editor.Internal.Text;
                        tcViewer.Show();


                        if (SharedTasks.TestSuitInstances.Count > 0 && SharedTasks.TestSuitInstances.Count >= row)
                        {
                            DataGridTasks.UpdateDataGridAtComplete(tcViewer.dgTestViewer, SharedTasks.TestSuitInstances[row]);
                        }
                        else
                        {
                            DataGridTasks.FillDataGrid(tcViewer.dgTestViewer);
                        }
                    }
                    catch
                    {
                        string test = testCases[row].ToString();
                        TreeViewItem items = (tvNavigation.Items[0] as TreeViewItem);
                        TreeViewItem found = new TreeViewItem();
                        
                        if (Find(items, test, out found))
                            found.IsSelected = true;



                    }

                }


            }


        }

  
        public void EnableTabControl(bool enable)
        {
            if (enable)
                tcScriptView.Visibility = Visibility.Visible;
        }
        public void EnableEditTabItem(bool enable)
        {
            if (enable)
                tabItemEdit.Visibility = System.Windows.Visibility.Visible;
            else
                tabItemEdit.Visibility = System.Windows.Visibility.Collapsed;

            tabItemView.IsSelected = true;
        }
        public void EnableFindTextControl()
        {

            FindReplace.FindReplaceMgr FRM = new FindReplace.FindReplaceMgr();
            FRM.CurrentEditor = new FindReplace.TextEditorAdapter(txtScriptEditor);
            FRM.ShowSearchIn = false;
            FRM.OwnerWindow = this;

            CommandBindings.Add(FRM.FindBinding);
            CommandBindings.Add(FRM.ReplaceBinding);
            CommandBindings.Add(FRM.FindNextBinding);
        }
        public void ChangeRunButtonState(string name, bool isReady)
        {
            EnableTabControl(isReady);
            //btnRunSave.IsEnabled = isEnabled;
            btnRunSave.ReleaseMouseCapture(); 
            btnRunSave.Content = name;
            btnRunSave.IsEnabled = isReady; 

            if (isReady)
            {
                dataGrid1.IsEnabled = true;
                IsTestRunning = false;
                ToggleNavigationControls(true); 
            }
            else
            {
                dataGrid1.IsEnabled = false;
                IsTestRunning = true;
                ToggleNavigationControls(false); 
            }
        }

        public void ToggleNavigationControls(bool isEnabled)
        {
                tvNavigation.IsEnabled = isEnabled;
                cmbProjects.IsEnabled = isEnabled; 

        }

        public void UpdateTestSuiteLable(string label)
        {

            lblTestName.Content = SharedTasks.CurrentTestTitle;  
            
            int failed = SharedTasks.GetFailedTestCaseCount();
            string resultmsg = String.Empty;

            if (failed > 0)
            {
                resultmsg = " Result: Failed - " + failed + " test/s case/s failed]";
                lblTestName.Foreground = Brushes.Red; 
                
            }
            else
            {
                resultmsg = " Result: Success - All tests have passed";
                lblTestName.Foreground = Brushes.Green; 
            }


            lblTestName.Content += " Suite "+label + resultmsg; 
        }
        public bool TryStartNewInstance()
        {
            SharedTasks.SetNewInstance();

            if (SharedTasks.TaskLibInstance.CommandLib.Commands.Count > 0)
                return true;

            else
                return false;

        }

        public bool LogToTestRail()
        {
            StringBuilder strBuilder = new StringBuilder();
            int result = 1;
            int status_id = 1;
            string test_id = String.Empty;


            if (SharedTasks.TaskLibInstance.CommandLib.Variables.TryGetValue("$tr_test_id", out test_id))
            {
                foreach (Command c in SharedTasks.TaskLibInstance.CommandLib.Log.LogStorage.Keys)
                {
                    if (!SharedTasks.TaskLibInstance.CommandLib.Log.LogStorage[c].IsTestPass)
                    {
                        result = 5;
                        status_id = 5;
                    }
                    else
                    {
                        result = 1;
                    }

                    string str = @"{
                                        ""content"": """ + c.Desc.Trim((new char[] { ' ', '\r', '\n' })) + @""",
                                        ""status_id"":" + result + @"
                                },";


                    strBuilder.Append(str);
                }
                string body = @"   {
                                  ""status_id"":" + status_id + @",
                                  ""created_by"":1,
                                  ""assignedto_id"":1,
                                  ""custom_comment"":""Step Test"",
                                  ""version"":""1.4"",
                                  ""elapsed"":""30s"",
                                  ""defects"":""none"",
                                  ""custom_steps"":[
                                     " + strBuilder.ToString().TrimEnd(',') + @"
                                  ]                                       
                               }";

                string url = "https://adastras.testrail.com/index.php?/api/v2/add_result/" + test_id;




                TestUtilities.JsonFormatter.PostJson(url, body);
            }
           

            return true;
        }
        private void RunTest()
        {

            RefreshDataGridTestSteps();

            lblTestName.Content = SharedTasks.TaskLibInstance.TestCaseName;
            lblTestName.Foreground = Brushes.Black;
            lblTestNameProgress.Content = SharedTasks.TaskLibInstance.TestCaseName;
            prbStatus.Maximum = 1; 

            ChangeRunButtonState("Running", false);
            using (BackgroundWorker _worker = new BackgroundWorker())
            {
                _worker.DoWork += delegate(object sender, System.ComponentModel.DoWorkEventArgs e)
                {
                    if (TryStartNewInstance())
                    {
                        IsTestRunning = true;
                        SharedTasks.TaskLibInstance.TestCaseCompleted += new TestInterfaceEngine.TestCaseCompletedHandler(TaskLibInstance_TestCaseCompleted);
  
                        Utilities.Timer.Start(); 
                        SharedTasks.TaskLibInstance.ScriptExecute();
                        
                    }
                };
                _worker.RunWorkerCompleted += delegate(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
                {
                    Utilities.Timer.Stop();
                    bool isPass = SharedTasks.GetTestCaseResult(SharedTasks.TaskLibInstance);

                    if (isPass)
                    {
                        lblTestName.Content += " [TestCase has Passed]";
                        lblTestName.Foreground = Brushes.DarkGreen;
                    }
                    else
                    {
                        lblTestName.Content += " [TestCase has Failed]";
                        lblTestName.Foreground = Brushes.Red;
                    }

                    lblTestName.Content += " [Finished in " + Utilities.Timer.GetElapsedTimeSecs() + "sec]";
                    lblTestNameProgress.Content = SharedTasks.TaskLibInstance.TestCaseName + " has finished";
                    prbStatus.Value = 1; 

                    ChangeRunButtonState("Run", true);
                    DataGridTasks.UpdateDataGridAtComplete(dataGrid1, SharedTasks.TaskLibInstance);

                    CommitTestResultsToDataBase();
                   

                    _worker.Dispose();
                    LogToTestRail(); 
                };



                _worker.RunWorkerAsync(5000);
            }
        }



      
        private void RunTestOverNetwork()
        {
            Client client = new Client(SharedTasks.Connections.ActiveConnection, SharedTasks.Connections.ActivePort);

            ChangeRunButtonState("Wait", false);
            using (BackgroundWorker _worker = new BackgroundWorker())
            {
                _worker.DoWork += delegate(object s, System.ComponentModel.DoWorkEventArgs args)
                {
                    //Start the Timer
                    
                    if (TryStartNewInstance())
                    {
                        Utilities.Timer.Start();
                        SharedTasks.TaskLibInstance = client.SendTLObject(SharedTasks.TaskLibInstance);
                        Utilities.Timer.Stop();
                        
                    }
                    else
                    {
                        MessageBox.Show("Unable to start the new instance. TestName: " + SharedTasks.TaskLibInstance.TestCaseName);
                    }

                    
                };
                _worker.RunWorkerCompleted += delegate(object s, System.ComponentModel.RunWorkerCompletedEventArgs e)
                {
                   
                    DataGridTasks.UpdateDataGridAtComplete(dataGrid1, SharedTasks.TaskLibInstance);
                    _worker.Dispose();
                    ChangeRunButtonState("Run", true);
                    lblTestName.Content += " [Finished in " + Utilities.Timer.GetElapsedTimeSecs() + "sec]";


                    if (SharedTasks.DataBaseLogging.IsEnabled)
                    {
                        SharedTasks.DataBaseLogging.LogToDatabase(SharedTasks.TaskLibInstance);

                    }
                    
                };
                _worker.RunWorkerAsync(5000);
                
            }

        }
        private void RunSuitesOverNetwork()
        {
            DisposeObjects();
            SetSuiteInstances();
            RefreshDataGridSuites(); 

            TestSuiteCompleted += new AddSuiteCompletedHandler(RemoteTestCompleted);
            Client client = new Client(SharedTasks.Connections.ActiveConnection, SharedTasks.Connections.ActivePort);

            ChangeRunButtonState("Wait", false);
            using (BackgroundWorker _worker = new BackgroundWorker())
            {

                _worker.DoWork += delegate(object s, System.ComponentModel.DoWorkEventArgs args)
                {                   
                    SharedTasks.TestSuitInstances = client.SendTLObject(SharedTasks.TestSuitInstances);
                    tlList = SharedTasks.TestSuitInstances; 

                };

                _worker.RunWorkerCompleted += delegate(object s, System.ComponentModel.RunWorkerCompletedEventArgs e)
                {
                        
                    // Stop the timer
                    Utilities.Timer.Stop();
                       

                    //Enable Run button again
                    ChangeRunButtonState("Run", true);
                       
                    //Update Test Lable and DataGrid with test results
                    UpdateTestSuiteLable(" (Finished " + SharedTasks.TestSuitInstances.Count + " test/s in " + Utilities.Timer.GetElapsedTimeSecs() + " seconds)");
                    DataGridTasks.UpdateDataGridSuites(dataGrid1, SharedTasks.TestSuitInstances);
                    CommitSuiteResultsToDataBase(); 
                        
                    // Enable rerun of the failed test cases if any failed test cases
                    if (SharedTasks.GetFailedTestCaseCount() > 0)
                        cbRerun.Visibility = System.Windows.Visibility.Visible;
                    else
                        cbRerun.Visibility = System.Windows.Visibility.Hidden;

                    _worker.Dispose();


                    // Send an email with Results
                    Utilities.SendEmail();

                };
                _worker.RunWorkerAsync(client);

            }

            // Start the timer here
            Utilities.Timer.Start(); 
        }

        private void RunFailedSuites()
        {

            List<TestInterfaceEngine> failedTests = SharedTasks.GetFailedTestCaseInstances();
            UnregisterEvents();
         

            TestSuiteCompleted += new AddSuiteCompletedHandler(FailedTestSuitesCompleted);
            
            FailedTestSuiteCount = failedTests.Count;
            SetProgressBar(FailedTestSuiteCount);
            Counter = 0;

            ChangeRunButtonState("Running", false);
            for(int i=0;i<FailedTestSuiteCount;i++)
            {
                TestInterfaceEngine test = failedTests[i]; 

                using (BackgroundWorker _worker = new BackgroundWorker())
                {
                    _worker.DoWork += delegate(object s, System.ComponentModel.DoWorkEventArgs args)
                    {  
                        test.TestCaseCompleted += new TestInterfaceEngine.TestCaseCompletedHandler(TestCaseCompleted);                     
                        test.ScriptExecute();
                    };

                    _worker.RunWorkerCompleted += delegate(object s, System.ComponentModel.RunWorkerCompletedEventArgs e)
                    {
                       
                        test.TestCaseCompleted -= TestCaseCompleted; 
                        OnTestSuiteCompleted(EventArgs.Empty,test);
                        _worker.Dispose();

                        

                    };
                    _worker.RunWorkerAsync(test);
                }

            }

            // Start Timer
            Utilities.Timer.Start(); 

        }
        private void RunSuite()
        {
            DisposeObjects();
            SetSuiteInstances();
            RefreshDataGridSuites(); 

            TestSuiteCompleted += new AddSuiteCompletedHandler(TestSuitesCompleted);

            int currentTestSuiteCount = TestSuiteCount; 

            ChangeRunButtonState("Running", false);
            

            for (int i = 0; i < SharedTasks.TestSuitInstances.Count; i++)
            {
                TestInterfaceEngine test = SharedTasks.TestSuitInstances[i];

                

                using (BackgroundWorker _worker = new BackgroundWorker())
                {

                    _worker.DoWork += delegate(object s, System.ComponentModel.DoWorkEventArgs args)
                    {
                        try
                        {
                            test.TestCaseCompleted += new TestInterfaceEngine.TestCaseCompletedHandler(TestCaseCompleted);
                            test.ScriptExecute();
                        }
                        catch
                        {
                            TestSuiteCount++; 
                        }

                    };
                 
                    _worker.RunWorkerCompleted += delegate(object s, System.ComponentModel.RunWorkerCompletedEventArgs e)
                    {
                        
                        OnTestSuiteCompleted(EventArgs.Empty,test);
                        _worker.Dispose();

                    };

                    
                    _worker.RunWorkerAsync(test);
                    


                    if (test.CommandLib.Variables.ContainsKey("$WAIT_TO_COMPLETE"))
                    {
                        string value = test.CommandLib.Variables["$WAIT_TO_COMPLETE"];

                        if (Convert.ToBoolean(value))
                        {
                            while (currentTestSuiteCount == TestSuiteCount) { }
                        }


                    }

                }

            }

            // Start the timer here
            Utilities.Timer.Start(); 

        }


        private void RefreshDataGridSuites()
        {

            DataGridTasks.FillDataGridWithTests(dataGrid1, testCases);
            
        }

        private void RefreshDataGridTestSteps()
        {
            DataGridTasks.FillDataGrid(dataGrid1); 
        }

        private void UnregisterEvents()
        {
            TestSuiteCompleted -= TestSuitesCompleted;
            TestSuiteCompleted -= FailedTestSuitesCompleted;
            TestSuiteCompleted -= RemoteTestCompleted;

        }

        private void DisposeObjects()
        {
            SharedTasks.ResetLogs();
            SharedTasks.ResetAllInstances();

            if (tlList != null)
                tlList.Clear();

            TestSuiteCount = 0;
            UnregisterEvents();
            lblTestName.Content = SharedTasks.CurrentTestTitle; 
             

        }



        public object ScriptEditor_NewTab(string selectedText, out TabItem newTabItem)
        {
            newTabItem = null;
            TextEditor newTextBox = new TextEditor();


            if (!TreeViewTasks.GetSpecificContent(selectedText).Equals(String.Empty) && !IsTabItemOpen(tcScriptView, selectedText))
            {

                //New Text Box
                newTextBox.Text = SharedTasks.Editor.Internal.Text;
                newTextBox.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
                newTextBox.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
                newTextBox.MouseDoubleClick += new MouseButtonEventHandler(txtScriptEditor_MouseDoubleClick);
                newTextBox.TextChanged += new EventHandler(txtScriptEditor_TextChanged);

                //New Tab Item
                newTabItem = new TabItem();
                newTabItem.Header = selectedText;
                newTabItem.Content = newTextBox;
                newTabItem.IsSelected = true;
                newTabItem.MouseDoubleClick += new MouseButtonEventHandler(TabItem_MouseDoubleClick);

                return tcScriptView;

            }

            return null;

        }
        private bool IsTabItemOpen(TabControl tcScript, string value)
        {
            foreach (TabItem item in tcScript.Items)
            {
                if (item.Header.Equals(value))
                    return true;
            }

            return false;
        }
        private void CloseAllOpenTabs(TabControl tcScript)
        {
            for (int i = 1; i < tcScript.Items.Count; i++)
            {
                tcScriptView.Items.RemoveAt(i);
            }
        }

        public void RefreshTreeView()
        {
            TreeViewTasks.GetTestDirectories(tvNavigation, cmbProjects.SelectedIndex);
            InitializeTreeView((TreeViewItem)tvNavigation.Items[0]);
        }
       

        #region Events

        private void TaskLibInstance_TestCaseCompleted(object sender, EventArgs e)
        {
            IsTestRunning = false; 
            TestInterfaceEngine test = sender as TestInterfaceEngine;
            test.TestCaseCompleted -= TaskLibInstance_TestCaseCompleted;
           
            

        }

        private void btnRunSave_Click(object sender, RoutedEventArgs e)
        {

            if (btnRunSave.Content.Equals("Run"))
            {

                if (!SharedTasks.Connections.ActiveConnection.Equals(String.Empty))
                {
                    
                    if (TreeViewTasks.IsScriptSelected())
                    {
                        RunTestOverNetwork();
                    }
                    else
                    {
                        RunSuitesOverNetwork();
                    }
                }
                else
                {
                                   
                    if (TreeViewTasks.IsScriptSelected())
                    {
                        RunTest();
                    }
                    else
                    {
                       
                        if (reRunTestSuites)
                            RunFailedSuites();
                        else
                            RunSuite();
                    }
                }
            }
            else if (btnRunSave.Content.Equals("Save"))
            {
                btnRunSave.IsEnabled = false;
                SharedTasks.Editor.Internal.TextChanged = false;
                SaveAllScripts(); 
            }
            else if (btnRunSave.Content.Equals("Running"))
            {
                ChangeRunButtonState("Run", true);
                RefreshDataGridSuites();
                
            }
            else
            {
                throw new Exception("You should never get here. Your code is making invalid modifications at run time");
            }

        }

        public void SaveAllScripts()
        {
            for (int i = 0; i < tcScriptView.Items.Count; i++)
            {
                TabItem tab = (tcScriptView.Items[i] as TabItem); 
                TextEditor editor = tab.Content as TextEditor;
                string content = editor.Text;
                string filepath = tab.ToolTip+".tc";

                try { File.WriteAllText(filepath, content); }
                catch (Exception e) { MessageBox.Show(e.Message); }

            }
        }

        private void txtScriptEditor_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            string selectedText = ((TextBox)sender).SelectedText + ".tc";

            TabItem newTabItem = null;
            TabControl tcScript = ScriptEditor_NewTab(selectedText, out newTabItem) as TabControl;

            if (newTabItem != null)
                tcScript.Items.Add(newTabItem);

        }


        public void GenerateNewScriptEditorTab(string path,string header,string scriptContent)
        {
            TabItem newTabItem = new TabItem();
            newTabItem.Content = CreateNewTextEditor(header, scriptContent); 
            newTabItem.Header = header;
            newTabItem.ToolTip = path;
 
            tcScriptView.Items.Add(newTabItem);

        }

        public TextEditor CreateNewTextEditor(string name,string content)
        {
            TextEditor editor = new TextEditor();
            editor.Name = name;
            editor.Text = content; 
            editor.FontFamily = txtScriptEditor.FontFamily;
            editor.FontSize = txtScriptEditor.FontSize;
            editor.SyntaxHighlighting = txtScriptEditor.SyntaxHighlighting; 
            editor.Height = txtScriptEditor.Height;
            editor.Width = txtScriptEditor.Width;
            editor.VerticalScrollBarVisibility = txtScriptEditor.VerticalScrollBarVisibility;
            editor.KeyDown+=editor_KeyDown;
            editor.TextChanged+=txtScriptEditor_TextChanged;
            editor.ContextMenu = SetupScriptEditorContextMenu((new ContextMenu())); 
            return editor;

           

        }

       

        void TabItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {

            var originalSource = e.OriginalSource as System.Windows.Media.Visual;

            if (!originalSource.DependencyObjectType.Name.Equals("TextView") && !originalSource.DependencyObjectType.Name.Equals("TextLayer"))
            {
                TreeViewItem path = tvNavigation.SelectedItem as TreeViewItem;
                TabItem name = sender as TabItem;

                // Open current text case in an external editor
                string editor = SharedTasks.Editor.External.Path;
                string file = path.Tag.ToString() + name.Header;

                // Set Internal Editor Values
                SharedTasks.Editor.Internal.FilePath = file;

                Utilities.StartNewProcess(editor, file);

            }

        }
        private void tabControl1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (tabItemEdit.IsSelected)
            {
                
                lblTestName.Content = "NEW HINT: Type '?' to see available actions. example: httpreq? and it will show you possible actions";
                btnRunSave.Content = "Save";
                btnRunSave.IsEnabled = SharedTasks.Editor.Internal.TextChanged;
               
            }
            if (tabItemView.IsSelected)
            {
                btnRunSave.Content = "Run"; 
                btnRunSave.IsEnabled = true;
            }
           
            
        }

        void TestCaseCompleted(object sender, EventArgs e)
        {
            if (IsTestRunning)
            {
                TestSuiteCount++;
            }
            else
            {
                DisposeObjects();
               
            }
        }

        private void FailedTestSuitesCompleted(object sender, EventArgs e,TestInterfaceEngine tl)
        {
            Counter++;
            prbStatus.Value = Counter; 

            SharedTasks.TestSuitInstances[tl.InstanceID] = tl;
            tlList[tl.InstanceID] = tl; 
            DataGridTasks.UpdateDataGridRow(dataGrid1, tl);
            CommitSuiteResultsToDataBase((new List<TestInterfaceEngine>() { tl }));

            if (FailedTestSuiteCount == Counter)
            {
   
                    Utilities.Timer.Stop();
                    UpdateTestSuiteLable(" (Finished " + FailedTestSuiteCount + " test/s in " + Utilities.Timer.GetElapsedTimeSecs() + " seconds)");


                    ChangeRunButtonState("Run", true);

                   

                    // Enable rerun of the failed test cases if any failed test cases
                    if (SharedTasks.GetFailedTestCaseCount() > 0 && !isSuiteAutoRun)
                    {
                        cbRerun.Visibility = System.Windows.Visibility.Visible;
                        cbRerun.IsChecked = true;
                    }
                    else
                    {
                        cbRerun.Visibility = System.Windows.Visibility.Hidden;
                        cbRerun.IsChecked = false;
                    }

                    // Send an email with Results
                    Utilities.SendEmail();
            }

        }

        private void RemoteTestCompleted(object sender, EventArgs e, TestInterfaceEngine tl)
        {
            Counter++;
            prbStatus.Value = Counter;

            tlList.Add(tl);
            SharedTasks.TestSuitInstances[tl.InstanceID] = tl;         
            DataGridTasks.UpdateDataGridAtSuiteCompleted(dataGrid1, tl);


            if (TestSuiteCount == SharedTasks.TestSuitInstances.Count)
            {
                Utilities.Timer.Stop();
                ChangeRunButtonState("Run", true);
                UpdateTestSuiteLable(" (Finished " + TestSuiteCount + " test/s in " + Utilities.Timer.GetElapsedTimeSecs() + " seconds)");

                if (SharedTasks.DataBaseLogging.IsEnabled)
                {
                    // Log Everything to Database
                    for (int i = 0; i < SharedTasks.TestSuitInstances.Count; i++)
                    {
                        SharedTasks.DataBaseLogging.LogToDatabase(SharedTasks.TestSuitInstances[i]);
                    }
                }

                // Enable rerun of the failed test cases if any failed test cases
                //if (SharedTasks.GetFailedTestCaseCount() > 0)
                //    cbRerun.Visibility = System.Windows.Visibility.Visible;
                //else
                //    cbRerun.Visibility = System.Windows.Visibility.Hidden;
            }

        }

        private void TestSuitesCompleted(object sender, EventArgs e, TestInterfaceEngine tl)
        {

            if (IsTestRunning)
            {
               
                    DataGridTasks.UpdateDataGridAtSuiteCompleted(dataGrid1, tl);

                    // Update Status Bar
                    prbStatus.Value = TestSuiteCount;
                    lblTestNameProgress.Content = tl.TestCaseName; 

                    //Update Instance List
                    SharedTasks.TestSuitInstances[tl.InstanceID] = tl;

                    if (Utilities.Timer.GetElapsedTime() > SUITE_TIMEOUT_PERIOUD)
                    {
                        ChangeRunButtonState("Run", true);
                    }

                    if (TestSuiteCount == SharedTasks.TestSuitInstances.Count)
                    {


                        if (isSuiteAutoRun && SharedTasks.GetFailedTestCaseCount() > 0)
                        {
                            autoCounter++; 
                            RunFailedSuites();
                        }
                        else
                        {

                         

                            Utilities.Timer.Stop();
                            TestSuiteCompleted -= TestSuitesCompleted;
                            tlList = SharedTasks.TestSuitInstances;

                            ChangeRunButtonState("Run", true);
                            lblTestNameProgress.Content = "Suite has completed!"; 

                            UpdateTestSuiteLable(" (Finished " + TestSuiteCount + " tests in " + Utilities.Timer.GetElapsedTimeSecs() + " seconds) ");

                            // Needs to be in here
                            CommitSuiteResultsToDataBase();

                            // Enable rerun of the failed test cases if any failed test cases
                            if (SharedTasks.GetFailedTestCaseCount() > 0 && !isSuiteAutoRun)
                                cbRerun.Visibility = System.Windows.Visibility.Visible;
                            else
                                cbRerun.Visibility = System.Windows.Visibility.Hidden;

                            // Send an email with Results
                            Utilities.SendEmail();
                        }
                    }

             }

            else
            {
                // Reset Progress Bar
                prbStatus.Value = 0;
                //Reset Title
                lblTestName.Content = SharedTasks.CurrentTestTitle;
            }
        }

        public List<TestInterfaceEngine> CreateNewObjectList(List<TestInterfaceEngine> oldObjectList)
        {
            List<TestInterfaceEngine> newList = new List<TestInterfaceEngine>();
            foreach (TestInterfaceEngine lb in oldObjectList)
            {
                TestInterfaceEngine newTL = new TestInterfaceEngine();
                newTL.ScriptSetup(lb.TestCasePath, "main.tc");
                newTL.CommandLib.Log = lb.CommandLib.Log; 
                newList.Add(newTL);
            }

          

            return newList; 
        }

        public void CommitSuiteResultsToDataBase()
        {
            CommitSuiteResultsToDataBase(SharedTasks.TestSuitInstances); 
        }

     
        public void CommitSuiteResultsToDataBase(List<TestInterfaceEngine> taskList)
        {
            //List<TestInterfaceEngine> newList = new List<TestInterfaceEngine>();
            //newList = CreateNewObjectList(taskList);

            List<TestInterfaceEngine> FailedCommits = new List<TestInterfaceEngine>();

            
            //Disable Navigation Control while commit data
            ToggleNavigationControls(false); 
           

            using (BackgroundWorker _worker = new BackgroundWorker())
            {
                _worker.ProgressChanged += _worker_ProgressChanged;
                _worker.WorkerReportsProgress = true; 
                _worker.DoWork += delegate(object sender, System.ComponentModel.DoWorkEventArgs e)
                {
                    if (SharedTasks.DataBaseLogging.IsEnabled)
                    {
                        // Log Everything to Database
                        for (int i = 0; i < taskList.Count; i++)
                        {

                            if (!SharedTasks.DataBaseLogging.LogToDatabase(taskList[i]))
                                FailedCommits.Add(taskList[i]);

             
                            decimal a = i+1;
                            decimal b = taskList.Count; 
                            _worker.ReportProgress(Convert.ToInt32(a / b * 100)); 
                        }

                    }
                };
                _worker.RunWorkerCompleted += delegate(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
                {
                    DataGridTasks.FillDataGridTestSuiteHistory(dgHistory, (new List<Chart>() { null, chartB }), tabItemHistory);

                    if (FailedCommits.Count > 0)
                        CommitSuiteResultsToDataBase(FailedCommits); 

                    _worker.Dispose();

                    ToggleNavigationControls(true); 
                };

               
                _worker.RunWorkerAsync(); 
            }
        }

        void _worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            lblTestNameProgress.Content = "Commiting to Database (" + e.ProgressPercentage.ToString() + "%)"; 
        }

        public void CommitTestResultsToDataBase(TestInterfaceEngine tl)
        {
            using (BackgroundWorker _worker = new BackgroundWorker())
            {
                _worker.DoWork += delegate(object sender, System.ComponentModel.DoWorkEventArgs e)
                {
                    if (SharedTasks.DataBaseLogging.IsEnabled)
                    {
                        SharedTasks.DataBaseLogging.LogToDatabase(tl);

                    }
                };
                _worker.RunWorkerCompleted += delegate(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
                {
                   
                    DataGridTasks.FillDataGridTestCaseHistory(dgHistory, (new List<Chart>() { null, chartB }), tabItemHistory);
                    _worker.Dispose();

                };


                _worker.RunWorkerAsync();
            }
        }
        public void CommitTestResultsToDataBase()
        {

            CommitTestResultsToDataBase(SharedTasks.TaskLibInstance); 
            
        }

       
        private void winTestManager_Loaded(object sender, RoutedEventArgs e)
        {
            SetupMenuItems();
            SetupTreeViewContextMenu();
            SetupScriptEditorContextMenu(cmScriptEditor);

        }
        
        protected virtual void OnTestSuiteCompleted(EventArgs e, TestInterfaceEngine tl)
        {
            if (TestSuiteCompleted != null)
                TestSuiteCompleted(this, e,tl);
        }

        private void winTestManager_Closed(object sender, EventArgs e)
        {
            System.Environment.Exit(0);
        }

        private void cmbProjects_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            RefreshTreeView(); 

        }

       

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            Utilities.StartNewProcess(SharedTasks.Editor.External.Path, @"..\Resources\text\UsageGuide.txt");
        }


        void editor_KeyDown(object sender, KeyEventArgs e)
        {
            //ContextMenu ctx = new ContextMenu();
            //actionsList.ForEach(x => ctx.Items.Add(x));
            //ctx.IsOpen = true; 

            string text = e.Key.ToString();
        }
        void txtScriptEditor_TextChanged(object sender, EventArgs e)
        {

            string text =((TextEditor)sender).Text;
            TextEditor ed = sender as TextEditor; 
            btnRunSave.IsEnabled = true;
            SharedTasks.Editor.Internal.Text = text;
            SharedTasks.Editor.Internal.TextChanged = true;

            string str = ed.Document.GetText(ed.Document.GetLineByOffset(ed.CaretOffset));
            ContextMenu ctx = new ContextMenu();
            
            
            
            actionsList.Where(word => word.StartsWith(str.TrimEnd('?')) && !str.Equals(String.Empty))
                .Select(word=>word).ToList().ForEach(word=>ctx.Items.Add(GetMenuItemFromString(word,ed)));

            if(str.StartsWith("$"))
                SharedTasks.TaskLibInstance.CommandLib.Variables.Keys.ToList().Where(word => word.StartsWith(str.TrimEnd('?')) && !str.Equals(String.Empty))
                    .Select(word=>word).ToList().ForEach(word=>ctx.Items.Add(GetMenuItemFromString(word,ed)));
            
           if (str.EndsWith("?"))
                ctx.IsOpen = true;

           
            
        }
        MenuItem GetMenuItemFromString(string text,TextEditor ed)
        {
            MenuItem mi = new MenuItem();
            mi.Header = text;
            try { mi.ToolTip = SharedTasks.TaskLibInstance.CommandLib.Variables[text]; }
            catch { } 

            mi.Click += delegate
            {
               int offset = ed.Document.GetLineByOffset(ed.CaretOffset).Offset;
               int len = ed.Document.GetLineByOffset(ed.CaretOffset).Length;
               ed.Document.Replace(offset, len, text); 
            };

            return mi; 
        }

        private void GetRelatedActions(string text)
        {
            actionsList.Where(word => word.StartsWith(text)).Select(word=>word); 
        }


        private void cbRerun_Checked(object sender, RoutedEventArgs e)
        {
            reRunTestSuites = true;
        }
        private void cbRerun_Unchecked(object sender, RoutedEventArgs e)
        {
            reRunTestSuites = false;
        }

        private void winTestManager_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (IsSharpMuleLoaded)
            {


                mainGrid.Width = winTestManager.ActualWidth;
                mainGrid.Height = winTestManager.ActualHeight - 50;
                mainGrid.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
                mainGrid.VerticalAlignment = System.Windows.VerticalAlignment.Top;
                viewBox.Width = winTestManager.ActualWidth;
                viewBox.Height = winTestManager.ActualHeight - 50;
                viewBox.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
                viewBox.VerticalAlignment = System.Windows.VerticalAlignment.Top;
                prbStatus.Width = winTestManager.ActualWidth;
                prbStatus.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
                menuMain.Width = winTestManager.ActualWidth;
            }


        }


        private void MenuItem_Email_Checked(object sender, RoutedEventArgs e)
        {
            SharedTasks.Email.IsEnabled = true;
        }
        private void MenuItem_Email_UnChecked(object sender, RoutedEventArgs e)
        {
            SharedTasks.Email.IsEnabled = false;
        }
        private void MenuItem_Checked(object sender, RoutedEventArgs e)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = "TcpClient.exe";

            Process proc = new Process();
            proc.StartInfo = startInfo;

            proc.Start();
        }

        private void winTestManager_Closing(object sender, CancelEventArgs e)
        {}


        private void dataGrid1_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            //MessageBox.Show("Test");
            // Get the DataRow corresponding to the DataGridRow that is loading.
            Data item = e.Row.Item as Data;
            DataGridRow row = e.Row;
            if (item != null)
            {
                // Access cell values values if needed...
                // var colValue = row["ColumnName1]";
                // var colValue2 = row["ColumName2]";
                if (item.TestResult.ToLower().Equals("fail"))
                    row.Background = Brushes.PaleVioletRed;
                else if (item.TestResult.ToLower().Equals("pass"))
                    row.Background = Brushes.LightGreen;


            }
        }


        // To select tree view item on right click
        private void tvNavigation_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            TreeViewItem treeViewItem = VisualUpwardSearch(e.OriginalSource as DependencyObject);

            if (treeViewItem != null)
            {
                treeViewItem.Focus();
                e.Handled = true;
            }
        }

        static TreeViewItem VisualUpwardSearch(DependencyObject source)
        {
            while (source != null && !(source is TreeViewItem))
                source = VisualTreeHelper.GetParent(source);

            return source as TreeViewItem;
        }

        private void cbAutoRerun_Checked(object sender, RoutedEventArgs e)
        {
            isSuiteAutoRun = true;
        }

        private void cbAutoRerun_Unchecked(object sender, RoutedEventArgs e)
        {
            isSuiteAutoRun = false;
        }

        private void btnAddEP_Click(object sender, RoutedEventArgs e)
        {
            dbCommitDialog = new Window
            {
                Title = "Endpoint Entry Dialog",
                WindowStyle = System.Windows.WindowStyle.ToolWindow,
                WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen,
                Content = new SharpMule.DialogBoxes.NewEndpointEntryUserControl(),
                Width = 360,
                Height = 101

            };

            dbCommitDialog.ShowDialog();


        }


        private void dgHistory_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            //MessageBox.Show("Test");
            // Get the DataRow corresponding to the DataGridRow that is loading.
            TestCaseLoggingData item = e.Row.Item as TestCaseLoggingData;

            DataGridCell cell = e.Row.Item as DataGridCell;


            DataGridRow row = e.Row;
            if (item != null)
            {
                // Access cell values values if needed...
                // var colValue = row["ColumnName1]";
                // var colValue2 = row["ColumName2]";
                if (item.TestStatus.ToString().ToLower().Equals("fail"))
                    row.Background = Brushes.PaleVioletRed;
                else if (item.TestStatus.ToString().ToLower().Equals("pass"))
                    row.Background = Brushes.LightGreen;




            }



        }

        private void dgHistory_MouseDoubleClick_1(object sender, MouseButtonEventArgs e)
        {
            TestCaseLoggingData data = dgHistory.CurrentItem as TestCaseLoggingData;

            List<Data> testdata = new List<Data>();
            List<string> resultsData = new List<string>();

            TestCaseViewer tcViewer = new TestCaseViewer();


            string result = String.Empty;
            string query = "select * from TestCaseStepLogging where TestCaseIdentifier=\'" + data.Identifier + "\' order by TestCaseStepIndex asc";

            try
            {
                DataBaseTasks dbtasks = new DataBaseTasks(true);
                dbtasks.Connect(SharedTasks.DataBaseLogging.DbServer, SharedTasks.DataBaseLogging.DbName);
                dbtasks.SendQuery(query, out result, true);

                XmlDocument doc = new XmlDocument();
                doc.LoadXml(result);


                XmlNodeList nodeList = doc.SelectNodes("/NewDataSet/Table1");

                foreach (XmlNode node in nodeList)
                {
                    string step = node["TestCaseStep"].InnerXml.Trim();
                    string steplog = node["TestCaseStepLog"].InnerText.Replace("ERROR:", "\nERROR:\n").Replace("DEBUG:", "\nDEBUG:\n").Replace("WARN:", "\nWARN:\n").Replace("INFO:", "\nINFO:\n");
                    string stepresult = node["TestCaseStepResult"].InnerXml.Trim();

                    resultsData.Add(steplog);
                    testdata.Add(new Data() { TestName = step, TestResult = (Convert.ToBoolean(stepresult) == false ? "PASS" : "FAIL") });
                }

                tcViewer.dgTestViewer.ItemsSource = testdata;
                tcViewer.MouseDoubleClick += delegate
                {
                    Viewer viewer = new Viewer();
                    viewer.txtViewer.Text = resultsData[tcViewer.dgTestViewer.SelectedIndex];
                    viewer.Show();
                };

                tcViewer.Show();



            }
            catch (Exception ex)
            {
                MainWindow.MessageBx(ex.Message);
            }
        }
        #endregion

        #region Environment Gadget

        private void SetUpdateTimer()
        {
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = new TimeSpan(0, 0, ENVIRONMENT_STATUS_TIMECHECK);
            timer.Tick += new EventHandler(Timer_Tick);
            timer.Start(); 

        }

        void Timer_Tick(object sender, EventArgs e)
        {
            // Add some Code Here
        }

        
        
        private ContextMenu SetupScriptEditorContextMenu(ContextMenu ctxMenu)
        {
            MenuItem miCopy = new MenuItem();
            MenuItem miPate = new MenuItem();
            MenuItem miCut = new MenuItem();
            
            miCopy.Header = "Copy";
            miPate.Header = "Paste";
            miCut.Header = "Cut";

            miCopy.Click += new RoutedEventHandler(miCopy_Click);
            miPate.Click += new RoutedEventHandler(miPate_Click);
            miCut.Click += new RoutedEventHandler(miCut_Click);

            ctxMenu.Items.Add(miCopy);
            ctxMenu.Items.Add(miPate);
            ctxMenu.Items.Add(miCut);

            return ctxMenu; 

        }
        

        void miCut_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Clipboard.SetText(txtScriptEditor.Text);
                txtScriptEditor.SelectedText = String.Empty;
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
                txtScriptEditor.SelectedText = text;
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
                Clipboard.SetText(txtScriptEditor.Text);
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message); 
            }
            
        }

        private void SetupTreeViewContextMenu()
        {
            MenuItem miNewTest = new MenuItem();
            MenuItem miNewSuite = new MenuItem();
            MenuItem miRefresh = new MenuItem();
            MenuItem miAdFeature = new MenuItem();
            MenuItem miOpenFolder = new MenuItem();
            MenuItem miCollapseTree = new MenuItem();
            MenuItem miExpendTree = new MenuItem(); 

            TextBox txtSuite = new TextBox();
            TextBox txtTest = new TextBox();

            txtSuite.Text = "Enter New Suite Name";
            txtSuite.SelectAll(); 
            txtSuite.MinWidth = 100.0;
            txtSuite.MaxWidth = 100.0; 

            txtTest.Text = "Enter New Test Name";
            txtTest.SelectAll(); 
            txtTest.MinWidth = 100.0;
            txtTest.MaxWidth = 100.0; 

            txtSuite.KeyDown += new KeyEventHandler(txtSuite_KeyDown);
            txtTest.KeyDown += new KeyEventHandler(txtTest_KeyDown);

            miNewTest.Header = "Add New Test";
            miNewTest.Items.Add(txtTest); 

            miNewSuite.Header = "Add New Suite";
            miNewSuite.Items.Add(txtSuite);

            // Disabled for now untill its completed/if decided to finish it up
            //miAdFeature.Header = "Open Advanced View";
            //miAdFeature.Click += new RoutedEventHandler(miAdFeature_Click);

            miRefresh.Header = "Refresh Tests";
            miRefresh.Click += new RoutedEventHandler(miRefresh_Click);

            miOpenFolder.Header = "Open Test Location";
            miOpenFolder.Click += new RoutedEventHandler(miOpenFolder_Click);

            miCollapseTree.Header = "Collapse Suite";
            miCollapseTree.Click += new RoutedEventHandler(miCollapseTree_Click);

            miExpendTree.Header = "Expend Suite";
            miExpendTree.Click += new RoutedEventHandler(miExpendTree_Click);

            tvContextMenu.Items.Add(miNewTest);
            tvContextMenu.Items.Add(miNewSuite);
            tvContextMenu.Items.Add(miAdFeature);
            tvContextMenu.Items.Add(miOpenFolder);
            tvContextMenu.Items.Add(miCollapseTree);
            tvContextMenu.Items.Add(miExpendTree); 
            tvContextMenu.Items.Add(miRefresh); 
            
        }

        void miExpendTree_Click(object sender, RoutedEventArgs e)
        {
            TreeViewItem item = tvNavigation.SelectedItem as TreeViewItem; 
            item.IsExpanded = true; 
        }

        void miCollapseTree_Click(object sender, RoutedEventArgs e)
        {
            TreeViewItem item = tvNavigation.SelectedItem as TreeViewItem; 
            item.IsExpanded = false; 
        }

        void miOpenFolder_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //Open Test Case Directory
                Process.Start(TreeViewTasks.TestPath);
            }
            catch
            {
                MessageBox.Show("Unable to open current location. Make sure the location hasn't changed. Try refreshing your test suites");
            }
        }

        void miAdFeature_Click(object sender, RoutedEventArgs e)
        {
            AdvancedFeaturesWindow afWindow = new AdvancedFeaturesWindow();
            DisposeObjects(); 
            SetSuiteInstances();
            DataGridTasks.FillDataGridWithTests(afWindow.dgResults, testCases);

    
                afWindow.TestList.Clear();
                afWindow.TestCaseObjectList.Clear();
                afWindow.lbTestList.Items.Clear();
            
            foreach (TestInterfaceEngine test in SharedTasks.TestSuitInstances)
            {
                CheckBox cbItem = new CheckBox();
                cbItem.Content = test.TestCaseName;
                cbItem.Tag = test.TestCasePath; 
                cbItem.IsChecked = true;
                cbItem.Uid = test.InstanceID.ToString();
                cbItem.Checked += new RoutedEventHandler(afWindow.item_Checked);
                cbItem.Unchecked += new RoutedEventHandler(afWindow.item_Unchecked);

                
                afWindow.TestList.Add(test.TestCasePath);
                afWindow.TestCaseObjectList.Add(test); 
                afWindow.lbTestList.Items.Add(cbItem);
                
            }

            afWindow.Show();
           
        }

   

        void miRefresh_Click(object sender, RoutedEventArgs e)
        {
            RefreshTreeView();
        }


        void txtTest_KeyDown(object sender, KeyEventArgs e)
        {
            TextBox txBox = sender as TextBox; 
            if (e.Key == Key.Enter)
            {
                CreateNewTestCase(txBox.Text);
                RefreshTreeView(); 

            }
        }

        void txtSuite_KeyDown(object sender, KeyEventArgs e)
        {
            TextBox txBox = sender as TextBox; 

            if (e.Key == Key.Enter)
            {
                CreateNewSuiteFolder(txBox.Text);
                RefreshTreeView(); 
            }
        }


        private void CreateNewSuiteFolder(string suitename)
        {
            string fullpath = TreeViewTasks.TestPath+suitename; 
            if(!Directory.Exists(fullpath))
                Directory.CreateDirectory(fullpath);
        }
        private void CreateNewTestCase(string testname)
        {
            CreateNewSuiteFolder(testname); 

            string testName = testname + "\\" + DEFAULT_SCRIPT; 
            File.Create(TreeViewTasks.TestPath+testName); 
        }
        private void SetupMenuItems()
        {
            IsSharpMuleLoaded = true; 
            miEnableEmail.IsChecked = SharedTasks.Email.IsEnabled;
            InitializeTestContext(); 
        }

        // Change the Script File
        private void ChangeScriptValue(string value, string variablename)
        {
            SharedTasks.Environments.Environment env = SharedTasks.Environments.GetEnvironmentByName(SharedTasks.Environments.DefaultEnvironment);

            string root = SharedTasks.Environments.Settings["root"].InnerXml;
            string file = env.EnvironmentNode["setting"].InnerXml;

            string _fsettings = File.ReadAllText(root + file);

            string _pattern = @"(?<=\" + variablename + @"\s*\=\s*\{)(.*?)(?=\})";
            _fsettings = Regex.Replace(_fsettings, _pattern, value);

            File.WriteAllText(root + file, _fsettings);
        }

        // Change Script Headers
        private void ChangeScriptHeaders(string oldHeader, string newHeader)
        {
            string root = SharedTasks.Environments.Settings["root"].InnerXml;
            string file = SharedTasks.Environments.Settings["global"].InnerXml;
            string _fsglobal = File.ReadAllText(root + file);

            string _pattern = @"(?<=\<)" + oldHeader + @"(?=\>)";
            _fsglobal = Regex.Replace(_fsglobal, _pattern, newHeader);

            File.WriteAllText(root + file, _fsglobal);
        }




        #endregion
       
    }

  
}
