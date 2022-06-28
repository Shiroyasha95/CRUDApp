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
using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace CRUDApp.Views
{
    public partial class DataGridPage : Page
    {
        SqlConnection con = new SqlConnection();
        SqlCommand com = new SqlCommand();
        SqlDataReader dr;
        string user = CRUDApp.App.user;

        private static Configuration Config = CRUDApp.App.LoadConfig();

        public ObservableCollection<string> dbList = new ObservableCollection<string>();
        public ObservableCollection<string> tableList = new ObservableCollection<string>();

        public int selectedIndexDB = -1;

        public DataGridPage(DataGridViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
            cbxDBPopulate(dbList);
            this.cbxDatabase.ItemsSource = dbList;

        }

        private static void cbxDBPopulate(ObservableCollection<string> myListView)
        {
            
            string pattern = @"[^\\]*(?=(\.mdf))";
            Regex r = new Regex(pattern);
            foreach (var cs in Config.ConnectionStrings.ConnectionStrings.Cast<ConnectionStringSettings>())
            {
                if (!cs.ConnectionString.Contains("aspnetdb"))
                {
                    var dbName = r.Match(cs.ConnectionString);
                    myListView.Add(dbName.Value);
                }
            }

        }

        private void cbxTablePopulate(ObservableCollection<string> tableList, SqlDataReader dr)
        {
            while (dr.Read())
            {
                tableList.Add(dr.GetString(0));
            }
        }

        private void cbxDatabase_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            AppDomain.CurrentDomain.SetData("DataDirectory", System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Databases"));

            string db = this.cbxDatabase.SelectedValue.ToString();
            string dbComplete = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Databases") + "\\" + db;
            //string myConn = "Data Source=(LocalDB)\\MSSQLLocalDB;Integrated Security=True";
            string str = "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES;";


            SqlConnection myConn = new SqlConnection("Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename=|DataDirectory|\\" + db + ".mdf;Integrated Security=True");

            SqlCommand myCommand = new SqlCommand(str, myConn);
            try
            {
                using (myConn) {
                    myConn.Open();
                    dr = myCommand.ExecuteReader();
                    cbxTablePopulate(tableList,dr); ;
                    this.cbxTables.ItemsSource = tableList;
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
        }

       
        private void gridTableUpdater()
        {
            AppDomain.CurrentDomain.SetData("DataDirectory", System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Databases"));

            string db = this.cbxDatabase.SelectedValue.ToString();
            string dbComplete = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Databases") + "\\" + db;
            //string myConn = "Data Source=(LocalDB)\\MSSQLLocalDB;Integrated Security=True";
            string table = this.cbxTables.SelectedValue.ToString();
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
                MessageBox.Show(ex.ToString(), "DB Creation", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            finally
            {
                if (myConn.State == ConnectionState.Open)
                {
                    myConn.Close();
                }

            }
        }

        private void cbxTables_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            gridTableUpdater();
        }

        private void btnInsert_Click(object sender, RoutedEventArgs e)
        {
            StringCollection columns = new StringCollection();
            StringCollection values = new StringCollection();

            foreach (DataGridColumn col in this.selectedTable.Columns)
            {
                columns.Add(col.Header.ToString());
            }

            var row = this.selectedTable.SelectedItem as DataRowView;
            var cells = this.selectedTable.SelectedItems;
            if (row != null)
            {
                foreach (DataRowView rows in cells)
                {
                    foreach (object r in rows.Row.ItemArray)
                    {
                        values.Add(r.ToString());
                    }
                    valuesInsert(columns,values);
                    values = new StringCollection();
                }
                this.gridTableUpdater();
            }
        }

        private void valuesInsert(StringCollection columns, StringCollection values)
        {
            string db = this.cbxDatabase.SelectedValue.ToString();
            string dbComplete = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Databases") + "\\" + db;
            //string myConn = "Data Source=(LocalDB)\\MSSQLLocalDB;Integrated Security=True";
            string table = this.cbxTables.SelectedValue.ToString();

            SqlConnection myConn = new SqlConnection("Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename=|DataDirectory|\\" + db + ".mdf;Integrated Security=True");

            var textBuild = "insert into " + table + " (";
            StringBuilder sb = new StringBuilder();
            sb.Append(textBuild);

            foreach (var column in columns)
            {
                sb.Append(column + ",");
            }
            sb.Remove(sb.Length - 1, 1);
            sb.Append(")");
            sb.Append(" values (");

            foreach (var column in columns)
            {
                sb.Append(@"@" + column + ",");
            }
            sb.Remove(sb.Length - 1, 1);
            sb.Append(")");

            var command = new SqlCommand(sb.ToString(), myConn);
            int i = 0;
            foreach (var val in columns)
            {
                command.Parameters.AddWithValue(@"@" + val, values[i]);
                i++;
            }

            try
            {
                using (myConn)
                {
                    myConn.Open();
                    command.ExecuteNonQuery();
                    MessageBox.Show("Row Inserted Successfully", "Insert", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Insert", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            finally
            {
                if (myConn.State == ConnectionState.Open)
                {
                    myConn.Close();
                }
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            string db = this.cbxDatabase.SelectedValue.ToString();
            string dbComplete = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Databases") + "\\" + db;
            //string myConn = "Data Source=(LocalDB)\\MSSQLLocalDB;Integrated Security=True";
            string table = this.cbxTables.SelectedValue.ToString();

            SqlConnection myConn = new SqlConnection("Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename=|DataDirectory|\\" + db + ".mdf;Integrated Security=True");

            var str = "select C.COLUMN_NAME FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS as T"
                       + " JOIN INFORMATION_SCHEMA.CONSTRAINT_COLUMN_USAGE as C "
                       + "ON C.CONSTRAINT_NAME = T.CONSTRAINT_NAME WHERE T.CONSTRAINT_TYPE = 'PRIMARY KEY' AND "
                       + "C.TABLE_NAME = '" + table + "';";

            SqlCommand myCommand = new SqlCommand(str, myConn);
            var idColumn = "";
            try
            {
                using (myConn)
                {
                    myConn.Open();
                    dr = myCommand.ExecuteReader();

                    while (dr.Read())
                    {
                        idColumn = dr.GetString(0);
                    }
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Delete Error", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            finally
            {
                if (myConn.State == ConnectionState.Open)
                {
                    myConn.Close();
                }
            }

            var row = this.selectedTable.SelectedItem;
            var cells = this.selectedTable.SelectedItems;
            ArrayList deleteValue = new ArrayList();
            if (row != null)
            {
                foreach (DataRowView rows in cells)
                {
                    deleteValue.Add(rows.Row[idColumn]);
                }
            }
            valuesDeleter(idColumn,deleteValue);
        }

        private void valuesDeleter(string idColumn, ArrayList deleteValue)
        {
            string db = this.cbxDatabase.SelectedValue.ToString();
            string dbComplete = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Databases") + "\\" + db;
            //string myConn = "Data Source=(LocalDB)\\MSSQLLocalDB;Integrated Security=True";
            string table = this.cbxTables.SelectedValue.ToString();

            SqlConnection myConn = new SqlConnection("Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename=|DataDirectory|\\" + db + ".mdf;Integrated Security=True");

            var textBuild = "DELETE FROM " + table + " WHERE " + idColumn + " in (" ;
            StringBuilder sb = new StringBuilder();
            sb.Append(textBuild);

            foreach (var val in deleteValue)
            {
                sb.Append("'"+ val + "',");
            }
            sb.Remove(sb.Length - 1, 1);
            sb.Append(")");

            SqlCommand myCommand = new SqlCommand(sb.ToString(), myConn);

            try
            {
                using (myConn)
                {
                    myConn.Open();
                    myCommand.ExecuteNonQuery();
                    MessageBox.Show("Rows Deleted Successfully", "Delete", MessageBoxButton.OK, MessageBoxImage.Information);
                    this.gridTableUpdater();
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Delete", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            finally
            {
                if (myConn.State == ConnectionState.Open)
                {
                    myConn.Close();
                }
            }
        }

        private void btnUpdate_Click(object sender, RoutedEventArgs e)
        {

        }

        
    }
}
