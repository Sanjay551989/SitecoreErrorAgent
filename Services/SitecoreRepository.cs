using SitecoreErrorAgent.Models;
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace SitecoreErrorAgent.Services
{
    public class SitecoreRepository
    {
        private readonly string _connectionString;

        public SitecoreRepository()
        {
            _connectionString =
                ConfigurationManager
                    .ConnectionStrings["SitecoreCustomDb"]
                    .ConnectionString;
        }

        public bool UserExists(string userName)
        {
            const string sql = @"
                SELECT COUNT(1)
                FROM CustomUsers
                WHERE UserName = @UserName";

            using (var connection =
                new SqlConnection(_connectionString))
            using (var command =
                new SqlCommand(sql, connection))
            {
                command.Parameters.Add(
                    "@UserName",
                    SqlDbType.NVarChar,
                    255).Value = userName;

                connection.Open();

                int count =
                    Convert.ToInt32(command.ExecuteScalar());

                return count > 0;
            }
        }

        public int UpdateUser(AgentAnalysis data)
        {
            const string sql = @"
                UPDATE CustomUsers
                SET
                    FirstName = @FirstName,
                    LastName = @LastName,
                    CssId = @CssId,
                    PhoneNumber = @PhoneNumber,
                    PreferredContactMethod = @PreferredContactMethod,
                    EmailAddress = @EmailAddress,
                    UserObjectId = @UserObjectId,
                    UpdatedOn = GETDATE()
                WHERE UserName = @UserName";

            using (var connection =
                new SqlConnection(_connectionString))
            using (var command =
                new SqlCommand(sql, connection))
            {
                AddUserParameters(command, data);

                connection.Open();

                return command.ExecuteNonQuery();
            }
        }

        public bool OrganisationExists(string identifier)
        {
            const string sql = @"
                SELECT COUNT(1)
                FROM CustomOrganisations
                WHERE ABN = @ABN";

            using (var connection =
                new SqlConnection(_connectionString))
            using (var command =
                new SqlCommand(sql, connection))
            {
                command.Parameters.Add(
                    "@ABN",
                    SqlDbType.NVarChar,
                    100).Value =
                    identifier;

                connection.Open();

                int count =
                    Convert.ToInt32(command.ExecuteScalar());

                return count > 0;
            }
        }

        public int UpdateOrganisation(
            AgentAnalysis data)
        {
            const string sql = @"
                UPDATE CustomOrganisations
                SET
                    OrganisationName = @OrganisationName,
                    EmailAddress = @EmailAddress,
                    PhoneNumber = @PhoneNumber,
                    UpdatedOn = GETDATE()
                WHERE ABN = @ABN";

            using (var connection =
                new SqlConnection(_connectionString))
            using (var command =
                new SqlCommand(sql, connection))
            {
                command.Parameters.Add(
                    "@OrganisationName",
                    SqlDbType.NVarChar,
                    255).Value =
                    ToDbValue(data.OrganisationName);

                command.Parameters.Add(
                    "@EmailAddress",
                    SqlDbType.NVarChar,
                    255).Value =
                    ToDbValue(data.EmailAddress);

                command.Parameters.Add(
                    "@PhoneNumber",
                    SqlDbType.NVarChar,
                    100).Value =
                    ToDbValue(data.PhoneNumber);

                command.Parameters.Add(
                    "@ABN",
                    SqlDbType.NVarChar,
                    100).Value =
                    ToDbValue(data.ABN);

                connection.Open();

                return command.ExecuteNonQuery();
            }
        }

        private void AddUserParameters(
            SqlCommand command,
            AgentAnalysis data)
        {
            command.Parameters.Add(
                "@UserName",
                SqlDbType.NVarChar,
                255).Value =
                ToDbValue(data.UserName);

            command.Parameters.Add(
                "@FirstName",
                SqlDbType.NVarChar,
                255).Value =
                ToDbValue(data.FirstName);

            command.Parameters.Add(
                "@LastName",
                SqlDbType.NVarChar,
                255).Value =
                ToDbValue(data.LastName);

            command.Parameters.Add(
                "@CssId",
                SqlDbType.NVarChar,
                100).Value =
                ToDbValue(data.CssId);

            command.Parameters.Add(
                "@PhoneNumber",
                SqlDbType.NVarChar,
                100).Value =
                ToDbValue(data.PhoneNumber);

            command.Parameters.Add(
                "@PreferredContactMethod",
                SqlDbType.NVarChar,
                100).Value =
                ToDbValue(data.PreferredContactMethod);

            command.Parameters.Add(
                "@EmailAddress",
                SqlDbType.NVarChar,
                255).Value =
                ToDbValue(data.EmailAddress);

            command.Parameters.Add(
                "@UserObjectId",
                SqlDbType.NVarChar,
                100).Value =
                ToDbValue(data.UserObjectId);
        }

        private object ToDbValue(string value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? (object)DBNull.Value
                : value;
        }
    }
}