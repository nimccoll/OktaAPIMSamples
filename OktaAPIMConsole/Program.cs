//===============================================================================
// Microsoft FastTrack for Azure
// Okta Azure API Management Samples
//===============================================================================
// Copyright © Microsoft Corporation.  All rights reserved.
// THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY
// OF ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT
// LIMITED TO THE IMPLIED WARRANTIES OF MERCHANTABILITY AND
// FITNESS FOR A PARTICULAR PURPOSE.
//===============================================================================
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;

namespace OktaAPIMConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            var environmentName = Environment.GetEnvironmentVariable("ENVIRONMENT");

            // Setup console application to read settings from appsettings.json
            IConfigurationBuilder configurationBuilder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", true, true)
                .AddJsonFile($"appsettings.{environmentName}.json", true, true);

            IConfigurationRoot configuration = configurationBuilder.Build();

            HttpClient client = new HttpClient();
            string clientID = configuration["Okta:ClientId"];
            string clientSecret = configuration["Okta:ClientSecret"];
            byte[] clientCreds = System.Text.Encoding.UTF8.GetBytes($"{clientID}:{clientSecret}");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", System.Convert.ToBase64String(clientCreds));

            Dictionary<string, string> postMessage = new Dictionary<string, string>();
            postMessage.Add("grant_type", "client_credentials");
            postMessage.Add("scope", "nimccollapimprod");
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, configuration["Okta:TokenUrl"])
            {
                Content = new FormUrlEncodedContent(postMessage)
            };

            HttpResponseMessage response = client.SendAsync(request).Result;
            if (response.IsSuccessStatusCode)
            {
                string json = response.Content.ReadAsStringAsync().Result;
                dynamic token = JsonConvert.DeserializeObject(json);
                string accessToken = token.access_token.ToString();
                string apiUrl = configuration["APIUrl"];
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
                client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", configuration["APISubscriptionKey"]);
                HttpResponseMessage echoResult = client.GetAsync(apiUrl).Result;
                if (echoResult.IsSuccessStatusCode)
                {
                    Console.WriteLine($"*** Successfully called the ECHO API at {apiUrl} ***");
                    dynamic echoData = JsonConvert.DeserializeObject(echoResult.Content.ReadAsStringAsync().Result);
                }
                else
                {
                    Console.WriteLine($"*** Call to the ECHO API failed with a status code of {echoResult.StatusCode} ***");
                }
            }
            else
            {
                Console.WriteLine("*** Unable to retrieve an access token from Okta ***");
            }
            Console.WriteLine("*** Press any key to exit ***");
            Console.Read();
        }
    }
}
