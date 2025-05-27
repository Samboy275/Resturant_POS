using POS.Models;
using System;
using System.Drawing;
using System.Drawing.Printing;
using System.Text;

namespace POS.Services
{
    public class PrintService
    {
        private Order _orderToPrint;
        private Font _printFont;
        private string _receiptContent;

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
                throw new Exception($"Printing failed: {ex.Message}");
            }
        }

        private void GenerateReceiptContent()
        {
            var sb = new StringBuilder();

            // Header
            sb.AppendLine("================================");
            sb.AppendLine("       RESTAURANT NAME");
            sb.AppendLine("     123 Main Street");
            sb.AppendLine("   City, State 12345");
            sb.AppendLine("     Tel: (555) 123-4567");
            sb.AppendLine("================================");
            sb.AppendLine();

            // Order info
            sb.AppendLine($"Order #: {_orderToPrint.OrderNumber}");
            sb.AppendLine($"Date: {_orderToPrint.OrderDate:MM/dd/yyyy HH:mm}");
            sb.AppendLine($"Type: {_orderToPrint.OrderType}");
            sb.AppendLine($"Cashier: {_orderToPrint.User?.FullName ?? "Unknown"}");

            if (_orderToPrint.OrderType == "Delivery")
            {
                sb.AppendLine($"Customer: {_orderToPrint.CustomerName}");
                sb.AppendLine($"Phone: {_orderToPrint.CustomerPhone}");
                sb.AppendLine($"Address: {_orderToPrint.DeliveryAddress}");
            }

            sb.AppendLine("--------------------------------");

            // Items
            foreach (var item in _orderToPrint.OrderItems)
            {
                sb.AppendLine($"{item.ItemName}");
                sb.AppendLine($"  {item.Quantity} x ${item.Price:F2} = ${item.LineTotal:F2}");
            }

            sb.AppendLine("--------------------------------");

            // Totals
            sb.AppendLine($"Subtotal:        ${_orderToPrint.SubTotal:F2}");
            sb.AppendLine($"Tax (10%):       ${_orderToPrint.Tax:F2}");
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
                yPos = topMargin + (lines.ToList().IndexOf(line) * _printFont.GetHeight(e.Graphics));
                e.Graphics.DrawString(line, _printFont, Brushes.Black, leftMargin, yPos, new StringFormat());
            }
        }
    }
}