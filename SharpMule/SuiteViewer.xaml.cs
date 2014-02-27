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

namespace TestManager
{
    /// <summary>
    /// Interaction logic for SuiteViewer.xaml
    /// </summary>
    public partial class SuiteViewer : Window
    {
        public SuiteViewer()
        {
            InitializeComponent();

            AddControls();

           
        }

        public void AddControls()
        {

            for (int i = 0; i < 10; i++)
            {

                Expander expSuite = NewExpander("Suite" + i);
                DataGrid dgTestView = NewDataGrid();
                expSuite.Content = dgTestView;
                stackPanel1.Children.Add(expSuite);
            }

        }
        public Expander NewExpander(string expName)
        {
            Expander exp = new Expander();
            exp.Width = this.Width;
            exp.IsExpanded = false;
            exp.Header = expName;

            return exp;
        }
        public DataGrid NewDataGrid()
        {
            DataGrid dg = new DataGrid();
            dg.Width = this.Width;
            dg.Height = 400;

            return dg;
        }
    }
}
