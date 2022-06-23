using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Microsoft.Data.SqlClient;
using System.Configuration;

namespace CRUDApp.Views
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class UserLogin : Window
    {
        string roleString;
        bool auth;
        SqlConnection con = new SqlConnection();
        SqlCommand com = new SqlCommand();
        SqlDataReader dr;
        public UserLogin()
        {
            AppDomain.CurrentDomain.SetData("DataDirectory", System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Databases"));
            InitializeComponent();
            con.ConnectionString = ConfigurationManager.ConnectionStrings["WPFConnectionString"].ConnectionString.ToString();
        
        }

        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            if (con.State == System.Data.ConnectionState.Open)
            {
                con.Close();
                
            }
            if (VerifyUser(txtUsername.Text, txtPassword.Password))
            {
                CRUDApp.App.role = roleString;
                CRUDApp.App.authenticated = auth;
                MessageBox.Show("Login Successfully", "Congrats", MessageBoxButton.OK, MessageBoxImage.Information);
                this.Hide();

            }
            else
            {
                MessageBox.Show("Username or password is incorrect", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }
        private bool VerifyUser(string username, string password)
        {
            con.Open();
            com.Connection = con;
            com.CommandText = "SELECT status,role from users where username='" + username + "'  COLLATE SQL_Latin1_General_CP1_CS_AS AND password='" + password + "' COLLATE SQL_Latin1_General_CP1_CS_AS";
            dr = com.ExecuteReader();
            if (dr.Read())
            {
                if (Convert.ToBoolean(dr["status"]) == true)
                {
                    roleString = dr["role"].ToString();
                    auth = true;
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
        
    }
}
