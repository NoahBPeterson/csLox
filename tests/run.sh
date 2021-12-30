#!/bin/bash
for file in ./*/*.lox
do
	echo "$file"
	../bin/x64/Release/net5.0/Lox.exe "$file" > "$file".txt &
done
for file in ./*.lox
do
	../bin/x64/Release/net5.0/Lox.exe "$file" > "$file".txt &
done