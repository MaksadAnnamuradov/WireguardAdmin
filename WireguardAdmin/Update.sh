#!/bin/bash
git pull && systemctl stop WireguardAdmin.service &&  dotnet publish -c Release -o /var/WireguardAdmin && systemctl start WireguardAdmin.service && systemctl status WireguardAdmin.service