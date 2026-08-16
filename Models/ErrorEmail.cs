namespace SitecoreErrorAgent.Models
{
    public class ErrorEmail
    {
        public int EmailNumber { get; set; }

        public string From { get; set; }

        public string Sent { get; set; }

        public string To { get; set; }

        public string Subject { get; set; }

        public string Body { get; set; }

        public string RawEmail { get; set; }
    }
}