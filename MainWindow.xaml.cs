using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ConvertCodeToChatGPT
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Browse_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog();
            dialog.Title = "Chọn thư mục dự án";
            dialog.CheckFileExists = false;
            dialog.CheckPathExists = true;
            dialog.FileName = "Chọn thư mục";
            dialog.Filter = "All files (*.*)|*.*";
            dialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            dialog.Multiselect = false;

            if (dialog.ShowDialog() == true)
            {
                txtProjectPath.Text = dialog.FileName;
            }
        }

        private void Merge_Click(object sender, RoutedEventArgs e)
        {
            string projectPath = txtProjectPath.Text;
            if (string.IsNullOrWhiteSpace(projectPath))
            {
                MessageBox.Show("Vui lòng chọn đường dẫn đến dự án.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!Directory.Exists(projectPath))
            {
                MessageBox.Show("Đường dẫn dự án không tồn tại.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            try
            {
                // Chuỗi tạm thời để lưu trữ dữ liệu trước khi hợp nhất
                StringBuilder previewData = new StringBuilder();

                // Lấy danh sách tất cả các tệp .cs trong dự án
                string[] csFiles = Directory.GetFiles(projectPath, "*.cs", SearchOption.AllDirectories);
                foreach (string csFile in csFiles)
                {
                    // Kiểm tra tên thư mục cha của tệp
                    string parentFolder = Directory.GetParent(csFile).Name;
                    if (csFile.Contains("bin", StringComparison.OrdinalIgnoreCase) || csFile.Contains("obj", StringComparison.OrdinalIgnoreCase) || csFile.Contains("Migrations", StringComparison.OrdinalIgnoreCase))
                        continue;

                    previewData.AppendLine($"// File: {csFile}");
                    previewData.AppendLine(File.ReadAllText(csFile));
                    previewData.AppendLine(); // Thêm một dòng trống sau mỗi file .cs
                }

                // Lấy nội dung các tệp .json và .cd và thêm vào chuỗi tạm thời
                string[] otherFiles = Directory.GetFiles(projectPath, "*.json", SearchOption.AllDirectories);
                foreach (string file in otherFiles)
                {
                    // Kiểm tra tên thư mục cha của tệp
                    string parentFolder = Directory.GetParent(file).Name;
                    if (file.Contains("bin", StringComparison.OrdinalIgnoreCase) || file.Contains("obj", StringComparison.OrdinalIgnoreCase) || file.Contains("Migrations", StringComparison.OrdinalIgnoreCase))
                        continue;

                    previewData.AppendLine($"// File: {file}");
                    previewData.AppendLine(File.ReadAllText(file));
                    previewData.AppendLine(); // Thêm một dòng trống sau mỗi file
                }

                // Hiển thị dữ liệu trước khi hợp nhất trong TextBox
                txtOutput.Text = previewData.ToString();

                MessageBox.Show("Dữ liệu đã được xem trước thành công.", "Hoàn thành", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Đã xảy ra lỗi: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Phương thức cập nhật ProgressBar
        private void UpdateProgressBar(int filesProcessed, int totalFiles)
        {
            // Sử dụng Dispatcher để cập nhật giao diện người dùng từ luồng khác
            Dispatcher.Invoke(() =>
            {
                double progress = (double)filesProcessed / totalFiles * 100;
                progressBar.Value = progress;
            });
        }

        private void Raw(object sender, RoutedEventArgs e)
        {
            string projectPath = txtProjectPath.Text;
            if (string.IsNullOrWhiteSpace(projectPath))
            {
                MessageBox.Show("Vui lòng chọn đường dẫn đến dự án.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!Directory.Exists(projectPath))
            {
                MessageBox.Show("Đường dẫn dự án không tồn tại.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            txtOutput.Text = GetFolderStructure(projectPath);
            MessageBox.Show("Xây dựng cấu trúc folder thành công", "Hoàn thành", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        static string GetFolderStructure(string folderPath)
        {
            StringBuilder sb = new StringBuilder();

            DirectoryInfo dirInfo = new DirectoryInfo(folderPath);
            BuildFolderStructure(dirInfo, 0, sb);

            return sb.ToString();
        }
        static void BuildFolderStructure(DirectoryInfo dirInfo, int indentLevel, StringBuilder sb)
        {
            string[] ignoreFolders = { ".vs", ".github", "bin", "obj", "Properties", ".git" };
            if (Array.Exists(ignoreFolders, folder => folder.Equals(dirInfo.Name, StringComparison.OrdinalIgnoreCase)))
            {
                return;
            }
            // Thêm tên của thư mục vào StringBuilder
            sb.AppendLine(new string(' ', indentLevel * 2) + "|-- " + dirInfo.Name);

            // Lấy tất cả các file trong thư mục hiện tại
            FileInfo[] files = dirInfo.GetFiles();
            foreach (FileInfo file in files)
            {
                sb.AppendLine(new string(' ', (indentLevel + 1) * 2) + "|-- " + file.Name);
            }

            // Lấy tất cả các thư mục con trong thư mục hiện tại
            DirectoryInfo[] subDirs = dirInfo.GetDirectories();
            foreach (DirectoryInfo subDir in subDirs)
            {
                // Gọi đệ quy để xây dựng cấu trúc của thư mục con
                BuildFolderStructure(subDir, indentLevel + 1, sb);
            }
        }
    }
}
