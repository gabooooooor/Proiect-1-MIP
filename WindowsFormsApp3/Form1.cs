using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Win32;

namespace WindowsFormsApp3
{
    public partial class Form1 : Form
    {
        private const string HomeUrl = "https://www.bing.com";

        List<string> blockedSites = new List<string>();
        KeywordStore store = new KeywordStore();
        ToolStripButton btnDelete;

        public Form1()
        {
            Bongeszoverzio();
            InitializeComponent();
            SetupInterface();

            Trace.Listeners.Add(new TextWriterTraceListener(Path.Combine(Application.StartupPath, "log.txt")));
            Trace.AutoFlush = true;

            this.Load += new EventHandler(Form1_Load);
            webBrowser1.Navigated += new WebBrowserNavigatedEventHandler(webBrowser1_Navigated);
        }

        // ---------- interfata pentru utilizator ne-tehnic ----------

        private void SetupInterface()
        {
            this.Text = "Safe Browser";
            this.Size = new System.Drawing.Size(1000, 650);

            // butoane cu text clar
            toolStripButton4.DisplayStyle = ToolStripItemDisplayStyle.Text;
            toolStripButton4.Text = "Home";
            toolStripButton2.DisplayStyle = ToolStripItemDisplayStyle.Text;
            toolStripButton2.Text = "Back";
            toolStripButton3.DisplayStyle = ToolStripItemDisplayStyle.Text;
            toolStripButton3.Text = "Forward";
            toolStripButton1.DisplayStyle = ToolStripItemDisplayStyle.Text;
            toolStripButton1.Text = "Go";

            toolStripTextBox1.AutoSize = false;
            toolStripTextBox1.Width = 320;

            toolStripTextBox2.AutoSize = false;
            toolStripTextBox2.Width = 110;
            toolStripButton5.Text = "Add";

            toolStripComboBox1.DropDownStyle = ComboBoxStyle.DropDownList;

            // etichete
            toolStrip1.Items.Insert(toolStrip1.Items.IndexOf(toolStripTextBox2),
                new ToolStripSeparator());
            toolStrip1.Items.Insert(toolStrip1.Items.IndexOf(toolStripTextBox2),
                new ToolStripLabel("Block word:"));
            toolStrip1.Items.Insert(toolStrip1.Items.IndexOf(toolStripComboBox1),
                new ToolStripLabel("Blocked list:"));

            // buton stergere
            btnDelete = new ToolStripButton("Delete selected");
            btnDelete.DisplayStyle = ToolStripItemDisplayStyle.Text;
            btnDelete.Click += new EventHandler(btnDelete_Click);
            toolStrip1.Items.Add(btnDelete);

            // meniu: Connect/Disconnect nu se vad
            databaseToolStripMenuItem.DropDownItems.Remove(menuConnect);
            databaseToolStripMenuItem.DropDownItems.Remove(menuDisconnect);
            databaseToolStripMenuItem.Text = "Keywords";
            menuAddKeyword.Text = "Add keyword...";
            menuViewKeyword.Text = "Show all keywords";
        }

        private void Bongeszoverzio()
        {
            string exe = Process.GetCurrentProcess().ProcessName + ".exe";
            using (RegistryKey key = Registry.CurrentUser.CreateSubKey(
                @"Software\Microsoft\Internet Explorer\Main\FeatureControl\FEATURE_BROWSER_EMULATION"))
            {
                key.SetValue(exe, 11001, RegistryValueKind.DWord);
            }
        }

        // ---------- pornire: cuvintele cheie se incarca automat ----------

        private async void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                List<string> words = await Task.Run(() => store.GetAll());
                foreach (string w in words)
                {
                    if (!blockedSites.Any(x => x == w))
                    {
                        blockedSites.Add(w);
                        toolStripComboBox1.Items.Add(w);
                    }
                }
                LogAsync("Loaded keywords: " + words.Count);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not load keywords: " + GetInnerMessage(ex));
            }

