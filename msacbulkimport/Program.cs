using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace msacbulkimport
{
    class Program
    {
        private static readonly StringBuilder output = new StringBuilder();

        // From https://docs.microsoft.com/en-us/dotnet/standard/base-types/how-to-verify-that-strings-are-in-valid-email-format
        private static readonly string emailAddressRegex = @"^(?("")("".+?(?<!\\)""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z])@))(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-0-9a-z]*[0-9a-z]*\.)+[a-z0-9][\-a-z0-9]{0,22}[a-z0-9]))$";

        static async Task Main(string[] args)
        {
            // Cast parameters to lower-case, but leave parameter values as they are.
            List<string> arguments = new List<string>(args.Select(arg => arg.StartsWith("--") ? arg.ToLowerInvariant() : arg));

            AddOutput("msacbulkimport - Microsoft App Center Bulk Import utility");
            AddOutput(string.Empty);

            if (arguments.Count == 0 || arguments.Contains(Parameters.Help1) || arguments.Contains(Parameters.Help2))
            {
                ShowHelp();
                return;
            }

            // Get the provided parameter values.
            string apiToken = GetParameterValue(arguments, Parameters.ApiToken);
            string organization = GetParameterValue(arguments, Parameters.Organization);
            string organizationRole = GetParameterValue(arguments, Parameters.OrganizationRole);
            string team = GetParameterValue(arguments, Parameters.Team);
            string teamRole = GetParameterValue(arguments, Parameters.TeamRole);
            string inputFile = GetParameterValue(arguments, Parameters.InputFile);
            string outputFile = GetParameterValue(arguments, Parameters.OutputFile, "bulkimportresult.txt");

            // Check that we have a valid set of parameters.
            bool isError =
                IsError(string.IsNullOrEmpty(apiToken),
                    $"Error: {Parameters.ApiToken} is required to connect to your App Center account.") ||

                IsError(string.IsNullOrEmpty(inputFile),
                    $"Error: {Parameters.InputFile} is required.") ||

                IsError(string.IsNullOrEmpty(organization),
                    $"Error: {Parameters.Organization} is required to connect to your App Center account.") ||

                IsError(string.IsNullOrEmpty(team) ^ string.IsNullOrEmpty(teamRole),
                    $"Error: {Parameters.Team} and {Parameters.TeamRole} must be provided together.") ||

                IsError(!string.IsNullOrEmpty(organization) && !string.IsNullOrEmpty(organizationRole) && (!string.IsNullOrEmpty(team) || !string.IsNullOrEmpty(teamRole)),
                    $"Error: When {Parameters.Organization} and {Parameters.OrganizationRole} are provided for inviting users to an organization, {Parameters.Team} and {Parameters.TeamRole} are not allowed.") ||

                IsError(!string.IsNullOrEmpty(organizationRole) && !Parameters.RoleValuesList.Contains(organizationRole),
                    $"Error: {Parameters.OrganizationRole} must be one of the following values: {Parameters.RoleValues()}.") ||

                IsError(!string.IsNullOrEmpty(teamRole) && !Parameters.RoleValuesList.Contains(teamRole),
                    $"Error: {Parameters.TeamRole} must be one of the following values: {Parameters.RoleValues()}.");

            if (!isError)
            {
                List<string> emailAddresses = await ReadEmailAddresses(inputFile);
                if (emailAddresses != null)
                {
                    using HttpClient httpClient = new HttpClient() { BaseAddress = new Uri("https://api.appcenter.ms/v0.1/") };
                    httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
                    httpClient.DefaultRequestHeaders.Add("X-API-Token", apiToken);

                    foreach (string emailAddress in emailAddresses)
                    {
                        bool validEmailAddress = false;
                        try
                        {
                            validEmailAddress = Regex.IsMatch(emailAddress, emailAddressRegex, RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(100));
                        }
                        catch (RegexMatchTimeoutException)
                        {
                            AddOutput($"{emailAddress}: Timed out when determining if this is a valid email address.");
                        }

                        if (validEmailAddress)
                        {
                            if (!string.IsNullOrEmpty(organizationRole))
                            {
                                // We're inviting users to an organization.
                                string content = $"{{\"user_email\":\"{emailAddress}\", \"role\":\"{organizationRole}\" }}";
                                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, $"orgs/{Uri.EscapeDataString(organization)}/invitations")
                                {
                                    Content = new StringContent(content, Encoding.UTF8, "application/json")
                                };

                                var response = await httpClient.SendAsync(request);
                                if (response.IsSuccessStatusCode)
                                {
                                    AddOutput($"{emailAddress}: Sent invitation to join organization.");
                                }
                                else
                                {
                                    AddOutput($"{emailAddress}: Error: failed to send invitation; {await response.Content.ReadAsStringAsync()}");
                                }
                            }
                            else
                            {
                                // We're adding users as team members.
                                string content = $"{{\"user_email\":\"{emailAddress}\", \"role\":\"{teamRole}\" }}";
                                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, $"orgs/{Uri.EscapeDataString(organization)}/teams/{Uri.EscapeDataString(team)}/users")
                                {
                                    Content = new StringContent(content, Encoding.UTF8, "application/json")
                                };

                                var response = await httpClient.SendAsync(request);
                                if (response.IsSuccessStatusCode)
                                {
                                    AddOutput($"{emailAddress}: Added team member.");
                                }
                                else
                                {
                                    AddOutput($"{emailAddress}: Error: failed to add team member; {await response.Content.ReadAsStringAsync()}");
                                }
                            }
                        }
                        else
                        {
                            AddOutput($"{emailAddress}: Error: doesn't appear to be a valid email address.");
                        }
                    }
                }
            }

            AddOutput($"{Environment.NewLine}Done.");

            await File.WriteAllTextAsync(outputFile, output.ToString());
        }

        private static bool IsError(bool errorCondition, string errorMessage)
        {
            if (errorCondition)
            {
                AddOutput(errorMessage);
            }

            return errorCondition;
        }

        private static async Task<List<string>> ReadEmailAddresses(string inputFile)
        {
            List<string> emailAddresses = null;
            try
            {
                emailAddresses = new List<string>(await File.ReadAllLinesAsync(inputFile));
            }
            catch (Exception ex)
            {
                AddOutput($"Error: failed to read input file {inputFile}.");
                AddOutput(ex.Message);
            }

            return emailAddresses;
        }

        private static void AddOutput(string outputValue)
        {
            Console.WriteLine(outputValue);
            output.AppendLine(outputValue);
        }

        private static string GetParameterValue(List<string> arguments, string parameter, string defaultValue = "")
        {
            string returnValue = defaultValue;

            int parameterIndex = arguments.IndexOf(parameter);
            if ((parameterIndex >= 0) && (parameterIndex < arguments.Count - 1))
            {
                string parameterValue = arguments[parameterIndex + 1];
                if (!parameterValue.StartsWith("--"))
                {
                    returnValue = parameterValue;
                }
            }

            return returnValue;
        }

        private static void ShowHelp()
        {
            Console.WriteLine("Examples:");
            Console.WriteLine("---------");
            Console.WriteLine("Invite users to an organization:");
            Console.WriteLine($"msacbuildimport {Parameters.ApiToken} <myApiToken> {Parameters.Organization} My-Organization-Name {Parameters.OrganizationRole} member {Parameters.InputFile} C:\\MyPath\\userfile.txt");
            Console.WriteLine();
            Console.WriteLine("Invite users to a team (they must already be in the organization):");
            Console.WriteLine($"msacbuildimport {Parameters.ApiToken} <myApiToken> {Parameters.Organization} My-Organization-Name {Parameters.Team} My-Team-Name {Parameters.TeamRole} member {Parameters.InputFile} C:\\MyPath\\userfile.txt");
            Console.WriteLine();
            Console.WriteLine("Show help:");
            Console.WriteLine($"msacbulkimport {Parameters.Help1} or msacbulkimport {Parameters.Help2}");
            Console.WriteLine();
            Console.WriteLine("Parameters:");
            Console.WriteLine("-----------");
            Console.WriteLine($"  {Parameters.ApiToken}: (required) The API token for your organization");
            Console.WriteLine($"  {Parameters.InputFile}: (required) The name of the file containing email addresses of users to import");
            Console.WriteLine($"  {Parameters.Organization}: (required) The name of your organization (i.e. the slug used in AppCenter URL's)");
            Console.WriteLine($"  {Parameters.OrganizationRole}: The organization role for new collaborators; either admin, collaborator, or member");
            Console.WriteLine($"  {Parameters.Team}: The name of the team (i.e. the slug used in AppCenter URL's), if adding team members");
            Console.WriteLine($"  {Parameters.TeamRole}: The team role for new team members; either admin, collaborator, or member");
            Console.WriteLine($"  {Parameters.OutputFile}: The name of the output file; default is bulkimportresult.txt");
            Console.WriteLine($"  {Parameters.Help1} | {Parameters.Help2}: Show this help message");
            Console.WriteLine();
        }
    }
}
