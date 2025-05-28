using POS.Models;
using System;
using System.Drawing;
using System.Drawing.Printing;
using System.Text;
using System.Linq; // Added for List.IndexOf to work
using POS.Enums;

namespace POS.Services
{
    public class PrintService
    {
        private Order _orderToPrint;
        private Font _printFont;
        private string _receiptContent;

        // CRITICAL: If PrintService needs DbContext (e.g., to fetch full User/Customer details
        // if they weren't fully loaded on the Order object), it should also use DI.
        // For now, it seems to rely on the Order object already having necessary data.
        // public PrintService(POSDbContext context) { _context = context; } // Example if needed

        public void PrintReceipt(Order order)
        {
            _orderToPrint = order;
            GenerateReceiptContent();

            PrintDocument printDocument = new PrintDocument();
            printDocument.PrintPage += PrintDocument_PrintPage;

            try
            {
                printDocument.Print();
            }
            catch (Exception ex)
            {
                throw new Exception($"Printing failed: {ex.Message}"); // Re-throw with more context
            }
        }

        private void GenerateReceiptContent()
        {
            var sb = new StringBuilder();

            // Header - Consider making these configurable (e.g., in app settings)
            sb.AppendLine("================================");
            sb.AppendLine("         RESTAURANT NAME");
            sb.AppendLine("       123 Main Street");
            sb.AppendLine("      City, State 12345");
            sb.AppendLine("      Tel: (555) 123-4567");
            sb.AppendLine("================================");
            sb.AppendLine();

            // Order info
            sb.AppendLine($"Order #: {_orderToPrint.OrderNumber}");
            sb.AppendLine($"Date: {_orderToPrint.OrderDate:MM/dd/yyyy HH:mm}");
            sb.AppendLine($"Type: {_orderToPrint.OrderType}"); // This will print enum name (e.g., "Takeaway")
            sb.AppendLine($"Cashier: {_orderToPrint.User?.FullName ?? "Unknown"}"); // Use null conditional for safety

            // If order.OrderType is an enum, compare with enum value, not string "Delivery"
            if (_orderToPrint.OrderType == OrderType.Delivery) // Correct comparison for enum
            {
                // Access customer details via navigation property
                sb.AppendLine($"Customer: {_orderToPrint.Customer?.Name ?? "N/A"}");
                sb.AppendLine($"Phone: {_orderToPrint.Customer?.Phone ?? "N/A"}"); // Use Phonetic property
                sb.AppendLine($"Address: {_orderToPrint.Customer?.Address ?? "N/A"}");
            }

            sb.AppendLine("--------------------------------");

            // Items
            foreach (var item in _orderToPrint.OrderItems)
            {
                sb.AppendLine($"{item.ItemName}");
                sb.AppendLine($"  {item.Quantity} x ${item.Price:F2} = ${item.LineTotal:F2}");
            }

            sb.AppendLine("--------------------------------");

            // Totals (Assuming SubTotal and Tax are calculated properties in Order model)
            sb.AppendLine($"TOTAL:           ${_orderToPrint.Total:F2}");
            sb.AppendLine($"Amount Paid:     ${_orderToPrint.AmountPaid:F2}");
            sb.AppendLine($"Change:          ${_orderToPrint.Change:F2}");

            sb.AppendLine("================================");
            sb.AppendLine("    Thank you for your visit!");
            sb.AppendLine("================================");

            _receiptContent = sb.ToString();
        }

        private void PrintDocument_PrintPage(object sender, PrintPageEventArgs e)
        {
            _printFont = new Font("Courier New", 8);

            float yPos = 0;
            float leftMargin = e.MarginBounds.Left;
            float topMargin = e.MarginBounds.Top;

            string[] lines = _receiptContent.Split('\n');

            foreach (string line in lines)
            {
                // This line calculation can be simplified using TextRenderer.MeasureText or a simple line height calculation
                // The way you have it `lines.ToList().IndexOf(line)` can be inefficient for large receipts
                // and has issues if multiple lines have the same content.
                // A better approach:
                // yPos += _printFont.GetHeight(e.Graphics); // Increment yPos after each line
                // e.Graphics.DrawString(line, _printFont, Brushes.Black, leftMargin, yPos, new StringFormat());

                yPos = topMargin + (Array.IndexOf(lines, line) * _printFont.GetHeight(e.Graphics)); // Using Array.IndexOf for string array
                e.Graphics.DrawString(line, _printFont, Brushes.Black, leftMargin, yPos, new StringFormat());
            }
        }
        // REMOVE THIS Dispose() method. Services typically don't manage DbContext directly.
        public void Dispose()
        {
            // _context?.Dispose(); // This line would be here if you had a _context field
        }
    }
}