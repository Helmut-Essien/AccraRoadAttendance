using DocumentFormat.OpenXml.ExtendedProperties;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Tables;
using MigraDoc.Rendering;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;

namespace AccraRoadAttendance.Views.Pages.Reports
{
    public class ReportGenerator
    {
        private Document _document;
        private Section _section;
        private readonly Color _blueColor = new Color(3, 119, 162);

        public void GenerateReport(string reportType, object data, DateTime startDate, DateTime endDate, string filePath, string memberName = null)
        {
            _document = new Document();
            //_document.DefaultPageSetup.LeftMargin = "2cm";
            //_document.DefaultPageSetup.RightMargin = "2cm";
            _section = _document.AddSection();
            // Set page margins on the section's PageSetup instead of modifying DefaultPageSetup
            _section.PageSetup.TopMargin = "5.7cm";
            _section.PageSetup.HeaderDistance = "1cm";
            _section.PageSetup.LeftMargin = "1cm";
            _section.PageSetup.RightMargin = "1cm";


            // Add header and footer using the letterhead template from the attached file
            AddHeader();
            AddReportContent(reportType, startDate, endDate, data, memberName);
            AddFooter();
            RenderAndSavePdf(filePath);
        }

        private void AddReportContent(string reportType, DateTime startDate, DateTime endDate, object data, string memberName)
        {
        
            
            // Add report title
            var title = _section.AddParagraph(reportType);
            title.Format.Font.Size = 14;
            title.Format.Font.Bold = true;
            title.Format.Font.Color = Colors.Black;
            title.Format.Alignment = ParagraphAlignment.Center;
            title.Format.SpaceAfter = "0.5cm";

            // Add date range
          

            var dateRange = _section.AddParagraph($"This is the attendance history of {memberName} from {startDate.ToString("dd-MMM-yyyy").ToUpper()} to {endDate.ToString("dd-MMM-yyyy").ToUpper()}");
            dateRange.Format.Font.Size = 12;
            dateRange.Format.Alignment = ParagraphAlignment.Left;
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
        }

