using System.Net.Http;
using System.Text;
using System.IO;
using Amazon.Lambda.Core;
using Newtonsoft.Json;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace EsepWebhook
{
    public class Function
    {
        public string FunctionHandler(object input, ILambdaContext context)
        {
            if (input == null)
            {
                // Handle the case where input is null (if needed)
                return "Input is null.";
            }
            try
            {
                dynamic json = JsonConvert.DeserializeObject<dynamic>(input.ToString());

                // Check if the payload contains "issue" information
                if (json.issue != null)
                {
                    // Extract the "html_url" from the "issue" information
                    string issueHtmlUrl = json.issue.html_url;

                    // Create a Slack message payload
                    string payload = $"{{'text':'Issue Created: {issueHtmlUrl}'}}";

                    // Post the payload to the Slack channel
                    using var client = new HttpClient();
                    using var webRequest = new HttpRequestMessage(HttpMethod.Post, Environment.GetEnvironmentVariable("SLACK_URL"))
                    {
                        Content = new StringContent(payload, Encoding.UTF8, "application/json")
                    };

                    var response = client.Send(webRequest);

                    // Read and return the response from the Slack API
                    using var reader = new StreamReader(response.Content.ReadAsStream());
                    return reader.ReadToEnd();
                }

                // If the payload doesn't contain "issue" information, return a message indicating that
                return "No issue information found in the payload.";
            }
            catch (Exception ex)
            {
                // Handle exceptions and return an error message
                return $"Error: {ex.Message}";
            }
        }
    }
}
