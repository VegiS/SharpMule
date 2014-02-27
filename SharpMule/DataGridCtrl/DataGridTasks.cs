using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions; 
using System.Windows.Controls;
using TestManager;
using System.Windows.Data; 
using SharpMule.Automation.Framework.Essentials;
using TestManager.Shared;
using System.Collections;
using System.Configuration;
using SharpMule;
using System.Data.SqlClient;
using System.Data;
using System.Windows.Controls.DataVisualization.Charting;
using System.Windows.Media;
using System.Windows; 




namespace TestManager.DataGridCtrl
{
    class DataGridTasks
    {
        static List<Data> source;
        static List<Data> viewsource;
        static bool exceptionOccured = false;
        const int DATABASE_TIME_OFFSET = 365; // Pull records that are no older then 30 days

        

        public static void FillDataGridTestCaseHistory(DataGrid dgrid, List<Chart> charts,TabItem tab)
        {
            Dictionary<object, int> testResults = new Dictionary<object, int>();
            Chart chartB = charts.ElementAt(1);


            if (SharedTasks.DataBaseLogging.IsEnabled)
            {
                try
                {

                    if (!exceptionOccured)
                    {
                        using (SqlConnection con = new SqlConnection(SharedTasks.DataBaseLogging.ConnectionString))
                        {
                            string cmdString = "SELECT * FROM TestCaseLogging where TestName=\'" + SharedTasks.CurrentTestTitle + "\' AND CreateDate > CURRENT_TIMESTAMP-" + DATABASE_TIME_OFFSET+" AND TestEnvironment=\'"+SharedTasks.Environments.GetDefaultEnvironment().EnvName+"\' ORDER BY CreateDate DESC";
                            SqlCommand cmd = new SqlCommand(cmdString, con);
                            SqlDataAdapter sda = new SqlDataAdapter(cmd);
                            DataTable dt = new DataTable("TestCaseLogging");
                            sda.Fill(dt);
                            List<TestCaseLoggingData> dataSource = new List<TestCaseLoggingData>();

                            foreach (DataRow row in dt.Rows)
                            {
                                /*
                                 * 0:UniqueKey
                                 * 1:SuiteName
                                 * 2:TestName
                                 * 3:TestStatus
                                 * 4:Tester
                                 * 5:TestEnvironment
                                 * 6:TestEndPoint
                                 * 7:CreateDate
                                */

                                List<object> items = row.ItemArray.ToList();
                                object keyB = (Convert.ToBoolean(items[3]) == true ? "FAIL" : "PASS");
                              


                                if (testResults.Count == 0)
                                {
                                    testResults.Add("PASS", 0);
                                    testResults.Add("FAIL", 0);
                                }

                                if (testResults.ContainsKey(keyB))
                                    testResults[keyB]++;
                                else
                                    testResults.Add(keyB, 1);

                                dataSource.Add(new TestCaseLoggingData() { Identifier = items[0], TestName = items[2], TestStatus = (Convert.ToBoolean(items[3]) == true ? "FAIL" : "PASS"), Tester = items[4], CreateDate = items[7]/*SuiteName = items[1],*/ /*Identifier = items[0],*/ });


                            }

                            dgrid.ItemsSource = dataSource;
                            chartB.DataContext = testResults;
                        }
                    }
                }
                catch (Exception ex)
                {
                    MainWindow.MessageBx("Failed to Load Test Case History" + Environment.NewLine + ex.Message);
                    exceptionOccured = true;
                    dgrid.ItemsSource = new List<TestCaseLoggingData>();
                    tab.IsEnabled = false; 
                }
            }
            else
            {
                tab.IsEnabled = false;
            }

        }

