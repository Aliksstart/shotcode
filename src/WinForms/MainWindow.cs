using Core;
using Core.Crypto;
using System.Security.Cryptography;
using System.Text;

namespace WinForms
{
    public partial class MainWindow : Form
    {
        private enum Screen { Wizzard, Valut, AddInfo };
        private SCDB? _db;
        private VaultOrigin _origin;
        private const string scdbFiletr = "ShotCode vault (*.scdb)|*.scdb|All files (*.*)|*.*";
        private int _timeout = 60000;

        private void ShowScreen(Screen s)
        {
            CodeUpdateTimer.Enabled = false;
            switch (s)
            {
                case Screen.Wizzard:
                    _db?.Dispose();
                    _db = null;
                    break;
                case Screen.Valut:
                    SecretsListView.Items.Clear();
                    int skip_block = 0;
                    foreach (var e in _origin.GetNameServices())
                    {
                        var item = new ListViewItem(e);
                        try
                        {
                            string code = _origin.GetCodeString(e);
                            item.SubItems.Add(code);
                        }
                        catch (Exception ex)
                        {
                            skip_block++;
                            item.ToolTipText = ex.Message;
                            item.SubItems.Add("---");
                        }
                        item.SubItems.Add($"{_origin.GetPeriodOrCounter(e)} s");
                        SecretsListView.Items.Add(item);
                    }
                    if (skip_block > 0)
                    {
                        MessageBox.Show($"Skiped block: {skip_block}");
                    }
                    CodeUpdateTimer.Enabled = true;
                    break;
                case Screen.AddInfo:
                    break;
            }
            SetPanel(WizzardPanel, s == Screen.Wizzard);
            SetPanel(VaultPanel, s == Screen.Valut);
            SetPanel(AddInfoPanel, s == Screen.AddInfo);
        }
        private void SetPanel(Panel p, bool on)
        {
            p.Visible = p.Enabled = on;
            p.Dock = on ? DockStyle.Fill : DockStyle.None;
        }

        public MainWindow()
        {
            InitializeComponent();
            SecretsListView.View = View.Details;
            PeriodAddInfoNumericUpDown.Minimum = Core.Crypto.Constraints.Totp.MinPeriodSeconds;
            PeriodAddInfoNumericUpDown.Maximum = Core.Crypto.Constraints.Totp.MaxPeriodSeconds;
            DigitsAddInfoNumericUpDown.Minimum = Core.Crypto.Constraints.Totp.MinDigits;
            DigitsAddInfoNumericUpDown.Maximum = Core.Crypto.Constraints.Totp.MaxDigits;
            AlgorithmAddInfoComboBox.Items.AddRange(Enum.GetNames(typeof(Core.Crypto.AlgorithmType)));
            ShowScreen(Screen.Wizzard);
        }

        private bool OpenOrCreateDB(string path)
        {
            if (_db != null)
                return false;
            try
            {
                _db = new SCDB(path);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private bool TryOpenVault(string path, bool create)
        {
            if (!OpenOrCreateDB(path)) { MessageBox.Show("Can't open file"); return false; }
            byte[] pass = Encoding.UTF8.GetBytes(PasswordBox.Text);
            try
            {
                _origin = create
                    ? new VaultOrigin(_db, ref pass, KdfType.Argon2id, _timeout, OnClosedDb)
                    : new VaultOrigin(_db, ref pass, _timeout, OnClosedDb);
                if (create) _origin.Save();
                return true;
            }
            catch (VaultDecryptionException) { MessageBox.Show("The password is incorrect, or the file is corrupted."); }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
            finally { CryptographicOperations.ZeroMemory(pass); }
            _db?.Dispose(); _db = null;
            return false;
        }

        private void OnClosedDb()
        {

            if (WizzardPanel.InvokeRequired)
            {
                WizzardPanel.Invoke(() => { OnClosedDb(); });
            }
            else
            {
                MessageBox.Show("DB closed");
                ShowScreen(Screen.Wizzard);
            }
        }

        private void ExitButton_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void CreateButton_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog()
            {
                Filter = scdbFiletr
            };
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                string path = sfd.FileName;
                if (File.Exists(path))
                {
                    MessageBox.Show("File is exists");
                    return;
                }
                if (TryOpenVault(path, true))
                {
                    ShowScreen(Screen.Valut);
                }
            }
        }

        private void OpenButton_Click(object sender, EventArgs e)
        {
            if (_db == null)
            {
                OpenFileDialog ofd = new OpenFileDialog()
                {
                    Filter = scdbFiletr
                };
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    string path = ofd.FileName;
                    if (TryOpenVault(path, false))
                    {
                        ShowScreen(Screen.Valut);
                    }
                }
            }
        }

        private void AddedSecButton_Click(object sender, EventArgs e)
        {
            ShowScreen(Screen.AddInfo);
        }

        private void CancelAddInfoButton_Click(object sender, EventArgs e)
        {
            ShowScreen(Screen.Valut);
        }

        private void addedSecInfoButton_Click(object sender, EventArgs e)
        {
            //Added
            int sidx = AlgorithmAddInfoComboBox.SelectedIndex;
            if (sidx >= 0)
            {
                try
                {
                    string secret_s = CodeAddInfoTextBox.Text.Replace(" ", "").Replace("-", "").ToUpperInvariant();
                    Span<byte> secret = stackalloc byte[Core.Crypto.Base32.GetDecodeLength(secret_s)];
                    Core.Crypto.Base32.Decode(secret_s, secret);
                    Core.Crypto.AlgorithmType alg = Enum.Parse<AlgorithmType>(AlgorithmAddInfoComboBox.SelectedItem.ToString());
                    Block b = new Block(BlockTypes.TOTP, ((int)DigitsAddInfoNumericUpDown.Value), alg, ((ulong)PeriodAddInfoNumericUpDown.Value), ServiceNameTextBox.Text, secret);
                    _origin.AddBlock(b);
                    _origin.Save();
                    ShowScreen(Screen.Valut);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            else
            {
                MessageBox.Show("The algorithm for obtaining the hash is not specified.", "ShotCode", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SecretsListView_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && SecretsListView.SelectedItems.Count == 1)
            {
                Clipboard.SetText(SecretsListView.SelectedItems[0].SubItems[1].Text);
            }
        }

        private void CodeUpdateTimer_Tick(object sender, EventArgs e)
        {
            long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            foreach (ListViewItem it in SecretsListView.Items)
            {
                string name = it.Text;
                long period = 0;
                try
                {
                    period = checked((long)_origin.GetPeriodOrCounter(name));
                    if (period <= 0) continue;
                }
                catch (InvalidOperationException)
                {
                    CodeUpdateTimer.Enabled = false;
                    return;
                }
                int left = checked((int)(period - (now % period)));
                if (left < 5 || left > period - 5)
                {
                    try
                    {
                        it.SubItems[1].Text = _origin.GetCodeString(name);
                    }
                    catch
                    {
                        it.SubItems[1].Text = "---";
                    }
                }
                it.SubItems[2].Text = left + "s";
            }
        }
    }
}
