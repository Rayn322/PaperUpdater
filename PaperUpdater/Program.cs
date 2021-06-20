using PaperUpdater.API;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace PaperUpdater {

    internal class Program {
        public static string PaperPath { get; set; }
        public static string OldPaperPath { get; set; }
        public static string MCVersion { get; } = "1.17";

        private static void Main(string[] args) {
            PaperAPI.InitializeClient();
            SelectPaper();
            DownloadPaper();
            Process.GetCurrentProcess().WaitForExit();
        }

        private static void SelectPaper() {
            Thread thread = new Thread(() => {
                OpenFileDialog openFileDialog = new OpenFileDialog();

                openFileDialog.Filter = "Paper Jar (*.jar)|*.jar";
                openFileDialog.Title = "Select your Paper jar file";

                if (openFileDialog.ShowDialog() == DialogResult.OK) {
                    PaperPath = openFileDialog.FileName;
                    OldPaperPath = openFileDialog.FileName;
                } else {
                    Environment.Exit(0);
                }
            });

            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();

            Console.WriteLine("Selected file: " + PaperPath);
        }

        private static void DownloadPaper() {
            VersionList versionList = PaperAPI.GetVersionList().Result;
            int[] builds = versionList.Builds;
            int latest = builds.Max();
            string url = $"https://papermc.io/api/v2/projects/paper/versions/{MCVersion}/builds/{latest}/downloads/paper-{MCVersion}-{latest}.jar";

            File.Delete(PaperPath);
            // changes name of file to align with new version
            string fileName = Path.GetFileName(PaperPath);
            PaperPath = PaperPath.Replace(fileName, $"paper-{MCVersion}-{latest}.jar");

            Console.WriteLine($"Downloading from: {url}");
            PaperAPI.DownloadJar(new Uri(url), PaperPath);
        }

        public static void EditStartScript() {
            string paperFolder = Path.GetDirectoryName(PaperPath);
            string scriptPath;

            if (File.Exists(Path.Combine(paperFolder, "run.bat"))) {
                scriptPath = Path.Combine(paperFolder, "run.bat");
                
            } else if (File.Exists(Path.Combine(paperFolder, "start.bat"))) {
                scriptPath = Path.Combine(paperFolder, "start.bat");

            } else {
                Console.WriteLine("Could not find start script");
                return;
            }

            string text = File.ReadAllText(scriptPath);
            text = text.Replace(Path.GetFileName(OldPaperPath), Path.GetFileName(PaperPath));
            File.WriteAllText(scriptPath, text);

            Console.WriteLine("Successfully updated start script.");
        }
    }
}