        public static void FillDataGridTestSuiteHistory(DataGrid dgrid,List<Chart> charts,TabItem tab)
        {
            Dictionary<object,int> testResults = new Dictionary<object,int>();
            Chart chartB = charts.ElementAt(1); 
            

            string suitepath = System.Text.RegularExpressions.Regex.Replace(SharedTasks.CurrentTestPath.Replace('\\', '.').Trim('.'),"[a-zA-Z]\\:\\.","");
            suitepath =  Regex.Match(suitepath, SharedTasks.Script.Root + @"(.*)").Value; 
            if (SharedTasks.DataBaseLogging.IsEnabled)
            {
                try
                {

                    if (!exceptionOccured)
                    {
                        using (SqlConnection con = new SqlConnection(SharedTasks.DataBaseLogging.ConnectionString))
                        {
                            string cmdString = "SELECT * FROM TestCaseLogging WHERE SuiteName LIKE \'" + suitepath + "%\' AND CreateDate > CURRENT_TIMESTAMP-"+DATABASE_TIME_OFFSET+" ORDER BY CreateDate DESC";
                            SqlCommand cmd = new SqlCommand(cmdString, con);
                            SqlDataAdapter sda = new SqlDataAdapter(cmd);
                            DataTable dt = new DataTable("TestCaseLogging");
                            sda.Fill(dt);
                            List<TestCaseLoggingData> dataSource = new List<TestCaseLoggingData>();

                            foreach (DataRow row in dt.Rows)
                            {
                                /*
                               * 0:UniqueKey
                               * 1:SuiteName
                               * 2:TestName
                               * 3:TestStatus
                               * 4:Tester
                               * 5:TestEnvironment
                               * 6:TestEndPoint
                               * 7:CreateDate
                              */

                                List<object> items = row.ItemArray.ToList();
                                object keyB = (Convert.ToBoolean(items[3]) == true ? "FAIL" : "PASS");
        

                                if (testResults.Count == 0)
                                {
                                    testResults.Add("PASS", 0);
                                    testResults.Add("FAIL", 0);
                                }

                                if (testResults.ContainsKey(keyB))
                                    testResults[keyB]++;
                                else
                                    testResults.Add(keyB, 1);



                                dataSource.Add(new TestCaseLoggingData() { Identifier = items[0], TestName = items[2], TestStatus = (Convert.ToBoolean(items[3]) == true ? "FAIL" : "PASS"), Tester = items[4], CreateDate = items[7]/*SuiteName = items[1],*/ /*Identifier = items[0],*/ });


                            }

                            dgrid.ItemsSource = dataSource;
                            chartB.DataContext = testResults;
                        }
                    }
                }
                catch (Exception ex)
                {
                    MainWindow.MessageBx("Failed to Load Test Suite History" + Environment.NewLine + ex.Message);
                    exceptionOccured = true;
                    dgrid.ItemsSource = new List<TestCaseLoggingData>();
                    tab.IsEnabled = false;
                }
            }
            else
            {
                tab.IsEnabled = false;
            }


        }


        public static void FillDataGridWithTests(DataGrid dgrid, ArrayList testList)
        {
            source = new List<Data>();

            foreach (string test in testList)
            {
                string[] dirs = test.TrimEnd('\\').Split('\\');
                if (dirs.Length > 0)
                {
                    source.Add(new Data()
                    {
                        TestName = dirs[dirs.Length-1], //Remove(0, 3).TrimEnd('\\').Replace('\\', '.'),
                        TestResult = ""
                    });
                }

            }
            dgrid.ItemsSource = source;
            dgrid.Items.Refresh();
        }

        public static void FillDataGrid(DataGrid dgrid)
        {

            viewsource = new List<Data>();
            foreach (Command cmd in SharedTasks.TaskLibInstance.CommandLib.Commands)
            {
                if(!cmd.Ignore){
                    viewsource.Add(new Data()
                    {
                        TestName = cmd.Desc.Trim(),
                        TestResult = ""
                    });
                }
            }


            dgrid.ItemsSource = viewsource;
            dgrid.Items.Refresh();
    


        }


        public static void UpdateDataGrid(DataGrid dgrid, Logger currlog)
        {

            int currpos = dgrid.Items.CurrentPosition;
            source[currpos].TestResult = currlog.LogStorage.Last().Value.IsTestPass.ToString();
            dgrid.Items.Refresh();
            dgrid.Items.MoveCurrentToNext();

        }

        public static void UpdateDataGridRow(DataGrid dgrid, TestInterfaceEngine tl)
        {
            int rowIndex = tl.InstanceID;
            if (SharedTasks.GetTestCaseResult(tl))
                source[rowIndex].TestResult = "Pass";
            else
                source[rowIndex].TestResult = "Fail";


            ColorDataGridRows(dgrid);

        }

        public static void UpdateDataGridAtSuiteCompleted(DataGrid dgrid, TestInterfaceEngine tl)
        {
            int index = tl.InstanceID;
            bool result = SharedTasks.GetTestCaseResult(tl);

            if (result)
            {
                source[index].ElapsedTime = GetTestCaseElapsedTime(tl); 
                source[index].TestResult = "Pass";
            }
            else
            {
                source[index].ElapsedTime = GetTestCaseElapsedTime(tl);
                source[index].TestResult = "Fail";
            }

            ColorDataGridRows(dgrid); 

        }

