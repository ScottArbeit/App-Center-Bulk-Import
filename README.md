# App-Center-Bulk-Import

## msacbulkimport - Microsoft App Center Bulk Import utility

Parameters:
-----------
  --apitoken: (required) The API token for your organization
  --inputfile: (required) The name of the file containing email addresses of users to import
  --organization: (required) The name of your organization
  --organizationrole: The organization role for new collaborators; either admin, collaborator, or member
  --team: The name of the team, if adding team members
  --teamrole: The team role for new team members; either admin, collaborator, or member
  --outputfile: The name of the output file; default is bulkimportresult.txt
  --help | /?: Show this help message

Note: If there are spaces in a parameter, wrap it in "".

Examples:
---------
Invite users to an organization:
msacbuildimport --apitoken <myApiToken> --organization MyOrganizationName --organizationrole member --inputfile C:\MyPath\userfile.txt

Invite users to a team (they must already be in the organization):
msacbuildimport --apitoken <myApiToken> --organization MyOrganizationName --team MyTeamName --teamrole member --inputfile C:\MyPath\userfile.txt

Show help:
msacbulkimport --help or msacbulkimport /?