            Navigalas(HomeUrl);
        }

        private string GetInnerMessage(Exception ex)
        {
            Exception inner = ex;
            while (inner.InnerException != null)
                inner = inner.InnerException;
            return inner.Message;
        }

        // ---------- navigare ----------

        private async void Navigalas(string url)
        {
            if (string.IsNullOrWhiteSpace(url)) return;

            if (!url.StartsWith("http://") && !url.StartsWith("https://"))
            {
                if (url.Contains(" ") || !url.Contains("."))
                {
                    url = "https://www.bing.com/search?q=" + Uri.EscapeDataString(url);
                }
                else
                {
                    url = "http://" + url;
                }
            }

            string lowerUrl = url.ToLower();
            List<string> snapshot = blockedSites.ToList();

            // filtrarea cu LINQ ruleaza pe un thread din Thread Pool
            string talalat = await Task.Run(() => snapshot.FirstOrDefault(w => lowerUrl.Contains(w)));

            if (talalat != null)
            {
                LogAsync("Blocked site attempt: " + url);
                MessageBox.Show("This site is blocked (keyword: " + talalat + ")");
            }
            else
            {
                LogAsync("Navigated to: " + url);
                webBrowser1.Navigate(url);
            }
        }

        // scrierea in fisier prin Trace, pe Thread Pool
        private Task LogAsync(string uzenet)
        {
            string sor = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " - " + uzenet;
            return Task.Run(() => Trace.WriteLine(sor));
        }

        private void webBrowser1_Navigating(object sender, WebBrowserNavigatingEventArgs e)
        {
            string url = e.Url.ToString().ToLower();
            string talalat = blockedSites.FirstOrDefault(w => url.Contains(w));

            if (talalat != null)
            {
                e.Cancel = true;
                LogAsync("Blocked site attempt (link): " + url);
                MessageBox.Show("This site is blocked (keyword: " + talalat + ")");
            }
        }

        private void webBrowser1_Navigated(object sender, WebBrowserNavigatedEventArgs e)
        {
            toolStripTextBox1.Text = e.Url.ToString();
        }

        // ---------- butoane de navigare ----------

        private void toolStripButton4_Click(object sender, EventArgs e)
        {
            LogAsync("Home button pushed");
            Navigalas(HomeUrl);
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            webBrowser1.GoBack();
            LogAsync("Back button pushed: " + webBrowser1.Url);
        }

        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            webBrowser1.GoForward();
            LogAsync("Forward button pushed: " + webBrowser1.Url);
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            Navigalas(toolStripTextBox1.Text);
        }

        private void toolStripTextBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                Navigalas(toolStripTextBox1.Text);
                e.SuppressKeyPress = true;
            }
        }

        // ---------- cuvinte cheie ----------

        // buton "Add" din bara de unelte
        private void toolStripButton5_Click(object sender, EventArgs e)
        {
            AddKeyword(toolStripTextBox2.Text);
            toolStripTextBox2.Clear();
        }

        // meniu Keywords -> Add keyword... (Form nou)
        private void menuAddKeyword_Click(object sender, EventArgs e)
        {
            KeywordForm f = new KeywordForm();
            if (f.ShowDialog() == DialogResult.OK)
            {
                AddKeyword(f.KeywordText);
            }
        }

        private async void AddKeyword(string text)
        {
            string word = text.Trim().ToLower();
            if (word == "")
            {
                MessageBox.Show("Please type a keyword.");
                return;
            }
            if (blockedSites.Any(w => w == word))
            {
                MessageBox.Show("This keyword is already in the list: " + word);
                return;
            }

            try
            {
                await Task.Run(() => store.Insert(word));
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not save keyword: " + GetInnerMessage(ex));
                return;
            }

            blockedSites.Add(word);
            toolStripComboBox1.Items.Add(word);
            LogAsync("New keyword: " + word);
        }

        private async void btnDelete_Click(object sender, EventArgs e)
        {
            string word = toolStripComboBox1.SelectedItem as string;
            if (word == null)
            {
                MessageBox.Show("Select a keyword from the list first.");
                return;
            }

            DialogResult r = MessageBox.Show("Delete keyword '" + word + "'?",
                "Confirm", MessageBoxButtons.YesNo);
            if (r != DialogResult.Yes) return;

            try
            {
                await Task.Run(() => store.Delete(word));
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not delete keyword: " + GetInnerMessage(ex));
                return;
            }

            blockedSites.Remove(word);
            toolStripComboBox1.Items.Remove(word);
            toolStripComboBox1.SelectedIndex = -1;
            toolStripComboBox1.Text = "";
            LogAsync("Deleted keyword: " + word);
        }

        private void menuViewKeyword_Click(object sender, EventArgs e)
        {
            if (blockedSites.Count == 0)
            {
                MessageBox.Show("No keywords yet.");
                return;
            }
            MessageBox.Show("Blocked keywords:\n" + string.Join("\n", blockedSites));
        }

        private void menuAbout_Click(object sender, EventArgs e)
        {
            MessageBox.Show(Application.ProductName + " " + Application.ProductVersion);
        }

        // ---------- metode pe care Designer-ul le cere, dar nu mai sunt folosite ----------

        private void menuConnect_Click(object sender, EventArgs e) { }
        private void menuDisconnect_Click(object sender, EventArgs e) { }
        private void toolStripTextBox1_Click(object sender, EventArgs e) { }
        private void toolStripTextBox2_Click(object sender, EventArgs e) { }
        private void toolStripComboBox1_Click(object sender, EventArgs e) { }
    }
}