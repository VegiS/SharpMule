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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;
using TestManager; 

namespace SharpMule.DialogBoxes
{
    /// <summary>
    /// Interaction logic for NewEndpointEntryUserControl.xaml
    /// </summary>
    public partial class NewEndpointEntryUserControl : UserControl
    {
        public NewEndpointEntryUserControl()
        {
            InitializeComponent();
        }

        public bool AddEndPoint(string ep,string port)
        {
            XmlDocument xmlConfig = TestManager.MainWindow.Config;
            string currentEnvironment = xmlConfig["configuration"]["environments"]["defaultenvironment"].InnerXml;
            string endpoint = ep + ":" + port; 

            foreach (XmlNode node in xmlConfig.SelectNodes("/configuration/environments/environment"))
            {
                if (node["name"].InnerXml.Equals(currentEnvironment))
                {
                   
                    XmlNode epNode = xmlConfig.CreateElement("endpoint");
                    epNode.InnerText = endpoint;
                    node["endpointlist"].AppendChild(epNode);
                    xmlConfig.Save(TestManager.MainWindow.CONFIG_PATH);
                    
                    txtEndpoint.Text = String.Empty;
                    txtPort.Text = String.Empty;

                    TestManager.Shared.SharedTasks.LoadAllEnvironments(xmlConfig);
                    TestManager.Shared.SharedTasks.Environments.Environment env = null;
                    TestManager.Shared.SharedTasks.GetEnvironmentByName(currentEnvironment, out env); 


                    var epItem = TestManager.MainWindow.MainWindowInstance.mainGrid.FindName("cmb" + env.EnvName.ToUpper()) as ComboBox;
                    epItem.ItemsSource = env.EndPoints;
                    epItem.SelectedIndex = env.DefaultEndpointIndex; 
                    

                    return true; 
                }
            }

            return false; 
        }

        private void btnAddEP_Click(object sender, RoutedEventArgs e)
        {
            if (AddEndPoint(txtEndpoint.Text, txtPort.Text))
                MessageBox.Show("Endpoint has been added successfuly!", "Success", MessageBoxButton.OK);
            else
                MessageBox.Show("Endpoint has failed to be added!", "Fail", MessageBoxButton.OK);

            // Close the Dialog Window once the endpoint has been added
            MainWindow.dbCommitDialog.Close(); 
        }
    }
}
