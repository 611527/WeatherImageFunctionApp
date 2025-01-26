# Weather Image Function App

## Overview
This project is an **Azure Functions-based serverless application** that fetches real-time weather data from **Buienradar API**, overlays it onto images, and stores them in **Azure Blob Storage** for public access.

The application supports **asynchronous job processing** using **Azure Queue Storage**, ensuring scalability and efficiency.

##  Features
**Public API** to request weather images  
**Queue-based background processing** for efficiency  
**Azure Blob Storage** for image storage & retrieval  
**Weather Data from Buienradar API**  
**Unsplash API** for fetching base images  
**Azure DevOps Deployment** using **Bicep & PowerShell**  
**HTTP API documentation** using `.http` files  

---

## Project Structure
```bash
WeatherImageFunctionApp/
├── infra/                  # Infrastructure as Code (Bicep Templates)
│   ├── main.bicep          # Deploys Function App, Storage, and Queues
│   ├── deploy.ps1          # PowerShell script for automated deployment
│
├── src/                    # Function App Source Code
│   ├── FetchImage.cs       # Fetches weather images from blob storage
│   ├── JobStatusFunction.cs # Checks the status of background jobs
│   ├── ProcessImageQueue.cs # Processes images from the queue
│   ├── StartJob.cs         # Handles HTTP request to start the job
│   ├── StartJobProcessor.cs # Processes job queue messages
│   ├── local.settings.json # Local configuration for function app
│   ├── Program.cs          # Entry point for Azure Functions app
│
├── docs/                   # API Documentation
│   ├── init.http           # HTTP file for API testing (VS Code / Postman)
│
├── .gitignore              # Git ignore file
├── README.md               # Project documentation (this file)
