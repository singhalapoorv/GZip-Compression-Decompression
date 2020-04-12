using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Rainier.Helper
{
    public static class TKSCompression
    {
        private const int CHUNK_SIZE = 10000;

        public static string CompressDataTableToString(DataTable orgTable)
        {
            int chunks = 0;
            int chunkLimit = 0;
            int rowCount = orgTable.Rows.Count;

            var newDt = new DataTable();
            newDt = orgTable.Clone();
            newDt.Clear();

            List<DataTable> chunkedTableList = new List<DataTable>();
            string finalContentString = "";

            //Separate DataTable into chunks of DataTable
            foreach (DataRow row in orgTable.Rows)
            {
                DataRow newRow = newDt.NewRow();
                newRow.ItemArray = row.ItemArray;
                newDt.Rows.Add(newRow);
                chunkLimit++;
                if (chunkLimit == CHUNK_SIZE || (chunks == rowCount / CHUNK_SIZE && chunkLimit == rowCount % CHUNK_SIZE))
                {
                    chunkedTableList.Add(newDt);
                    chunks++;
                    newDt = new DataTable();
                    newDt = orgTable.Clone();
                    newDt.Clear();
                    chunkLimit = 0;
                }
            }

            //Compress each DataTable into binary and add to content string
            foreach (DataTable table in chunkedTableList)
            {
                byte[] binaryDataResult = null;
                using (MemoryStream memStream = new MemoryStream())
                {
                    using (GZipStream gzip = new GZipStream(memStream,
                    CompressionMode.Compress, true))
                    {
                        var formatter = new BinaryFormatter();
                        table.RemotingFormat = SerializationFormat.Binary;
                        formatter.Serialize(gzip, table);
                    }
                    binaryDataResult = memStream.ToArray();
                }
                using (MemoryStream memStream = new MemoryStream())
                {
                    using (GZipStream gzip = new GZipStream(memStream,
                    CompressionMode.Compress, true))
                    {
                        gzip.Write(binaryDataResult, 0, binaryDataResult.Length);
                    }
                    binaryDataResult = memStream.ToArray();
 
                }
                string chunkedContent = System.Convert.ToBase64String(binaryDataResult) + ';';
                finalContentString += chunkedContent;
            }
            finalContentString = finalContentString.Remove(finalContentString.Length - 1);
            return finalContentString;
        }

        public static DataTable DecompressStringToDataTable(string compressedString)
        {
            DataSet tempDataSet = new DataSet();
            var splitDataTables = compressedString.Split(';');

            //Decompress and add to DataSet
            foreach (var tableString in splitDataTables)
            {
                DataTable dt = new DataTable();
                byte[] convertedString = System.Convert.FromBase64String(tableString);
                using (MemoryStream stream = new MemoryStream(convertedString))
                {
                    using (GZipStream gzip = new GZipStream(stream, CompressionMode.Decompress))
                    {
                        using (var outputStream = new MemoryStream())
                        {
                            gzip.CopyTo(outputStream);
                            convertedString = outputStream.ToArray();
                        }
                    }
                }
                using (MemoryStream stream = new MemoryStream(convertedString))
                {
                    using (GZipStream gzip = new GZipStream(stream, CompressionMode.Decompress))
                    {
                        BinaryFormatter bformatter = new BinaryFormatter();
                        dt = (DataTable)bformatter.Deserialize(gzip);
                    }
                }
                tempDataSet.Merge(dt);
            }
            return tempDataSet.Tables[0];
        }
    }
}
