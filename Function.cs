using System;
using System.IO;
using System.Net.Http;
using System.Text;
using Amazon.Lambda.Core;
using Newtonsoft.Json;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace EsepWebhook
{
    public class Function
    {
        public string FunctionHandler(string input, ILambdaContext context)
        {
            try
            {
                // Deserialize the input JSON string
                dynamic json = JsonConvert.DeserializeObject<dynamic>(input);

                // Check if the "issue" property exists
                if (json.issue != null)
                {
                    // Access the "html_url" property within the "issue" object
                    string issueUrl = json.issue.html_url;

                    // Do something with the extracted URL, e.g., send to Slack
                    string payload = $"{{'text':'Issue Created: {issueUrl}'}}";

                    var client = new HttpClient();
                    var webRequest = new HttpRequestMessage(HttpMethod.Post, Environment.GetEnvironmentVariable("SLACK_URL"))
                    {
                        Content = new StringContent(payload, Encoding.UTF8, "application/json")
                    };

                    var response = client.Send(webRequest);
                    using var reader = new StreamReader(response.Content.ReadAsStream());

                    return reader.ReadToEnd();
                }
                else
                {
                    // Log a message or handle the case when "issue" property is not present
                    context.Logger.LogLine("No 'issue' property found in the input JSON.");
                    return "No 'issue' property found.";
                }
            }
            catch (Exception ex)
            {
                // Log the exception and rethrow it
                context.Logger.LogLine($"Error processing JSON: {ex}");
                throw;
            }
        }
    }
}
