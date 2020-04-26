using System.Windows;
using System;
using System.IO;
using System.IO.Compression;
using System.Diagnostics;
using System.Collections.Generic;
using System.Xml.Linq;

namespace WpfApp1
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
        public static void ExecuteCommand(string command)
        {
            var processInfo = new ProcessStartInfo("cmd.exe", "/c " + command);
            processInfo.CreateNoWindow = true;
            processInfo.UseShellExecute = false;
            processInfo.RedirectStandardError = true;
            processInfo.RedirectStandardOutput = true;

            var process = Process.Start(processInfo);
            process.OutputDataReceived += (object sender, DataReceivedEventArgs e) =>
            process.BeginOutputReadLine();

            process.ErrorDataReceived += (object sender, DataReceivedEventArgs e) =>
            process.BeginErrorReadLine();

            process.WaitForExit();

            Console.WriteLine("ExitCode: {0}", process.ExitCode);
            process.Close();
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string zipPath = UnZip.Text;
            string unzipPath = @".\" + Path.GetFileNameWithoutExtension(zipPath);
            if (Directory.Exists(unzipPath))
            {
                Directory.Delete(unzipPath, true);
            }
            XDocument xdoc = XDocument.Load("Jer_Schema.xml");
            List<JerSchema> list_jerSchema = new List<JerSchema>();
            var xml_content = xdoc.Descendants();
            Dictionary<String, string> file_to_dir = new Dictionary<string, string>();
            Directory.CreateDirectory(unzipPath);
            foreach (var xe in xdoc.Descendants())
            {
                if (xe.Attribute("Directory_Name")?.Value != null && xe.Attribute("name")?.Value != null)
                {
                    file_to_dir.Add(xe.Attribute("name")?.Value, xe.Attribute("Directory_Name")?.Value);

                    list_jerSchema.Add(new JerSchema(xe.Attribute("name")?.Value, xe.Attribute("Directory_Name")?.Value,
                                                     xe.Attribute("decodingext")?.Value));
                }
                if (xe.Attribute("Directory_Name")?.Value != null)
                {
                    Directory.CreateDirectory(Path.Combine(unzipPath, xe.Attribute("Directory_Name")?.Value));
                }
            }
            ZipFile.ExtractToDirectory(@".\" + zipPath, unzipPath);

            string[] files = Directory.GetFiles(unzipPath, "*.*", SearchOption.AllDirectories);
            string TELg2Txt = "TELg2Txt.exe";
            var file_names = file_to_dir.Keys;
            
            foreach (var file in files)
            {
                foreach (var jerFile in list_jerSchema)
                {
                    if (file.Contains(jerFile.Name))
                    {
                        string temp;
                        temp = TELg2Txt + " -in " + "\"" + file + "\"" + " -out " + "\"" + Path.Combine(unzipPath, jerFile.Dir_name, Path.GetFileName(file)) + ".txt" + "\"";
                        ExecuteCommand(temp);
                    }
                }
            }
            MessageBox.Show("DONE !!!!!!!!!!!!");
        }
    }
}
