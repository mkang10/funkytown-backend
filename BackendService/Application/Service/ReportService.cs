using System;
using System.IO;
using System.Linq;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using Domain.Entities;
using Domain.Interfaces;  // Bao gồm Import, Dispatch, Transfer, các chi tiết, ProductVariant, Product, Color, Size, Warehouse,...

namespace Application.Services
{
    public class ReportService
    {
        private readonly IImportRepos _impRepos;

        public ReportService(IImportRepos impRepos)
        {
            _impRepos = impRepos;
        }
        /// <summary>
        /// Tạo biên bản nhập kho từ Import, ImportDetail và ImportStoreDetail.
        /// </summary>
        public byte[] GenerateImportSlip(Import import)
        {
            using (MemoryStream memStream = new MemoryStream())
            {
                using (var wordDoc = WordprocessingDocument.Create(memStream, WordprocessingDocumentType.Document, true))
                {
                    MainDocumentPart mainPart = wordDoc.AddMainDocumentPart();
                    mainPart.Document = new Document(new Body());
                    Body body = mainPart.Document.Body;

                    // Header
                    AddParagraph(body, "PHIẾU NHẬP KHO", true, "32", JustificationValues.Center);
                    AddParagraph(body, $"Reference Number: {import.ReferenceNumber}", false, "24", JustificationValues.Center);
                    AddParagraph(body, $"Ngày nhập: {(import.CreatedDate.HasValue ? import.CreatedDate.Value.ToString("dd/MM/yyyy HH:mm:ss") : "N/A")}", false, "24", JustificationValues.Center);
                    AddParagraph(body, $"Tổng tiền: {string.Format(new System.Globalization.CultureInfo("vi-VN"), "{0:c0}", import.TotalCost)}", false, "24", JustificationValues.Center);
                    AddParagraph(body, " ", false, "24");

                    // Bảng chi tiết nhập kho
                    Table importTable = CreateImportDetailTable(import);
                    body.Append(importTable);

                    // Ký tên
                    AddParagraph(body, " ", false, "24");
                    AddSignatureSection(body, "Người lập phiếu", "Người nhận hàng", "Thủ kho");

                    mainPart.Document.Save();
                }
                return memStream.ToArray();
            }
        }

        /// <summary>
        /// Tạo biên bản xuất kho từ Dispatch, DispatchDetail và StoreExportStoreDetail.
        /// </summary>
        public byte[] GenerateExportSlip(Dispatch dispatch)
        {
            using (MemoryStream memStream = new MemoryStream())
            {
                using (var wordDoc = WordprocessingDocument.Create(memStream, WordprocessingDocumentType.Document, true))
                {
                    MainDocumentPart mainPart = wordDoc.AddMainDocumentPart();
                    mainPart.Document = new Document(new Body());
                    Body body = mainPart.Document.Body;

                    // Header
                    AddParagraph(body, "PHIẾU XUẤT KHO", true, "32", JustificationValues.Center);
                    AddParagraph(body, $"Reference Number: {dispatch.ReferenceNumber}", false, "24", JustificationValues.Center);
                    AddParagraph(body, $"Ngày xuất: {dispatch.CreatedDate.ToString("dd/MM/yyyy HH:mm:ss")}", false, "24", JustificationValues.Center);
                    AddParagraph(body, " ", false, "24");

                    // Bảng chi tiết xuất kho
                    Table exportTable = CreateDispatchDetailTable(dispatch);
                    body.Append(exportTable);

                    // Ký tên
                    AddParagraph(body, " ", false, "24");
                    AddSignatureSection(body, "Người lập phiếu", "Người giao hàng", "Thủ kho");

                    mainPart.Document.Save();
                }
                return memStream.ToArray();
            }
        }