        private void AddHeader()
        {
            var header = _section.Headers.Primary;

            // Step 1: Load the image from the application's resources
            var imageUri = new Uri("pack://application:,,,/AccraRoadAttendance;component/AppImages/CLogoCropped.png", UriKind.Absolute);
            var imageStream = System.Windows.Application.GetResourceStream(imageUri).Stream;

            // Step 2: Save the image to a temporary file
            string tempImagePath = Path.Combine(Path.GetTempPath(), "CLogoc.png");
            using (var fileStream = new FileStream(tempImagePath, FileMode.Create))
            {
                imageStream.CopyTo(fileStream);
            }

            // Logo and Church Name Section
            var mainTable = header.AddTable();
            mainTable.AddColumn("6.5cm"); // Left column for logo
            mainTable.AddColumn("11cm"); // Center column for text
            mainTable.AddColumn("1cm"); // Right column (spacer for centering)


            var mainRow = mainTable.AddRow();

            // Instead of adding the image directly, add a paragraph to control alignment
            var logoParagraph = mainRow.Cells[0].AddParagraph();
            logoParagraph.Format.Alignment = ParagraphAlignment.Right; // Align the paragraph (and image) to the right
            var logo = logoParagraph.AddImage(tempImagePath);
            logo.Height = "1.5cm"; // Adjust based on your actual text height


            // Text container
            var textCell = mainRow.Cells[1];
            textCell.VerticalAlignment = MigraDoc.DocumentObjectModel.Tables.VerticalAlignment.Center;



            // Main church name
            var churchName = textCell.AddParagraph("Church of Christ");
            churchName.Format.Font.Name = "Arial"; // Ensure font supports bold
            churchName.Format.Font.Size = 26;
            churchName.Format.Font.Bold = true;
            churchName.Format.Alignment = ParagraphAlignment.Left; // Align left relative to the cell to position beside the logo

            // Subtitle with expanded character spacing
            var subtitleText = "The Pillar and Foundation of the Truth";
            var spacedText = string.Join("\u200A", subtitleText.ToCharArray()); // Using hair space for subtle spacing
            var subtitle = textCell.AddParagraph(spacedText);
            subtitle.Format.Font.Name = "Arial";
            subtitle.Format.Font.Size = 10;
            subtitle.Format.Font.Italic = true;
            subtitle.Format.Alignment = ParagraphAlignment.Left;
            //subtitle.Format.SpaceBefore = "0.2cm";


            // Congregation Name
            var congregation = header.AddParagraph("Accra-Road Congregation");
            congregation.Format.Font.Size = 16;
            congregation.Format.Font.Color = _blueColor;
            congregation.Format.Font.Bold = true;
            congregation.Format.SpaceBefore = "0.2cm";
            congregation.Format.SpaceAfter = "0.2cm";
            congregation.Format.Alignment = ParagraphAlignment.Center;

            // Contact Information Table
            var contactTable = header.AddTable();
            var T1 = contactTable.AddColumn("9.5cm");
            T1.Format.Alignment = ParagraphAlignment.Left;
            var T2 = contactTable.AddColumn("9.5cm");
            T2.Format.Alignment = ParagraphAlignment.Right;

            // First row
            var row = contactTable.AddRow();
            row.Cells[0].AddParagraph("Location: Y0582, SONATA ST");
            row.Cells[1].AddParagraph("Amanfro behind JD Restaurant");
            //row.Format.Alignment = ParagraphAlignment.Center;



            // Second row
            row = contactTable.AddRow();
            row.Cells[0].AddParagraph("Email Address: archurchofchrist@gmail.com");
            row.Cells[1].AddParagraph("Digital Address: GS-0686-7830");
            //row.Format.Alignment = ParagraphAlignment.Center;

            // Third row
            row = contactTable.AddRow();
            var telCell = row.Cells[0].AddParagraph("TEL: 0244265642/0244161872");
            row.Cells[1].AddParagraph("P.O.Box WU 554, Kasoa.");
            ////row.Format.Alignment = ParagraphAlignment.Center;
            //telCell.Format.SpaceAfter = "0.1cm";


            // Add simulated document classification
            var classificationBar = _section.AddParagraph();
            classificationBar.Format.Shading.Color = Colors.Black;
            classificationBar.Format.SpaceBefore = "0.01cm";
            classificationBar.Format.SpaceAfter = "0.3cm"; // Removed the invalid 'Height' property

        }

        private void AddFooter()
        {
            var footer = _section.Footers.Primary;
            // Horizontal line
            var line = footer.AddParagraph();
            line.Format.Borders.Top.Width = 0.75;
            line.Format.Borders.Top.Color = Colors.Black;
            line.Format.SpaceBefore = "0.2cm";

            var footTable = footer.AddTable();
            footTable.AddColumn("11cm"); // Left column for Bible verse
            footTable.AddColumn("5cm"); // Right column for pageNumber

            var footRow = footTable.AddRow();


            var bibleVerse = footRow.Cells[0].AddParagraph();
            bibleVerse.AddText("“Salute one another with holy kiss, the Churches of Christ salute you” Romans 16:16");
            bibleVerse.Format.Font.Name = "Arial";
            bibleVerse.Format.Font.Size = 10;
            bibleVerse.Format.Font.Italic = true;
            bibleVerse.Format.Alignment = ParagraphAlignment.Left;
            
              // Text container
            var pageCell = footRow.Cells[1].AddParagraph();
            pageCell.AddText("Page ");
            pageCell.AddPageField();
            pageCell.AddText(" of ");
            pageCell.AddNumPagesField();
            pageCell.Format.Alignment = ParagraphAlignment.Right;

            
        }

        private void RenderAndSavePdf(string filePath)
        {
            var renderer = new PdfDocumentRenderer(true);
            renderer.Document = _document;
            renderer.RenderDocument();
            renderer.PdfDocument.Save(filePath);
        }

