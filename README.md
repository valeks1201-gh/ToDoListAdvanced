This C# ToDoListAdvanced solution is intended to study and research different IT technologies using minimalistic approach. 

Approximately in year 2020 the ToDoList WebApi public solution was downloaded from GitHub and 
since then was evolved during studying different technologies: 
.Net Core, EntityFramework (EF), secured REST services, endpoints security, Identity Server, Docker. 

Currently the ToDoListAdvanced solution includes 2 major projects: ToDoListApi (WebApi project with secured endpoints) and 
ToDoListBlazor (simple Blazor Server based client interacting with ToDoListApi endpoints). 
The DAL (Data Access Layer) EF based project ensures work with database (DB) using “Code first” approach. 
The ToDoListCore library type project includes common classes. 

The ToDoListAdvanced solution has the following major features:
-	Can be deployed under both Windows and Linux based operating systems (OS) without additional changes in settings
-	Can use MS SQL or PostgreSQL database depending on settings
-	IdentityServer8 nuget package is integrated in ToDoListApi project
-	IdentityServer8 integration for production environment (it uses DB, not just memory)
-	EF migration scripts are adapted to the environment with different contexts related with this solution’s data and IdentityServer8
-	After debugging solution under Visual Studio no need to change settings before deployment under 
	IIS, Kestrel or Docker (the host type is determined by code automatically)

///////////////////////////////////////
appsettings.json

The appsettings.json files contain projects adjustments.
The appsettings.json file contains Database (DB) connection strings and URLs for different deployment options.

///////////////////////////////////////
Certificates

Within ToDoList system all the communications are based on https protocol which requires certificates. 
If something is wrong with certificates then there are issues.
Usually for development the self-signed certificates are used. But Docker container within Linux based server or PC has no idea about localhost. 
Because of this the right certificates to ensure development and test under Windows (using localhost) and 
Docker (Docker desktop under Windows or Docker under Linux based OS) are required.

The generate-todolist-self-signed-certificates.ps1 script under solution’s root folder creates the root certificate. 
It also creates for each project (Docker container) the corresponding certificate signed by the root certificate. 
After all the certificate files in different formats are created under Certificates subfolders of each project.

From the solution folder run the script to create certificates on the host Windows PC and certificates related files 
in Certificates subfolders of each project:
.\generate-todolist-self-signed-certificates.ps1

Run this script within each docker container (if ToDoListApi and ToDoListBlazor projects are deployed under docker):
openssl x509 -inform DER -in /https-root/todolistroot.cer -out /https-root/todolistroot.crt
cp /https-root/todolistroot.crt /usr/local/share/ca-certificates/
update-ca-certificates

///////////////////////////////////////
IdentityServer8

The IdentityServer8 nuget package within ToDoListApi project is used to secure API endpoints. It is for .Net8.
Initially IdentityServer worked in memory without using database. This is enough for development.  
Currently the corresponding lines of code are commented in program.cs.
Under production environment it is recommended to run IdentityServer8 using database.

///////////////////////////////////////
How to run solution

From the ToDoListApi project folder run in PowerShell script to create or update database.

If ToDoListApi/Migration folder is empty (you need to can clean up this folder for the first run) then run the following scripts:
dotnet ef migrations add InitConfigration --context ApplicationDbContext
dotnet ef database update --context ApplicationDbContext

dotnet ef migrations add InitConfigration --context ConfigurationDbContext
dotnet ef database update --context ConfigurationDbContext 

dotnet ef migrations add InitConfigration --context PersistedGrantDbContext 
dotnet ef database update --context PersistedGrantDbContext

If ToDoListApi/Migration folder is not empty, the database was created for the first run and DAL project code was updated, 
then the database migration is to be implemented by script:
dotnet ef migrations add update1 --context ApplicationDbContext
dotnet ef database update --context ApplicationDbContext

The ConfigurationDbContext and PersistedGrantDbContext contexts are for IdentityServer8. 

From VisualStudio by F5 run application (ToDoListApi & ToDoListBlazor are start up projects). 
User=Admin, AdminOrg1 или AdminOrg2. Password=Password-123.
https://localhost:7123/swagger/index.html - API endpoints. The ToDoItems can be created using this Swagger’s user interface (UI).
https://localhost:7171 – Blazor client from gets from IdentitiyServer8 within ToDoListApi jwt token and 
using this token gets from ToDoListApi current ToDoItemsCount as a prove of concept example. 

///////////////////////////////////////
How to deploy under Windows server

It is assumed both ToDoListApi and ToDoListBlazor are running under IIS or Kestrel. 
Under IIS the ToDoListApi and ToDoListBlazor sites with the port numbers defined in appsettings.json files of these projects are to be created. 

IIS:
https://localhost:557/swagger/index.html - ToDoListApi
https://localhost:558 - ToDoListBlazor

