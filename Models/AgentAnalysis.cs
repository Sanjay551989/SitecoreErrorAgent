namespace SitecoreErrorAgent.Models
{
    public class AgentAnalysis
    {
        public string RequestType { get; set; }

        public string Identifier { get; set; }

        public string UserName { get; set; }

        public string UserObjectId { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string CssId { get; set; }

        public string PhoneNumber { get; set; }

        public string PreferredContactMethod { get; set; }

        public string EmailAddress { get; set; }

        public string OrganisationName { get; set; }

        public string ABN { get; set; }

        public string RequestBodyJson { get; set; }

        public string Analysis { get; set; }

        public bool IsValid { get; set; }
    }
}