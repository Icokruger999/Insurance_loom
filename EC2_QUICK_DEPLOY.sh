#!/bin/bash
# Quick EC2 Deployment Script
# Run this on your EC2 instance

cd /var/www/api
git pull origin main
dotnet publish -c Release -o ./publish
sudo systemctl stop insuranceloom-api.service
sudo cp -r ./publish/* /var/www/api/
sudo chown -R ec2-user:ec2-user /var/www/api
sudo systemctl start insuranceloom-api.service
sudo systemctl status insuranceloom-api.service

