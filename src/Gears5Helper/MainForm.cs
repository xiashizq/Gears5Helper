namespace Gears5Helper;

public sealed class MainForm : Form
{
    private readonly DataGridView _voices = new();
    private readonly ComboBox _interface = new();
    private readonly ListView _targets = new();
    private readonly TextBox _log = new();
    private readonly Label _titleLabel = new();
    private readonly Label _gameUiLabel = new();
    private readonly Label _appLangLabel = new();
    private readonly ComboBox _uiLanguage = new();
    private readonly Button _applyButton;
    private readonly Button _restoreButton;
    private readonly Button _readButton;
    private readonly Button _scanButton;
    private readonly Button _openButton;
    private readonly Button _addButton;
    private bool _syncing;
    private bool _applyingLocale;

    public MainForm()
    {
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(980, 640);
        ClientSize = new Size(1100, 720);
        var iconFile = Path.Combine(AppContext.BaseDirectory, "app.ico");
        if (File.Exists(iconFile))
        {
            Icon = new Icon(iconFile);
        }

        Font = Theme.Face;
        ForeColor = Theme.Ink;
        BackColor = Theme.Canvas;
        DoubleBuffered = true;

        var header = new Panel { Dock = DockStyle.Top, Height = 48, BackColor = Theme.Bar };
        _titleLabel.Dock = DockStyle.Fill;
        _titleLabel.TextAlign = ContentAlignment.MiddleLeft;
        _titleLabel.Padding = new Padding(16, 0, 0, 0);
        _titleLabel.Font = Theme.Title;
        _titleLabel.ForeColor = Theme.BarText;
        _titleLabel.BackColor = Theme.Bar;

        _appLangLabel.AutoSize = true;
        _appLangLabel.Font = Theme.Face;
        _appLangLabel.ForeColor = Color.FromArgb(190, 194, 200);
        _appLangLabel.TextAlign = ContentAlignment.MiddleRight;
        _appLangLabel.Margin = new Padding(0, 5, 8, 0);

        _uiLanguage.DropDownStyle = ComboBoxStyle.DropDownList;
        _uiLanguage.FlatStyle = FlatStyle.Standard;
        _uiLanguage.Font = Theme.Face;
        _uiLanguage.Width = 128;
        _uiLanguage.Margin = new Padding(0);
        _uiLanguage.BackColor = Color.White;
        _uiLanguage.ForeColor = Theme.Ink;
        foreach (var language in Locale.Languages)
        {
            _uiLanguage.Items.Add(language);
        }

        _uiLanguage.SelectedIndexChanged += (_, _) =>
        {
            if (_applyingLocale || _uiLanguage.SelectedItem is not UiLanguage language)
            {
                return;
            }

            Locale.Set(language.Code);
            ApplyLocale();
        };

        var languageHost = new FlowLayoutPanel
        {
            Dock = DockStyle.Right,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Padding = new Padding(8, 10, 16, 10),
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            BackColor = Theme.Bar,
        };
        languageHost.Controls.Add(_appLangLabel);
        languageHost.Controls.Add(_uiLanguage);
        header.Controls.Add(languageHost);
        header.Controls.Add(_titleLabel);

        var voiceHost = Theme.CardHost(16);
        voiceHost.Dock = DockStyle.Top;
        voiceHost.Height = 304;
        voiceHost.Padding = new Padding(16, 16, 16, 8);
        var voiceCard = Theme.CardBody();
        StyleVoices();
        voiceCard.Controls.Add(_voices);
        voiceHost.Controls.Add(voiceCard);
        FillGrid(CultureMap.DefaultVoices());

        _interface.DropDownStyle = ComboBoxStyle.DropDownList;
        _interface.FlatStyle = FlatStyle.Standard;
        _interface.Font = Theme.Face;
        _interface.Width = 168;
        foreach (var row in CultureMap.Rows)
        {
            _interface.Items.Add(new InterfaceChoice(row.Code));
        }

        _interface.SelectedIndex = Array.FindIndex(CultureMap.Rows, row => row.Code == "zh-Hant");
        _interface.SelectedIndexChanged += (_, _) => SyncGridToInterface();
        _voices.SelectionChanged += (_, _) => SyncInterfaceToGrid();
        SyncGridToInterface();

        var listHost = Theme.CardHost(16);
        listHost.Padding = new Padding(16, 8, 16, 8);
        StyleTargets();
        var listCard = Theme.CardBody();
        listCard.Controls.Add(_targets);
        listHost.Controls.Add(listCard);

        var buttonHost = Theme.CardHost(16);
        buttonHost.Dock = DockStyle.Bottom;
        buttonHost.Height = 64;
        buttonHost.Padding = new Padding(16, 8, 16, 8);
        var buttonCard = Theme.CardBody();
        var buttons = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(12, 8, 12, 8),
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            BackColor = Theme.Card,
        };
        _gameUiLabel.AutoSize = true;
        _gameUiLabel.Font = Theme.Face;
        _gameUiLabel.ForeColor = Theme.Muted;
        _gameUiLabel.Margin = new Padding(0, 6, 8, 0);
        _interface.Font = Theme.Face;
        _interface.Margin = new Padding(0, 2, 16, 0);
        _applyButton = MakeButton(ApplySelected, primary: true);
        _restoreButton = MakeButton(RestoreSelected);
        _readButton = MakeButton(ReadSelected);
        _scanButton = MakeButton(LoadTargets);
        _openButton = MakeButton(OpenSelectedFolder);
        _addButton = MakeButton(AddFolder);
        buttons.Controls.Add(_gameUiLabel);
        buttons.Controls.Add(_interface);
        buttons.Controls.Add(_applyButton);
        buttons.Controls.Add(_restoreButton);
        buttons.Controls.Add(_readButton);
        buttons.Controls.Add(_scanButton);
        buttons.Controls.Add(_openButton);
        buttons.Controls.Add(_addButton);
        buttonCard.Controls.Add(buttons);
        buttonHost.Controls.Add(buttonCard);

