using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace SQLAzureMW
{
    public partial class ErrorForm : Form
    {
        public ErrorForm()
        {
            InitializeComponent();
        }
        public ErrorForm(Exception ex)
        {
            InitializeComponent();
            tbErrorMessage.Text = ex.ToString();
        }
    }
}
