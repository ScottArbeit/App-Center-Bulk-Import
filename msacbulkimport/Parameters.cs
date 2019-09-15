using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Text;

namespace msacbulkimport
{
    public static class Parameters
    {
        public static string ApiToken = "--apitoken";
        public static string Organization = "--organization";
        public static string OrganizationRole = "--organizationrole";
        public static string Team = "--team";
        public static string TeamRole = "--teamrole";
        public static string InputFile = "--inputfile";
        public static string OutputFile = "--outputfile";
        public static string Help1 = "--help";
        public static string Help2 = "/?";

        public static List<string> RoleValuesList = new List<string> { "admin", "collaborator", "member" };

        public static string RoleValues()
        {
            StringBuilder sb = new StringBuilder();
            foreach (string role in RoleValuesList)
            {
                sb.Append($"{role}, ");
            }

            sb.Remove(sb.Length - 2, 2);
            return sb.ToString();
        }
    }
}
