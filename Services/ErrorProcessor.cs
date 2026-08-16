using SitecoreErrorAgent.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SitecoreErrorAgent.Services
{
    public class ErrorProcessor
    {
        private readonly AiAgentService _aiAgent;

        private readonly SitecoreRepository _repository;

        public ErrorProcessor()
        {
            _aiAgent = new AiAgentService();

            _repository = new SitecoreRepository();
        }

        public async Task ProcessAsync(
            List<ErrorEmail> emails)
        {
            foreach (ErrorEmail email in emails)
            {
                Console.WriteLine();
                Console.WriteLine(
                    "==================================================");

                Console.WriteLine(
                    "PROCESSING EMAIL #" +
                    email.EmailNumber);

                Console.WriteLine(
                    "==================================================");

                try
                {
                    await ProcessSingleEmailAsync(email);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(
                        "ERROR: " + ex.Message);
                }
            }
        }

        private async Task ProcessSingleEmailAsync(
            ErrorEmail email)
        {
            Console.WriteLine(
                "Subject: " + email.Subject);

            Console.WriteLine(
                "Calling AI Agent...");

            AgentAnalysis analysis =
                await _aiAgent.AnalyzeEmailAsync(email);

            DisplayAnalysis(analysis);

            if (!analysis.IsValid)
            {
                Console.WriteLine(
                    "AI marked request as invalid.");

                return;
            }

            if (string.Equals(
                analysis.RequestType,
                "User",
                StringComparison.OrdinalIgnoreCase))
            {
                await ProcessUserAsync(analysis);

                return;
            }

            if (string.Equals(
                analysis.RequestType,
                "Organisation",
                StringComparison.OrdinalIgnoreCase))
            {
                await ProcessOrganisationAsync(
                    analysis);

                return;
            }

            Console.WriteLine(
                "Unknown request type. No database update performed.");
        }

        private Task ProcessUserAsync(
            AgentAnalysis analysis)
        {
            Console.WriteLine();
            Console.WriteLine(
                "AI DECISION: USER REQUEST");

            if (string.IsNullOrWhiteSpace(
                analysis.UserName))
            {
                Console.WriteLine(
                    "UserName is missing.");

                return Task.CompletedTask;
            }

            Console.WriteLine(
                "Checking Sitecore custom User table...");

            bool exists =
                _repository.UserExists(
                    analysis.UserName);

            if (!exists)
            {
                Console.WriteLine(
                    "USER NOT FOUND: " +
                    analysis.UserName);

                return Task.CompletedTask;
            }

            Console.WriteLine(
                "USER FOUND: " +
                analysis.UserName);

            Console.WriteLine(
                "Updating Sitecore custom User table...");

            int rows =
                _repository.UpdateUser(
                    analysis);

            Console.WriteLine(
                "Rows updated: " + rows);

            if (rows > 0)
            {
                Console.WriteLine(
                    "SUCCESS: User updated.");
            }
            else
            {
                Console.WriteLine(
                    "No user record updated.");
            }

            return Task.CompletedTask;
        }

        private Task ProcessOrganisationAsync(
            AgentAnalysis analysis)
        {
            Console.WriteLine();
            Console.WriteLine(
                "AI DECISION: ORGANISATION REQUEST");

            if (string.IsNullOrWhiteSpace(
                analysis.ABN))
            {
                Console.WriteLine(
                    "Organisation ABN is missing.");

                return Task.CompletedTask;
            }

            Console.WriteLine(
                "Checking Sitecore custom Organisation table...");

            bool exists =
                _repository.OrganisationExists(
                    analysis.ABN);

            if (!exists)
            {
                Console.WriteLine(
                    "ORGANISATION NOT FOUND: " +
                    analysis.ABN);

                return Task.CompletedTask;
            }

            Console.WriteLine(
                "ORGANISATION FOUND: " +
                analysis.ABN);

            int rows =
                _repository.UpdateOrganisation(
                    analysis);

            Console.WriteLine(
                "Rows updated: " + rows);

            if (rows > 0)
            {
                Console.WriteLine(
                    "SUCCESS: Organisation updated.");
            }

            return Task.CompletedTask;
        }

        private void DisplayAnalysis(
            AgentAnalysis analysis)
        {
            Console.WriteLine();
            Console.WriteLine(
                "--------------- AI ANALYSIS ---------------");

            Console.WriteLine(
                "Request Type : " +
                analysis.RequestType);

            Console.WriteLine(
                "Identifier   : " +
                analysis.Identifier);

            Console.WriteLine(
                "UserName     : " +
                analysis.UserName);

            Console.WriteLine(
                "Email        : " +
                analysis.EmailAddress);

            Console.WriteLine(
                "UserObjectId : " +
                analysis.UserObjectId);

            Console.WriteLine(
                "ABN          : " +
                analysis.ABN);

            Console.WriteLine(
                "Analysis     : " +
                analysis.Analysis);

            Console.WriteLine(
                "--------------------------------------------");
        }
    }
}