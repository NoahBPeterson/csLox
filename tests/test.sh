#!/bin/bash
for file in ./*/*.lox
do
	../bin/Release/Lox.exe "$file" > "$file".test
done
for file in ./*.lox
do
	../bin/Release/Lox.exe "$file" > "$file".test
done
echo "" > ./testDifference.txt
for file in ./*/*.txt
do
	filename="$(basename $file .txt)"
	path="$(dirname $file)"
	diff -q "$file" "$path/$filename".test >> ./testDifference.txt
done

for file in ./*.txt
do
	filename="$(basename $file .txt)"
	path="$(dirname $file)"
	diff -q "$file" "$path/$filename".test >> ./testDifference.txt
done

