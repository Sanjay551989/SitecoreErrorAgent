using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SitecoreErrorAgent.Models;
using System;
using System.Configuration;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace SitecoreErrorAgent.Services
{
    public class AiAgentService
    {
        private readonly HttpClient _httpClient;

        private readonly string _endpoint;
        private readonly string _apiKey;
        private readonly string _model;

        public AiAgentService()
        {
            _httpClient = new HttpClient();

            _endpoint = "https://api.openai.com/v1/chat/completions";

            _apiKey = ConfigurationManager
                .AppSettings["OpenAIApiKey"];

            _model = ConfigurationManager
                .AppSettings["OpenAIModel"];
        }

        public async Task<AgentAnalysis> AnalyzeEmailAsync(
            ErrorEmail email)
        {
            string prompt = BuildPrompt(email);


            string response = await CallOpenAIAsync(prompt);

            return ParseAgentResponse(response);
        }

        private string BuildPrompt(ErrorEmail email)
        {
            return $@"
You are an enterprise support automation agent.

Analyze the following Sitecore error email.

Your responsibilities:

1. Understand the failure message.
2. Locate RequestBody.
3. Parse the RequestBody JSON.
4. Determine whether the request represents:
   - User
   - Organisation
   - Unknown
5. Extract the identifier.
6. Extract all useful fields.
7. Return ONLY valid JSON.

Classification rules:

- If RequestBody contains UserObjectId, UserName,
  FirstName, LastName, EmailAddress or CssId,
  classify it as User.

- If RequestBody contains organisation-specific fields
  such as OrganisationName, OrganisationId, ABN,
  classify it as Organisation.

- Do not classify based only on the phrase
  'user/ org ABN'.

- If both user and organisation fields exist,
  prefer User when UserObjectId or UserName exists.

Return exactly this JSON structure:

{{
  ""RequestType"": ""User|Organisation|Unknown"",
  ""Identifier"": """",
  ""UserName"": """",
  ""UserObjectId"": """",
  ""FirstName"": """",
  ""LastName"": """",
  ""CssId"": """",
  ""PhoneNumber"": """",
  ""PreferredContactMethod"": """",
  ""EmailAddress"": """",
  ""OrganisationName"": """",
  ""ABN"": """",
  ""RequestBodyJson"": """",
  ""Analysis"": """",
  ""IsValid"": true
}}

Do not invent values.

EMAIL:

{email.RawEmail}
";
        }

        private async Task<string> CallOpenAIAsync(string prompt)
        {
            if (string.IsNullOrWhiteSpace(_apiKey))
            {
                throw new Exception(
                    "OpenAIApiKey is missing in App.config.");
            }

            if (string.IsNullOrWhiteSpace(_model))
            {
                throw new Exception(
                    "OpenAIModel is missing in App.config.");
            }

            var requestBody = new
            {
                model = _model,

                messages = new[]
                {
            new
            {
                role = "system",
                content =
                    "You are a reliable enterprise data processing agent. Return only valid JSON."
            },

            new
            {
                role = "user",
                content = prompt
            }
        },

                temperature = 0
            };

            string json =
                JsonConvert.SerializeObject(requestBody);

            using (var request = new HttpRequestMessage(
                HttpMethod.Post,
                _endpoint))
            {
                request.Headers.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue(
                        "Bearer",
                        _apiKey);

                request.Content = new StringContent(
                    json,
                    Encoding.UTF8,
                    "application/json");

                using (HttpResponseMessage response =
                    await _httpClient.SendAsync(request))
                {
                    string responseContent =
                        await response.Content.ReadAsStringAsync();

                    if (!response.IsSuccessStatusCode)
                    {
                        throw new Exception(
                            "OpenAI API error: " +
                            response.StatusCode +
                            Environment.NewLine +
                            responseContent);
                    }

                    return responseContent;
                }
            }
        }

        private AgentAnalysis ParseAgentResponse(
            string apiResponse)
        {
            JObject root =
                JObject.Parse(apiResponse);

            string content =
                root["choices"]?[0]?["message"]?["content"]?
                    .ToString();

            if (string.IsNullOrWhiteSpace(content))
            {
                throw new Exception(
                    "AI returned an empty response.");
            }

            // Sometimes AI returns ```json ... ```
            content = CleanJson(content);

            AgentAnalysis result =
                JsonConvert.DeserializeObject<AgentAnalysis>(
                    content);

            if (result == null)
            {
                throw new Exception(
                    "Unable to parse AI analysis.");
            }

            return result;
        }

        private string CleanJson(string content)
        {
            content = content.Trim();

            if (content.StartsWith("```json"))
            {
                content = content.Substring(7);
            }
            else if (content.StartsWith("```"))
            {
                content = content.Substring(3);
            }

            if (content.EndsWith("```"))
            {
                content = content.Substring(
                    0,
                    content.Length - 3);
            }

            return content.Trim();
        }
    }
}