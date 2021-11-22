using System;
using System.ComponentModel;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using PaperUpdater.API;

namespace PaperUpdater {
    internal class PaperAPI {
        private static int Progress;
        private static HttpClient APIClient { get; set; }

        public static void InitializeClient() {
            APIClient = new HttpClient();
            APIClient.DefaultRequestHeaders.Accept.Clear();
            APIClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public static async Task<VersionList> GetVersionList() {
            using HttpResponseMessage response = await APIClient.GetAsync("https://papermc.io/api/v2/projects/paper");
            if (!response.IsSuccessStatusCode) throw new Exception(response.ReasonPhrase);
            VersionList versionList = await response.Content.ReadAsAsync<VersionList>();
            return versionList;
        }

        public static async Task<BuildList> GetBuildList() {
            using HttpResponseMessage response =
                await APIClient.GetAsync("https://papermc.io/api/v2/projects/paper/versions/" + Program.McVersion);
            if (!response.IsSuccessStatusCode) throw new Exception(response.ReasonPhrase);
            BuildList buildList = await response.Content.ReadAsAsync<BuildList>();
            return buildList;
        }

        public static void DownloadJar(Uri uri, string fileName) {
            WebClient client = new();
            client.DownloadFileCompleted += OnDownloadComplete;
            client.DownloadProgressChanged += OnDownloadProgressChanged;
            client.DownloadFileAsync(uri, fileName);
        }

        private static void OnDownloadComplete(object sender, AsyncCompletedEventArgs e) {
            if (e.Cancelled) {
                Console.WriteLine("File download cancelled.");
            } else if (e.Error != null) {
                Console.WriteLine(e.Error.ToString());
            } else {
                Console.WriteLine($"Download Successful! File saved to {Program.PaperPath}");
                Program.EditStartScript();
            }
        }

        private static void OnDownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e) {
            if (e.ProgressPercentage > Progress) {
                Progress = e.ProgressPercentage;
                Console.WriteLine($"{Progress}% complete");
            }
        }
    }
}