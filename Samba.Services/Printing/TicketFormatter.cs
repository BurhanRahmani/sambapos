﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Samba.Domain.Models.Customers;
using Samba.Domain.Models.Settings;
using Samba.Domain.Models.Tickets;
using Samba.Localization.Properties;
using Samba.Persistance.Data;

namespace Samba.Services.Printing
{
    public class TagData
    {
        public TagData(string data, string tag)
        {
            data = ReplaceInBracketValues(data, "\r\n", "<newline>", '[', ']');

            data = data.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).Where(x => x.Contains(tag)).FirstOrDefault();

            Tag = tag;
            DataString = tag;
            if (string.IsNullOrEmpty(data)) return;

            StartPos = data.IndexOf(tag);
            EndPos = StartPos + 1;

            while (data[EndPos] != '}') { EndPos++; }
            EndPos++;
            Length = EndPos - StartPos;

            DataString = BracketContains(data, '[', ']', Tag) ? GetBracketValue(data, '[', ']') : data.Substring(StartPos, Length);
            DataString = DataString.Replace("<newline>", "\r\n");
            Title = DataString.Trim('[', ']').Replace(Tag, "<value>");
            Length = DataString.Length;
            StartPos = data.IndexOf(DataString);
            EndPos = StartPos + Length;
        }

        public string DataString { get; set; }
        public string Tag { get; set; }
        public string Title { get; set; }
        public int StartPos { get; set; }
        public int EndPos { get; set; }
        public int Length { get; set; }

        public static string ReplaceInBracketValues(string content, string find, string replace, char open, char close)
        {
            var result = content;
            var v1 = GetBracketValue(result, open, close);
            while (!string.IsNullOrEmpty(v1))
            {
                var value = v1.Replace(find, replace);
                value = value.Replace(open.ToString(), "<op>");
                value = value.Replace(close.ToString(), "<cl>");
                result = result.Replace(v1, value);
                v1 = GetBracketValue(result, open, close);
            }
            result = result.Replace("<op>", open.ToString());
            result = result.Replace("<cl>", close.ToString());
            return result;
        }

        public static bool BracketContains(string content, char open, char close, string testValue)
        {
            if (!content.Contains(open)) return false;
            var br = GetBracketValue(content, open, close);
            return (br.Contains(testValue));
        }

