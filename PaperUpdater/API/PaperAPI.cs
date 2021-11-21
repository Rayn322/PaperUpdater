using PaperUpdater.API;
using System;
using System.ComponentModel;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace PaperUpdater {

    internal class PaperAPI {
        private static HttpClient APIClient { get; set; }
        private static int Progress;

        public static void InitializeClient() {
            APIClient = new HttpClient();
            APIClient.DefaultRequestHeaders.Accept.Clear();
            APIClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public static async Task<VersionList> GetVersionList() {
            using HttpResponseMessage response = await APIClient.GetAsync("https://papermc.io/api/v2/projects/paper");
            if (response.IsSuccessStatusCode) {
                VersionList versionList = await response.Content.ReadAsAsync<VersionList>();
                return versionList;
            }

            throw new Exception(response.ReasonPhrase);
        }

        public static async Task<BuildList> GetBuildList() {
            using HttpResponseMessage response = await APIClient.GetAsync("https://papermc.io/api/v2/projects/paper/versions/" + Program.McVersion);
            if (response.IsSuccessStatusCode) {
                BuildList buildList = await response.Content.ReadAsAsync<BuildList>();
                return buildList;
            }

            throw new Exception(response.ReasonPhrase);
        }

        public static void DownloadJar(Uri uri, string fileName) {
            // TODO: switch to HTTPClient
            WebClient client = new WebClient();
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