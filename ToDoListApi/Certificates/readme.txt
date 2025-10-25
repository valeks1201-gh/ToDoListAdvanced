Within ToDoList system all the communications are based on https protocol which requires certificates. 
If something is wrong with certificates then there are issues.
Usually for development the self signed certificates are used. But Docker container within Linux based server or PC has no idea about localhost. 
Because of this the right certificates to ensure development and test under Windows (using localhost) and Docker (Docker desktop under Winndows or Docker under Linux based OS) are required.
The script below first of all creates the root certificate. After this for each project (Docker container) the corresponding certificate signed by the root certificate is created. 
After all the certificate files under different formats are created under Certificates subfolders of each project.

From the solution folder run the script to create certificates on the host Windows PC and certificates related files in Certificates subfolders of each project:
.\generate-todolist-self-signed-certificates.ps1


Run this script within each docker container:
openssl x509 -inform DER -in /https-root/todolistroot.cer -out /https-root/todolistroot.crt
cp /https-root/todolistroot.crt /usr/local/share/ca-certificates/
update-ca-certificates