        public static void UpdateDataGridSuites(DataGrid dgrid, List<TestInterfaceEngine> tlList)
        {
            for (int i = 0; i < tlList.Count; i++)
            {
                if (i < source.Count)
                {
                    if (SharedTasks.GetTestCaseResult(tlList[i]))
                    {
                        source[i].TestResult = "Pass";
                    }
                    else
                    { 
                        source[i].TestResult = "Fail";
                    }
                }


                dgrid.Items.Refresh();
            }

            ColorDataGridRows(dgrid); 

        }
        public static void UpdateDataGridAtComplete(DataGrid dgrid, TestInterfaceEngine dl)
        {
            FillDataGrid(dgrid);
            List<Command> cmds = dl.CommandLib.Commands.ToList<Command>();
            DebugLogger log = new DebugLogger();
            int viewindex = 0; 

 

            for (int i = 0; i < cmds.Count; i++)
            {
                // If user choses to ignore the command we do not want to show it in the grid. 
                // This is the logic to remove it. We also need to make sure that we keep the index
                // of the gird in the range so we use viewindex for that. 
                if (cmds[i].Ignore)
                {
                    if (viewindex > 0)
                        viewindex -= 1;
                }
                else
                {
                    if (dl.CommandLib.Log.LogStorage.TryGetValue(cmds[i], out log))
                    {
                        if (log == null)
                        {
                            viewsource[viewindex].ElapsedTime = log.TimeElappsed + " ms"; 
                            viewsource[viewindex].TestResult = "Fail";
                        }
                        else
                        {
                            viewsource[viewindex].ElapsedTime = log.TimeElappsed + " ms"; 
                            viewsource[viewindex].TestResult = log.IsTestPass ? "Pass" : "Fail";
                        }
                    }
                }
                dgrid.Items.Refresh();

                viewindex++; 

            }


            ColorDataGridRows(dgrid); 


        }

        public static string GetTestCaseElapsedTime(TestInterfaceEngine tl)
        {
            List<Command> cmds = tl.CommandLib.Commands.ToList<Command>();
            DebugLogger log = new DebugLogger();
            double timeElap = 0.0; 

            for (int i = 0; i < cmds.Count; i++)
            {

                if (tl.CommandLib.Log.LogStorage.TryGetValue(cmds[i], out log))
                {
                    timeElap += Convert.ToDouble(log.TimeElappsed); 
                }
            }


            return Convert.ToInt32(timeElap).ToString(); 
        }
        public static void ColorDataGridRows(DataGrid dgrid)
        {

            dgrid.ScrollIntoView(dgrid);
            dgrid.Items.Refresh();
            //for (int j = 0; j < dgrid.Items.Count; j++)
            //{
            //    DataGridRow row = (DataGridRow)dgrid.ItemContainerGenerator.ContainerFromIndex(j);

            //    dgrid.UpdateLayout();
            //    dgrid.ScrollIntoView(dgrid.Items[j]);
            //    row = (DataGridRow)dgrid.ItemContainerGenerator.ContainerFromIndex(j);

            //    Data data = row.Item as Data;

            //    if (data.TestResult.ToLower().Equals("pass"))
            //        row.Background = System.Windows.Media.Brushes.LightGreen;
            //    if (data.TestResult.ToLower().Equals("fail"))
            //        row.Background = System.Windows.Media.Brushes.PaleVioletRed;

            //}
        }
    }
    public class Data
    {
        public string TestName { get; set; }
        public string ElapsedTime { get; set; }
        public string TestResult { get; set; }

    }

    public class TestCaseLoggingData
    {
        public object CreateDate { get; set; }
        public object TestName { get; set; }
        public object TestStatus { get; set; }
        //public object SuiteName { get; set; }
        public object Tester { get; set; }
        public object Identifier { get; set; }
        //public object Environment { get; set; }
        //public object Endpoint { get; set; }

        
        


    }
    public class ColorRenglon
    {
        public string Valor { get; set; }
        public string StatusColor { get; set; }
    }
    class DataGridModel
    {
        public DataGridModel(string testname, string result)
        {
            TestName = testname;
            Result = result;
        }
        public string TestName { get; set; }
        public string Result { get; set; }

    }

}
