using DotnetGRPC.Model.DTO;
using Grpc.Core;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.IO;

namespace DotnetGRPC.Services
{

    public class ReportServiceImp : ReportService.ReportServiceBase
    {
        private readonly ContributionRepository _contributionRepository;

        public ReportServiceImp(ContributionRepository contributionRepository)
        {
            _contributionRepository = contributionRepository;
        }
        public async Task<byte[]> CreatePdfWithTable()
        {
            using (MemoryStream ms = new MemoryStream())
            {
                // Create a new PDF document
                Document document = new Document();
                PdfWriter.GetInstance(document, ms);

                // Open the document
                document.Open();

                // Add a title
                Paragraph title = new Paragraph("Faculty Contribution Report", FontFactory.GetFont("Arial", 24, Font.BOLD));
                title.Alignment = Element.ALIGN_CENTER;
                document.Add(title);
                document.Add(new Paragraph(" "));

                // Table 1
                Paragraph header = new Paragraph("1. Number of Contributions per Faculty:", FontFactory.GetFont("Arial", 20, Font.BOLD));
                document.Add(header);
                document.Add(new Paragraph(" "));
                PdfPTable table = new PdfPTable(2);
                table.AddCell("Faculty");
                table.AddCell("Number of Contributions");
                Dictionary<string, int> countsByFaculty = await _contributionRepository.CountContributionsByFaculty();
                int total = 0;
                foreach (KeyValuePair<string, int> pair in countsByFaculty)
                {
                    table.AddCell(pair.Key);
                    table.AddCell(pair.Value.ToString());
                    total += pair.Value;
                }
                table.AddCell("Total");
                table.AddCell(total.ToString());
                document.Add(table);

                //Table 2
                document.Add(new Paragraph(" "));
                Paragraph header2 = new Paragraph("2. Percentage of Contribution by Each Faculty:", FontFactory.GetFont("Arial", 20, Font.BOLD));
                document.Add(header2);
                document.Add(new Paragraph(" "));
                PdfPTable table2 = new PdfPTable(2);
                table2.AddCell("Faculty");
                table2.AddCell("Percentage of Contributions");
                foreach (KeyValuePair<string, int> pair in countsByFaculty)
                {
                    double percentage = (double)pair.Value / total * 100;
                    table2.AddCell(pair.Key);
                    table2.AddCell($"{percentage:0.00}%");
                }
                document.Add(table2);

                //Table 3
                document.Add(new Paragraph(" "));
                Paragraph header3 = new Paragraph("3. Number of Contributors (Students) per Faculty:", FontFactory.GetFont("Arial", 20, Font.BOLD));
                document.Add(header3);
                document.Add(new Paragraph(" "));
                PdfPTable table3 = new PdfPTable(2);
                table3.AddCell("Faculty");
                table3.AddCell("Number of Contributors");
                Dictionary<string, int> contributorsByFaculty = await _contributionRepository.CountContributorsByFaculty();
                foreach (KeyValuePair<string, int> pair in contributorsByFaculty)
                {
                    table3.AddCell(pair.Key);
                    table3.AddCell(pair.Value.ToString());
                }
                document.Add(table3);

                //Table 4
                document.Add(new Paragraph(" "));
                Paragraph header4 = new Paragraph("4. Average Contribution per Contributor by Faculty:", FontFactory.GetFont("Arial", 20, Font.BOLD));
                document.Add(header4);
                document.Add(new Paragraph(" "));
                PdfPTable table4 = new PdfPTable(2);
                table4.AddCell("Faculty");
                table4.AddCell("Average Contribution per Contributor");
                Dictionary<string, double> averagesByFaculty = await _contributionRepository.AverageContributionsByFaculty();
                foreach (KeyValuePair<string, double> pair in averagesByFaculty)
                {
                    table4.AddCell(pair.Key);
                    table4.AddCell($"{pair.Value:0.00}");
                }
                document.Add(table4);

                //Table 5
                document.Add(new Paragraph(" "));
                Paragraph header5 = new Paragraph("5. Contribution Trends Over Time by Faculty:", FontFactory.GetFont("Arial", 20, Font.BOLD));
                document.Add(header5);
                document.Add(new Paragraph(" "));
                List<ContributionTrend> trends = await _contributionRepository.ContributionTrendsByFaculty();
                var dates = trends.Select(t => t.Date).Distinct().OrderBy(d => d).ToList();
                var faculties = trends.Select(t => t.Faculty).Distinct().OrderBy(f => f).ToList();
                var pivotData = dates.ToDictionary(date => date, date => faculties.ToDictionary(faculty => faculty, faculty => trends.FirstOrDefault(t => t.Date == date && t.Faculty == faculty)?.Count ?? 0));
                PdfPTable table5 = new PdfPTable(faculties.Count + 1);
                table5.AddCell("Month");
                foreach (string faculty in faculties)
                {
                    table5.AddCell(faculty);
                }
                foreach (KeyValuePair<string, Dictionary<string, int>> pair in pivotData)
                {
                    table5.AddCell(pair.Key);
                    foreach (int count in pair.Value.Values)
                    {
                        table5.AddCell(count.ToString());
                    }
                }
                document.Add(table5);

                // Close the document
                document.Close();
                return ms.ToArray();
            }
        }

        public override Task<ReportResponse> GetReport(Google.Protobuf.WellKnownTypes.Empty request, ServerCallContext context)
        {
            return Task.FromResult(new ReportResponse
            {
                ReportData = Google.Protobuf.ByteString.CopyFrom(CreatePdfWithTable().Result)
            });
        }
    }
}