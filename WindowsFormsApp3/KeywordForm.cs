using System;
using System.Drawing;
using System.Windows.Forms;

namespace WindowsFormsApp3
{
    public class KeywordForm : Form
    {
        private TextBox tbWord;
        private Button btnOk;
        private Button btnCancel;

        public string KeywordText
        {
            get { return tbWord.Text.Trim().ToLower(); }
        }

        public KeywordForm()
        {
            this.Text = "Add keyword";
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;
            this.ClientSize = new Size(320, 110);

            Label l = new Label();
            l.Text = "Keyword to block:";
            l.Left = 15; l.Top = 18; l.Width = 120;

            tbWord = new TextBox();
            tbWord.Left = 140; tbWord.Top = 15; tbWord.Width = 165;

            btnOk = new Button();
            btnOk.Text = "Add";
            btnOk.Left = 80; btnOk.Top = 60; btnOk.Width = 90;
            btnOk.Click += new EventHandler(btnOk_Click);

            btnCancel = new Button();
            btnCancel.Text = "Cancel";
            btnCancel.Left = 185; btnCancel.Top = 60; btnCancel.Width = 90;
            btnCancel.DialogResult = DialogResult.Cancel;

            this.Controls.Add(l);
            this.Controls.Add(tbWord);
            this.Controls.Add(btnOk);
            this.Controls.Add(btnCancel);
            this.AcceptButton = btnOk;
            this.CancelButton = btnCancel;
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            if (tbWord.Text.Trim().Length == 0)
            {
                MessageBox.Show("Please type a keyword.");
                return;
            }
            this.DialogResult = DialogResult.OK;
        }
    }
}