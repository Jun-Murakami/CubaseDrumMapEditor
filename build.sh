#!/bin/bash

dotnet restore -r osx-x64
dotnet publish -c Release --self-contained -p:PublishSingleFile=true --runtime osx-x64
mkdir -p CubaseDrumMapEditor.app/Contents/MacOS
cp -r bin/Release/net6.0/osx-x64/publish/* CubaseDrumMapEditor.app/Contents/MacOS
cp Info.plist CubaseDrumMapEditor.app/Contents/
mkdir CubaseDrumMapEditor.app/Contents/Resources
cp Assets/avalonia-logo.icns CubaseDrumMapEditor.app/Contents/Resources