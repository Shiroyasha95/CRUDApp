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
using MahApps.Metro.Controls;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;


namespace CRUDApp.Views
{
    
    public partial class MainPage : Page
    {
        SqlConnection con = new SqlConnection();
        SqlCommand com = new SqlCommand();
        SqlDataReader dr;
        string user = CRUDApp.App.user;

        private static Configuration Config = CRUDApp.App.LoadConfig();
        public ObservableCollection<string> tableList = new ObservableCollection<string>();
        public string selectedDb { get; set; }
        public string selectedTb { get; set; }

        private static void ListServers(ListView myListView)
        {
            myListView.ItemsSource = null;
            myListView.Items.Clear();
            string pattern = @"[^\\]*(?=(\.mdf))";
            Regex r = new Regex(pattern);
            foreach (var cs in Config.ConnectionStrings.ConnectionStrings.Cast<ConnectionStringSettings>())
            {

                if (!cs.ConnectionString.Contains("aspnetdb"))
                {
                    if (cs.ConnectionString.Contains("WPF"))
                    {
                        if (CRUDApp.App.role == "admin")
                        {
                            var dbName = r.Match(cs.ConnectionString);
                            ListViewItem item = new ListViewItem { Content = dbName.Value };

                            myListView.Items.Add(item);
                        }
                    }
                    else
                    {
                        var dbName = r.Match(cs.ConnectionString);
                        ListViewItem item = new ListViewItem { Content = dbName.Value };

                        myListView.Items.Add(item);
                    }
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

        public MainPage()
        {
        }

        private void createDB_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            AppDomain.CurrentDomain.SetData("DataDirectory", System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Databases"));

            string db = nameText.Text;
            string dbComplete = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Databases") + "\\" + nameText.Text;
            //string myConn = "Data Source=(LocalDB)\\MSSQLLocalDB;Integrated Security=True";
            string str = "CREATE DATABASE " + db + " ON PRIMARY " +
                             "(NAME = " + db + "_Data, " +
                             "FILENAME = '" + dbComplete + ".mdf', " +
                             "SIZE = 2MB, MAXSIZE = 10MB, FILEGROWTH = 10%)" +
                             "LOG ON (NAME ="+ db +"_Log, " +
                             "FILENAME = '" + dbComplete + "Log.ldf', " +
                             "SIZE = 1MB, " +
                             "MAXSIZE = 5MB, " +
                             "FILEGROWTH = 10%)"; 

            SqlConnection myConn = new SqlConnection("Data Source=(LocalDB)\\MSSQLLocalDB;Integrated Security=True;database=master");

            SqlCommand myCommand = new SqlCommand(str, myConn);
            try
            {
                myConn.Open();
                myCommand.ExecuteNonQuery();
                MessageBox.Show("DataBase is Created Successfully", "DB Creation", MessageBoxButton.OK, MessageBoxImage.Information);

                if (String.Equals(this.user, "admin"))
                {
                    String usrQuery = "if not exists(select * from sys.server_principals where name = 'admin')" + " BEGIN CREATE LOGIN admin WITH PASSWORD = 'admin' END; " + "if not exists(select * from sys.database_principals where name = 'admin')" + " BEGIN CREATE USER admin FOR LOGIN admin END;" + "BEGIN exec sp_addrolemember 'db_owner', 'admin' END;";
                    SqlConnection myConn2 = new SqlConnection("Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename=" + dbComplete + ".mdf;Integrated Security=True" );
                    SqlCommand myCommandUser = new SqlCommand(usrQuery, myConn2);
                    myConn2.Open();
                    myCommandUser.ExecuteNonQuery();
                    myConn2.Close();
                    AddUpdateConnectionString(db);
                }

            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.ToString(), "DB Creation", MessageBoxButton.OK, MessageBoxImage.Information);
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

        private void AddUpdateConnectionString(string name)
        {
            bool isNew = false;
            string path = Config.FilePath;
            XmlDocument doc = new XmlDocument();
            doc.Load(path);
            XmlNodeList list = doc.DocumentElement.SelectNodes(string.Format("connectionStrings/add[@name='{0}']", name));
            XmlNode node;
            isNew = list.Count == 0;
            if (isNew)
            {
                node = doc.CreateNode(XmlNodeType.Element, "add", null);
                XmlAttribute attribute = doc.CreateAttribute("name");
                attribute.Value = name;
                node.Attributes.Append(attribute);

                attribute = doc.CreateAttribute("connectionString");
                attribute.Value = "";
                node.Attributes.Append(attribute);

                attribute = doc.CreateAttribute("providerName");
                attribute.Value = "Microsoft.Data.SqlClient";
                node.Attributes.Append(attribute);
            }
            else
            {
                node = list[0];
            }
            string conString = node.Attributes["connectionString"].Value;
            SqlConnectionStringBuilder conStringBuilder = new SqlConnectionStringBuilder(conString);
            conStringBuilder.InitialCatalog = "master";
            conStringBuilder.DataSource = "(LocalDB)\\MSSQLLocalDB";
            conStringBuilder.AttachDBFilename = "|DataDirectory|" + "\\" + name + ".mdf";
            conStringBuilder.IntegratedSecurity = true;
            node.Attributes["connectionString"].Value = conStringBuilder.ConnectionString;
            if (isNew)
            {
                doc.DocumentElement.SelectNodes("connectionStrings")[0].AppendChild(node);
            }
            doc.Save(path);
        }

        private void listTablePopulate(ObservableCollection<string> tableList, SqlDataReader dr)
        {
            while (dr.Read())
            {
                tableList.Add(dr.GetString(0));
            }
        }

        private void listView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            AppDomain.CurrentDomain.SetData("DataDirectory", System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Databases"));

            string db = ((System.Windows.Controls.ContentControl)this.listView.SelectedValue).Content.ToString();
            selectedDb = db;
            string dbComplete = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Databases") + "\\" + db;
            //string myConn = "Data Source=(LocalDB)\\MSSQLLocalDB;Integrated Security=True";
            string str = "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES;";

            SqlConnection myConn = new SqlConnection("Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename=|DataDirectory|\\" + db + ".mdf;Integrated Security=True;");

            SqlCommand myCommand = new SqlCommand(str, myConn);
            try
            {
                using (myConn)
                {
                    myConn.Open();
                    dr = myCommand.ExecuteReader();
                    this.listViewTables.ItemsSource = null;
                    this.tableList = new ObservableCollection<string>();
                    listTablePopulate(tableList, dr);
                    this.listViewTables.ItemsSource = tableList;
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Table Fetching Error", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            finally
            {
                if (myConn.State == ConnectionState.Open)
                {
                    myConn.Close();
                }
            }
        }

        private void btnInsertTable_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnDisplayTable_Click(object sender, RoutedEventArgs e)
        {
            string tb = this.listViewTables.SelectedValue.ToString();
            selectedTb = tb;

            ShellDialogWindowDisplay shellDialogWindowDisplay = new ShellDialogWindowDisplay(new ShellDialogViewModel(),selectedDb,selectedTb);
            shellDialogWindowDisplay.ShowDialog();

        }

        private void btnDelTable_Click(object sender, RoutedEventArgs e)
        {

        }

        //public static void UpdateAppSettings(string keyName, string keyValue)
        //{
        //    XmlDocument doc = new XmlDocument();

        //    doc.Load(Config.FilePath);

        //    foreach (XmlElement elem in doc.DocumentElement)
        //    {
        //        if (elem.Name == "connectionStrings")
        //        {
        //            foreach (XmlNode node in elem.ChildNodes)
        //            {
        //                node = 
        //            }
        //        }
        //    }
        //    doc.Save(Config.FilePath);
        //}


    }
}
