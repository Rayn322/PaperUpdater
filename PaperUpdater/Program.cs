using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading;
using Modern.Forms;
using PaperUpdater.API;

namespace PaperUpdater {
    internal class Program {
        private static readonly Form Form = new();
        public static string PaperPath { get; set; }
        public static string OldPaperPath { get; set; }
        public static string McVersion { get; private set; }

        private static void Main() {
            if (NetworkInterface.GetIsNetworkAvailable()) {
                PaperAPI.InitializeClient();
                SelectPaper();
                DownloadPaper();
            } else {
                Console.WriteLine("No internet connection detected!");
                UpdateFinish();
            }

            Process.GetCurrentProcess().WaitForExit();
        }

        private static void SelectPaper() {
            Thread thread = new(() => {
                OpenFileDialog openFileDialog = new();

                openFileDialog.AddFilter("Paper Jar (*.jar)", "jar");
                openFileDialog.Title = "Select your Paper jar file";
                openFileDialog.AllowMultiple = false;

                if (openFileDialog.ShowDialog(Form).Result == DialogResult.OK) {
                    PaperPath = OldPaperPath = openFileDialog.FileName;
                } else {
                    Environment.Exit(0);
                }
            });

            if (OperatingSystem.IsWindows()) thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();

            Console.WriteLine("Selected file: " + PaperPath);
        }

        private static void DownloadPaper() {
            try {
                // assuming that the most recent version is last in the array, this will get the most recent Minecraft version
                VersionList versionList = PaperAPI.GetVersionList().Result;
                int i = versionList.Versions.Length;
                McVersion = versionList.Versions[i - 1];
            } catch {
                Console.WriteLine("Could not connect to Paper!");
                UpdateFinish();
                return;
            }

            BuildList buildList = PaperAPI.GetBuildList().Result;
            int[] builds = buildList.Builds;
            int latest = builds.Max();
            string url =
                $"https://papermc.io/api/v2/projects/paper/versions/{McVersion}/builds/{latest}/downloads/paper-{McVersion}-{latest}.jar";

            // changes name of file to align with new version
            string fileName = Path.GetFileName(PaperPath);
            File.Delete(PaperPath);
            PaperPath = PaperPath.Replace(fileName, $"paper-{McVersion}-{latest}.jar");

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

            UpdateFinish();
        }

        public static void UpdateFinish() {
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
            Environment.Exit(0);
        }
    }
}