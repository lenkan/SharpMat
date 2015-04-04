# SharpMat
.NET API for reading and writing Matlab .MAT files

This is a work in progress to create a .NET API for reading and writing Matlab .MAT-files. 
Since it is recommended to use the libraries provided by Matlab, i.e. libmat and libmx,
to read and write .MAT files, this project was created almost purely for training purposes.

However, it would be nice to not have to use interop for performing IO operations on these files.
In addition, this API is designed to support Streaming from large files.
