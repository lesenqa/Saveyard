using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace Saveyard
{
    public partial class Form1 : Form
    {
        public SaveConfig currentConfig = new SaveConfig();
        public string currentConfigPath = "";
        int currentPageIndex = 0;

        public Form1()
        {
            InitializeComponent();

            // Привязываем событие
            this.openToolStripMenuItem.MouseEnter += new System.EventHandler(this.openToolStripMenuItem_MouseEnter);

            // ВЫЗЫВАЕМ МЕТОД СРАЗУ, чтобы стрелочка появилась мгновенно
            openToolStripMenuItem_MouseEnter(null, null);

            LoadInitialConfig();
        }

        private void LoadInitialConfig()
        {
            if (File.Exists("last_config.ini"))
            {
                string path = File.ReadAllText("last_config.ini");
                if (File.Exists(path))
                {
                    LoadConfigFromFile(path);
                    return;
                }
            }

            // Если мы дошли сюда, значит конфиг новый
            currentConfig = new SaveConfig();
            ConfigPage firstPage = new ConfigPage();

            // Явно добавляем 8 слотов
            for (int i = 1; i <= 8; i++)
            {
                firstPage.Saves.Add(new SaveEntry { Slot = i });
            }

            currentConfig.Pages.Add(firstPage);
            currentPageIndex = 0;
            UpdateUI();
        }

        private void LoadConfigFromFile(string path)
        {
            try
            {
                string json = File.ReadAllText(path);
                var loaded = JsonConvert.DeserializeObject<SaveConfig>(json);
                if (loaded != null)
                {
                    currentConfig = loaded;
                    currentConfigPath = path;
                    currentPageIndex = 0;

                    // Исправляем каждую страницу: если слотов нет или их слишком много/мало
                    foreach (var page in currentConfig.Pages)
                    {
                        if (page.Saves.Count > 8)
                        {
                            page.Saves = page.Saves.Take(8).ToList();
                        }
                        while (page.Saves.Count < 8)
                        {
                            page.Saves.Add(new SaveEntry { Slot = page.Saves.Count + 1 });
                        }
                    }

                    File.WriteAllText("last_config.ini", path);
                }
            }
            catch (Exception ex) { MessageBox.Show("Error: " + ex.Message); }
            currentConfigPath = path;
            UpdateWindowTitle();
            UpdateUI();
        }

        private void SyncToMemory()
        {
            if (currentConfig.Pages.Count <= currentPageIndex) return;
            var page = currentConfig.Pages[currentPageIndex];

            for (int i = 0; i < 8; i++)
            {
                // Поля сохранений
                var tName = pnlSaves.Controls.Find("txtName" + i, true).FirstOrDefault() as TextBox;
                var tPath = pnlSaves.Controls.Find("txtPath" + i, true).FirstOrDefault() as TextBox;
                if (tName != null) page.Saves[i].Name = tName.Text;
                if (tPath != null) page.Saves[i].FilePath = tPath.Text;

                // Поля индивидуальных повторов
                var tReplayName = pnlSaves.Controls.Find("txtReplayName" + i, true).FirstOrDefault() as TextBox;
                var tReplayPath = pnlSaves.Controls.Find("txtReplayPath" + i, true).FirstOrDefault() as TextBox;
                if (tReplayName != null) page.Saves[i].ReplayName = tReplayName.Text;
                if (tReplayPath != null) page.Saves[i].OriginalReplayPath = tReplayPath.Text;
            }

            // Поля повтора на всю страницу
            var tPageReplayName = pnlSaves.Controls.Find("txtPageReplayName", true).FirstOrDefault() as TextBox;
            var tPageReplayPath = pnlSaves.Controls.Find("txtPageReplayPath", true).FirstOrDefault() as TextBox;
            if (tPageReplayName != null) page.PageReplayName = tPageReplayName.Text;
            if (tPageReplayPath != null) page.OriginalPageReplayPath = tPageReplayPath.Text;
        }

        public void UpdateUI()
        {
            pnlSaves.SuspendLayout();
            pnlSaves.Controls.Clear();

            if (currentConfig.Pages.Count == 0 || currentPageIndex < 0 || currentPageIndex >= currentConfig.Pages.Count)
            {
                lblPageInfo.Text = "No pages available";
                pnlSaves.ResumeLayout();
                return;
            }

            var page = currentConfig.Pages[currentPageIndex];
            int countToDraw = Math.Min(8, page.Saves.Count);
            int rowHeight = 65;

            // 1. Отрисовка обычных слотов
            for (int i = 0; i < countToDraw; i++)
            {
                Panel row = new Panel { Width = 560, Height = rowHeight, Top = i * rowHeight, Name = "row" + i };
                // ... (код элементов такой же) ...
                TextBox tName = new TextBox { Name = "txtName" + i, Width = 140, Left = 30, Top = 5, Text = page.Saves[i].Name };
                TextBox tPath = new TextBox { Name = "txtPath" + i, Width = 160, Left = 175, Top = 5, Text = page.Saves[i].FilePath };
                Button bBrowse = new Button { Text = "...", Left = 340, Top = 4, Width = 30, Height = 23 };
                int currentIdx = i;
                bBrowse.Click += (s, e) => { using (OpenFileDialog ofd = new OpenFileDialog()) { if (ofd.ShowDialog() == DialogResult.OK) { tPath.Text = ofd.FileName; page.Saves[currentIdx].FilePath = ofd.FileName; } } };
                Button bLoad = new Button { Text = "Load", Left = 380, Top = 4, Width = 50, Height = 23 };
                bLoad.Click += (s, e) => LoadSingleSave(currentIdx);

                Label lblRep = new Label { Text = ".rep", Left = 0, Top = 38, Width = 30, ForeColor = System.Drawing.Color.Gray };
                TextBox tReplayName = new TextBox { Name = "txtReplayName" + i, Width = 140, Left = 30, Top = 35, Text = page.Saves[i].ReplayName };
                TextBox tReplayPath = new TextBox { Name = "txtReplayPath" + i, Width = 160, Left = 175, Top = 35, Text = page.Saves[i].OriginalReplayPath };
                Button bReplayBrowse = new Button { Text = "...", Left = 340, Top = 34, Width = 30, Height = 23 };
                bReplayBrowse.Click += (s, e) => { using (OpenFileDialog ofd = new OpenFileDialog { Filter = "Replay Files (*.rep)|*.rep|All Files (*.*)|*.*" }) { if (ofd.ShowDialog() == DialogResult.OK) { tReplayPath.Text = ofd.FileName; page.Saves[currentIdx].OriginalReplayPath = ofd.FileName; } } };
                Button bReplaySet = new Button { Text = "Set", Left = 380, Top = 34, Width = 50, Height = 23 };
                bReplaySet.Click += (s, e) => SetReplay(tReplayPath.Text);

                row.Controls.Add(new Label { Text = page.Saves[i].Slot + ".", Left = 0, Top = 8, Width = 25 });
                row.Controls.Add(tName); row.Controls.Add(tPath); row.Controls.Add(bBrowse); row.Controls.Add(bLoad);
                row.Controls.Add(lblRep); row.Controls.Add(tReplayName); row.Controls.Add(tReplayPath); row.Controls.Add(bReplayBrowse); row.Controls.Add(bReplaySet);

                pnlSaves.Controls.Add(row);
            }

            // 2. Нижняя строка
            // Сделаем панель с фиксированной высотой и четким Top
            Panel pnlPageReplay = new Panel
            {
                Width = 560,
                Height = 65,
                Top = countToDraw * rowHeight,
                BackColor = System.Drawing.Color.Transparent
            };

            Label lblPageRepTitle = new Label { Text = ".rep for this page", Left = 0, Top = 5, AutoSize = true, ForeColor = System.Drawing.Color.Black };
            TextBox tPageReplayName = new TextBox { Name = "txtPageReplayName", Width = 140, Left = 30, Top = 30, Text = page.PageReplayName };
            TextBox tPageReplayPath = new TextBox { Name = "txtPageReplayPath", Width = 160, Left = 175, Top = 30, Text = page.OriginalPageReplayPath };
            Button bPageReplayBrowse = new Button { Text = "...", Left = 340, Top = 29, Width = 30, Height = 23 };
            bPageReplayBrowse.Click += (s, e) => {
                using (OpenFileDialog ofd = new OpenFileDialog { Filter = "Replay Files (*.rep)|*.rep|All Files (*.*)|*.*" })
                {
                    if (ofd.ShowDialog() == DialogResult.OK)
                    {
                        tPageReplayPath.Text = ofd.FileName;
                        page.OriginalPageReplayPath = ofd.FileName;
                    }
                }
            };
            Button bPageReplaySet = new Button { Text = "Set", Left = 380, Top = 29, Width = 50, Height = 23 };
            bPageReplaySet.Click += (s, e) => {
                page.PageReplayName = tPageReplayName.Text;
                page.OriginalPageReplayPath = tPageReplayPath.Text;
                SetReplay(tPageReplayPath.Text);
            };

            pnlPageReplay.Controls.Add(lblPageRepTitle);
            pnlPageReplay.Controls.Add(tPageReplayName);
            pnlPageReplay.Controls.Add(tPageReplayPath);
            pnlPageReplay.Controls.Add(bPageReplayBrowse);
            pnlPageReplay.Controls.Add(bPageReplaySet);

            pnlSaves.Controls.Add(pnlPageReplay);

            lblPageInfo.Text = $"Page {currentPageIndex + 1} / {currentConfig.Pages.Count}";
            pnlSaves.ResumeLayout();
        }

        private void SetReplay(string sourceFilePath)
        {
            SyncToMemory();

            if (string.IsNullOrWhiteSpace(currentConfig.SavesDirectory) || !Directory.Exists(currentConfig.SavesDirectory))
            {
                MessageBox.Show("save directory missing or not valid. check settings.", "smh", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string targetReplayPath = Path.Combine(currentConfig.SavesDirectory, "replay.rep");

            // --- Проверка на пустую строку для удаления replay.rep ---
            if (string.IsNullOrWhiteSpace(sourceFilePath))
            {
                if (File.Exists(targetReplayPath))
                {
                    try
                    {
                        File.Delete(targetReplayPath);
                        lblStatus.Text = ".rep deleted";
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("error deleting replay: " + ex.Message, "error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                else
                {
                    lblStatus.Text = ".rep deleted";
                }
                return;
            }

            // --- Если путь указан, но файла не существует ---
            if (!File.Exists(sourceFilePath))
            {
                MessageBox.Show("replay file not found!", "attention", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                File.Copy(sourceFilePath, targetReplayPath, true);
                lblStatus.Text = "Replay set: " + Path.GetFileName(sourceFilePath);
            }
            catch (Exception ex)
            {
                MessageBox.Show("error setting replay: " + ex.Message, "error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadSingleSave(int index)
        {
            SyncToMemory();

            var page = currentConfig.Pages[currentPageIndex];
            if (index < 0 || index >= page.Saves.Count) return;

            var save = page.Saves[index];

            string targetDir = currentConfig.SavesDirectory;
            string prefix = currentConfig.Prefix;
            string postfix = currentConfig.Postfix;

            if (string.IsNullOrEmpty(targetDir) || !Directory.Exists(targetDir))
            {
                lblStatus.Text = "Error: Invalid Saves Directory!";
                return;
            }

            string fileName = $"{prefix}{save.Slot}{postfix}";
            string fullDestPath = Path.Combine(targetDir, fileName);

            try
            {
                if (!string.IsNullOrEmpty(save.FilePath) && File.Exists(save.FilePath))
                {
                    File.Copy(save.FilePath, fullDestPath, true);
                    lblStatus.Text = $"Slot {save.Slot} loaded successfully.";
                }
                else
                {
                    if (File.Exists(fullDestPath))
                    {
                        File.Delete(fullDestPath);
                        lblStatus.Text = $"Slot {save.Slot} cleared from folder.";
                    }
                }
            }
            catch (Exception ex)
            {
                lblStatus.Text = "File Error!";
                MessageBox.Show($"error while working with file {fileName}: {ex.Message}");
            }
        }

        private void CreateNewConfig()
        {
            currentConfig = new SaveConfig();
            currentConfigPath = "";
            currentPageIndex = 0;

            var firstPage = new ConfigPage();
            firstPage.Saves.Clear();
            for (int i = 1; i <= 8; i++)
            {
                firstPage.Saves.Add(new SaveEntry { Slot = i });
            }
            currentConfig.Pages.Add(firstPage);

            UpdateUI();

            lblStatus.Text = "New config created.";
            this.Text = "Saveyard - New";

            if (File.Exists("last_config.ini")) File.Delete("last_config.ini");
            currentConfigPath = "";
            UpdateWindowTitle();
            UpdateUI();
        }

        private void UpdateWindowTitle()
        {
            string programName = "Saveyard";

            if (string.IsNullOrEmpty(currentConfigPath))
            {
                this.Text = $"{programName} - New Config";
            }
            else
            {
                string fileName = Path.GetFileName(currentConfigPath);
                this.Text = $"{programName} - {fileName}";
            }
        }

        private void btnSaveConfig_Click(object sender, EventArgs e)
        {
            SyncToMemory();

            if (string.IsNullOrEmpty(currentConfigPath))
            {
                if (!Directory.Exists("Configs")) Directory.CreateDirectory("Configs");
                currentConfigPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Configs", "Unnamed.cfg");
            }

            try
            {
                string json = JsonConvert.SerializeObject(currentConfig, Formatting.Indented);
                File.WriteAllText(currentConfigPath, json);

                File.WriteAllText("last_config.ini", currentConfigPath);

                UpdateWindowTitle();
                lblStatus.Text = "Saved: " + Path.GetFileName(currentConfigPath);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Save error: " + ex.Message);
            }
        }

        private void btnAddPage_Click(object sender, EventArgs e)
        {
            SyncToMemory();

            var newPage = new ConfigPage();
            for (int i = 1; i <= 8; i++)
                newPage.Saves.Add(new SaveEntry { Slot = i });

            currentConfig.Pages.Add(newPage);
            currentPageIndex = currentConfig.Pages.Count - 1;
            UpdateUI();
        }

        private void btnNextPage_Click(object sender, EventArgs e) { if (currentPageIndex < currentConfig.Pages.Count - 1) { SyncToMemory(); currentPageIndex++; UpdateUI(); } }
        private void btnPrevPage_Click(object sender, EventArgs e) { if (currentPageIndex > 0) { SyncToMemory(); currentPageIndex--; UpdateUI(); } }
        private void btnDeletePage_Click(object sender, EventArgs e) { if (currentConfig.Pages.Count > 1) { currentConfig.Pages.RemoveAt(currentPageIndex); if (currentPageIndex > 0) currentPageIndex--; UpdateUI(); } }

        private void btnLoadAll_Click(object sender, EventArgs e)
        {
            SyncToMemory();

            string targetDir = currentConfig.SavesDirectory;
            string prefix = currentConfig.Prefix;
            string postfix = currentConfig.Postfix;

            if (string.IsNullOrEmpty(targetDir) || !Directory.Exists(targetDir))
            {
                lblStatus.Text = "Error: Invalid Saves Directory!";
                return;
            }

            var page = currentConfig.Pages[currentPageIndex];
            int loadedCount = 0;
            int deletedCount = 0;

            try
            {
                // 1. Загрузка/удаление всех слотов сохранений
                foreach (var save in page.Saves)
                {
                    string fileName = $"{prefix}{save.Slot}{postfix}";
                    string fullPathInSaves = Path.Combine(targetDir, fileName);

                    if (!string.IsNullOrEmpty(save.FilePath) && File.Exists(save.FilePath))
                    {
                        File.Copy(save.FilePath, fullPathInSaves, true);
                        loadedCount++;
                    }
                    else
                    {
                        if (File.Exists(fullPathInSaves))
                        {
                            File.Delete(fullPathInSaves);
                            deletedCount++;
                        }
                    }
                }

                // 2. Обработка .rep для всей страницы
                string targetReplayPath = Path.Combine(targetDir, "replay.rep");
                string sourceReplayPath = page.OriginalPageReplayPath;

                if (!string.IsNullOrWhiteSpace(sourceReplayPath) && File.Exists(sourceReplayPath))
                {
                    File.Copy(sourceReplayPath, targetReplayPath, true);
                    lblStatus.Text = $"Done. Loaded: {loadedCount}, Cleared: {deletedCount}. Replay set.";
                }
                else if (string.IsNullOrWhiteSpace(sourceReplayPath))
                {
                    // Если путь пустой — удаляем replay.rep, если он есть
                    if (File.Exists(targetReplayPath))
                    {
                        File.Delete(targetReplayPath);
                    }
                    lblStatus.Text = $"Done. Loaded: {loadedCount}, Cleared: {deletedCount}. .rep deleted.";
                }
                else
                {
                    // Если путь есть, но файла не существует
                    lblStatus.Text = $"Done. Loaded: {loadedCount}, Cleared: {deletedCount}. (Replay file not found)";
                }
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Error during file operations!";
                MessageBox.Show($"crtkl error (phobos reference): {ex.Message}");
            }
        }

        private void btnSaveCurrent_Click(object sender, EventArgs e)
        {
            string sourceDir = currentConfig.SavesDirectory;
            if (string.IsNullOrEmpty(sourceDir) || !Directory.Exists(sourceDir))
            {
                MessageBox.Show("save file directory not providedddddddd", "attention or something idk");
                return;
            }

            if (string.IsNullOrEmpty(currentConfigPath))
            {
                MessageBox.Show("save or open config first", "attention or something");
                return;
            }

            try
            {
                string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH'h'mm'm'ss's'");
                string baseSavesPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Saves");

                string safeGameName = string.Join("_", currentConfig.GameName.Split(Path.GetInvalidFileNameChars()));
                string safeCategoryName = string.Join("_", currentConfig.CategoryName.Split(Path.GetInvalidFileNameChars()));
                string configFileName = Path.GetFileNameWithoutExtension(currentConfigPath);

                string targetPath = Path.Combine(
                    baseSavesPath,
                    safeGameName,
                    safeCategoryName,
                    configFileName,
                    timestamp
                );

                if (!Directory.Exists(targetPath))
                {
                    Directory.CreateDirectory(targetPath);
                }

                int backedUpCount = 0;
                string prefix = currentConfig.Prefix;
                string postfix = currentConfig.Postfix;

                for (int i = 1; i <= 8; i++)
                {
                    string fileName = $"{prefix}{i}{postfix}";
                    string sourceFile = Path.Combine(sourceDir, fileName);

                    if (File.Exists(sourceFile))
                    {
                        string destFile = Path.Combine(targetPath, fileName);
                        File.Copy(sourceFile, destFile, true);
                        backedUpCount++;
                    }
                }

                // Бекап replay.rep
                string replayFile = Path.Combine(sourceDir, "replay.rep");
                if (File.Exists(replayFile))
                {
                    File.Copy(replayFile, Path.Combine(targetPath, "replay.rep"), true);
                }

                lblStatus.Text = $"Backup created: {timestamp} ({backedUpCount} files + replay)";
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Backup Error!";
                MessageBox.Show($"error backing up saves: {ex.Message}");
            }
        }

        private void btnClearFolder_Click(object sender, EventArgs e)
        {
            string targetDir = currentConfig.SavesDirectory;
            string prefix = currentConfig.Prefix;
            string postfix = currentConfig.Postfix;

            if (string.IsNullOrEmpty(targetDir) || !Directory.Exists(targetDir))
            {
                lblStatus.Text = "Error: Invalid Saves Directory!";
                return;
            }

            int deletedCount = 0;

            try
            {
                var page = currentConfig.Pages[currentPageIndex];

                foreach (var save in page.Saves)
                {
                    string fileName = $"{prefix}{save.Slot}{postfix}";
                    string fullPath = Path.Combine(targetDir, fileName);

                    if (File.Exists(fullPath))
                    {
                        File.Delete(fullPath);
                        deletedCount++;
                    }
                }

                lblStatus.Text = $"Folder cleared. Removed {deletedCount} save files.";
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Error during clearing!";
                MessageBox.Show($"error while deleting files: {ex.Message}");
            }
        }

        private void btnSettings_Click(object sender, EventArgs e) { }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            btnSaveConfig_Click(sender, e);
        }

        private void settingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FormSettings settingsForm = new FormSettings(this);
            settingsForm.StartPosition = FormStartPosition.CenterParent;
            settingsForm.ShowDialog();
        }

        private void closeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to clear everything and start a new config?",
                                "New Config", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                CreateNewConfig();
            }
        }

        private void openToolStripMenuItem_MouseEnter(object sender, EventArgs e)
        {
            openToolStripMenuItem.DropDownItems.Clear();

            string configDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Configs");
            if (!Directory.Exists(configDir)) Directory.CreateDirectory(configDir);

            var files = Directory.GetFiles(configDir, "*.cfg");
            foreach (var file in files)
            {
                var fileItem = new ToolStripMenuItem(Path.GetFileName(file));
                fileItem.Click += (s, ev) => LoadConfigFromFile(file);
                openToolStripMenuItem.DropDownItems.Add(fileItem);
            }

            if (files.Length > 0) openToolStripMenuItem.DropDownItems.Add(new ToolStripSeparator());

            var browseItem = new ToolStripMenuItem("Browse...");
            browseItem.Click += (s, ev) => {
                using (OpenFileDialog ofd = new OpenFileDialog())
                {
                    ofd.InitialDirectory = configDir;
                    ofd.Filter = "Config files (*.cfg)|*.cfg|All files (*.*)|*.*";
                    if (ofd.ShowDialog() == DialogResult.OK)
                    {
                        LoadConfigFromFile(ofd.FileName);
                    }
                }
            };
            openToolStripMenuItem.DropDownItems.Add(browseItem);
        }

        private void btnOpenUserFiles_Click(object sender, EventArgs e)
        {
            string path = currentConfig.SavesDirectory;

            if (!string.IsNullOrEmpty(path) && Directory.Exists(path))
            {
                Process.Start("explorer.exe", path);
            }
            else
            {
                MessageBox.Show("Directory not provided. Edit config settings first.", "smh");
            }
        }

        private void btnOpenProgramFolder_Click(object sender, EventArgs e)
        {
            string path = AppDomain.CurrentDomain.BaseDirectory;

            if (Directory.Exists(path))
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = path,
                    UseShellExecute = true,
                    Verb = "open"
                });
            }
            else
            {
                lblStatus.Text = "couldn't find the folder. somehow";
            }
        }

        private void infoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (FormInfo infoForm = new FormInfo())
            {
                infoForm.ShowDialog(this);
            }
        }
    }
}