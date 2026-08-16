using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using SitecoreErrorAgent.Models;

namespace SitecoreErrorAgent.Services
{
    public class EmailParser
    {
        public List<ErrorEmail> ParseFile(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException(
                    "Email input file was not found.",
                    filePath);
            }

            string content = File.ReadAllText(filePath);

            return ParseEmails(content);
        }

        public List<ErrorEmail> ParseEmails(string content)
        {
            var result = new List<ErrorEmail>();

            if (string.IsNullOrWhiteSpace(content))
            {
                return result;
            }

            // Every email starts with From:
            var parts = Regex.Split(
                content,
                @"(?=^\s*From:\s*)",
                RegexOptions.Multiline);

            int emailNumber = 1;

            foreach (string part in parts)
            {
                if (string.IsNullOrWhiteSpace(part))
                {
                    continue;
                }

                var email = ParseSingleEmail(part.Trim());

                if (email != null)
                {
                    email.EmailNumber = emailNumber++;
                    email.RawEmail = part.Trim();

                    result.Add(email);
                }
            }

            return result;
        }

        private ErrorEmail ParseSingleEmail(string rawEmail)
        {
            var email = new ErrorEmail();

            Match fromMatch = Regex.Match(
                rawEmail,
                @"^From:\s*(.+)$",
                RegexOptions.Multiline | RegexOptions.IgnoreCase);

            Match sentMatch = Regex.Match(
                rawEmail,
                @"^Sent:\s*(.+)$",
                RegexOptions.Multiline | RegexOptions.IgnoreCase);

            Match toMatch = Regex.Match(
                rawEmail,
                @"^To:\s*(.+)$",
                RegexOptions.Multiline | RegexOptions.IgnoreCase);

            Match subjectMatch = Regex.Match(
                rawEmail,
                @"^Subject:\s*(.+)$",
                RegexOptions.Multiline | RegexOptions.IgnoreCase);

            if (!fromMatch.Success && !subjectMatch.Success)
            {
                return null;
            }

            email.From = fromMatch.Success
                ? fromMatch.Groups[1].Value.Trim()
                : string.Empty;

            email.Sent = sentMatch.Success
                ? sentMatch.Groups[1].Value.Trim()
                : string.Empty;

            email.To = toMatch.Success
                ? toMatch.Groups[1].Value.Trim()
                : string.Empty;

            email.Subject = subjectMatch.Success
                ? subjectMatch.Groups[1].Value.Trim()
                : string.Empty;

            // Everything after the Subject line is considered body.
            if (subjectMatch.Success)
            {
                int bodyStart =
                    subjectMatch.Index + subjectMatch.Length;

                email.Body = rawEmail
                    .Substring(bodyStart)
                    .Trim();
            }
            else
            {
                email.Body = rawEmail;
            }

            return email;
        }
    }
}