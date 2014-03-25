using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApplication2
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //string encodedUserPass = "admin:ad1";
            //Encoding encoding = Encoding.GetEncoding("iso-8859-1");
            //string userPass = Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes(string.Format("{0}:{1}", "admin", "ad1")));

            //string input = @"some text {{text in double brackets}}{{another text}}...";
            //var matches = Regex.Matches(input, @"\{\{(.+?)\}\}")
            //                    .Cast<Match>()
            //                    .Select(m => m.Groups[1].Value)
            //                    .ToList();
            string input = @"Select getdate()";
            var ParamMatches = Regex.Matches(input, @"\[(.+?)\]")
                                .Cast<Match>()
                                .Select(m => m.Groups[1].Value)
                                .ToList();
            MessageBox.Show(ParamMatches.Count.ToString());
        }
    }
}
