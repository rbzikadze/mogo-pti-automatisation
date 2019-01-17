using HtmlAgilityPack;
using MogoMyPti.Entities;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace MogoMyPti
{
    public class MyPtiGrabber
    {
        public bool LoggedIn { get; set; } = false;
        private string username;
        private string password;
        private CookieCollection cookies;

        public MyPtiGrabber(string username, string password)
        {
            this.username = username;
            this.password = password;
        }

        public async Task Login()
        {
            using (var httpHandler = new HttpClientHandler())
            {
                httpHandler.CookieContainer = new System.Net.CookieContainer();
                httpHandler.AllowAutoRedirect = true;

                using (var client = new HttpClient(httpHandler) { })
                {

                    var doc = new HtmlDocument();
                    var httpResult = client.GetAsync("http://my.pti.ge/login").Result;

                    if (httpResult.StatusCode == HttpStatusCode.Forbidden || httpResult.StatusCode == HttpStatusCode.Redirect)
                    {
                        var uri = new Uri("http://my.pti.ge/login");
                        httpResult = client.GetAsync(uri).Result;
                    }

                    doc.LoadHtml(await httpResult.Content.ReadAsStringAsync());

                    var token = doc.DocumentNode.Descendants("input").Where(x => x.Attributes["name"].Value == "_token").FirstOrDefault().Attributes["value"].Value;

                    var content = new FormUrlEncodedContent(new[]
             {
                new KeyValuePair<string, string>("email", username),
                new KeyValuePair<string, string>("password", password),
                                new KeyValuePair<string, string>("_token", token),
                });

                    var loginResponse = client.PostAsync("http://my.pti.ge/login", content).Result.Content.ReadAsStringAsync().Result;
                    if (loginResponse.ToLower().Contains("sorry") || loginResponse.ToLower().Contains("პაროლი"))
                        throw new LoginException();
                    cookies = httpHandler.CookieContainer.GetCookies(new Uri("http://my.pti.ge"));
                }
            }
        }

        public async Task<List<ScrapeResult>> GrabDataFromPtiAsync(List<string> licenseNumbers)
        {
            var result = new List<ScrapeResult>();
            var tasksList = new Dictionary<string, Task<HttpResponseMessage>>();

            using (var handler = new HttpClientHandler())
            {
                handler.CookieContainer = new System.Net.CookieContainer();
                handler.CookieContainer.Add(cookies);
                handler.UseCookies = true;

                using (var client = new HttpClient(handler))
                {
                    client.Timeout = new TimeSpan(0, 2, 0);

                    foreach (var licenseNumber in licenseNumbers)
                    {
                        tasksList.Add(licenseNumber, client.GetAsync($"http://my.pti.ge/nextpti?gov_num={licenseNumber}"));
                    }

                    foreach (var task in tasksList)
                    {
                        var taskResult = new HttpResponseMessage();

                        try
                        {
                            taskResult = await task.Value;
                        }
                        catch (Exception)
                        {
                            result.Add(new ScrapeResult()
                            {
                                License = task.Key
                            });
                            continue;
                        }

                        if (taskResult.IsSuccessStatusCode)
                        {
                            var doc = new HtmlDocument();
                            doc.LoadHtml(await taskResult.Content.ReadAsStringAsync());

                            var nodes = doc.DocumentNode.Descendants("h5").ToList();
                            if (nodes.Count >= 3)
                            {
                                var lastDate = ExtractDataFromNode(nodes[2]);
                                var startDate = ExtractDataFromNode(nodes[1]);
                                if (lastDate.Length > 4 && startDate.Length > 4)
                                {
                                    try
                                    {

                                        result.Add(new ScrapeResult()
                                        {
                                            License = nodes[0].InnerHtml.Trim(),
                                            LastDate = DateTime.ParseExact(lastDate, "dd.MM.yyyy", CultureInfo.InvariantCulture),
                                            StartDate = DateTime.ParseExact(startDate, "dd.MM.yyyy", CultureInfo.InvariantCulture)
                                        });
                                    }
                                    catch (Exception)
                                    {
                                        throw new Exception($"lastDate : {lastDate} startDate: {startDate}");
                                    }
                                }
                                else
                                    result.Add(new ScrapeResult()
                                    {
                                        License = nodes[0].InnerHtml.Trim(),
                                        LastDate = null,
                                        StartDate = null
                                    });
                            }
                        }
                    }
                }
            }
            return result;
        }

        private string ExtractDataFromNode(HtmlNode node)
        {
            return node.InnerHtml.Remove(0, node.InnerHtml.IndexOf('>') + 1).Trim();
        }
    }
}
