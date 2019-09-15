# App-Center-Bulk-Import

## msacbulkimport - Microsoft App Center Bulk Import utility

msacbulkimport is a .NET Core utility that allows users to import multiple users to an organization or team.

It is intended as a demo for how to use the App Center REST API for account management. No warranties about its correctness or performance are offered, but best-effort has been made to ensure that it runs correctly.

### Building the code
msacbulkimport uses .NET Core 3.0 Preview 9, which you can download from [dot.net](https://dotnet.microsoft.com/download/dotnet-core/3.0). Build the project by running

    dotnet build msacbulkimport.csproj

or by using your favorite editor or IDE.

### Input file
The input file is expected to be a list of email addresses, one-per-line, with no headers or other information.

For example:

```
vanessa.manager@mydomain.com
rebecca.programmer@mydomain.com
brandon.productmanager@mydomain.com
megan.betatester@someotherdomain.com
```

Lines that are not email addresses will be shown as errors in the output file, and ignored.

### Output file

All output is shown in the terminal window, and also written to an output file. By default, this file is called `bulkimportresult.txt`.

### Examples:

Invite users to an organization:

    msacbulkimport --apitoken <myApiToken> --organization MyOrganizationName --organizationrole member --inputfile C:\MyPath\userfile.txt


Invite users to a team (they must already be in the organization):

    msacbulkimport --apitoken <myApiToken> --organization MyOrganizationName --team MyTeamName --teamrole member --inputfile C:\MyPath\userfile.txt


Show help:

    msacbulkimport --help
    msacbulkimport /?

### Parameters:

  `--apitoken` _(required)_ The API token for your organization

  `--inputfile` _(required)_ The name of the file containing email addresses of users to import

  `--organization` _(required)_ The name of your organization

  `--organizationrole` The organization role for new collaborators; either admin, collaborator, or member

  `--team` The name of the team, if adding team members

  `--teamrole` The team role for new team members; either admin, collaborator, or member

  `--outputfile` The name of the output file; default is bulkimportresult.txt

  `--help | /?` Show help message

_Note:_ If there are spaces in a parameter, wrap it in "".