        var logHost = Theme.CardHost(16);
        logHost.Dock = DockStyle.Bottom;
        logHost.Height = 112;
        logHost.Padding = new Padding(16, 8, 16, 16);
        _log.Dock = DockStyle.Fill;
        _log.Multiline = true;
        _log.ReadOnly = true;
        _log.BorderStyle = BorderStyle.None;
        _log.Font = Theme.Face;
        _log.BackColor = Theme.Card;
        _log.ForeColor = Theme.Muted;
        _log.ScrollBars = ScrollBars.Vertical;
        _log.Padding = new Padding(8, 8, 8, 8);
        var logCard = Theme.CardBody();
        logCard.Controls.Add(_log);
        logHost.Controls.Add(logCard);

        Controls.Add(listHost);
        Controls.Add(buttonHost);
        Controls.Add(logHost);
        Controls.Add(voiceHost);
        Controls.Add(header);

        ApplyLocale();
        Shown += (_, _) => LoadTargets();
    }

    private void ApplyLocale()
    {
        _applyingLocale = true;
        _syncing = true;
        Text = Locale.Get("title");
        _titleLabel.Text = Locale.Get("title");
        _appLangLabel.Text = Locale.Get("appLang");
        _voices.Columns[0].HeaderText = Locale.Get("subtitle");
        _voices.Columns[1].HeaderText = Locale.Get("voice");
        _gameUiLabel.Text = Locale.Get("gameUi");
        _applyButton.Text = Locale.Get("apply");
        _restoreButton.Text = Locale.Get("restore");
        _readButton.Text = Locale.Get("read");
        _scanButton.Text = Locale.Get("scan");
        _openButton.Text = Locale.Get("open");
        _addButton.Text = Locale.Get("add");
        _targets.Columns[0].Text = Locale.Get("platform");
        _targets.Columns[1].Text = Locale.Get("game");
        _targets.Columns[2].Text = Locale.Get("status");
        _targets.Columns[3].Text = Locale.Get("path");

        foreach (DataGridViewRow row in _voices.Rows)
        {
            if (row.Tag is string code && CultureMap.Rows.Any(item => item.Code.Equals(code, StringComparison.OrdinalIgnoreCase)))
            {
                row.Cells[0].Value = Locale.CultureName(code);
            }
        }

        var interfaceCode = (_interface.SelectedItem as InterfaceChoice)?.Code ?? "zh-Hant";
        _interface.Items.Clear();
        foreach (var row in CultureMap.Rows)
        {
            _interface.Items.Add(new InterfaceChoice(row.Code));
        }

        for (var index = 0; index < _interface.Items.Count; index++)
        {
            if (_interface.Items[index] is InterfaceChoice choice && choice.Code == interfaceCode)
            {
                _interface.SelectedIndex = index;
                break;
            }
        }

        for (var index = 0; index < _uiLanguage.Items.Count; index++)
        {
            if (_uiLanguage.Items[index] is UiLanguage language && language.Code == Locale.Current)
            {
                _uiLanguage.SelectedIndex = index;
                break;
            }
        }

        foreach (ListViewItem item in _targets.Items)
        {
            if (item.Tag is ConfigTarget target)
            {
                item.Text = target.ChannelText;
                item.SubItems[1].Text = target.TitleText;
                item.SubItems[2].Text = target.DescribeStatus();
            }
        }

        _syncing = false;
        _applyingLocale = false;
    }

    private void StyleVoices()
    {
        _voices.Dock = DockStyle.Fill;
        _voices.BackgroundColor = Theme.Card;
        _voices.BorderStyle = BorderStyle.None;
        _voices.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
        _voices.GridColor = Theme.Line;
        _voices.EnableHeadersVisualStyles = false;
        _voices.ColumnHeadersHeight = 38;
        _voices.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
        _voices.ColumnHeadersDefaultCellStyle.BackColor = Theme.Header;
        _voices.ColumnHeadersDefaultCellStyle.ForeColor = Theme.Muted;
        _voices.ColumnHeadersDefaultCellStyle.SelectionBackColor = Theme.Header;
        _voices.ColumnHeadersDefaultCellStyle.SelectionForeColor = Theme.Muted;
        _voices.ColumnHeadersDefaultCellStyle.Padding = new Padding(10, 0, 10, 0);
        _voices.ColumnHeadersDefaultCellStyle.Font = Theme.FaceBold;
        _voices.DefaultCellStyle.Font = Theme.Face;
        _voices.DefaultCellStyle.BackColor = Theme.Card;
        _voices.DefaultCellStyle.ForeColor = Theme.Ink;
        _voices.DefaultCellStyle.Padding = new Padding(10, 0, 10, 0);
        _voices.DefaultCellStyle.SelectionBackColor = Theme.Selected;
        _voices.DefaultCellStyle.SelectionForeColor = Theme.Ink;
        _voices.RowTemplate.Height = 34;
        _voices.AdvancedColumnHeadersBorderStyle.All = DataGridViewAdvancedCellBorderStyle.None;
        _voices.AllowUserToAddRows = false;
        _voices.AllowUserToDeleteRows = false;
        _voices.AllowUserToResizeRows = false;
        _voices.RowHeadersVisible = false;
        _voices.MultiSelect = false;
        _voices.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        _voices.EditMode = DataGridViewEditMode.EditOnEnter;
        _voices.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        _voices.DataError += (_, e) => e.ThrowException = false;
        _voices.Columns.Add(new DataGridViewTextBoxColumn
        {
            HeaderText = Locale.Get("subtitle"),
            ReadOnly = true,
            FillWeight = 40,
        });
        var voiceColumn = new DataGridViewComboBoxColumn
        {
            HeaderText = Locale.Get("voice"),
            FlatStyle = FlatStyle.Flat,
            DisplayStyle = DataGridViewComboBoxDisplayStyle.ComboBox,
            FillWeight = 60,
        };
        foreach (var voice in CultureMap.Voices)
        {
            voiceColumn.Items.Add(voice);
        }

        _voices.Columns.Add(voiceColumn);
    }

    private void StyleTargets()
    {
        _targets.Dock = DockStyle.Fill;
        _targets.View = View.Details;
        _targets.FullRowSelect = true;
        _targets.GridLines = false;
        _targets.BorderStyle = BorderStyle.None;
        _targets.BackColor = Theme.Card;
        _targets.Font = Theme.Face;
        _targets.ForeColor = Theme.Ink;
        _targets.OwnerDraw = true;
        _targets.DrawColumnHeader += DrawTargetHeader;
        _targets.DrawItem += (_, e) =>
        {
            if (_targets.View != View.Details)
            {
                e.DrawDefault = true;
            }
        };
        _targets.DrawSubItem += (_, e) => e.DrawDefault = true;
        _targets.CheckBoxes = true;
        _targets.HeaderStyle = ColumnHeaderStyle.Nonclickable;
        _targets.Columns.Add(Locale.Get("platform"), 130);
        _targets.Columns.Add(Locale.Get("game"), 150);
        _targets.Columns.Add(Locale.Get("status"), 110);
        _targets.Columns.Add(Locale.Get("path"), 420);
        _targets.DoubleClick += (_, _) => OpenSelectedFolder();
        _targets.Resize += (_, _) => FitPathColumn();
    }

    private void DrawTargetHeader(object? sender, DrawListViewColumnHeaderEventArgs e)
    {
        using var background = new SolidBrush(Theme.Header);
        using var line = new Pen(Theme.Line);
        e.Graphics.FillRectangle(background, e.Bounds);
        e.Graphics.DrawLine(line, e.Bounds.Left, e.Bounds.Bottom - 1, e.Bounds.Right, e.Bounds.Bottom - 1);
        var textBounds = new Rectangle(e.Bounds.X + 8, e.Bounds.Y, e.Bounds.Width - 12, e.Bounds.Height);
        TextRenderer.DrawText(e.Graphics, e.Header?.Text, Theme.Face, textBounds, Theme.Muted, TextFormatFlags.VerticalCenter | TextFormatFlags.Left | TextFormatFlags.EndEllipsis);
    }

    private void FitPathColumn()
    {
        if (_targets.Columns.Count == 0)
        {
            return;
        }

        var used = 0;
        for (var index = 0; index < _targets.Columns.Count - 1; index++)
        {
            used += _targets.Columns[index].Width;
        }

        _targets.Columns[^1].Width = Math.Max(160, _targets.ClientSize.Width - used - 8);
    }

    private static Button MakeButton(Action click, bool primary = false)
    {
        var button = new Button
        {
            AutoSize = true,
            Font = Theme.Face,
            MinimumSize = new Size(72, 30),
            Margin = new Padding(0, 0, 8, 0),
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand,
        };
        button.FlatAppearance.BorderSize = primary ? 0 : 1;
        button.FlatAppearance.BorderColor = Theme.Line;
        if (primary)
        {
            button.BackColor = Theme.Accent;
            button.ForeColor = Color.White;
            button.FlatAppearance.MouseOverBackColor = Theme.AccentHover;
        }
        else
        {
            button.BackColor = Theme.Card;
            button.ForeColor = Theme.Ink;
            button.FlatAppearance.MouseOverBackColor = Theme.Hover;
        }

        button.Click += (_, _) => click();
        return button;
    }

    private void LoadTargets()
    {
        var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var found = ConfigScanner.Scan(localAppData);
        BindTargets(found, replace: true);
        WriteLog(found.Count == 0 ? Locale.Get("notFound") : Locale.Format("found", found.Count));
    }

    private void BindTargets(IReadOnlyList<ConfigTarget> found, bool replace)
    {
        if (replace)
        {
            _targets.Items.Clear();
        }

        foreach (var target in found)
        {
            if (!replace && ContainsDirectory(target.ConfigDirectory))
            {
                continue;
            }

            var item = new ListViewItem(target.ChannelText);
            item.SubItems.Add(target.TitleText);
            item.SubItems.Add(target.DescribeStatus());
            item.SubItems.Add(target.ConfigDirectory);
            item.Tag = target;
            item.Checked = true;
            _targets.Items.Add(item);
        }
    }

    private bool ContainsDirectory(string directory)
    {
        foreach (ListViewItem item in _targets.Items)
        {
            if (item.Tag is ConfigTarget target &&
                target.ConfigDirectory.Equals(directory, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    private void ApplySelected()
    {
        var selected = CheckedTargets();
        if (selected.Count == 0)
        {
            MessageBox.Show(this, Locale.Get("pickOne"), Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        if (TryGetRunningGame(out var processName))
        {
            var answer = MessageBox.Show(this, Locale.Format("running", processName), Text, MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (answer != DialogResult.Yes)
            {
                return;
            }
        }

        var voices = ReadGrid();
        var interfaceCode = (_interface.SelectedItem as InterfaceChoice)?.Code;
        foreach (var target in selected)
        {
            try
            {
                foreach (var message in LanguagePatch.Apply(target, voices, interfaceCode))
                {
                    WriteLog(message);
                }
            }
            catch (Exception ex)
            {
                WriteLog(target.ConfigDirectory + " " + ex.Message);
            }
        }

        RefreshStatus();
    }

    private void RestoreSelected()
    {
        var selected = CheckedTargets();
        if (selected.Count == 0)
        {
            MessageBox.Show(this, Locale.Get("pickOne"), Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        foreach (var target in selected)
        {
            try
            {
                foreach (var message in LanguagePatch.Restore(target))
                {
                    WriteLog(message);
                }
            }
            catch (Exception ex)
            {
                WriteLog(target.ConfigDirectory + " " + ex.Message);
            }
        }

        RefreshStatus();
    }

    private void ReadSelected()
    {
        var target = SelectedOrFirstChecked();
        if (target == null || !File.Exists(target.GameIniPath))
        {
            WriteLog(Locale.Get("noMap"));
            return;
        }

        var (text, _) = IniFile.Read(target.GameIniPath);
        var parsed = CultureMap.Parse(text);
        if (!text.Contains("UnrealCultureToWwiseCulture=", StringComparison.Ordinal))
        {
            WriteLog(Locale.Get("noMap"));
            return;
        }

        FillGrid(CultureMap.Merge(parsed));
    }

    private void SyncInterfaceToGrid()
    {
        if (_syncing || _voices.CurrentRow?.Tag is not string code)
        {
            return;
        }

        var index = -1;
        for (var i = 0; i < _interface.Items.Count; i++)
        {
            if (_interface.Items[i] is InterfaceChoice choice && choice.Code.Equals(code, StringComparison.OrdinalIgnoreCase))
            {
                index = i;
                break;
            }
        }

        if (index < 0 || index == _interface.SelectedIndex)
        {
            return;
        }

        _syncing = true;
        _interface.SelectedIndex = index;
        _syncing = false;
    }

    private void SyncGridToInterface()
    {
        if (_syncing || _interface.SelectedItem is not InterfaceChoice choice)
        {
            return;
        }

        _syncing = true;
        SelectVoiceRow(choice.Code);
        _syncing = false;
    }

    private void SelectVoiceRow(string code)
    {
        foreach (DataGridViewRow row in _voices.Rows)
        {
            if (row.Tag is string rowCode && rowCode.Equals(code, StringComparison.OrdinalIgnoreCase) && row.Cells.Count > 0)
            {
                _voices.CurrentCell = row.Cells[0];
                row.Selected = true;
                return;
            }
        }
    }

    private void FillGrid(IReadOnlyDictionary<string, string> voices)
    {
        _syncing = true;
        var column = (DataGridViewComboBoxColumn)_voices.Columns[1];
        var choices = new List<string>(CultureMap.Voices);
        foreach (var voice in voices.Values)
        {
            if (!string.IsNullOrWhiteSpace(voice) && !choices.Contains(voice, StringComparer.Ordinal))
            {
                choices.Add(voice);
            }
        }

        _voices.Rows.Clear();
        column.Items.Clear();
        foreach (var voice in choices)
        {
            column.Items.Add(voice);
        }

        foreach (var row in CultureMap.Rows)
        {
            var voice = voices.TryGetValue(row.Code, out var value) ? value : row.DefaultVoice;
            var index = _voices.Rows.Add(Locale.CultureName(row.Code), voice);
            _voices.Rows[index].Tag = row.Code;
        }

        foreach (var extra in voices.Keys.OrderBy(key => key, StringComparer.OrdinalIgnoreCase))
        {
            if (CultureMap.Rows.Any(row => row.Code.Equals(extra, StringComparison.OrdinalIgnoreCase)))
            {
                continue;
            }

            var index = _voices.Rows.Add(extra, voices[extra]);
            _voices.Rows[index].Tag = extra;
        }

        _syncing = false;
        SyncGridToInterface();
    }

    private Dictionary<string, string> ReadGrid()
    {
        _voices.EndEdit();
        var map = CultureMap.DefaultVoices();
        foreach (DataGridViewRow row in _voices.Rows)
        {
            if (row.Tag is not string code || string.IsNullOrWhiteSpace(code))
            {
                continue;
            }

            var voice = row.Cells[1].Value as string;
            if (!string.IsNullOrWhiteSpace(voice))
            {
                map[code] = voice;
            }
        }

        return map;
    }

    private void AddFolder()
    {
        using var dialog = new FolderBrowserDialog();
        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        var found = ConfigScanner.FromUserFolder(dialog.SelectedPath);
        if (found.Count == 0)
        {
            MessageBox.Show(this, Locale.Get("noGameIni"), Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        BindTargets(found, replace: false);
    }

    private void OpenSelectedFolder()
    {
        var target = SelectedOrFirstChecked();
        if (target == null || !Directory.Exists(target.ConfigDirectory))
        {
            return;
        }

        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
        {
            FileName = "explorer.exe",
            Arguments = "\"" + target.ConfigDirectory + "\"",
            UseShellExecute = true,
        });
    }

    private List<ConfigTarget> CheckedTargets()
    {
        var list = new List<ConfigTarget>();
        foreach (ListViewItem item in _targets.Items)
        {
            if (item.Checked && item.Tag is ConfigTarget target)
            {
                list.Add(target);
            }
        }

        return list;
    }

    private ConfigTarget? SelectedOrFirstChecked()
    {
        if (_targets.SelectedItems.Count > 0 && _targets.SelectedItems[0].Tag is ConfigTarget selected)
        {
            return selected;
        }

        foreach (ListViewItem item in _targets.Items)
        {
            if (item.Checked && item.Tag is ConfigTarget target)
            {
                return target;
            }
        }

        return null;
    }

    private void RefreshStatus()
    {
        foreach (ListViewItem item in _targets.Items)
        {
            if (item.Tag is ConfigTarget target)
            {
                item.SubItems[2].Text = target.DescribeStatus();
            }
        }
    }

    private void WriteLog(string message)
    {
        _log.AppendText(message + Environment.NewLine);
    }

    private static bool TryGetRunningGame(out string processName)
    {
        string[] names = ["Gears5", "GoWEDay-Steam", "GoWEDay", "GearGame"];
        foreach (var process in System.Diagnostics.Process.GetProcesses())
        {
            try
            {
                if (names.Contains(process.ProcessName, StringComparer.OrdinalIgnoreCase))
                {
                    processName = process.ProcessName;
                    return true;
                }
            }
            catch (InvalidOperationException)
            {
            }
            catch (System.ComponentModel.Win32Exception)
            {
            }
            finally
            {
                process.Dispose();
            }
        }

        processName = string.Empty;
        return false;
    }

    private sealed class InterfaceChoice
    {
        public InterfaceChoice(string code) => Code = code;

        public string Code { get; }

        public override string ToString() => Locale.CultureName(Code);
    }
}
