#!/bin/bash
for file in ./*/*.lox
do
	echo "$file"
	../bin/Release/Lox.exe "$file" > "$file".txt &
done
for file in ./*.lox
do
	../bin/Release/Lox.exe "$file" > "$file".txt &
done