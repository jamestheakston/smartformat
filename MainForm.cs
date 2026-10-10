using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SmartFormat
{
    public partial class MainForm : Form
    {
        private ComboBox? cmbDrives;
        private Button? btnRefresh;
        private CheckBox? chkPreserveImages;
        private CheckBox? chkPreserveDocuments;
        private CheckBox? chkPreserveAudioVideo;
        private ProgressBar? progressBar;
        private Button? btnSmartFormat;
        private Label? lblStatus;
        private Label? lblDrives;
        private Label? lblPreserve;
        private CancellationTokenSource? _cancellationTokenSource;

        private static readonly string[] SystemFilesToDelete = new[]
        {
            "$RECYCLE.BIN",
            "System Volume Information",
            ".Trashes",
            ".Spotlight-V100",
            ".fseventsd",
            ".DocumentRevisions-V100",
            "FOUND.000",
            "FOUND.001",
            "FOUND.002",
            "desktop.ini",
            "Thumbs.db",
            ".DS_Store",
            ".TemporaryItems",
            "RECYCLER"
        };

        private static readonly string[] JunkFoldersToDelete = new[]
        {
            "$RECYCLE.BIN",
            "System Volume Information",
            ".Trashes",
            ".Spotlight-V100",
            ".fseventsd",
            ".DocumentRevisions-V100",
            "FOUND.000",
            "FOUND.001",
            "FOUND.002",
            "RECYCLER",
            ".TemporaryItems"
        };

        private static readonly string[] ImageExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".tiff", ".tif", ".webp", ".ico", ".svg", ".raw", ".cr2", ".nef", ".arw" };
        private static readonly string[] DocumentExtensions = new[] { ".doc", ".docx", ".pdf", ".txt", ".rtf", ".odt", ".xls", ".xlsx", ".ppt", ".pptx", ".csv", ".json", ".xml", ".html", ".md" };
        private static readonly string[] AudioVideoExtensions = new[] { ".mp3", ".mp4", ".avi", ".mkv", ".mov", ".wmv", ".flv", ".wav", ".aac", ".ogg", ".m4a", ".m4v", ".flac", ".wma" };

        public MainForm()
        {
            InitializeComponent();
            LoadDrives();
        }

        private void InitializeComponent()
        {
            this.Text = "Smart Format - USB Cleaner";
            this.Size = new System.Drawing.Size(500, 450);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            lblDrives = new Label
            {
                Text = "Select USB Drive:",
                Location = new System.Drawing.Point(20, 20),
                Size = new System.Drawing.Size(150, 23),
                Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold)
            };

            cmbDrives = new ComboBox
            {
                Location = new System.Drawing.Point(20, 50),
                Size = new System.Drawing.Size(320, 23),
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            btnRefresh = new Button
            {
                Text = "Refresh",
                Location = new System.Drawing.Point(350, 48),
                Size = new System.Drawing.Size(100, 27),
                UseVisualStyleBackColor = true
            };
            btnRefresh.Click += BtnRefresh_Click;

            lblPreserve = new Label
            {
                Text = "Files to Preserve:",
                Location = new System.Drawing.Point(20, 90),
                Size = new System.Drawing.Size(150, 23),
                Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold)
            };

            chkPreserveImages = new CheckBox
            {
                Text = "Images (.jpg, .png, etc.)",
                Location = new System.Drawing.Point(20, 120),
                Size = new System.Drawing.Size(200, 23),
                Checked = true
            };

            chkPreserveDocuments = new CheckBox
            {
                Text = "Documents (.doc, .pdf, etc.)",
                Location = new System.Drawing.Point(20, 150),
                Size = new System.Drawing.Size(220, 23),
                Checked = true
            };

            chkPreserveAudioVideo = new CheckBox
            {
                Text = "Audio/Video (.mp3, .mp4, etc.)",
                Location = new System.Drawing.Point(20, 180),
                Size = new System.Drawing.Size(230, 23),
                Checked = true
            };

            progressBar = new ProgressBar
            {
                Location = new System.Drawing.Point(20, 230),
                Size = new System.Drawing.Size(430, 23),
                Style = ProgressBarStyle.Continuous
            };

            lblStatus = new Label
            {
                Text = "Ready",
                Location = new System.Drawing.Point(20, 260),
                Size = new System.Drawing.Size(430, 23),
                ForeColor = System.Drawing.Color.Gray
            };

            btnSmartFormat = new Button
            {
                Text = "Smart Format",
                Location = new System.Drawing.Point(20, 300),
                Size = new System.Drawing.Size(430, 45),
                UseVisualStyleBackColor = true,
                Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold),
                BackColor = System.Drawing.Color.FromArgb(0, 120, 215),
                ForeColor = System.Drawing.Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnSmartFormat.FlatAppearance.BorderSize = 0;
            btnSmartFormat.Click += BtnSmartFormat_Click;

            this.Controls.Add(lblDrives);
            this.Controls.Add(cmbDrives);
            this.Controls.Add(btnRefresh);
            this.Controls.Add(lblPreserve);
            this.Controls.Add(chkPreserveImages);
            this.Controls.Add(chkPreserveDocuments);
            this.Controls.Add(chkPreserveAudioVideo);
            this.Controls.Add(progressBar);
            this.Controls.Add(lblStatus);
            this.Controls.Add(btnSmartFormat);
        }

        private void BtnRefresh_Click(object? sender, EventArgs e)
        {
            LoadDrives();
            lblStatus!.Text = "Drive list refreshed";
        }

        private void LoadDrives()
        {
            cmbDrives!.Items.Clear();
            
            try
            {
                var drives = DriveInfo.GetDrives()
                    .Where(d => d.DriveType == DriveType.Removable && d.IsReady)
                    .Select(d => $"{d.Name} ({d.VolumeLabel}) - {FormatBytes(d.TotalFreeSpace)} free")
                    .ToList();

                if (drives.Count == 0)
                {
                    cmbDrives.Items.Add("No removable drives found");
                    cmbDrives.SelectedIndex = 0;
                    btnSmartFormat!.Enabled = false;
                }
                else
                {
                    foreach (var drive in drives)
                    {
                        cmbDrives.Items.Add(drive);
                    }
                    cmbDrives.SelectedIndex = 0;
                    btnSmartFormat!.Enabled = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading drives: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                cmbDrives.Items.Add("Error loading drives");
                cmbDrives.SelectedIndex = 0;
                btnSmartFormat!.Enabled = false;
            }
        }

        private string FormatBytes(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            int order = 0;
            double size = bytes;
            while (size >= 1024 && order < sizes.Length - 1)
            {
                order++;
                size /= 1024;
            }
            return $"{size:0.##} {sizes[order]}";
        }

        private async void BtnSmartFormat_Click(object? sender, EventArgs e)
        {
            if (cmbDrives!.SelectedItem == null || cmbDrives.SelectedItem.ToString()!.StartsWith("No"))
            {
                MessageBox.Show("Please select a valid USB drive.", "No Drive Selected", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var selectedDriveText = cmbDrives.SelectedItem.ToString()!;
            var driveLetter = selectedDriveText.Substring(0, 3);

            var result = MessageBox.Show(
                $"Are you sure you want to clean drive {driveLetter}?\n\n" +
                "This will delete system files, junk folders, and hidden caches.\n" +
                "Your personal files will be preserved based on your selections.",
                "Confirm Smart Format",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result != DialogResult.Yes)
            {
                return;
            }

            btnSmartFormat!.Enabled = false;
            btnRefresh!.Enabled = false;
            cmbDrives.Enabled = false;
            _cancellationTokenSource = new CancellationTokenSource();

            try
            {
                await PerformSmartFormat(driveLetter, _cancellationTokenSource.Token);
                MessageBox.Show("Smart Format completed successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (OperationCanceledException)
            {
                MessageBox.Show("Operation was cancelled.", "Cancelled", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error during Smart Format: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnSmartFormat.Enabled = true;
                btnRefresh.Enabled = true;
                cmbDrives.Enabled = true;
                progressBar!.Value = 0;
                lblStatus!.Text = "Ready";
                _cancellationTokenSource?.Dispose();
                _cancellationTokenSource = null;
            }
        }

        private async Task PerformSmartFormat(string driveLetter, CancellationToken cancellationToken)
        {
            await Task.Run(() =>
            {
                var driveInfo = new DriveInfo(driveLetter);

                if (driveInfo.DriveType != DriveType.Removable)
                {
                    throw new InvalidOperationException("Selected drive is not a removable drive. Operation cancelled for safety.");
                }

                if (driveLetter.StartsWith("C:", StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidOperationException("Cannot format the C: drive (OS drive). Operation cancelled for safety.");
                }

                var rootPath = driveLetter;
                var filesToPreserve = new HashSet<string>();

                if (chkPreserveImages!.Checked)
                {
                    foreach (var ext in ImageExtensions)
                    {
                        filesToPreserve.Add(ext.ToLowerInvariant());
                    }
                }

                if (chkPreserveDocuments!.Checked)
                {
                    foreach (var ext in DocumentExtensions)
                    {
                        filesToPreserve.Add(ext.ToLowerInvariant());
                    }
                }

                if (chkPreserveAudioVideo!.Checked)
                {
                    foreach (var ext in AudioVideoExtensions)
                    {
                        filesToPreserve.Add(ext.ToLowerInvariant());
                    }
                }

                this.Invoke((MethodInvoker)(() =>
                {
                    progressBar!.Value = 5;
                    lblStatus!.Text = "Scanning drive...";
                }));

                var allItems = Directory.GetFileSystemEntries(rootPath, "*", SearchOption.AllDirectories);
                var totalItems = allItems.Length;
                var processedItems = 0;

                foreach (var item in allItems)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    var isDirectory = Directory.Exists(item);
                    var fileName = Path.GetFileName(item);
                    var extension = Path.GetExtension(item).ToLowerInvariant();

                    bool shouldDelete = false;

                    if (isDirectory)
                    {
                        if (JunkFoldersToDelete.Contains(fileName, StringComparer.OrdinalIgnoreCase))
                        {
                            shouldDelete = true;
                        }
                    }
                    else
                    {
                        if (SystemFilesToDelete.Contains(fileName, StringComparer.OrdinalIgnoreCase))
                        {
                            shouldDelete = true;
                        }
                        else if (!filesToPreserve.Contains(extension))
                        {
                            shouldDelete = true;
                        }
                    }

                    if (shouldDelete)
                    {
                        try
                        {
                            if (isDirectory)
                            {
                                DeleteDirectoryRecursively(item);
                            }
                            else
                            {
                                File.SetAttributes(item, FileAttributes.Normal);
                                File.Delete(item);
                            }
                        }
                        catch (UnauthorizedAccessException)
                        {
                        }
                        catch (IOException)
                        {
                        }
                    }

                    processedItems++;
                    var progress = (int)((double)processedItems / totalItems * 95) + 5;
                    if (progress > 100) progress = 100;

                    this.Invoke((MethodInvoker)(() =>
                    {
                        progressBar!.Value = progress;
                        lblStatus!.Text = $"Processing: {processedItems}/{totalItems} items";
                    }));
                }

                this.Invoke((MethodInvoker)(() =>
                {
                    progressBar!.Value = 100;
                    lblStatus!.Text = "Smart Format complete!";
                }));

            }, cancellationToken);
        }

        private void DeleteDirectoryRecursively(string path)
        {
            try
            {
                foreach (var dir in Directory.GetDirectories(path))
                {
                    DeleteDirectoryRecursively(dir);
                }

                foreach (var file in Directory.GetFiles(path))
                {
                    try
                    {
                        File.SetAttributes(file, FileAttributes.Normal);
                        File.Delete(file);
                    }
                    catch
                    {
                    }
                }

                Directory.Delete(path, true);
            }
            catch
            {
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
            base.OnFormClosing(e);
        }
    }
}
