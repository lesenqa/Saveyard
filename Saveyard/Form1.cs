using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;
// using static System.Windows.Forms.VisualStyles.VisualStyleElement;

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

            // Явно добавляем 8 слотов, чтобы UpdateUI было что рисовать
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
                        // Если слотов больше 8 (как в твоем случае), оставляем только первые 8
                        if (page.Saves.Count > 8)
                        {
                            page.Saves = page.Saves.Take(8).ToList();
                        }
                        // Если слотов меньше 8, добавляем недостающие
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
            UpdateWindowTitle(); // Добавь здесь
            UpdateUI();
        }

        private void SyncToMemory()
        {
            if (currentConfig.Pages.Count <= currentPageIndex) return;
            var page = currentConfig.Pages[currentPageIndex];
            for (int i = 0; i < 8; i++)
            {
                var tName = pnlSaves.Controls.Find("txtName" + i, true).FirstOrDefault() as TextBox;
                var tPath = pnlSaves.Controls.Find("txtPath" + i, true).FirstOrDefault() as TextBox;
                if (tName != null) page.Saves[i].Name = tName.Text;
                if (tPath != null) page.Saves[i].FilePath = tPath.Text;
            }
        }

        public void UpdateUI()
        {
            // Очищаем панель перед отрисовкой
            pnlSaves.Controls.Clear();

            // Проверка наличия страниц
            if (currentConfig.Pages.Count == 0 || currentPageIndex < 0 || currentPageIndex >= currentConfig.Pages.Count)
            {
                lblPageInfo.Text = "No pages available";
                return;
            }

            var page = currentConfig.Pages[currentPageIndex];
            int countToDraw = Math.Min(8, page.Saves.Count);

            for (int i = 0; i < countToDraw; i++)
            {
                // Создаем контейнер для строки
                Panel row = new Panel { Width = 560, Height = 35, Top = i * 35, Name = "row" + i };

                // Текстовое поле для имени сейва
                TextBox tName = new TextBox
                {
                    Name = "txtName" + i,
                    Width = 140,
                    Left = 30,
                    Top = 5,
                    Text = page.Saves[i].Name
                };

                // Текстовое поле для пути к файлу
                TextBox tPath = new TextBox
                {
                    Name = "txtPath" + i,
                    Width = 160,
                    Left = 175,
                    Top = 5,
                    Text = page.Saves[i].FilePath
                };

                // Кнопка выбора файла (...)
                Button bBrowse = new Button { Text = "...", Left = 340, Top = 4, Width = 30, Height = 23 };
                int currentIdx = i; // Фиксируем индекс для замыкания
                bBrowse.Click += (s, e) => {
                    using (OpenFileDialog ofd = new OpenFileDialog())
                    {
                        if (ofd.ShowDialog() == DialogResult.OK)
                        {
                            tPath.Text = ofd.FileName;
                            // Обновляем в памяти сразу, чтобы LoadSingleSave увидел путь
                            page.Saves[currentIdx].FilePath = ofd.FileName;
                        }
                    }
                };

                // Кнопка Load
                Button bLoad = new Button { Text = "Load", Left = 380, Top = 4, Width = 50, Height = 23 };
                bLoad.Click += (s, e) => {
                    // Вызываем загрузку конкретного сохранения по его индексу в списке (0-7)
                    LoadSingleSave(currentIdx);
                };

                // Добавляем все элементы
                row.Controls.Add(new Label { Text = page.Saves[i].Slot + ".", Left = 0, Top = 8, Width = 25 });
                row.Controls.Add(tName);
                row.Controls.Add(tPath);
                row.Controls.Add(bBrowse);
                row.Controls.Add(bLoad);

                pnlSaves.Controls.Add(row);
            }

            lblPageInfo.Text = $"Page {currentPageIndex + 1} / {currentConfig.Pages.Count}";
        }

        private void LoadSingleSave(int index)
        {
            // 1. Синхронизируем изменения из TextBox-ов в память
            SyncToMemory();

            var page = currentConfig.Pages[currentPageIndex];
            if (index < 0 || index >= page.Saves.Count) return;

            var save = page.Saves[index];

            // 2. Данные для сборки пути из твоего Models.cs
            string targetDir = currentConfig.SavesDirectory;
            string prefix = currentConfig.Prefix;
            string postfix = currentConfig.Postfix;

            // Проверка пути к директории сохранений
            if (string.IsNullOrEmpty(targetDir) || !Directory.Exists(targetDir))
            {
                lblStatus.Text = "Error: Invalid Saves Directory!";
                return;
            }

            // 3. Собираем имя системного файла (напр. GTAVCsf4.b)
            string fileName = $"{prefix}{save.Slot}{postfix}";
            string fullDestPath = Path.Combine(targetDir, fileName);

            try
            {
                // 4. Логика замены или удаления
                if (!string.IsNullOrEmpty(save.FilePath) && File.Exists(save.FilePath))
                {
                    // Если путь в менеджере указан — копируем с заменой
                    File.Copy(save.FilePath, fullDestPath, true);
                    lblStatus.Text = $"Slot {save.Slot} loaded successfully.";
                }
                else
                {
                    // Если путь пустой — удаляем существующий файл в папке сохранений
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
            // 1. Сбрасываем объект конфига к начальным значениям
            currentConfig = new SaveConfig();
            currentConfigPath = "";
            currentPageIndex = 0;

            // 2. Гарантируем наличие одной чистой страницы
            var firstPage = new ConfigPage();
            // На всякий случай очистим и пересоздадим слоты, 
            // так как мы убрали цикл из конструктора ConfigPage
            firstPage.Saves.Clear();
            for (int i = 1; i <= 8; i++)
            {
                firstPage.Saves.Add(new SaveEntry { Slot = i });
            }
            currentConfig.Pages.Add(firstPage);

            // 3. Обновляем интерфейс
            UpdateUI();

            // 4. Очищаем статус и заголовок
            lblStatus.Text = "New config created.";
            this.Text = "Saveyard - New";

            // 5. Очищаем файл последнего конфига, чтобы при перезапуске не открылся старый
            if (File.Exists("last_config.ini")) File.Delete("last_config.ini");
            // ... код очистки ...
            currentConfigPath = "";
            UpdateWindowTitle(); // Установит заголовок "New Config"
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
                // Path.GetFileName извлечет "MySave.cfg" из полного пути "C:\Games\Configs\MySave.cfg"
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

                // --- ДОБАВЛЯЕМ ЭТУ СТРОКУ ---
                // Записываем путь к только что сохраненному файлу, чтобы он открылся при следующем запуске
                File.WriteAllText("last_config.ini", currentConfigPath);
                // ----------------------------

                // Вместо MessageBox просто обновляем статус внизу формы (если есть lblStatus)
                // lblStatus.Text = "Config Saved: " + DateTime.Now.ToShortTimeString();
                UpdateWindowTitle();
                lblStatus.Text = "Saved: " + Path.GetFileName(currentConfigPath);
            }
            catch (Exception ex)
            {
                // Ошибку лучше оставить, чтобы знать, если вдруг файл заблокирован
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
        /* private void btnOpen_Click(object sender, EventArgs e)
        {
            ContextMenuStrip menu = new ContextMenuStrip();
            if (!Directory.Exists("Configs")) Directory.CreateDirectory("Configs");
            foreach (var file in Directory.GetFiles("Configs", "*.cfg"))
            {
                var item = new ToolStripMenuItem(Path.GetFileName(file));
                item.Click += (s, ev) => LoadConfigFromFile(file);
                menu.Items.Add(item);
            }
            menu.Show(menuStrip1, new System.Drawing.Point(0, menuStrip1.Height));
        }
        */

        // Заглушки, чтобы проект собрался
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

                // Обновляем только статусную строку внизу формы
                lblStatus.Text = $"Done. Loaded: {loadedCount}, Cleared: {deletedCount}";
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Error during file operations!";
                // Ошибки лучше всё же оставлять в MessageBox, чтобы понимать, если что-то пошло не так (например, доступ запрещен)
                MessageBox.Show($"crtkl error (phobos reference): {ex.Message}");
            }
        }
        private void btnSaveCurrent_Click(object sender, EventArgs e)
        {
            // 1. Проверки директории
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
                // 2. Генерируем строку даты и времени в твоем формате: 2026-01-08 00h21m16s
                // Используем экранирование для букв h, m, s, чтобы они не превратились в коды времени
                string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH'h'mm'm'ss's'");

                // 3. Формируем базовый путь до папки конфига
                string baseSavesPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Saves");

                string safeGameName = string.Join("_", currentConfig.GameName.Split(Path.GetInvalidFileNameChars()));
                string safeCategoryName = string.Join("_", currentConfig.CategoryName.Split(Path.GetInvalidFileNameChars()));
                string configFileName = Path.GetFileNameWithoutExtension(currentConfigPath);

                // 4. Добавляем папку с датой в конец пути
                string targetPath = Path.Combine(
                    baseSavesPath,
                    safeGameName,
                    safeCategoryName,
                    configFileName,
                    timestamp
                );

                // 5. Создаем дерево директорий (включая папку с датой)
                if (!Directory.Exists(targetPath))
                {
                    Directory.CreateDirectory(targetPath);
                }

                // 6. Копируем файлы
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

                lblStatus.Text = $"Backup created: {timestamp} ({backedUpCount} files)";
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Backup Error!";
                MessageBox.Show($"error backing up saves: {ex.Message}");
            }
        }
        private void btnClearFolder_Click(object sender, EventArgs e)
        {
            // 1. Берем данные из текущего конфига
            string targetDir = currentConfig.SavesDirectory;
            string prefix = currentConfig.Prefix;
            string postfix = currentConfig.Postfix;

            // 2. Проверка пути
            if (string.IsNullOrEmpty(targetDir) || !Directory.Exists(targetDir))
            {
                lblStatus.Text = "Error: Invalid Saves Directory!";
                return;
            }

            int deletedCount = 0;

            try
            {
                // Проходим по слотам (обычно их 8)
                // Если хочешь привязаться строго к текущей странице:
                var page = currentConfig.Pages[currentPageIndex];

                foreach (var save in page.Saves)
                {
                    // Формируем имя: GTAVCsf + Номер слота + .b
                    string fileName = $"{prefix}{save.Slot}{postfix}";
                    string fullPath = Path.Combine(targetDir, fileName);

                    // Удаляем только если файл существует
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

        private void LoadSingle(int slot) { /* Логика Copy файла */ }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            btnSaveConfig_Click(sender, e);
        }

        // private void openToolStripMenuItem_Click(object sender, EventArgs e)
        // {
        //     btnOpen_Click(sender, e);
        // }

        private void settingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FormSettings settingsForm = new FormSettings(this);

            // Устанавливаем режим центрирования относительно родительского окна
            settingsForm.StartPosition = FormStartPosition.CenterParent;

            settingsForm.ShowDialog();
        }

        private void closeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Вместо Application.Exit() теперь вызываем очистку
            if (MessageBox.Show("Are you sure you want to clear everything and start a new config?",
                                "New Config", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                CreateNewConfig();
            }
        }

        private void openToolStripMenuItem_MouseEnter(object sender, EventArgs e)
        {
            // 1. Очищаем старые пункты, чтобы список не дублировался
            openToolStripMenuItem.DropDownItems.Clear();

            // 2. Проверяем наличие папки Configs
            string configDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Configs");
            if (!Directory.Exists(configDir)) Directory.CreateDirectory(configDir);

            // 3. Ищем все файлы .cfg и добавляем их в меню
            var files = Directory.GetFiles(configDir, "*.cfg");
            foreach (var file in files)
            {
                var fileItem = new ToolStripMenuItem(Path.GetFileName(file));
                fileItem.Click += (s, ev) => LoadConfigFromFile(file); // При клике — загружаем
                openToolStripMenuItem.DropDownItems.Add(fileItem);
            }

            // 4. Добавляем разделитель, если файлы есть
            if (files.Length > 0) openToolStripMenuItem.DropDownItems.Add(new ToolStripSeparator());

            // 5. Добавляем кнопку "Browse..." в самый низ
            var browseItem = new ToolStripMenuItem("Browse...");
            browseItem.Click += (s, ev) => {
                using (OpenFileDialog ofd = new OpenFileDialog())
                {
                    ofd.InitialDirectory = configDir; // Путь по умолчанию
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
                // Запускаем процесс проводника с указанным путем
                Process.Start("explorer.exe", path);
            }
            else
            {
                MessageBox.Show("Directory not provided. Edit config settings first.", "smh");
            }
        }

        private void btnOpenProgramFolder_Click(object sender, EventArgs e)
        {
            // Получаем путь к папке, где лежит запущенный .exe
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
                // Это маловероятный сценарий, но для надежности:
                lblStatus.Text = "couldn't find the folder. somehow";
            }
        }

        private void infoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Создаем экземпляр новой формы
            using (FormInfo infoForm = new FormInfo())
            {
                // Открываем как модальное окно (пользователь не сможет кликать на основную форму, пока не закроет эту)
                infoForm.ShowDialog(this);
            }
        }
    }
}