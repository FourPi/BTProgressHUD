#!/bin/bash

pushd BTProgressHUD
rm -rf bin/
rm -rf obj/
msbuild /t:Clean,Build /restore /p:Configuration=Release BTProgressHUD.csproj
popd