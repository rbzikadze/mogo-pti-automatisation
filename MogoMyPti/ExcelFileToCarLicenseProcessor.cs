using ExcelDataReader;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                            result.Add(reader.GetString(0));
                        }
                    } while (reader.NextResult());
                }
            }
            result.RemoveAt(0);

            return result;
        }

    }
}
