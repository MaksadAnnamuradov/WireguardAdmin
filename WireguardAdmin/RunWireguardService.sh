﻿#!/bin/bash
sudo systemctl stop WireguardAdmin.service &&  dotnet publish -c Release -o /var/WireguardAdmin && sudo systemctl start WireguardAdmin.service