        private void AddIndividualAttendanceTable(object data)
        {
            //// Add redacted-style heading
            //var title = _section.AddParagraph("OPERATION: SANCTUARY ATTENDANCE");
            //title.Format.Font.Size = 14;
            //title.Format.Font.Bold = true;
            //title.Format.Font.Color = Colors.Black;
            //title.Format.Alignment = ParagraphAlignment.Center;
            //title.Format.SpaceAfter = "0.5cm";



            // Create dossier-style table
            var table = _section.AddTable();
            table.Style = "Table";
            //table.Borders.Color = Colors.Black;
            //table.Borders.Width = 0.75;

            // Columns: Date | Operation Codename | Status | Authentication
            table.AddColumn("3.5cm").Format.Alignment = ParagraphAlignment.Center;
            table.AddColumn("4.5cm").Format.Alignment = ParagraphAlignment.Left;
            table.AddColumn("3cm").Format.Alignment = ParagraphAlignment.Center;
            table.AddColumn("4cm").Format.Alignment = ParagraphAlignment.Left;

            // Create header row with black background
            var headerRow = table.AddRow();
            headerRow.HeadingFormat = true;
            headerRow.Format.Font.Bold = true;
            headerRow.Format.Font.Color = Colors.White;
            headerRow.Format.Shading.Color = Colors.Black;
            headerRow.VerticalAlignment = MigraDoc.DocumentObjectModel.Tables.VerticalAlignment.Center;
            headerRow.Format.Alignment = ParagraphAlignment.Center;
            headerRow.Height = "1cm";

            headerRow.Cells[0].AddParagraph("Service Date");
            headerRow.Cells[1].AddParagraph("Service Type");
            headerRow.Cells[2].AddParagraph("Status");
            headerRow.Cells[3].AddParagraph("Notes");

            foreach (var record in (IEnumerable<dynamic>)data)
            {
                var row = table.AddRow();
                row.VerticalAlignment = MigraDoc.DocumentObjectModel.Tables.VerticalAlignment.Center;
                row.Format.Font.Name = "Courier New";
                row.Format.Font.Size = 10;

                // Convert dates to classified format
                var dateCell = row.Cells[0];
                dateCell.AddParagraph(record.ServiceDate.ToString("dd-MMM-yyyy").ToUpper());
                
                row.Cells[1].AddParagraph(record.ServiceType.ToString());
                row.Cells[2].AddParagraph(record.Status.ToString());
                row.Cells[3].AddParagraph(record.Notes?.ToString() ?? "");

                // Key Change: Allow rows to expand vertically
                row.HeightRule = RowHeightRule.Auto;
            }
        }

        private void AddChurchAttendanceSummaryTable(object data)
        {
            var table = _section.AddTable();
            table.Style = "Table";

            //table.Borders.Width = 0.5;
            table.AddColumn("2cm"); // Summary Date
            table.AddColumn("4cm"); // Service Type
            table.AddColumn("2cm"); // Total Present
            table.AddColumn("2cm"); // Male Present
            table.AddColumn("2cm"); // Female Present
            table.AddColumn("2cm"); // Visitors
            table.AddColumn("2cm"); // Children
            table.AddColumn("2cm"); // Offering

            var headerRow = table.AddRow();
            headerRow.HeadingFormat = true;
            headerRow.Format.Font.Bold = true;
            headerRow.Format.Font.Color = Colors.White;
            headerRow.Format.Shading.Color = Colors.Black;
            headerRow.VerticalAlignment = MigraDoc.DocumentObjectModel.Tables.VerticalAlignment.Center;
            headerRow.Format.Alignment = ParagraphAlignment.Center;
            headerRow.Height = "1cm";

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
                row.VerticalAlignment = MigraDoc.DocumentObjectModel.Tables.VerticalAlignment.Center;
                row.Format.Font.Name = "Courier New";
                row.Format.Font.Size = 10;
                row.Height = "0.6cm";

                row.Cells[0].AddParagraph(record.SummaryDate.ToShortDateString());
                row.Cells[1].AddParagraph(record.ServiceType.ToString());
                row.Cells[2].AddParagraph(record.TotalPresent.ToString());
                row.Cells[3].AddParagraph(record.TotalMalePresent.ToString());
                row.Cells[4].AddParagraph(record.TotalFemalePresent.ToString());
                row.Cells[5].AddParagraph(record.Visitors.ToString());
                row.Cells[6].AddParagraph(record.Children.ToString());
                row.Cells[7].AddParagraph(record.OfferingAmount.ToString("C"));

                // Enable automatic row height for wrapped text
                //row.HeightRule = RowHeightRule.Auto;

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