        public byte[] GenerateImportSupplementSlip(Import supplementImport, Import oldImport)
        {
            using (MemoryStream memStream = new MemoryStream())
            {
                using (WordprocessingDocument wordDoc =
                    WordprocessingDocument.Create(memStream, WordprocessingDocumentType.Document, true))
                {
                    MainDocumentPart mainPart = wordDoc.AddMainDocumentPart();
                    mainPart.Document = new Document(new Body());
                    Body body = mainPart.Document.Body;

                    // PHẦN TIÊU ĐỀ CHUNG
                    AddParagraph(body, "BIÊN BẢN NHẬP BỔ SUNG", true, "32", JustificationValues.Center);
                    AddParagraph(body, $"Reference Number: {supplementImport.ReferenceNumber}", false, "24", JustificationValues.Center);
                    AddParagraph(body, $"Ngày tạo báo cáo: {DateTime.Now:dd/MM/yyyy HH:mm:ss}", false, "24", JustificationValues.Center);
                    AddParagraph(body, " ", false, "24");

                    // PHẦN 1: THÔNG TIN ĐƠN NHẬP CŨ
                    AddParagraph(body, "ĐƠN NHẬP CŨ", true, "28", JustificationValues.Left);
                    Table oldImportTable = CreateImportDetailTable(oldImport);
                    body.Append(oldImportTable);
                    AddParagraph(body, " ", false, "24");

                    // PHẦN 2: THÔNG TIN ĐƠN NHẬP BỔ SUNG
                    AddParagraph(body, "ĐƠN NHẬP BỔ SUNG", true, "28", JustificationValues.Left);
                    Table supplementImportTable = CreateImportDetailTable(supplementImport);
                    body.Append(supplementImportTable);
                    AddParagraph(body, " ", false, "24");

                    // PHẦN 3: TỔNG KẾT (nếu cần – ví dụ tổng tiền đơn bổ sung)

                    AddParagraph(
                        body,
                        $"Tổng tiền đơn bổ sung: {string.Format(new System.Globalization.CultureInfo("vi-VN"), "{0:c0}", supplementImport.TotalCost)}",
                        true,
                        "24",
                        JustificationValues.Right
                    );
                    AddParagraph(body, " ", false, "24");

                    // PHẦN KÝ TÊN
                    AddSignatureSection(body, "Người lập phiếu", "Người nhận hàng", "Thủ kho");

                    mainPart.Document.Save();
                }
                return memStream.ToArray();
            }
        }




