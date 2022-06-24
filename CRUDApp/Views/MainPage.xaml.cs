using System.Windows.Controls;
using System;
using System.Data;
using System.Data.Common;
using System.Windows;
using System.Configuration;
using Microsoft.Data.SqlClient;
using CRUDApp;
using CRUDApp.ViewModels;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;


namespace CRUDApp.Views
{
    
    public partial class MainPage : Page
    {
        SqlConnection con = new SqlConnection();
        SqlCommand com = new SqlCommand();
        SqlDataReader dr;
        string user = CRUDApp.App.user;

        private static readonly Configuration Config = CRUDApp.App.LoadConfig();

        private static void ListServers(ListView myListView)
        {
            string pattern = @"[^\\]*(?=(\.mdf))";
            Regex r = new Regex(pattern);
            foreach (var cs in ConfigurationManager.ConnectionStrings.Cast<ConnectionStringSettings>())
            {
                if  (!cs.ConnectionString.Contains("aspnetdb")){
                    var dbName = r.Match(cs.ConnectionString);
                    ListViewItem item = new ListViewItem { Content = dbName.Value };
                
                    myListView.Items.Add(item);
                }
            }
            
        }

        public MainPage(MainViewModel viewModel)
        {
            AppDomain.CurrentDomain.SetData("DataDirectory", System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Databases"));
            InitializeComponent();
            DataContext = viewModel;

            var myListViewVar = this.FindName("listView") as ListView;
            ListServers(myListViewVar);

        }

        private void createDB_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            AppDomain.CurrentDomain.SetData("DataDirectory", System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Databases"));

            string db = nameText.Text;
            //string myConn = "Data Source=(LocalDB)\\MSSQLLocalDB;Integrated Security=True";
            string str = "CREATE DATABASE " + db + " ON PRIMARY " +
                             "(NAME = " + db + "_Data, " +
                             "FILENAME = '|DataDirectory|\\" + db + ".mdf', " +
                             "SIZE = 2MB, MAXSIZE = 10MB, FILEGROWTH = 10%)" +
                             "LOG ON (NAME ="+ db +"_Log, " +
                             "FILENAME = '|DataDirectory|\\" + db + "Log.ldf', " +
                             "SIZE = 1MB, " +
                             "MAXSIZE = 5MB, " +
                             "FILEGROWTH = 10%)"; 

            SqlConnection myConn = new SqlConnection("Data Source=(LocalDB)\\MSSQLLocalDB;Integrated Security=True;database=master");

            SqlCommand myCommand = new SqlCommand(str, myConn);
            try
            {
                myConn.Open();
                myCommand.ExecuteNonQuery();
                MessageBox.Show("DataBase is Created Successfully", "MyProgram", MessageBoxButton.OK, MessageBoxImage.Information);

                //if (this.user.Equals("admin"))
                //{
                //   SqlCommand myCommandUser = new SqlCommand(usrQuery, myConn);
                //}
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.ToString(), "MyProgram", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            finally
            {
                if (myConn.State == ConnectionState.Open)
                {
                    myConn.Close();
                }
            }

            var myListViewVar = this.FindName("listView") as ListView;
            ListServers(myListViewVar);

        }

        public static void UpdateAppSettings(string keyName, string keyValue)
        {
            XmlDocument doc = new XmlDocument();

            doc.Load(Config.FilePath);

            foreach (XmlElement elem in doc.DocumentElement)
            {
                if (elem.Name == "connectionStrings")
                {
                    foreach (XmlNode node in elem.ChildNodes)
                    {
                        if (node.Attributes[0].Value == keyName)
                        {
                            node.Attributes[1].Value = keyValue;
                        }
                    }
                }
            }
            doc.Save(Config.FilePath);
        }

    
    }
}
