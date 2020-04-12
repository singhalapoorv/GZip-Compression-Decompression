# GZip-Compression-Decompression
Double Compression and Decompression algorithm 

This algorithm can convert DataTables of very large sizes(in order of GBs) into byte strings of order in ranges of 100-300 times less than the original size.
Actual compression ratio depends on System fctors also like RAM, maximum variable size e.t.c.

It breaks down the DataTable into chunks of tables and double compresses each table into a byte string which is concatenated and returned.

Similarly, while decompression, the string separated by ',' into list of strings which is double decompressed to generate each chunk table. 
These all are then combined to return the actual table.

Feel free to raise an MR to improve the efficiency of the algorithm.

