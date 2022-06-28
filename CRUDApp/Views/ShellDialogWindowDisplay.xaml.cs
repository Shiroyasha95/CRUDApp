using System.Windows.Controls;
using System.Configuration;
using System;
using System.Data;
using CRUDApp.Contracts.Views;
using CRUDApp.ViewModels;
using Microsoft.Data.SqlClient;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using MahApps.Metro.Controls;
using System.Windows;

namespace CRUDApp.Views
{
    public partial class ShellDialogWindowDisplay : MetroWindow, IShellDialogWindow
    {
        SqlConnection con = new SqlConnection();
        SqlCommand com = new SqlCommand();
        SqlDataReader dr;
        string user = CRUDApp.App.user;

        private static Configuration Config = CRUDApp.App.LoadConfig();

        public ObservableCollection<string> dbList = new ObservableCollection<string>();
        public ObservableCollection<string> tableList = new ObservableCollection<string>();

        public int selectedIndexDB = -1;

        public ShellDialogWindowDisplay(ShellDialogViewModel viewModel,string db,string tb)
        {
            InitializeComponent();
            viewModel.SetResult = OnSetResult;
            DataContext = viewModel;
            this.gridTableUpdater(db,tb);
        }

        public Frame GetDialogFrame()
            => dialogFrame;

        private void OnSetResult(bool? result)
        {
            DialogResult = result;
            Close();
        }

        private void gridTableUpdater(string db, string tb)
        {
            AppDomain.CurrentDomain.SetData("DataDirectory", System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Databases"));
            string dbComplete = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Databases") + "\\" + db;
            //string myConn = "Data Source=(LocalDB)\\MSSQLLocalDB;Integrated Security=True";
            string table = tb;
            string str = "SELECT * FROM " + table + ";";

            SqlConnection myConn = new SqlConnection("Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename=|DataDirectory|\\" + db + ".mdf;Integrated Security=True");

            SqlCommand myCommand = new SqlCommand(str, myConn);
            try
            {
                using (myConn)
                {
                    myConn.Open();
                    SqlDataAdapter sqlDataTables = new SqlDataAdapter(str, myConn);
                    DataTable dataTable = new DataTable();
                    sqlDataTables.Fill(dataTable);

                    this.selectedTable.DataContext = dataTable.DefaultView;
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Table Creation", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            finally
            {
                if (myConn.State == ConnectionState.Open)
                {
                    myConn.Close();
                }

            }
        }
    }
}
