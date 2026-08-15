namespace WinForms
{
    partial class MainWindow
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            WizzardPanel = new Panel();
            WizzardSplitContainer = new SplitContainer();
            PathToFileTextBox = new TextBox();
            CreateButton = new Button();
            PasswordBox = new TextBox();
            ExitButton = new Button();
            OpenButton = new Button();
            LogoPictureBox = new PictureBox();
            VaultPanel = new Panel();
            SecretsListView = new ListView();
            NameService = new ColumnHeader();
            CodeService = new ColumnHeader();
            LeftTime = new ColumnHeader();
            AddedPanel = new Panel();
            AddedSecButton = new Button();
            AddInfoPanel = new Panel();
            ServiceNameTextBox = new TextBox();
            ServiceNameLabel = new Label();
            CodeAddInfoTextBox = new TextBox();
            CodeAddInfoLabel = new Label();
            PeriodAddInfoNumericUpDown = new NumericUpDown();
            DigitsAddInfoNumericUpDown = new NumericUpDown();
            AlgorithmAddInfoComboBox = new ComboBox();
            PeriodAddInfoLabel = new Label();
            DigitsAddInfoLabel = new Label();
            AlgorithmAddInfoLabel = new Label();
            AddInfoActionPanel = new Panel();
            CancelAddInfoButton = new Button();
            addedSecInfoButton = new Button();
            WizzardPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)WizzardSplitContainer).BeginInit();
            WizzardSplitContainer.Panel1.SuspendLayout();
            WizzardSplitContainer.Panel2.SuspendLayout();
            WizzardSplitContainer.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)LogoPictureBox).BeginInit();
            VaultPanel.SuspendLayout();
            AddedPanel.SuspendLayout();
            AddInfoPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)PeriodAddInfoNumericUpDown).BeginInit();
            ((System.ComponentModel.ISupportInitialize)DigitsAddInfoNumericUpDown).BeginInit();
            AddInfoActionPanel.SuspendLayout();
            SuspendLayout();
            // 
            // WizzardPanel
            // 
            WizzardPanel.Controls.Add(WizzardSplitContainer);
            WizzardPanel.Controls.Add(LogoPictureBox);
            WizzardPanel.Enabled = false;
            WizzardPanel.Location = new Point(42, 12);
            WizzardPanel.Name = "WizzardPanel";
            WizzardPanel.Size = new Size(584, 341);
            WizzardPanel.TabIndex = 0;
            WizzardPanel.Visible = false;
            // 
            // WizzardSplitContainer
            // 
            WizzardSplitContainer.Dock = DockStyle.Fill;
            WizzardSplitContainer.Location = new Point(0, 201);
            WizzardSplitContainer.Name = "WizzardSplitContainer";
            WizzardSplitContainer.Orientation = Orientation.Horizontal;
            // 
            // WizzardSplitContainer.Panel1
            // 
            WizzardSplitContainer.Panel1.Controls.Add(PathToFileTextBox);
            WizzardSplitContainer.Panel1.Controls.Add(CreateButton);
            WizzardSplitContainer.Panel1.Controls.Add(PasswordBox);
            // 
            // WizzardSplitContainer.Panel2
            // 
            WizzardSplitContainer.Panel2.Controls.Add(ExitButton);
            WizzardSplitContainer.Panel2.Controls.Add(OpenButton);
            WizzardSplitContainer.Size = new Size(584, 140);
            WizzardSplitContainer.SplitterDistance = 64;
            WizzardSplitContainer.TabIndex = 1;
            // 
            // PathToFileTextBox
            // 
            PathToFileTextBox.Location = new Point(93, 36);
            PathToFileTextBox.Name = "PathToFileTextBox";
            PathToFileTextBox.Size = new Size(442, 23);
            PathToFileTextBox.TabIndex = 2;
            PathToFileTextBox.Text = "Path to file";
            // 
            // CreateButton
            // 
            CreateButton.Location = new Point(12, 35);
            CreateButton.Name = "CreateButton";
            CreateButton.Size = new Size(75, 23);
            CreateButton.TabIndex = 1;
            CreateButton.Text = "Create";
            CreateButton.UseVisualStyleBackColor = true;
            CreateButton.Click += CreateButton_Click;
            // 
            // PasswordBox
            // 
            PasswordBox.Location = new Point(12, 6);
            PasswordBox.Name = "PasswordBox";
            PasswordBox.Size = new Size(560, 23);
            PasswordBox.TabIndex = 0;
            PasswordBox.Text = "Password";
            // 
            // ExitButton
            // 
            ExitButton.Location = new Point(399, 23);
            ExitButton.Name = "ExitButton";
            ExitButton.Size = new Size(75, 23);
            ExitButton.TabIndex = 1;
            ExitButton.Text = "Exit";
            ExitButton.UseVisualStyleBackColor = true;
            ExitButton.Click += ExitButton_Click;
            // 
            // OpenButton
            // 
            OpenButton.Location = new Point(107, 23);
            OpenButton.Name = "OpenButton";
            OpenButton.Size = new Size(75, 23);
            OpenButton.TabIndex = 0;
            OpenButton.Text = "Open";
            OpenButton.UseVisualStyleBackColor = true;
            OpenButton.Click += OpenButton_Click;
            // 
            // LogoPictureBox
            // 
            LogoPictureBox.Dock = DockStyle.Top;
            LogoPictureBox.Image = Properties.Resources.shx;
            LogoPictureBox.Location = new Point(0, 0);
            LogoPictureBox.Name = "LogoPictureBox";
            LogoPictureBox.Size = new Size(584, 201);
            LogoPictureBox.SizeMode = PictureBoxSizeMode.Zoom;
            LogoPictureBox.TabIndex = 0;
            LogoPictureBox.TabStop = false;
            // 
            // VaultPanel
            // 
            VaultPanel.Controls.Add(SecretsListView);
            VaultPanel.Controls.Add(AddedPanel);
            VaultPanel.Location = new Point(276, 23);
            VaultPanel.Name = "VaultPanel";
            VaultPanel.Size = new Size(293, 306);
            VaultPanel.TabIndex = 1;
            // 
            // SecretsListView
            // 
            SecretsListView.Columns.AddRange(new ColumnHeader[] { NameService, CodeService, LeftTime });
            SecretsListView.Dock = DockStyle.Fill;
            SecretsListView.Location = new Point(0, 0);
            SecretsListView.Name = "SecretsListView";
            SecretsListView.ShowItemToolTips = true;
            SecretsListView.Size = new Size(293, 274);
            SecretsListView.TabIndex = 1;
            SecretsListView.UseCompatibleStateImageBehavior = false;
            SecretsListView.View = View.Details;
            // 
            // NameService
            // 
            NameService.Text = "Service";
            // 
            // CodeService
            // 
            CodeService.Text = "Code";
            // 
            // LeftTime
            // 
            LeftTime.Text = "Left";
            // 
            // AddedPanel
            // 
            AddedPanel.Controls.Add(AddedSecButton);
            AddedPanel.Dock = DockStyle.Bottom;
            AddedPanel.Location = new Point(0, 274);
            AddedPanel.Name = "AddedPanel";
            AddedPanel.Size = new Size(293, 32);
            AddedPanel.TabIndex = 0;
            // 
            // AddedSecButton
            // 
            AddedSecButton.Location = new Point(63, 9);
            AddedSecButton.Name = "AddedSecButton";
            AddedSecButton.Size = new Size(75, 23);
            AddedSecButton.TabIndex = 0;
            AddedSecButton.Text = "Added";
            AddedSecButton.UseVisualStyleBackColor = true;
            AddedSecButton.Click += AddedSecButton_Click;
            // 
            // AddInfoPanel
            // 
            AddInfoPanel.Controls.Add(ServiceNameTextBox);
            AddInfoPanel.Controls.Add(ServiceNameLabel);
            AddInfoPanel.Controls.Add(CodeAddInfoTextBox);
            AddInfoPanel.Controls.Add(CodeAddInfoLabel);
            AddInfoPanel.Controls.Add(PeriodAddInfoNumericUpDown);
            AddInfoPanel.Controls.Add(DigitsAddInfoNumericUpDown);
            AddInfoPanel.Controls.Add(AlgorithmAddInfoComboBox);
            AddInfoPanel.Controls.Add(PeriodAddInfoLabel);
            AddInfoPanel.Controls.Add(DigitsAddInfoLabel);
            AddInfoPanel.Controls.Add(AlgorithmAddInfoLabel);
            AddInfoPanel.Controls.Add(AddInfoActionPanel);
            AddInfoPanel.Location = new Point(12, 113);
            AddInfoPanel.Name = "AddInfoPanel";
            AddInfoPanel.Size = new Size(370, 281);
            AddInfoPanel.TabIndex = 2;
            // 
            // ServiceNameTextBox
            // 
            ServiceNameTextBox.Location = new Point(73, 9);
            ServiceNameTextBox.Name = "ServiceNameTextBox";
            ServiceNameTextBox.Size = new Size(120, 23);
            ServiceNameTextBox.TabIndex = 10;
            // 
            // ServiceNameLabel
            // 
            ServiceNameLabel.AutoSize = true;
            ServiceNameLabel.Location = new Point(3, 12);
            ServiceNameLabel.Name = "ServiceNameLabel";
            ServiceNameLabel.Size = new Size(47, 15);
            ServiceNameLabel.TabIndex = 9;
            ServiceNameLabel.Text = "Service:";
            // 
            // CodeAddInfoTextBox
            // 
            CodeAddInfoTextBox.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            CodeAddInfoTextBox.Location = new Point(73, 125);
            CodeAddInfoTextBox.Multiline = true;
            CodeAddInfoTextBox.Name = "CodeAddInfoTextBox";
            CodeAddInfoTextBox.Size = new Size(294, 118);
            CodeAddInfoTextBox.TabIndex = 8;
            // 
            // CodeAddInfoLabel
            // 
            CodeAddInfoLabel.AutoSize = true;
            CodeAddInfoLabel.Location = new Point(3, 128);
            CodeAddInfoLabel.Name = "CodeAddInfoLabel";
            CodeAddInfoLabel.Size = new Size(38, 15);
            CodeAddInfoLabel.TabIndex = 7;
            CodeAddInfoLabel.Text = "Code:";
            // 
            // PeriodAddInfoNumericUpDown
            // 
            PeriodAddInfoNumericUpDown.Location = new Point(72, 96);
            PeriodAddInfoNumericUpDown.Name = "PeriodAddInfoNumericUpDown";
            PeriodAddInfoNumericUpDown.Size = new Size(120, 23);
            PeriodAddInfoNumericUpDown.TabIndex = 6;
            // 
            // DigitsAddInfoNumericUpDown
            // 
            DigitsAddInfoNumericUpDown.Location = new Point(72, 67);
            DigitsAddInfoNumericUpDown.Name = "DigitsAddInfoNumericUpDown";
            DigitsAddInfoNumericUpDown.Size = new Size(121, 23);
            DigitsAddInfoNumericUpDown.TabIndex = 5;
            // 
            // AlgorithmAddInfoComboBox
            // 
            AlgorithmAddInfoComboBox.FormattingEnabled = true;
            AlgorithmAddInfoComboBox.Location = new Point(73, 38);
            AlgorithmAddInfoComboBox.Name = "AlgorithmAddInfoComboBox";
            AlgorithmAddInfoComboBox.Size = new Size(121, 23);
            AlgorithmAddInfoComboBox.TabIndex = 4;
            // 
            // PeriodAddInfoLabel
            // 
            PeriodAddInfoLabel.AutoSize = true;
            PeriodAddInfoLabel.Location = new Point(3, 98);
            PeriodAddInfoLabel.Name = "PeriodAddInfoLabel";
            PeriodAddInfoLabel.Size = new Size(44, 15);
            PeriodAddInfoLabel.TabIndex = 3;
            PeriodAddInfoLabel.Text = "Period:";
            // 
            // DigitsAddInfoLabel
            // 
            DigitsAddInfoLabel.AutoSize = true;
            DigitsAddInfoLabel.Location = new Point(3, 69);
            DigitsAddInfoLabel.Name = "DigitsAddInfoLabel";
            DigitsAddInfoLabel.Size = new Size(40, 15);
            DigitsAddInfoLabel.TabIndex = 2;
            DigitsAddInfoLabel.Text = "Digits:";
            // 
            // AlgorithmAddInfoLabel
            // 
            AlgorithmAddInfoLabel.AutoSize = true;
            AlgorithmAddInfoLabel.Location = new Point(3, 41);
            AlgorithmAddInfoLabel.Name = "AlgorithmAddInfoLabel";
            AlgorithmAddInfoLabel.Size = new Size(64, 15);
            AlgorithmAddInfoLabel.TabIndex = 1;
            AlgorithmAddInfoLabel.Text = "Algorithm:";
            // 
            // AddInfoActionPanel
            // 
            AddInfoActionPanel.Controls.Add(CancelAddInfoButton);
            AddInfoActionPanel.Controls.Add(addedSecInfoButton);
            AddInfoActionPanel.Dock = DockStyle.Bottom;
            AddInfoActionPanel.Location = new Point(0, 249);
            AddInfoActionPanel.Name = "AddInfoActionPanel";
            AddInfoActionPanel.Size = new Size(370, 32);
            AddInfoActionPanel.TabIndex = 0;
            // 
            // CancelAddInfoButton
            // 
            CancelAddInfoButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            CancelAddInfoButton.Location = new Point(292, 6);
            CancelAddInfoButton.Name = "CancelAddInfoButton";
            CancelAddInfoButton.Size = new Size(75, 23);
            CancelAddInfoButton.TabIndex = 1;
            CancelAddInfoButton.Text = "Cancel";
            CancelAddInfoButton.UseVisualStyleBackColor = true;
            CancelAddInfoButton.Click += CancelAddInfoButton_Click;
            // 
            // addedSecInfoButton
            // 
            addedSecInfoButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            addedSecInfoButton.Location = new Point(3, 6);
            addedSecInfoButton.Name = "addedSecInfoButton";
            addedSecInfoButton.Size = new Size(75, 23);
            addedSecInfoButton.TabIndex = 0;
            addedSecInfoButton.Text = "Added";
            addedSecInfoButton.UseVisualStyleBackColor = true;
            addedSecInfoButton.Click += addedSecInfoButton_Click;
            // 
            // MainWindow
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(584, 341);
            Controls.Add(AddInfoPanel);
            Controls.Add(VaultPanel);
            Controls.Add(WizzardPanel);
            FormBorderStyle = FormBorderStyle.None;
            Name = "MainWindow";
            Text = "ShotCode";
            WizzardPanel.ResumeLayout(false);
            WizzardSplitContainer.Panel1.ResumeLayout(false);
            WizzardSplitContainer.Panel1.PerformLayout();
            WizzardSplitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)WizzardSplitContainer).EndInit();
            WizzardSplitContainer.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)LogoPictureBox).EndInit();
            VaultPanel.ResumeLayout(false);
            AddedPanel.ResumeLayout(false);
            AddInfoPanel.ResumeLayout(false);
            AddInfoPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)PeriodAddInfoNumericUpDown).EndInit();
            ((System.ComponentModel.ISupportInitialize)DigitsAddInfoNumericUpDown).EndInit();
            AddInfoActionPanel.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private Panel WizzardPanel;
        private PictureBox LogoPictureBox;
        private SplitContainer WizzardSplitContainer;
        private TextBox PathToFileTextBox;
        private Button CreateButton;
        private TextBox PasswordBox;
        private Button ExitButton;
        private Button OpenButton;
        private Panel VaultPanel;
        private Panel AddedPanel;
        private Button AddedSecButton;
        private ListView SecretsListView;
        private ColumnHeader NameService;
        private Panel AddInfoPanel;
        private Panel AddInfoActionPanel;
        private Button CancelAddInfoButton;
        private Button addedSecInfoButton;
        private Label AlgorithmAddInfoLabel;
        private Label DigitsAddInfoLabel;
        private Label PeriodAddInfoLabel;
        private NumericUpDown DigitsAddInfoNumericUpDown;
        private ComboBox AlgorithmAddInfoComboBox;
        private Label CodeAddInfoLabel;
        private NumericUpDown PeriodAddInfoNumericUpDown;
        private TextBox CodeAddInfoTextBox;
        private Label ServiceNameLabel;
        private TextBox ServiceNameTextBox;
        private ColumnHeader CodeService;
        private ColumnHeader LeftTime;
    }
}