        /// <summary>
        /// Tạo biên bản chuyển hàng từ Transfer và TransferDetail, và kèm theo thông tin chi tiết của phiếu xuất (Dispatch) và phiếu nhập (Import).
        /// </summary>
        public byte[] GenerateTransferSlip(Transfer transfer)
        {
            using (MemoryStream memStream = new MemoryStream())
            {
                using (var wordDoc = WordprocessingDocument.Create(memStream, WordprocessingDocumentType.Document, true))
                {
                    MainDocumentPart mainPart = wordDoc.AddMainDocumentPart();
                    mainPart.Document = new Document(new Body());
                    Body body = mainPart.Document.Body;

                    // PHẦN 1: THÔNG TIN CHUYỂN HÀNG
                    AddParagraph(body, "PHIẾU CHUYỂN HÀNG", true, "32", JustificationValues.Center);
                    AddParagraph(body, $"Transfer Order ID: {transfer.TransferOrderId}", false, "24", JustificationValues.Center);
                    AddParagraph(body, $"Ngày chuyển: {(transfer.CreatedDate.HasValue ? transfer.CreatedDate.Value.ToString("dd/MM/yyyy HH:mm:ss") : "N/A")}", false, "24", JustificationValues.Center);
                    AddParagraph(body, $"Trạng thái: {transfer.Status}", false, "24", JustificationValues.Center);
                    AddParagraph(body, " ", false, "24");

                    // Bảng chi tiết chuyển hàng
                    Table transferTable = CreateTransferDetailTable(transfer);
                    body.Append(transferTable);

                    // PHẦN 2: THÔNG TIN PHIẾU XUẤT (Dispatch)
                    if (transfer.Dispatch != null)
                    {
                        AddParagraph(body, " ", false, "24");
                        AddParagraph(body, "THÔNG TIN PHIẾU XUẤT", true, "28", JustificationValues.Center);
                        // Bạn có thể chèn lại bảng chi tiết xuất kho vào đây, ví dụ:
                        Table exportTable = CreateDispatchDetailTable(transfer.Dispatch);
                        body.Append(exportTable);
                    }

                    // PHẦN 3: THÔNG TIN PHIẾU NHẬP (Import)
                    if (transfer.Import != null)
                    {
                        AddParagraph(body, " ", false, "24");
                        AddParagraph(body, "THÔNG TIN PHIẾU NHẬP", true, "28", JustificationValues.Center);
                        Table importTable = CreateImportDetailTable(transfer.Import);
                        body.Append(importTable);
                    }

                    // Ký tên tổng kết
                    AddParagraph(body, " ", false, "24");
                    AddSignatureSection(body, "Người lập phiếu", "Người nhận hàng", "Quản lý kho");

                    mainPart.Document.Save();
                }
                return memStream.ToArray();
            }
        }



private Table CreateImportDetailTable(Import import)
{
    // Khởi tạo Table với style và border
    Table table = new Table();
    TableProperties tblProps = new TableProperties(
        new TableStyle { Val = "TableGrid" },
        new TableWidth { Type = TableWidthUnitValues.Pct, Width = "5000" },
        new TableBorders(
            new TopBorder { Val = BorderValues.Single, Size = 4 },
            new BottomBorder { Val = BorderValues.Single, Size = 4 },
            new LeftBorder { Val = BorderValues.Single, Size = 4 },
            new RightBorder { Val = BorderValues.Single, Size = 4 },
            new InsideHorizontalBorder { Val = BorderValues.Single, Size = 4 },
            new InsideVerticalBorder { Val = BorderValues.Single, Size = 4 }
        )
    );
    table.Append(tblProps);

    // Header row
    TableRow headerRow = new TableRow();
    headerRow.Append(CreateTableCell("STT", true));
    headerRow.Append(CreateTableCell("Tên sản phẩm", true));
    headerRow.Append(CreateTableCell("SL thực tế", true));
    headerRow.Append(CreateTableCell("SL phân bổ", true));
    headerRow.Append(CreateTableCell("Đơn giá", true));
    headerRow.Append(CreateTableCell("Thành tiền", true));
    headerRow.Append(CreateTableCell("Kho", true));
    headerRow.Append(CreateTableCell("Nhân viên kiểm nhập", true));
    headerRow.Append(CreateTableCell("Trạng thái", true));
    headerRow.Append(CreateTableCell("Ghi chú", true));
    table.Append(headerRow);

    int stt = 1;
    // Duyệt qua từng ImportDetail và ImportStoreDetail
    foreach (var detail in import.ImportDetails)
    {
        string productName = detail.ProductVariant != null
            ? $"{detail.ProductVariant.Product.Name} - {detail.ProductVariant.Color.ColorName} - {detail.ProductVariant.Size.SizeName}"
            : detail.ProductVariantId.ToString();

        foreach (var sd in detail.ImportStoreDetails)
        {
            string warehouseName = sd.Warehouse?.WarehouseName ?? GetWarehouseName(sd.WarehouseId ?? 0);
            string allocatedQty = sd.AllocatedQuantity.ToString();
                    string actualQty = sd.ActualReceivedQuantity.HasValue
                        ? sd.ActualReceivedQuantity.Value.ToString()
                        : "..."; string staffName = GetStaffName(sd.StaffDetailId ?? 0);
             
              
        
            string status = sd.Status ?? string.Empty;
            string comment = sd.Comments ?? string.Empty;

            // Tạo row mới
            TableRow row = new TableRow();
            row.Append(CreateTableCell(stt.ToString()));
            row.Append(CreateTableCell(productName));
            row.Append(CreateTableCell(actualQty));
           row.Append(CreateTableCell(allocatedQty));

            row.Append(CreateTableCell(FormatCurrency(detail.CostPrice ?? 0)));
            row.Append(CreateTableCell(FormatCurrency(detail.Quantity * (detail.CostPrice ?? 0))));
            row.Append(CreateTableCell(warehouseName));
            row.Append(CreateTableCell(staffName));
            row.Append(CreateTableCell(status));
            row.Append(CreateTableCell(comment));
            table.Append(row);

            stt++;
        }
    }

    return table;
}

