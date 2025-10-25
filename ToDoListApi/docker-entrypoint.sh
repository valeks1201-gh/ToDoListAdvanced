#!/usr/bin/env bash

# exit when any command fails
set -e

# trust dev root CA
openssl x509 -inform DER -in /https-root/todolistroot.cer -out /https-root/todolistroot.crt
cp /https-root/todolistroot.crt /usr/local/share/ca-certificates/
update-ca-certificates

# start the app
dotnet watch run
