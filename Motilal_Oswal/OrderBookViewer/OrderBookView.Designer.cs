namespace Order_Book_Viewer
{
    partial class OrderBookViewer
    {
        private System.Windows.Forms.DataGridView bidsGridView;
        private System.Windows.Forms.DataGridView asksGridView;
        private System.Windows.Forms.TextBox logTextBox;
        private System.Windows.Forms.Button _pauseResumeButton;
        private System.Windows.Forms.TrackBar _speedSlider;
        private System.Windows.Forms.Label _speedLabel;
        private System.Windows.Forms.ComboBox _symbolDropdown;
        private System.Windows.Forms.Label _symbolLabel;
        private System.Windows.Forms.FlowLayoutPanel _controlsPanel;

        private void InitializeComponent()
        {
            // Initializing controls
            this.bidsGridView = new System.Windows.Forms.DataGridView();
            this.asksGridView = new System.Windows.Forms.DataGridView();
            this.logTextBox = new System.Windows.Forms.TextBox();
            this._pauseResumeButton = new System.Windows.Forms.Button();
            this._speedSlider = new System.Windows.Forms.TrackBar();
            this._speedLabel = new System.Windows.Forms.Label();
            this._symbolDropdown = new System.Windows.Forms.ComboBox();
            this._symbolLabel = new System.Windows.Forms.Label();
            this._controlsPanel = new System.Windows.Forms.FlowLayoutPanel();

            // Form properties
            this.Text = "Real-Time Order Book Viewer";
            this.Size = new System.Drawing.Size(800, 600); // Set a larger default size
            this.Padding = new System.Windows.Forms.Padding(10);
            this.MinimumSize = new System.Drawing.Size(660, 650); // Set a minimum size

            // Control panel for interactive elements
            this._controlsPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this._controlsPanel.Height = 50;
            this._controlsPanel.FlowDirection = System.Windows.Forms.FlowDirection.LeftToRight;
            this._controlsPanel.Padding = new System.Windows.Forms.Padding(5);
            this._controlsPanel.WrapContents = false;
            this._controlsPanel.Controls.AddRange(new System.Windows.Forms.Control[]
            {
                this._speedLabel,
                this._speedSlider,
                this._pauseResumeButton,
                this._symbolLabel,
                this._symbolDropdown
            });

            // Control properties using object initializers
            this._speedLabel.Text = "Speed:";
            this._speedLabel.AutoSize = true;
            this._speedLabel.Margin = new System.Windows.Forms.Padding(5);

            this._speedSlider.Minimum = 1;
            this._speedSlider.Maximum = 100;
            this._speedSlider.Value = 10;
            this._speedSlider.Width = 150;
            this._speedSlider.Margin = new System.Windows.Forms.Padding(5);
            this._speedSlider.Scroll += new System.EventHandler(this._speedSlider_Scroll);

            this._pauseResumeButton.Text = "Pause";
            this._pauseResumeButton.Margin = new System.Windows.Forms.Padding(5);
            this._pauseResumeButton.Height = 30;
            this._pauseResumeButton.Click += new System.EventHandler(this._pauseResumeButton_Click);

            this._symbolLabel.Text = "Symbol:";
            this._symbolLabel.AutoSize = true;
            this._symbolLabel.Margin = new System.Windows.Forms.Padding(5);

            this._symbolDropdown.Items.AddRange(new object[] { "BTC/INR", "ETH/INR" });
            this._symbolDropdown.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._symbolDropdown.SelectedIndex = 0;
            this._symbolDropdown.Margin = new System.Windows.Forms.Padding(5);
            this._symbolDropdown.SelectedIndexChanged += new System.EventHandler(this._symbolDropdown_SelectedIndexChanged);

            // Bids GridView
            this.bidsGridView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.bidsGridView.BackgroundColor = System.Drawing.Color.White;
            this.bidsGridView.ReadOnly = true;
            this.bidsGridView.AllowUserToAddRows = false;
            this.bidsGridView.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.bidsGridView.VirtualMode = true;
            this.bidsGridView.ColumnHeadersVisible = true;
            this.bidsGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[]
            {
                new System.Windows.Forms.DataGridViewTextBoxColumn { Name = "Price", HeaderText = "Price", AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill },
                new System.Windows.Forms.DataGridViewTextBoxColumn { Name = "Quantity", HeaderText = "Quantity", AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill }
            });
            this.bidsGridView.CellValueNeeded += this.BidsGridView_CellValueNeeded;

            // Asks GridView
            this.asksGridView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.asksGridView.BackgroundColor = System.Drawing.Color.White;
            this.asksGridView.ReadOnly = true;
            this.asksGridView.AllowUserToAddRows = false;
            this.asksGridView.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.asksGridView.VirtualMode = true;
            this.asksGridView.ColumnHeadersVisible = true;
            this.asksGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[]
            {
                new System.Windows.Forms.DataGridViewTextBoxColumn { Name = "Price", HeaderText = "Price", AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill },
                new System.Windows.Forms.DataGridViewTextBoxColumn { Name = "Quantity", HeaderText = "Quantity", AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill }
            });
            this.asksGridView.CellValueNeeded += this.AsksGridView_CellValueNeeded;

            // Log Textbox
            this.logTextBox.Multiline = true;
            this.logTextBox.ReadOnly = true;
            this.logTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.logTextBox.Dock = System.Windows.Forms.DockStyle.Fill;

            // Main layout using TableLayoutPanel
            var gridPanel = new System.Windows.Forms.TableLayoutPanel
            {
                Dock = System.Windows.Forms.DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1
            };
            gridPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            gridPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            gridPanel.Controls.Add(this.bidsGridView, 0, 0);
            gridPanel.Controls.Add(this.asksGridView, 1, 0);

            var mainPanel = new System.Windows.Forms.TableLayoutPanel
            {
                Dock = System.Windows.Forms.DockStyle.Fill,
                RowCount = 3,
                ColumnStyles = { new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100) },
                RowStyles =
                {
                    new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 50),
                    new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 80),
                    new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 20)
                }
            };
            mainPanel.Controls.Add(this._controlsPanel, 0, 0);
            mainPanel.Controls.Add(gridPanel, 0, 1);
            mainPanel.Controls.Add(this.logTextBox, 0, 2);

            this.Controls.Add(mainPanel);
            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}