        private string GetStaffName(int staffDetailId)
        {
            var staff = _impRepos.GetStaffDetailByIdAsync(staffDetailId).GetAwaiter().GetResult();
            return staff != null
                ? $"{staff.Account.FullName}"
                : "Unknown Staff";
        }

        private string FormatCurrency(decimal amount)
        {
            return string.Format(new System.Globalization.CultureInfo("vi-VN"), "{0:c0}", amount);
        }


        private Table CreateDispatchDetailTable(Dispatch dispatch)
        {
            Table table = new Table();

            TableProperties tblProps = new TableProperties(
                new TableStyle { Val = "TableGrid" },
                new TableWidth { Type = TableWidthUnitValues.Pct, Width = "5000" },
                new TableBorders(
                    new TopBorder { Val = BorderValues.Single, Size = 4 },
                    new BottomBorder { Val = BorderValues.Single, Size = 4 },
                    new LeftBorder { Val = BorderValues.Single, Size = 4 },
                    new RightBorder { Val = BorderValues.Single, Size = 4 },
                    new InsideHorizontalBorder { Val = BorderValues.Single, Size = 4 },
                    new InsideVerticalBorder { Val = BorderValues.Single, Size = 4 }
                )
            );
            table.Append(tblProps);

            TableRow headerRow = new TableRow();
            headerRow.Append(CreateTableCell("STT", true));
            headerRow.Append(CreateTableCell("Tên sản phẩm", true));
            headerRow.Append(CreateTableCell("Số lượng", true));
            headerRow.Append(CreateTableCell("Kho", true));
            headerRow.Append(CreateTableCell("Ghi chú", true));
            table.Append(headerRow);

            int stt = 1;
            foreach (var detail in dispatch.DispatchDetails)
            {
                string productName = detail.Variant != null
                    ? $"{detail.Variant.Product.Name} - {detail.Variant.Color.ColorName} - {detail.Variant.Size.SizeName}"
                    : detail.VariantId.ToString();
                string warehouseNames = string.Join(", ",
                    detail.StoreExportStoreDetails
                          .Where(se => se.Warehouse != null)
                          .Select(se => se.Warehouse.WarehouseName));
                string notes = string.Join("; ",
                    detail.StoreExportStoreDetails.Select(se => se.Comments));

                TableRow row = new TableRow();
                row.Append(CreateTableCell(stt.ToString()));
                row.Append(CreateTableCell(productName));
                row.Append(CreateTableCell(detail.Quantity.ToString()));
                row.Append(CreateTableCell(warehouseNames));
                row.Append(CreateTableCell(notes));
                table.Append(row);
                stt++;
            }
            return table;
        }

        private Table CreateTransferDetailTable(Transfer transfer)
        {
            Table table = new Table();

            TableProperties tblProps = new TableProperties(
                new TableStyle { Val = "TableGrid" },
                new TableWidth { Type = TableWidthUnitValues.Pct, Width = "5000" },
                new TableBorders(
                    new TopBorder { Val = BorderValues.Single, Size = 4 },
                    new BottomBorder { Val = BorderValues.Single, Size = 4 },
                    new LeftBorder { Val = BorderValues.Single, Size = 4 },
                    new RightBorder { Val = BorderValues.Single, Size = 4 },
                    new InsideHorizontalBorder { Val = BorderValues.Single, Size = 4 },
                    new InsideVerticalBorder { Val = BorderValues.Single, Size = 4 }
                )
            );
            table.Append(tblProps);

            TableRow headerRow = new TableRow();
            headerRow.Append(CreateTableCell("STT", true));
            headerRow.Append(CreateTableCell("Tên sản phẩm", true));
            headerRow.Append(CreateTableCell("Số lượng", true));
            table.Append(headerRow);

            int stt = 1;
            foreach (var detail in transfer.TransferDetails)
            {
                string productName = detail.Variant != null
                    ? $"{detail.Variant.Product.Name} - {detail.Variant.Color.ColorName} - {detail.Variant.Size.SizeName}"
                    : detail.VariantId.ToString();

                TableRow row = new TableRow();
                row.Append(CreateTableCell(stt.ToString()));
                row.Append(CreateTableCell(productName));
                row.Append(CreateTableCell(detail.Quantity.ToString()));
                table.Append(row);
                stt++;
            }
            return table;
        }

