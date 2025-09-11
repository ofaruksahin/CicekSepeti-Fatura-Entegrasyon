using Core.Entities;
using Core.Enums;
using Core.Interfaces;
using Core.Utilities;
using EArchiveClient.Commands;
using EArchiveClient.DTO.Request;
using MailKit.Net.Smtp;
using MimeKit;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace EArchive.Consumer
{
    public class DraftInvoiceConsumer : BaseConsumer
    {
        CreateDraftInvoiceCommand draftInvoiceCommand;

        string companyName = string.Empty;

        public DraftInvoiceConsumer()
        {
            ConnectionInfo connectionInfo = ConnectionInfo.Instance;
            companyName = connectionInfo.CompanyName;

            draftInvoiceCommand = new CreateDraftInvoiceCommand();

            try
            {
                IUnitOfWork uow = new UnitOfWork();
                var connection = uow.RabbitMQ.GetRabbitMQConnection();
                var channel = connection.CreateModel();
                var queue = $"{companyName}_{EnumQueue.Invoice.ToString()}";
                channel.QueueDeclare(queue: queue,
                                  durable: false,
                                  exclusive: false,
                                  autoDelete: false,
                                  arguments: null);
                var consumer = new EventingBasicConsumer(channel);

                consumer.Received += (ch, ea) =>
                {
                    Console.WriteLine("Fatura oluşturma isteği geldi");
                    var body = ea.Body.ToArray();
                    DraftInvoice(body);
                };
                channel.BasicConsume(queue, true, consumer);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void DraftInvoice(byte[] body)
        {
            try
            {
                var data = JsonConvert.DeserializeObject<List<int>>(Encoding.UTF8.GetString(body));
                var fileId = 0;
                if (data.Any())
                {
                    using (IUnitOfWork uow = new UnitOfWork())
                    {
                        var invoicesResult = uow.Invoice.List(data);
                        if (invoicesResult.Success)
                        {
                            var invoices = invoicesResult.Data;
                            invoices.RemoveAll(f => f.InvoiceStatusId != (int)EnumInvoiceStatus.WaitingDraft && f.InvoiceStatusId != (int)EnumInvoiceStatus.NotCreatedDraft);
                            if (invoices.Any())
                            {
                                foreach (var invoice in invoices)
                                {
                                    invoice.InvoiceStatusId = (int)EnumInvoiceStatus.CreatingDraft;
                                    uow.Invoice.Update(invoice);
                                }

                                List<Invoice> createdInvoices = new List<Invoice>();
                                List<Invoice> notCreatedInvoice = new List<Invoice>();
                                if (uow.SaveChanges() && uow.Commit())
                                {
                                    foreach (var invoice in invoices)
                                    {
                                        if (fileId == 0)
                                        {
                                            fileId = invoice.FileId;
                                        }
                                        var draftInvoiceToken = tokenCommand.Execute(tokenRequest);
                                        invoice.Message = draftInvoiceToken.Message;
                                        if (draftInvoiceToken.StatusCode != HttpStatusCode.OK || String.IsNullOrEmpty(draftInvoiceToken.Data.Token))
                                        {
                                            Console.WriteLine("Token Alınamadı {0} {1}", draftInvoiceToken.StatusCode, JsonConvert.SerializeObject(draftInvoiceToken));
                                            invoice.InvoiceStatusId = (int)EnumInvoiceStatus.NotCreatedDraft;
                                            invoice.InvoiceUniqueKey = Guid.NewGuid().ToString();
                                            notCreatedInvoice.Add(invoice);
                                            uow.Invoice.Update(invoice);
                                            uow.SaveChanges();
                                            uow.Commit();
                                            continue;
                                        }

                                        var customerName = invoice.CustomerName.Split(' ').ToList();
                                        var lastName = customerName.LastOrDefault();
                                        customerName.Remove(lastName);
                                        
                                        Fatura draftInvoice = new Fatura();
                                        draftInvoice.faturaUuid = invoice.InvoiceUniqueKey;
                                        draftInvoice.belgeNumarasi = "";
                                        draftInvoice.faturaTarihi = invoice.InvoiceDate.ToString("dd/MM/yyyy").Replace(".", "/");
                                        draftInvoice.saat = "00:00:00";
                                        draftInvoice.paraBirimi = "TRY";
                                        draftInvoice.dovzTLkur = "0";
                                        draftInvoice.faturaTipi = "SATIS";
                                        draftInvoice.hangiTip = "5000/30000";
                                        draftInvoice.vknTckn = invoice.CustomerVKN;
                                        draftInvoice.aliciUnvan = invoice.CustomerName;
                                        draftInvoice.aliciAdi = string.Join(' ', customerName);
                                        draftInvoice.aliciSoyadi = lastName;
                                        draftInvoice.binaAdi = "";
                                        draftInvoice.binaNo = "";
                                        draftInvoice.kapiNo = "";
                                        draftInvoice.kasabaKoy = "";
                                        draftInvoice.vergiDairesi = invoice.TaxOffice;
                                        draftInvoice.ulke = "Türkiye";
                                        draftInvoice.bulvarcaddesokak = invoice.Address;
                                        draftInvoice.irsaliyeNumarasi = "";
                                        draftInvoice.irsaliyeTarihi = "";
                                        draftInvoice.mahalleSemtIlce = invoice.Address;
                                        draftInvoice.sehir = "+";
                                        draftInvoice.postaKodu = "";
                                        draftInvoice.tel = "";
                                        draftInvoice.fax = "";
                                        draftInvoice.eposta = "";
                                        draftInvoice.websitesi = "";
                                        draftInvoice.vergiCesidi = "+";
                                        draftInvoice.tip = "İskonto";
                                        draftInvoice.not = "";
                                        draftInvoice.siparisNumarasi = "";
                                        draftInvoice.siparisTarihi = "";
                                        draftInvoice.fisNo = "";
                                        draftInvoice.fisTarihi = "";
                                        draftInvoice.fisSaati = " ";
                                        draftInvoice.fisTipi = " ";
                                        draftInvoice.zRaporNo = "";
                                        draftInvoice.okcSeriNo = "";

                                        draftInvoice.matrah = format(invoice.Price);
                                        draftInvoice.malhizmetToplamTutari = format(invoice.Price);
                                        draftInvoice.hesaplanankdv = format(invoice.Tax);
                                        draftInvoice.vergilerToplami = format(invoice.Tax);
                                        draftInvoice.vergilerDahilToplamTutar = format(invoice.SubTotal);
                                        draftInvoice.odenecekTutar = format(invoice.SubTotal);

                                        MalHizmetTable malHizmetTable = new MalHizmetTable();
                                        malHizmetTable.malHizmet = "Dekoratif Hediye";
                                        malHizmetTable.miktar = 1;
                                        malHizmetTable.birim = "C62";
                                        malHizmetTable.birimFiyat = format(invoice.Price);
                                        malHizmetTable.fiyat = format(invoice.SubTotal);
                                        malHizmetTable.malHizmetTutari = format(invoice.Price);
                                        malHizmetTable.kdvOrani = ((int)invoice.TaxRate).ToString(); //DEĞİŞKEN
                                        malHizmetTable.kdvTutari = format(invoice.Tax);
                                        malHizmetTable.iskontoOrani = 0;
                                        malHizmetTable.iskontoTutari = "0";
                                        malHizmetTable.iskontoNedeni = "";
                                        malHizmetTable.vergiOrani = 0;
                                        malHizmetTable.vergininKdvTutari = "0";
                                        malHizmetTable.ozelMatrahTutari = "0";
                                        malHizmetTable.hesaplananotvtevkifatakatkisi = "0";
                                        draftInvoice.malHizmetTable.Add(malHizmetTable);
                                        var requestDTO = new CreateDraftInvoiceRequest
                                        {
                                            Token = draftInvoiceToken.Data.Token,
                                            Invoice = JsonConvert.SerializeObject(draftInvoice)
                                        };
                                        var draftResult = draftInvoiceCommand.Execute(requestDTO);
                                        invoice.Message = draftResult.Message;
                                        try
                                        {
                                            if (draftResult.StatusCode == HttpStatusCode.OK && draftResult.Data.data.Contains("Faturanız başarıyla oluşturulmuştur"))
                                            {
                                                createdInvoices.Add(invoice);
                                                invoice.InvoiceStatusId = (int)EnumInvoiceStatus.CreatedDraft;
                                            }
                                            else
                                            {
                                                notCreatedInvoice.Add(invoice);
                                                invoice.InvoiceStatusId = (int)EnumInvoiceStatus.NotCreatedDraft;
                                            }
                                        }
                                        catch (Exception)
                                        {
                                            notCreatedInvoice.Add(invoice);
                                            invoice.InvoiceStatusId = (int)EnumInvoiceStatus.NotCreatedDraft;
                                        }

                                        uow.Invoice.Update(invoice);

                                        if (uow.SaveChanges() && uow.Commit())
                                        {
                                            Console.WriteLine("{0} Fatura Veritabanına Kaydedildi!", invoice.InvoiceUniqueKey);
                                        }
                                        else
                                        {
                                            Console.WriteLine("{0} Fatura Veritabanına Yazılamadı!", invoice.InvoiceUniqueKey);
                                        }

                                    }


                                }
                                else
                                {
                                    Console.WriteLine("Faturalar Taslak Oluşturuluyor Olarak Güncellenemedi!");
                                }


                                StringBuilder sb = new StringBuilder();
                                sb.AppendLine(string.Format("<p>Kesilmek İstenilen Fatura Adeti ={0}</p>", invoices.Count));
                                sb.AppendLine(string.Format("<p>Başarıyla Kesilen Fatura Adeti ={0}</p>", createdInvoices.Count));
                                sb.AppendLine(string.Format("<p>Kesilemeyen Fatura Adeti ={0}</p>", notCreatedInvoice.Count));

                                if (notCreatedInvoice.Any())
                                {
                                    sb.AppendLine("<h1>Kesilemeyen Faturalar</h1>");
                                    sb.AppendLine("<table border=\"1\">");
                                    sb.AppendLine("<thead>");
                                    sb.AppendLine("<th>Alt Sipariş No</th>");
                                    sb.AppendLine("<th>ETTN</td>");
                                    sb.AppendLine("<th>Müşteri Adı</th>");
                                    sb.AppendLine("<th>Müşteri VKN</th>");
                                    sb.AppendLine("<th>Müşteri Vergi Dairesi</th>");
                                    sb.AppendLine("<th>Müşteri Adresi</th>");
                                    sb.AppendLine("<th>Birim Fiyat</th>");
                                    sb.AppendLine("<th>KDV</td>");
                                    sb.AppendLine("<th>Toplam</th>");
                                    sb.AppendLine("<th>Mesaj</th>");
                                    sb.AppendLine("</thead>");
                                    sb.AppendLine("<tbody>");

                                    foreach (var item in notCreatedInvoice)
                                    {
                                        sb.AppendLine("<tr>");
                                        sb.AppendLine(string.Format("<td>{0}</td>", item.SubOrderId));
                                        sb.AppendLine(string.Format("<td>{0}</td>", item.InvoiceUniqueKey));
                                        sb.AppendLine(string.Format("<td>{0}</td>", item.CustomerName));
                                        sb.AppendLine(string.Format("<td>{0}</td>", item.CustomerVKN));
                                        sb.AppendLine(string.Format("<td>{0}</td>", item.TaxOffice));
                                        sb.AppendLine(string.Format("<td>{0}</td>", item.Address));
                                        sb.AppendLine(string.Format("<td>{0}</td>", item.Price));
                                        sb.AppendLine(string.Format("<td>{0}</td>", item.Tax));
                                        sb.AppendLine(string.Format("<td>{0}</td>", item.SubTotal));
                                        sb.AppendLine(string.Format("<td>{0}</td>", item.Message));
                                        sb.AppendLine("</tr>");
                                    }

                                    sb.AppendLine("</tbody>");
                                    sb.AppendLine("</table>");
                                }

                                if (createdInvoices.Any())
                                {
                                    sb.AppendLine("Kesilen Faturalar");
                                    sb.AppendLine("<table border=\"1\">");
                                    sb.AppendLine("<thead>");
                                    sb.AppendLine("<th>Alt Sipariş No</th>");
                                    sb.AppendLine("<th>ETTN</td>");
                                    sb.AppendLine("<th>Müşteri Adı</th>");
                                    sb.AppendLine("<th>Müşteri VKN</th>");
                                    sb.AppendLine("<th>Müşteri Vergi Dairesi</th>");
                                    sb.AppendLine("<th>Müşteri Adresi</th>");
                                    sb.AppendLine("<th>Birim Fiyat</th>");
                                    sb.AppendLine("<th>KDV</td>");
                                    sb.AppendLine("<th>Toplam</th>");
                                    sb.AppendLine("<th>Mesaj</th>");
                                    sb.AppendLine("</thead>");
                                    sb.AppendLine("<tbody>");

                                    foreach (var item in createdInvoices)
                                    {
                                        sb.AppendLine("<tr>");
                                        sb.AppendLine(string.Format("<td>{0}</td>", item.SubOrderId));
                                        sb.AppendLine(string.Format("<td>{0}</td>", item.InvoiceUniqueKey));
                                        sb.AppendLine(string.Format("<td>{0}</td>", item.CustomerName));
                                        sb.AppendLine(string.Format("<td>{0}</td>", item.CustomerVKN));
                                        sb.AppendLine(string.Format("<td>{0}</td>", item.TaxOffice));
                                        sb.AppendLine(string.Format("<td>{0}</td>", item.Address));
                                        sb.AppendLine(string.Format("<td>{0}</td>", item.Price));
                                        sb.AppendLine(string.Format("<td>{0}</td>", item.Tax));
                                        sb.AppendLine(string.Format("<td>{0}</td>", item.SubTotal));
                                        sb.AppendLine(string.Format("<td>{0}</td>", item.Message));
                                        sb.AppendLine("</tr>");
                                    }

                                    sb.AppendLine("</tbody>");
                                    sb.AppendLine("</table>");
                                }

                                var file = uow.File.Find(fileId);

                                if (file.Success)
                                {
                                    file.Data.TotalPrice = createdInvoices.Sum(f => f.Price);
                                    file.Data.TotalTax = createdInvoices.Sum(f => f.Tax);
                                    file.Data.TotalSubTotal = createdInvoices.Sum(f => f.SubTotal);
                                    uow.File.Update(file.Data);
                                    uow.SaveChanges();
                                    uow.Commit();
                                }

                                MailHelper.Send(new MailItem
                                {
                                    SmtpHost = smtpHost,
                                    SmtpPort = smtpPort,
                                    SmtpSender = smtpSender,
                                    SmtpPassword = smtpPassword,
                                    InfoMail = InfoMail,
                                    Subject = "FATURA ÖZETİ",
                                    Message = sb.ToString(),
                                    AttachmentFile = string.Empty
                                });
                            }
                            else
                            {
                                Console.WriteLine("Kesilecek Fatura Bulunamadı");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

        }

        string format(decimal d) => Math.Round(d, 2).ToString().Replace(",", ".");
    }
}
