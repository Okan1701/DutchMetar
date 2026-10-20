> [!WARNING]  
> This project is still a work in progress! Features are still missing. Any published binary or docker image is not meant to be used yet.
# About

[![Azure Pipelines](https://dev.azure.com/okan1701/DutchMetar/_apis/build/status%2FDutchMetar%20CICD?branchName=master)](https://dev.azure.com/okan1701/DutchMetar/_build/latest?definitionId=2&branchName=master)
[![GitHub Actions](https://github.com/Okan1701/DutchMetar/actions/workflows/build.yaml/badge.svg)](https://github.com/Okan1701/DutchMetar/actions/workflows/build.yaml)
![Static Badge](https://img.shields.io/badge/dockerhub-available-green?logo=docker)


DutchMetar is a .NET project for displaying METAR and TAF data for Dutch airports. 
The goal is to visualize the latest data and to provide a historical view.
The data is retrieved from the KNMI Data Platform. [Click here to learn more about KNMI Data platform](https://dataplatform.knmi.nl/).

The project currently consists of a background job worker that retreives and stores data and a interactive web application for displaying it.

The main purpose for this project is purely for hobbyist reasons. 

# Used technologies

The project uses the following tech stacks:
* (ASP) .NET 10
* Hangfire for .NET
* Entityframework Core
* Microsoft SQL Server
* Docker

# How to Contribute

If you wish to contribute to this repository, then checkout the [Contributing](CONTRIBUTING.MD) file.