        public static string GetBracketValue(string content, char open, char close)
        {
            var closePass = 1;
            var start = content.IndexOf(open);
            var end = start;
            if (start > -1)
            {
                while (end < content.Length - 1 && closePass > 0)
                {
                    end++;
                    if (content[end] == open && close != open) closePass++;
                    if (content[end] == close) closePass--;
                }
                return content.Substring(start, (end - start) + 1);
            }
            return string.Empty;
        }
    }

    public static class TicketFormatter
    {
        public static string[] GetFormattedTicket(Ticket ticket, IEnumerable<TicketItem> lines, PrinterTemplate template)
        {
            if (template.MergeLines) lines = MergeLines(lines);
            var orderNo = lines.Count() > 0 ? lines.ElementAt(0).OrderNumber : 0;
            var userNo = lines.Count() > 0 ? lines.ElementAt(0).CreatingUserId : 0;
            var header = ReplaceDocumentVars(template.HeaderTemplate, ticket, orderNo, userNo);
            var footer = ReplaceDocumentVars(template.FooterTemplate, ticket, orderNo, userNo);
            var lns = lines.SelectMany(x => FormatLines(template, x).Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)).ToArray();

            var result = header.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            result.AddRange(lns);
            result.AddRange(footer.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries));

            return result.ToArray();
        }

        private static IEnumerable<TicketItem> MergeLines(IEnumerable<TicketItem> lines)
        {
            var group = lines.Where(x => x.Properties.Count == 0).GroupBy(x => new
                                                {
                                                    x.MenuItemId,
                                                    x.MenuItemName,
                                                    x.Voided,
                                                    x.Gifted,
                                                    x.Price,
                                                    x.VatAmount,
                                                    x.VatTemplateId,
                                                    x.PortionName,
                                                    x.PortionCount,
                                                    x.ReasonId,
                                                    x.CurrencyCode
                                                });

            var result = group.Select(x => new TicketItem
                                    {
                                        MenuItemId = x.Key.MenuItemId,
                                        MenuItemName = x.Key.MenuItemName,
                                        ReasonId = x.Key.ReasonId,
                                        Voided = x.Key.Voided,
                                        Gifted = x.Key.Gifted,
                                        Price = x.Key.Price,
                                        VatAmount = x.Key.VatAmount,
                                        VatTemplateId = x.Key.VatTemplateId,
                                        CreatedDateTime = x.Last().CreatedDateTime,
                                        OrderNumber = x.Last().OrderNumber,
                                        TicketId = x.Last().TicketId,
                                        PortionName = x.Key.PortionName,
                                        PortionCount = x.Key.PortionCount,
                                        CurrencyCode = x.Key.CurrencyCode,
                                        Quantity = x.Sum(y => y.Quantity)
                                    });

            result = result.Union(lines.Where(x => x.Properties.Count > 0)).OrderBy(x => x.CreatedDateTime);

            return result;
        }

        private static string ReplaceDocumentVars(string document, Ticket ticket, int orderNo, int userNo)
        {
            string result = document;
            if (string.IsNullOrEmpty(document)) return "";

            result = FormatData(result, Resources.TF_TicketDate, ticket.Date.ToShortDateString());
            result = FormatData(result, Resources.TF_TicketTime, ticket.Date.ToShortTimeString());
            result = FormatData(result, Resources.TF_DayDate, DateTime.Now.ToShortDateString());
            result = FormatData(result, Resources.TF_DayTime, DateTime.Now.ToShortTimeString());
            result = FormatData(result, Resources.TF_UniqueTicketId, ticket.Id.ToString());
            result = FormatData(result, Resources.TF_TicketNumber, ticket.TicketNumber);
            result = FormatData(result, Resources.TF_LineOrderNumber, orderNo.ToString());
            result = FormatData(result, Resources.TF_TicketTag, ticket.GetTagData());
            if (result.Contains(Resources.TF_OptionalTicketTag))
            {
                var start = result.IndexOf(Resources.TF_OptionalTicketTag);
                var end = result.IndexOf("}", start) + 1;
                var value = result.Substring(start, end - start);
                var tags = "";
                try
                {
                    var tag = value.Trim('{', '}').Split(':')[1];
                    tags = tag.Split(',').Aggregate(tags, (current, t) => current +
                        (!string.IsNullOrEmpty(ticket.GetTagValue(t.Trim()))
                        ? (t + ": " + ticket.GetTagValue(t.Trim()) + "\r")
                        : ""));
                    result = FormatData(result.Trim('\r'), value, tags);
                }
                catch (Exception)
                {
                    result = FormatData(result, value, "");
                }
            }

            var userName = AppServices.MainDataContext.GetUserName(userNo);

            var title = ticket.LocationName;
            if (string.IsNullOrEmpty(ticket.LocationName))
                title = userName;

            result = FormatData(result, Resources.TF_TableOrUserName, title);
            result = FormatData(result, Resources.TF_UserName, userName);
            result = FormatData(result, Resources.TF_TableName, ticket.LocationName);
            result = FormatData(result, Resources.TF_TicketNote, ticket.Note);
            result = FormatData(result, Resources.TF_AccountName, ticket.CustomerName);

            if (ticket.CustomerId > 0 && (result.Contains(Resources.TF_AccountAddress) || result.Contains(Resources.TF_AccountPhone)))
            {
                var customer = Dao.SingleWithCache<Customer>(x => x.Id == ticket.CustomerId);
                result = FormatData(result, Resources.TF_AccountAddress, customer.Address);
                result = FormatData(result, Resources.TF_AccountPhone, customer.PhoneNumber);
            }

            result = RemoveTag(result, Resources.TF_AccountAddress);
            result = RemoveTag(result, Resources.TF_AccountPhone);

            var payment = ticket.GetPaymentAmount();
            var remaining = ticket.GetRemainingAmount();
            var discount = ticket.GetDiscountAndRoundingTotal();
            var plainTotal = ticket.GetPlainSum();
            var giftAmount = ticket.GetTotalGiftAmount();
            var vatAmount = ticket.CalculateVat();
            var taxServicesTotal = ticket.GetTaxServicesTotal();

            result = FormatDataIf(vatAmount > 0 || discount > 0 || taxServicesTotal > 0, result, "{PLAIN TOTAL}", plainTotal.ToString("#,#0.00"));
            result = FormatDataIf(discount > 0, result, "{DISCOUNT TOTAL}", discount.ToString("#,#0.00"));
            result = FormatDataIf(vatAmount > 0, result, "{VAT TOTAL}", vatAmount.ToString("#,#0.00"));
            result = FormatDataIf(vatAmount > 0, result, "{TAX TOTAL}", taxServicesTotal.ToString("#,#0.00"));

            if (result.Contains("{VAT DETAILS}"))
                result = FormatDataIf(vatAmount > 0, result, "{VAT DETAILS}", GetVatDetails(ticket.TicketItems, plainTotal, discount));

            if (result.Contains("{TAX DETAILS}"))
                result = FormatDataIf(taxServicesTotal > 0, result, "{TAX DETAILS}", GetTaxDetails(ticket));

            result = FormatDataIf(payment > 0, result, Resources.TF_RemainingAmountIfPaid,
                string.Format(Resources.RemainingAmountIfPaidValue_f, payment.ToString("#,#0.00"), remaining.ToString("#,#0.00")));

            result = FormatDataIf(discount > 0, result, Resources.TF_DiscountTotalAndTicketTotal,
                string.Format(Resources.DiscountTotalAndTicketTotalValue_f, (plainTotal).ToString("#,#0.00"), discount.ToString("#,#0.00")));

            result = FormatDataIf(giftAmount > 0, result, Resources.TF_GiftTotal, giftAmount.ToString("#,#0.00"));
            result = FormatDataIf(discount < 0, result, Resources.TF_IfFlatten, string.Format(Resources.IfNegativeDiscountValue_f, discount.ToString("#,#0.00")));

            result = FormatData(result, Resources.TF_TicketTotal, ticket.GetSum().ToString("#,#0.00"));
            result = FormatData(result, Resources.TF_TicketPaidTotal, ticket.GetPaymentAmount().ToString("#,#0.00"));
            result = FormatData(result, Resources.TF_TicketRemainingAmount, ticket.GetRemainingAmount().ToString("#,#0.00"));

            if (result.Contains("{TOTAL TEXT}"))
                result = FormatData(result, "{TOTAL TEXT}", HumanFriendlyInteger.CurrencyToWritten(ticket.GetSum()));
            if (result.Contains("{TOTALTEXT}"))
                result = FormatData(result, "{TOTALTEXT}", HumanFriendlyInteger.CurrencyToWritten(ticket.GetSum(), true));

            return result;
        }

        private static string GetTaxDetails(Ticket ticket)
        {
            var sb = new StringBuilder();
            foreach (var taxService in ticket.TaxServices)
            {
                var service = taxService;
                var ts = AppServices.MainDataContext.TaxServiceTemplates.FirstOrDefault(x => x.Id == service.TaxServiceId);
                var tsTitle = ts != null ? ts.Name : Resources.UndefinedWithBrackets;
                sb.AppendLine("<J>" + tsTitle + ":|" + service.CalculationAmount.ToString("#,#0.00"));
            }
            return string.Join("\r", sb);
        }

        private static string GetVatDetails(IEnumerable<TicketItem> ticketItems, decimal plainSum, decimal discount)
        {
            var sb = new StringBuilder();
            var groups = ticketItems.Where(x => x.VatTemplateId > 0).GroupBy(x => x.VatTemplateId);
            foreach (var @group in groups)
            {
                var iGroup = @group;
                var tb = AppServices.MainDataContext.VatTemplates.FirstOrDefault(x => x.Id == iGroup.Key);
                var tbTitle = tb != null ? tb.Name : Resources.UndefinedWithBrackets;
                var total = @group.Sum(x => x.VatAmount * x.Quantity);
                if (discount > 0)
                {
                    total -= (total * discount) / plainSum;
                }
                if (total > 0) sb.AppendLine("<J>" + tbTitle + ":|" + total.ToString("#,#0.00"));
            }
            return string.Join("\r", sb);
        }

        private static string FormatData(string data, string tag, string value)
        {
            var tagData = new TagData(data, tag);
            if (!string.IsNullOrEmpty(value)) value =
                !string.IsNullOrEmpty(tagData.Title) && tagData.Title.Contains("<value>")
                ? tagData.Title.Replace("<value>", value)
                : tagData.Title + value;
            return data.Replace(tagData.DataString, value);
        }

        private static string FormatDataIf(bool condition, string data, string tag, string value)
        {
            if (condition) return FormatData(data, tag, value);
            return RemoveTag(data, tag);
        }

        private static string RemoveTag(string data, string tag)
        {
            var tagData = new TagData(data, tag);
            return data.Replace(tagData.DataString, "");
        }

        private static string FormatLines(PrinterTemplate template, TicketItem ticketItem)
        {
            if (ticketItem.Gifted)
            {
                if (!string.IsNullOrEmpty(template.GiftLineTemplate))
                {
                    return template.GiftLineTemplate.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                        .Aggregate("", (current, s) => current + ReplaceLineVars(s, ticketItem));
                }
                return "";
            }

            if (ticketItem.Voided)
            {
                if (!string.IsNullOrEmpty(template.VoidedLineTemplate))
                {
                    return template.VoidedLineTemplate.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                        .Aggregate("", (current, s) => current + ReplaceLineVars(s, ticketItem));
                }
                return "";
            }

            if (!string.IsNullOrEmpty(template.LineTemplate))
                return template.LineTemplate.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                    .Aggregate("", (current, s) => current + ReplaceLineVars(s, ticketItem));
            return "";
        }

        private static string ReplaceLineVars(string line, TicketItem ticketItem)
        {
            string result = line;

            if (ticketItem != null)
            {
                result = FormatData(result, Resources.TF_LineItemQuantity, ticketItem.Quantity.ToString("#,#0.##"));
                result = FormatData(result, Resources.TF_LineItemName, ticketItem.MenuItemName + ticketItem.GetPortionDesc());
                result = FormatData(result, Resources.TF_LineItemPrice, ticketItem.Price.ToString("#,#0.00"));
                result = FormatData(result, Resources.TF_LineItemTotal, ticketItem.GetItemPrice().ToString("#,#0.00"));
                result = FormatData(result, Resources.TF_LineItemTotalAndQuantity, ticketItem.GetItemValue().ToString("#,#0.00"));
                result = FormatData(result, Resources.TF_LineItemPriceCents, (ticketItem.Price * 100).ToString("#,##"));
                result = FormatData(result, Resources.TF_LineItemTotalWithoutGifts, ticketItem.GetTotal().ToString("#,#0.00"));
                result = FormatData(result, Resources.TF_LineOrderNumber, ticketItem.OrderNumber.ToString());
                result = FormatData(result, Resources.TF_LineGiftOrVoidReason, AppServices.MainDataContext.GetReason(ticketItem.ReasonId));
                result = FormatData(result, "{PRICE TAG}", ticketItem.PriceTag);
                if (result.Contains(Resources.TF_LineItemDetails.Substring(0, Resources.TF_LineItemDetails.Length - 1)))
                {
                    string lineFormat = result;
                    if (ticketItem.Properties.Count > 0)
                    {
                        string label = "";
                        foreach (var property in ticketItem.Properties)
                        {
                            var lineValue = FormatData(lineFormat, Resources.TF_LineItemDetails, property.Name);
                            lineValue = FormatData(lineValue, Resources.TF_LineItemDetailQuantity, property.Quantity.ToString("#.##"));
                            lineValue = FormatData(lineValue, Resources.TF_LineItemDetailPrice, property.CalculateWithParentPrice ? "" : property.PropertyPrice.Amount.ToString("#,#0.00"));
                            label += lineValue + "\r\n";
                        }
                        result = "\r\n" + label;
                    }
                    else result = "";
                }
            }
            return result;
        }
    }

    public static class HumanFriendlyInteger
    {
        static readonly string[] Ones = new[] { "", Resources.One, Resources.Two, Resources.Three, Resources.Four, Resources.Five, Resources.Six, Resources.Seven, Resources.Eight, Resources.Nine };
        static readonly string[] Teens = new[] { Resources.Ten, Resources.Eleven, Resources.Twelve, Resources.Thirteen, Resources.Fourteen, Resources.Fifteen, Resources.Sixteen, Resources.Seventeen, Resources.Eighteen, Resources.Nineteen };
        static readonly string[] Tens = new[] { Resources.Twenty, Resources.Thirty, Resources.Forty, Resources.Fifty, Resources.Sixty, Resources.Seventy, Resources.Eighty, Resources.Ninety };
        static readonly string[] ThousandsGroups = { "", " " + Resources.Thousand, " " + Resources.Million, " " + Resources.Billion };

        private static string FriendlyInteger(int n, string leftDigits, int thousands)
        {
            if (n == 0)
            {
                return leftDigits;
            }
            string friendlyInt = leftDigits;
            if (friendlyInt.Length > 0)
            {
                friendlyInt += " ";
            }

            if (n < 10)
            {
                friendlyInt += Ones[n];
            }
            else if (n < 20)
            {
                friendlyInt += Teens[n - 10];
            }
            else if (n < 100)
            {
                friendlyInt += FriendlyInteger(n % 10, Tens[n / 10 - 2], 0);
            }
            else if (n < 1000)
            {
                var t = Ones[n / 100] + " " + Resources.Hundred;
                if (n / 100 == 1) t = Resources.OneHundred;
                friendlyInt += FriendlyInteger(n % 100, t, 0);
            }
            else if (n < 10000 && thousands == 0)
            {
                var t = Ones[n / 1000] + " " + Resources.Thousand;
                if (n / 1000 == 1) t = Resources.OneThousand;
                friendlyInt += FriendlyInteger(n % 1000, t, 0);
            }
            else
            {
                friendlyInt += FriendlyInteger(n % 1000, FriendlyInteger(n / 1000, "", thousands + 1), 0);
            }

            return friendlyInt + ThousandsGroups[thousands];
        }

        public static string CurrencyToWritten(decimal d, bool upper = false)
        {
            var result = "";
            var fraction = d - Math.Floor(d);
            var value = d - fraction;
            if (value > 0)
            {
                var start = IntegerToWritten(Convert.ToInt32(value));
                if (upper) start = start.Replace(" ", "").ToUpper();
                result += string.Format("{0} {1} ", start, Resources.Dollar + GetPlural(value));
            }

            if (fraction > 0)
            {
                var end = IntegerToWritten(Convert.ToInt32(fraction * 100));
                if (upper) end = end.Replace(" ", "").ToUpper();
                result += string.Format("{0} {1} ", end, Resources.Cent + GetPlural(fraction));
            }
            return result.Replace("  ", " ").Trim();
        }

        private static string GetPlural(decimal number)
        {
            return number == 1 ? "" : Resources.PluralCurrencySuffix;
        }

        public static string IntegerToWritten(int n)
        {
            if (n == 0)
            {
                return Resources.Zero;
            }
            if (n < 0)
            {
                return Resources.Negative + " " + IntegerToWritten(-n);
            }

            return FriendlyInteger(n, "", 0);
        }

    }
}
