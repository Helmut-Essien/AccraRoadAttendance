using MigraDoc.DocumentObjectModel;
using MigraDoc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccraRoadAttendance.Views.Pages.Reports
{
    public class ReportGenerator
    {
        private Document _document;
        private Section _section;

        public void GenerateReport(string reportType, object data, DateTime startDate, DateTime endDate, string filePath)
        {
            _document = new Document();
            _section = _document.AddSection();

            // Add header and footer
            AddHeader();
            AddFooter();

            // Add report title
            var title = _section.AddParagraph(reportType);
            title.Format.Font.Size = 14;
            title.Format.Font.Bold = true;
            title.Format.Alignment = ParagraphAlignment.Center;
            title.Format.SpaceAfter = "1cm";

            // Add date range
            var dateRange = _section.AddParagraph($"From {startDate.ToShortDateString()} to {endDate.ToShortDateString()}");
            dateRange.Format.Font.Size = 10;
            dateRange.Format.Alignment = ParagraphAlignment.Center;
            dateRange.Format.SpaceAfter = "1cm";

            // Add report-specific table
            switch (reportType)
            {
                case "Individual Attendance":
                    AddIndividualAttendanceTable(data);
                    break;
                case "Church Attendance Summary":
                    AddChurchAttendanceSummaryTable(data);
                    break;
                case "Service Type Report":
                    AddServiceTypeReportTable(data);
                    break;
                case "Demographic Report":
                    AddDemographicReportTable(data);
                    break;
                case "Offering Report":
                    AddOfferingReportTable(data);
                    break;
                case "Visitor and Newcomer Report":
                    AddVisitorReportTable(data);
                    break;
                case "Absentee Report":
                    AddAbsenteeReportTable(data);
                    break;
            }

            // Render and save the PDF
            var renderer = new PdfDocumentRenderer(true);
            renderer.Document = _document;
            renderer.RenderDocument();
            renderer.PdfDocument.Save(filePath);
        }

        private void AddHeader()
        {
            var header = _section.Headers.Primary;
            var table = header.AddTable();
            table.AddColumn("2cm"); // Logo
            table.AddColumn("12cm"); // Church info

            var row = table.AddRow();
            var logo = row.Cells[0].AddImage("church_logo.png");
            logo.Width = "2cm";

            var churchInfo = row.Cells[1].AddParagraph();
            churchInfo.Format.Font.Size = 12;
            churchInfo.AddText("Accra Road Church\n");
            churchInfo.AddText("123 Faith Street, Accra\n");
            churchInfo.AddText("Phone: 123-456-7890 | Email: info@church.org | www.church.org");
        }

        private void AddFooter()
        {
            var footer = _section.Footers.Primary;
            var paragraph = footer.AddParagraph();
            paragraph.Format.Font.Size = 10;
            paragraph.Format.Alignment = ParagraphAlignment.Center;
            paragraph.AddText("Page ");
            paragraph.AddPageField();
            paragraph.AddText(" of ");
            paragraph.AddNumPagesField();
        }

        private void AddIndividualAttendanceTable(object data)
        {
            var table = _section.AddTable();
            table.Borders.Width = 0.5;
            table.AddColumn("3cm"); // Service Date
            table.AddColumn("3cm"); // Service Type
            table.AddColumn("2cm"); // Status
            table.AddColumn("6cm"); // Notes

            var headerRow = table.AddRow();
            headerRow.HeadingFormat = true;
            headerRow.Format.Font.Bold = true;
            headerRow.Cells[0].AddParagraph("Service Date");
            headerRow.Cells[1].AddParagraph("Service Type");
            headerRow.Cells[2].AddParagraph("Status");
            headerRow.Cells[3].AddParagraph("Notes");

            foreach (var record in (IEnumerable<dynamic>)data)
            {
                var row = table.AddRow();
                row.Cells[0].AddParagraph(record.ServiceDate.ToShortDateString());
                row.Cells[1].AddParagraph(record.ServiceType.ToString());
                row.Cells[2].AddParagraph(record.Status.ToString());
                row.Cells[3].AddParagraph(record.Notes?.ToString() ?? "");
            }
        }

        private void AddChurchAttendanceSummaryTable(object data)
        {
            var table = _section.AddTable();
            table.Borders.Width = 0.5;
            table.AddColumn("2cm"); // Summary Date
            table.AddColumn("2cm"); // Service Type
            table.AddColumn("2cm"); // Total Present
            table.AddColumn("2cm"); // Male Present
            table.AddColumn("2cm"); // Female Present
            table.AddColumn("2cm"); // Visitors
            table.AddColumn("2cm"); // Children
            table.AddColumn("2cm"); // Offering

            var headerRow = table.AddRow();
            headerRow.HeadingFormat = true;
            headerRow.Format.Font.Bold = true;
            headerRow.Cells[0].AddParagraph("Date");
            headerRow.Cells[1].AddParagraph("Service");
            headerRow.Cells[2].AddParagraph("Total");
            headerRow.Cells[3].AddParagraph("Males");
            headerRow.Cells[4].AddParagraph("Females");
            headerRow.Cells[5].AddParagraph("Visitors");
            headerRow.Cells[6].AddParagraph("Children");
            headerRow.Cells[7].AddParagraph("Offering");

            foreach (var record in (IEnumerable<dynamic>)data)
            {
                var row = table.AddRow();
                row.Cells[0].AddParagraph(record.SummaryDate.ToShortDateString());
                row.Cells[1].AddParagraph(record.ServiceType.ToString());
                row.Cells[2].AddParagraph(record.TotalPresent.ToString());
                row.Cells[3].AddParagraph(record.TotalMalePresent.ToString());
                row.Cells[4].AddParagraph(record.TotalFemalePresent.ToString());
                row.Cells[5].AddParagraph(record.Visitors.ToString());
                row.Cells[6].AddParagraph(record.Children.ToString());
                row.Cells[7].AddParagraph(record.OfferingAmount.ToString("C"));
            }
        }

        private void AddServiceTypeReportTable(object data)
        {
            var table = _section.AddTable();
            table.Borders.Width = 0.5;
            table.AddColumn("2cm"); // Summary Date
            table.AddColumn("2cm"); // Service Type
            table.AddColumn("2cm"); // Males
            table.AddColumn("2cm"); // Females
            table.AddColumn("2cm"); // Children
            table.AddColumn("2cm"); // Visitors
            table.AddColumn("2cm"); // Total

            var headerRow = table.AddRow();
            headerRow.HeadingFormat = true;
            headerRow.Format.Font.Bold = true;
            headerRow.Cells[0].AddParagraph("Date");
            headerRow.Cells[1].AddParagraph("Service");
            headerRow.Cells[2].AddParagraph("Males");
            headerRow.Cells[3].AddParagraph("Females");
            headerRow.Cells[4].AddParagraph("Children");
            headerRow.Cells[5].AddParagraph("Visitors");
            headerRow.Cells[6].AddParagraph("Total");

            foreach (var record in (IEnumerable<dynamic>)data)
            {
                var row = table.AddRow();
                row.Cells[0].AddParagraph(record.SummaryDate.ToShortDateString());
                row.Cells[1].AddParagraph(record.ServiceType.ToString());
                row.Cells[2].AddParagraph(record.MalePresent.ToString());
                row.Cells[3].AddParagraph(record.FemalePresent.ToString());
                row.Cells[4].AddParagraph(record.Children.ToString());
                row.Cells[5].AddParagraph(record.Visitors.ToString());
                row.Cells[6].AddParagraph(record.TotalPresent.ToString());
            }
        }

        private void AddDemographicReportTable(object data)
        {
            var table = _section.AddTable();
            table.Borders.Width = 0.5;
            table.AddColumn("5cm"); // Gender
            table.AddColumn("5cm"); // Total Present

            var headerRow = table.AddRow();
            headerRow.HeadingFormat = true;
            headerRow.Format.Font.Bold = true;
            headerRow.Cells[0].AddParagraph("Gender");
            headerRow.Cells[1].AddParagraph("Total Present");

            foreach (var record in (IEnumerable<dynamic>)data)
            {
                var row = table.AddRow();
                row.Cells[0].AddParagraph(record.Gender.ToString());
                row.Cells[1].AddParagraph(record.TotalPresent.ToString());
            }
        }

        private void AddOfferingReportTable(object data)
        {
            var table = _section.AddTable();
            table.Borders.Width = 0.5;
            table.AddColumn("5cm"); // Summary Date
            table.AddColumn("5cm"); // Service Type
            table.AddColumn("4cm"); // Offering Amount

            var headerRow = table.AddRow();
            headerRow.HeadingFormat = true;
            headerRow.Format.Font.Bold = true;
            headerRow.Cells[0].AddParagraph("Date");
            headerRow.Cells[1].AddParagraph("Service");
            headerRow.Cells[2].AddParagraph("Offering");

            foreach (var record in (IEnumerable<dynamic>)data)
            {
                var row = table.AddRow();
                row.Cells[0].AddParagraph(record.SummaryDate.ToShortDateString());
                row.Cells[1].AddParagraph(record.ServiceType.ToString());
                row.Cells[2].AddParagraph(record.OfferingAmount.ToString("C"));
            }
        }

        private void AddVisitorReportTable(object data)
        {
            var table = _section.AddTable();
            table.Borders.Width = 0.5;
            table.AddColumn("5cm"); // Summary Date
            table.AddColumn("5cm"); // Service Type
            table.AddColumn("4cm"); // Visitors

            var headerRow = table.AddRow();
            headerRow.HeadingFormat = true;
            headerRow.Format.Font.Bold = true;
            headerRow.Cells[0].AddParagraph("Date");
            headerRow.Cells[1].AddParagraph("Service");
            headerRow.Cells[2].AddParagraph("Visitors");

            foreach (var record in (IEnumerable<dynamic>)data)
            {
                var row = table.AddRow();
                row.Cells[0].AddParagraph(record.SummaryDate.ToShortDateString());
                row.Cells[1].AddParagraph(record.ServiceType.ToString());
                row.Cells[2].AddParagraph(record.Visitors.ToString());
            }
        }

        private void AddAbsenteeReportTable(object data)
        {
            var table = _section.AddTable();
            table.Borders.Width = 0.5;
            table.AddColumn("8cm"); // Full Name
            table.AddColumn("6cm"); // Attendance %

            var headerRow = table.AddRow();
            headerRow.HeadingFormat = true;
            headerRow.Format.Font.Bold = true;
            headerRow.Cells[0].AddParagraph("Full Name");
            headerRow.Cells[1].AddParagraph("Attendance %");

            foreach (var record in (IEnumerable<dynamic>)data)
            {
                var row = table.AddRow();
                row.Cells[0].AddParagraph(record.FullName.ToString());
                row.Cells[1].AddParagraph($"{record.AttendancePercentage:F2}%");
            }
        }
    }
}
