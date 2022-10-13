#!/bin/bash

rm -rf ./publish

dotnet publish -c Release -o ./publish -r linux-x64 /p:PublishSingleFile=true /p:PublishTrimmed=true /p:DebugType=None /p:DebugSymbols=false --self-contained true