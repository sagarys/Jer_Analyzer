using System.Windows;
using System;
using System.IO;
using System.IO.Compression;
using System.Diagnostics;
using System.Collections.Generic;
using System.Xml.Linq;
using System.Windows.Input;
using System.Threading.Tasks;

namespace Jer_Analyzer
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
        private void DeleteEmptyDirectory(string dirPath)
        {
            if (Directory.Exists(dirPath))
            {
                //Delete all child Directories if they are empty
                foreach (string subdirectory in Directory.GetDirectories(dirPath))
                {
                    string[] file = Directory.GetFiles(subdirectory, "*.*");
                    if (file.Length == 0)
                        Directory.Delete(subdirectory);
                }
            }
        }
        public static string LOGS_OF_INTREST = "LOGS_OF_INTREST";
        private void findKeyowrd(string decFileName, List<string> keywords)
        {
            try
            {
                // Create an instance of StreamReader to read from a file.
                // The using statement also closes the StreamReader.
                using (StreamReader sr = new StreamReader(decFileName))
                {
                    string line;

                    // Read and display lines from the file until 
                    // the end of the file is reached. 
                    while ((line = sr.ReadLine()) != null)
                    {
                        bool found = false;
                        var foundKeyword = "";
                        foreach (var keyword in keywords)
                        {
                            if (line.Contains(keyword))
                            {
                                found = true;
                                foundKeyword = keyword;
                                break;
                            }
                        }
                        if (found)
                        {
                            var logsOfIntrestDirPath = Path.Combine(Path.GetDirectoryName(Path.GetDirectoryName(decFileName)), LOGS_OF_INTREST);
                            File.Copy(decFileName, Path.Combine(logsOfIntrestDirPath, Path.GetFileNameWithoutExtension(decFileName) + "_" + foundKeyword + Path.GetExtension(decFileName)));
                            break;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                // Let the user know what went wrong.
                Console.WriteLine("The file could not be read:");
                Console.WriteLine(e.Message);
            }
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
            Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait; // set the cursor to loading spinner
            var list = new List<Task>();
            string zipPath = UnZip.Text;
            string unzipPath = @".\" + Path.GetFileNameWithoutExtension(zipPath);
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            if (Directory.Exists(unzipPath))
            {
                Directory.Delete(unzipPath, true);
            }
            XDocument xdoc = XDocument.Load(@".\src\Jer_Schema.xml");
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
                                                      xe.Attribute("DecodingExt")?.Value, xe.Attribute("keyword")?.Value));
                }
                if (xe.Attribute("Directory_Name")?.Value != null)
                {
                    Directory.CreateDirectory(Path.Combine(unzipPath, xe.Attribute("Directory_Name")?.Value));
                }
            }
            Directory.CreateDirectory(Path.Combine(unzipPath, LOGS_OF_INTREST));
            ZipFile.ExtractToDirectory(@".\" + zipPath, unzipPath);

            string[] files = Directory.GetFiles(unzipPath, "*.*", SearchOption.AllDirectories);
            string TELg2Txt = @".\src\TELg2Txt.exe";
            var file_names = file_to_dir.Keys;
            foreach (var file in files)
            {
                foreach (var jerFile in list_jerSchema)
                {
                    string temp = null;
                    if (jerFile?.Dec_name == "None" && file.Contains(jerFile.Name))
                    {
                        temp = "copy /y " + file + " " + "\"" + Path.Combine(unzipPath, jerFile.Dir_name) + "\"";
                        var t = new Task(() =>
                        {
                            ExecuteCommand(temp);
                        });
                        list.Add(t);
                        t.Start();
                        continue;
                    }
                    if (file.Contains(jerFile.Name))
                    {
                        temp = TELg2Txt + " -in " + "\"" + file + "\"" + " -out " + "\"" + Path.Combine(unzipPath, jerFile.Dir_name, Path.GetFileName(file)) + ".txt" + "\"";
                        var t = new Task(() =>
                        {
                            ExecuteCommand(temp);
                            if (jerFile.Keyword != null)
                            {
                                findKeyowrd(Path.Combine(unzipPath, jerFile.Dir_name, Path.GetFileName(file)) + ".txt", jerFile.Keyword);
                            }
                        });
                        list.Add(t);
                        t.Start();
                    }
                }
            }
            Task.WaitAll(list.ToArray());
            DeleteEmptyDirectory(unzipPath);

            Mouse.OverrideCursor = System.Windows.Input.Cursors.Arrow; // set the cursor back to arrow
            stopwatch.Stop();

            MessageBox.Show("Done and Time Taken is :- " + stopwatch.Elapsed);
        }
    }
}
