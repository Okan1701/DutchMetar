> [!WARNING]  
> This project is still a work in progress! Features are still missing. Any published binary or Docker image is not meant to be used yet.
# About

[![Azure Pipelines](https://dev.azure.com/okan1701/DutchMetar/_apis/build/status%2FDutchMetar%20CICD?branchName=master)](https://dev.azure.com/okan1701/DutchMetar/_build/latest?definitionId=2&branchName=master)
[![GitHub Actions](https://github.com/Okan1701/DutchMetar/actions/workflows/build.yaml/badge.svg)](https://github.com/Okan1701/DutchMetar/actions/workflows/build.yaml)
![Static Badge](https://img.shields.io/badge/dockerhub-available-green?logo=docker)


DutchMetar is a .NET project for displaying METAR and TAF data for Dutch airports. 
The goal is to visualize the latest data and to provide a historical view.
The data is retrieved from the KNMI Data Platform. [Click here to learn more about KNMI Data platform](https://dataplatform.knmi.nl/).

The project currently consists of a Web SPA for displaying the data and a worker service that provides the data.

The main purpose for this project is purely for hobbyist reasons. 

# Used technologies

The project uses the following tech stacks:
* (ASP) .NET 10
* Hangfire for .NET
* Entity Framework Core
* Microsoft SQL Server
* Angular 
* Docker

# How to self-host

The project has its own official website. But it is also possible to self-host on your own servers.

### Docker

The following docker images on Docker Hub are available to run:
- [ultimateantic/dutchmetar](https://hub.docker.com/r/ultimateantic/dutchmetar) _Web application_
- [ultimateantic/dutchmetar-worker](https://hub.docker.com/r/ultimateantic/dutchmetar-worker) _Worker service_

You must provide an SQL Server instance to both containers.
See the [docker compose file](docker-compose.yml) for an example on how to run this.

You must also provide your own access token to the KNMI Data Platform. To obtain this, check out their developer hub.

### Windows / Linux native

Currently only Docker images are published to the internet. If you wish to run this natively on a Linux or Windows Server, you must manually build the project.

# How to Contribute

If you wish to contribute to this repository, then checkout the [Contributing](CONTRIBUTING.MD) file.