In PowerShell run both .\ToDoListApi.exe and .\ToDoListBlazor.exe
Kestrel:
https://localhost:7124/swagger/index.html - ToDoListApi

https://localhost:7125 - ToDoListBlazor

///////////////////////////////////////
Docker

The ToDoListApi and ToDoListBlazor projects can be deployed under both Windows and Linux based operating systems without additional changes in settings.

The docker-compose.yml file contains mapping between internal and external ports.

To deploy within Docker Desktop under PowerShell select solution’s folder and run the script:
docker compose up

This script creates containers within Docker Desktop. 

Within ToDoListApi and ToDoListBlazor containers in terminal mode it is required to run the following script:
openssl x509 -inform DER -in /https-root/todolistroot.cer -out /https-root/todolistroot.crt
cp /https-root/todolistroot.crt /usr/local/share/ca-certificates/
update-ca-certificates

The docker-compose.yml file includes todolistproxy container corresponding to ToDoListProxy project. 
The ToDoListProxy project is not included into ToDoList C# solution.
The todolistproxy ensures reverse proxy based on nginx. 
It ensures ToDoLIstBlazor url availability from outside. 
This is especially important when the todolistproxy, todolistapi, todolistblazor containers are 
deployed under Linux based OS. 

Links for test:
https://localhost:1234/swagger/index.html  - ToDoListApi
https://localhost:1112 - ToDoListBlazor
https://localhost:8443   - ToDoListBlazor via Nginx

///////////////////////////////////////
Docker containers deployment under Linux based OS

1.	Copy entire solution into server. 
2.	Create certificates by script run in terminal. The script is below.
3.	From the solution folder run the following script: “docker compose up”.
4.	Run the following script within each docker container:
openssl x509 -inform DER -in /https-root/todolistroot.cer -out /https-root/todolistroot.crt
cp /https-root/todolistroot.crt /usr/local/share/ca-certificates/
update-ca-certificates
	 To run script within docker container run: 
udo docker exec -it 7703e8ef50ff /bin/bash 
5. Put database server info into hosts file. 
	If in appconfig.json of the project the IP address is used instead of server name, then this is fine for the server, but not for docker
	
	Run the following for hosts file update:
    sudo nano /etc/hosts
6.	From outside the ToDoLIstBlazor site is available by https://<Server name>:8443

Script to create certificates:
sudo openssl x509 -inform DER -in ./ToDoListApi/Certificates/todolistroot.cer -out ./ToDoListApi/Certificates/testlabroot.crt
sudo cp ./ToDoListApi/Certificates/todolistroot.crt /usr/local/share/ca-certificates/
sudo update-ca-certificates

sudo openssl pkcs12 -in ./ToDoListApi/Certificates/todolistapi.pfx -nocerts -out ./ToDoListApi/Certificates/todolistapi.pem -nodes
sudo openssl pkcs12 -in ./ToDoListApi/Certificates/todolistapi.pfx -nokeys -out ./ToDoListApi/Certificates/todolistapi.crt -nodes
sudo cp ./ToDoListApi/Certificates/todolistapi.crt /usr/local/share/ca-certificates/todolistapi.crt
sudo cp ./TestLabApi/Certificates/todolistapi.pem /usr/local/share/ca-certificates/todolistapi.pem
sudo update-ca-certificates

openssl pkcs12 -in ./ToDoListApi/Certificates/todolistroot.pfx -nocerts -out ./ToDoListApi/Certificates/todolistroot.pem -nodes
sudo openssl pkcs12 -in ./ToDoListApi/Certificates/todolistroot.pfx -nokeys -out ./ToDoListApi/Certificates/todolistroot.crt -nodes
sudo cp ./ToDoListApi/Certificates/todolistroot.crt /usr/local/share/ca-certificates/todolistroot.crt
sudo cp ./TestLabApi/Certificates/todolistroot.pem /usr/local/share/ca-certificates/todolistroot.pem
sudo update-ca-certificates

openssl pkcs12 -in ./ToDoListBlazor/Certificates/todolistblazor.pfx -nocerts -out ./ToDoListBlazor/Certificates/todolistblazor.pem -nodes
sudo openssl pkcs12 -in ./ToDoListBlazor/Certificates/todolistblazor.pfx -nokeys -out ./ToDoListBlazor /Certificates/todolistblazor.crt -nodes
sudo cp ./ToDoListBlazor/Certificates/todolistblazor.crt /usr/local/share/ca-certificates/todolistblazor.crt
sudo cp ./ToDoListBlazor/Certificates/todolistblazor.pem /usr/local/share/ca-certificates/todolistblazor.pem
sudo update-ca-certificates


///////////////////////////////////////
REST and gRPC services

1. Both REST and gRPC services can be secured using IdentityServer8.
2. The same c# code (for example from from ToDoListService.cs) can be used in both REST and gRPC services.
3. gRPC services can be called from both WinForm and Blazor clients.





