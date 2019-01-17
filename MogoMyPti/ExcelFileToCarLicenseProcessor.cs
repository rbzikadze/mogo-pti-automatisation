using ExcelDataReader;
using System;
using System.Collections.Generic;
using System.IO;

namespace MogoMyPti
{
    public class ExcelFileToCarLicenseProcessor
    {
        private readonly string filePath;


        public ExcelFileToCarLicenseProcessor(string filePath)
        {
            this.filePath = filePath;
        }

        public List<string> GetResult()
        {
            var result = new List<string>();

            using (var stream = File.Open(filePath, FileMode.Open, FileAccess.Read))
            {
                using (var reader = ExcelReaderFactory.CreateReader(stream))
                {
                    do
                    {
                        while (reader.Read())
                        {
                            var rowData = reader.GetString(0);
                            if (!string.IsNullOrEmpty(rowData))
                            {
                                if (rowData.Trim().Length != 0)
                                {
                                    result.Add(rowData);
                                }
                            }
                        }
                    }
                    while (reader.NextResult());
                }
            }
            if (result.Count >= 1)
                result.RemoveAt(0);

            return result;
        }

    }
}