        private void AddSignatureSection(Body body, string leftTitle, string centerTitle, string rightTitle)
        {
            Table table = new Table();

            TableProperties tblProps = new TableProperties(
                new TableBorders(
                    new TopBorder { Val = BorderValues.None },
                    new LeftBorder { Val = BorderValues.None },
                    new BottomBorder { Val = BorderValues.None },
                    new RightBorder { Val = BorderValues.None },
                    new InsideHorizontalBorder { Val = BorderValues.None },
                    new InsideVerticalBorder { Val = BorderValues.None }
                )
            );
            table.Append(tblProps);

            TableRow headerRow = new TableRow();
            headerRow.Append(CreateTableCell(leftTitle, true, border: false));
            headerRow.Append(CreateTableCell(centerTitle, true, border: false));
            headerRow.Append(CreateTableCell(rightTitle, true, border: false));
            table.Append(headerRow);

            TableRow signRow = new TableRow();
            signRow.Append(CreateTableCell("(Ký, ghi rõ họ tên)", false, border: false));
            signRow.Append(CreateTableCell("(Ký, ghi rõ họ tên)", false, border: false));
            signRow.Append(CreateTableCell("(Ký, ghi rõ họ tên)", false, border: false));
            table.Append(signRow);

            body.Append(table);
        }

        private TableCell CreateTableCell(string text, bool bold = false, bool border = true)
        {
            TableCell cell = new TableCell();
            Paragraph para = CreateParagraph(text, bold, "22", JustificationValues.Left);
            cell.Append(para);

            if (!border)
            {
                TableCellProperties props = new TableCellProperties(
                    new TableCellBorders(
                        new TopBorder { Val = BorderValues.None },
                        new LeftBorder { Val = BorderValues.None },
                        new BottomBorder { Val = BorderValues.None },
                        new RightBorder { Val = BorderValues.None }
                    )
                );
                cell.PrependChild(props);
            }
            return cell;
        }

        // Overloads cho CreateParagraph & AddParagraph để tránh sử dụng default parameter cho enum

        private Paragraph CreateParagraph(string text, bool bold, string fontSize)
        {
            return CreateParagraph(text, bold, fontSize, JustificationValues.Left);
        }

        private Paragraph CreateParagraph(string text, bool bold, string fontSize, JustificationValues justification)
        {
            Run run = new Run();
            RunProperties runProps = new RunProperties(new FontSize() { Val = fontSize });
            if (bold)
                runProps.Bold = new Bold();
            run.Append(runProps);
            run.Append(new Text(text) { Space = SpaceProcessingModeValues.Preserve });

            ParagraphProperties paraProps = new ParagraphProperties(new Justification() { Val = justification });
            Paragraph para = new Paragraph();
            para.Append(paraProps);
            para.Append(run);
            return para;
        }

        private void AddParagraph(Body body, string text, bool bold, string fontSize)
        {
            AddParagraph(body, text, bold, fontSize, JustificationValues.Left);
        }

        private void AddParagraph(Body body, string text, bool bold, string fontSize, JustificationValues justification)
        {
            Paragraph para = CreateParagraph(text, bold, fontSize, justification);
            body.Append(para);
        }

        private string GetWarehouseName(int warehouseId)
        {
            // Sử dụng _impRepos để truy vấn warehouse theo warehouseId.
            // Lưu ý: đây là gọi blocking với GetAwaiter().GetResult(), cần chú ý về deadlock nếu gọi trên UI thread.
            var warehouse = _impRepos.GetWareHouseByIdAsync(warehouseId).GetAwaiter().GetResult();
            return warehouse != null ? warehouse.WarehouseName : "Unknown Warehouse";
        }



    }
}
