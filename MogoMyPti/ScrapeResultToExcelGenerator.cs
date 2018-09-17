using MogoMyPti.Entities;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MogoMyPti
{
    public class ScrapeResultToExcelGenerator
    {
        private readonly List<ScrapeResult> scrapeResults;

        public ScrapeResultToExcelGenerator(List<ScrapeResult> scrapeResults)
        {
            this.scrapeResults = scrapeResults;
        }

        public void GenerateExcel(string filePath)
        {
            using (ExcelPackage excel = new ExcelPackage())
            {
                excel.Workbook.Worksheets.Add("Sheet1");

                var headerRow = new List<string[]>()
                  {
                    new string[] { "მანქანის სახელმწიფო ნომერი", "პტი დასაწყისი", "პტი ბოლო ვადა" }
                  };

                // Determine the header range (e.g. A1:D1)
                string headerRange = "A1:" + Char.ConvertFromUtf32(headerRow[0].Length + 64) + "1";

                // Target a worksheet
                var worksheet = excel.Workbook.Worksheets["Sheet1"];

                // Popular header row data
                worksheet.Cells[headerRange].LoadFromArrays(headerRow);

                var cellData = scrapeResults.Select((result) =>
                {
                    return new object[] { result.License, result.StartDate.HasValue ? result.StartDate.Value.ToString("MM/dd/yyyy") : "", result.LastDate.HasValue ? result.LastDate.Value.ToString("MM/dd/yyyy") : "" };
                });

                worksheet.Cells[2, 1].LoadFromArrays(cellData);

                FileInfo excelFile = new FileInfo(filePath);
                excel.SaveAs(excelFile);
            }
        }
    }
}
