using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace SQLAzureMW
{
    public static class ErrorHelper
    {
        public static void ShowException(IWin32Window owner, Exception ex)
        {
            ErrorForm cdForm = new ErrorForm(ex);
            DialogResult dr = cdForm.ShowDialog();

            //ExceptionMessageBox emb = new ExceptionMessageBox(ex);
            //emb.Show(owner);
        }
    }
}
