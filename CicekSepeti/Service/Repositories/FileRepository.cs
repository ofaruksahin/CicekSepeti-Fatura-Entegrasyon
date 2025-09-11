using ClosedXML.Excel;
using Core.Entities;
using Core.Enums;
using Core.Extensions;
using Core.Interfaces;
using Core.Utilities;
using Core.Utilities.Result;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using Stream = System.IO.Stream;

namespace Service.Repositories
{
    public class FileRepository : BaseRepository, IFileRepository
    {
        public FileRepository(DataContext context) : base(context)
        {

        }

        public IResult Delete(File entity)
        {
            entity.StatusId = (int)EnumStatus.Passive;
            Context.File.Add(entity);
            Context.Entry(entity).State = EntityState.Modified;
            return new SuccessResult();
        }

        public IDataResult<File> Find(int id)
        {
            var entity = Context.File.Where(f => f.Id == id && f.StatusId == (int)EnumStatus.Active).FirstOrDefault();
            return entity != null ? new SuccessDataResult<File>(entity) : new ErrorDataResult<File>(null, "");
        }

        public IResult Insert(File entity)
        {
            Context.File.Add(entity);
            Context.Entry(entity).State = EntityState.Added;
            return new SuccessResult();
        }

        public IDataResult<List<File>> List()
        {
            var files = Context.File.Where(f => f.StatusId == (int)EnumStatus.Active).ToList();
            return new SuccessDataResult<List<File>>(files);
        }

        public IResult Update(File entity)
        {
            entity.ModifiedDate = DateTime.Now;
            //Context.File.Add(entity);
            Context.Entry(entity).State = EntityState.Modified;
            return new SuccessResult();
        }

        public IDataResult<File> ReadData(Stream stream, File file, Core.Entities.Parameter taxParameter)
        {
            try
            {

                decimal taxRate = 0;

                if (!decimal.TryParse(taxParameter.Value, out taxRate))
                {
                    return new ErrorDataResult<File>(null, Messages.Error);
                }

                using (var workbook = new XLWorkbook(stream))
                {
                    var sheet = workbook.Worksheets.FirstOrDefault();

                    var rowCount = sheet.RowsUsed().Count();

                    bool firstRow = true;
                    int rowIndex = 1;
                    for (int r = 0; r < rowCount; r++)
                    {
                        if (firstRow)
                        {
                            firstRow = false;
                            continue;
                        }
                        rowIndex++;

                        Invoice invoice = new Invoice();
                        invoice.OrderId = sheet.Cell(rowIndex, 1).Value.ToString();
                        invoice.SubOrderId = sheet.Cell(rowIndex, 2).Value.ToString();
                        invoice.InvoiceUniqueKey = Guid.NewGuid().ToString();
                        invoice.ProductName = sheet.Cell(rowIndex, 3).Value.ToString();
                        invoice.ProductSecondName = sheet.Cell(rowIndex, 4).Value.ToString();
                        invoice.Piece = int.Parse(sheet.Cell(rowIndex, 5).Value.ToString().Split(' ')[0]);
                        invoice.TaxRate = taxRate;
                        invoice.SubTotal = decimal.Parse(sheet.Cell(rowIndex, 37).Value.ToString());
                        invoice.Tax = invoice.SubTotal - ((invoice.SubTotal * 100) / 120);
                        invoice.Price = invoice.SubTotal - invoice.Tax;
                        invoice.CustomerName = sheet.Cell(rowIndex, 22).Value.ToString();
                        invoice.TaxOffice = sheet.Cell(rowIndex, 25).Value.ToString();
                        invoice.Address = sheet.Cell(rowIndex, 24).Value.ToString();
                        var customerCompany = sheet.Cell(rowIndex, 23).Value.ToString();
                        if (!string.IsNullOrEmpty(customerCompany))
                        {
                            invoice.CustomerName = customerCompany;
                        }
                        invoice.CustomerVKN = sheet.Cell(rowIndex, 26).Value.ToString();
                        if (string.IsNullOrEmpty(invoice.CustomerVKN))
                        {
                            invoice.CustomerVKN = "11111111111";
                        }
                        invoice.CustomerName = invoice.CustomerName.ToLowerInvariant();
                        invoice.InvoiceDate = file.TermEndDate;
                        invoice.InvoiceStatusId = !invoice.CustomerName.HasUnicodeCharacter() ? (int)EnumInvoiceStatus.WaitingDraft : (int)EnumInvoiceStatus.IncorrectInvoiceName;
                        invoice.CreatorId = file.CreatorId;
                        file.Invoices.Add(invoice);
                    }
                }
                file.InvoiceCount = file.Invoices.Count;
                Context.File.Add(file);
                Context.Entry(file).State = EntityState.Added;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return new ErrorDataResult<File>(null, Messages.Error);
            }

            return file.InvoiceCount > 0 ? new SuccessDataResult<File>(file) : new ErrorDataResult<File>(file, Messages.Error);
        }

    }
}
