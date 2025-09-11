using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace EArchiveClient.DTO.Request
{
    public class CreateDraftInvoiceRequest
    {
        /// <summary>
        /// 
        /// </summary>
        public string Token { get; set; }

        /// <summary>
        ///
        /// </summary>
        public string Invoice { get; set; } = string.Empty;


        //public Fatura Invoice { get; set; }
    }

    public class MalHizmetTable
    {
        public string malHizmet { get; set; }
        public int miktar { get; set; }
        public string birim { get; set; }
        public string birimFiyat { get; set; }
        public string fiyat { get; set; }
        public int iskontoOrani { get; set; }
        public string iskontoTutari { get; set; }
        public string iskontoNedeni { get; set; }
        public string malHizmetTutari { get; set; }
        public string kdvOrani { get; set; }
        public int vergiOrani { get; set; }
        public string kdvTutari { get; set; }
        public string vergininKdvTutari { get; set; }
        public string ozelMatrahTutari { get; set; }
        public string hesaplananotvtevkifatakatkisi { get; set; }
    }

    public class Fatura
    {
        public string faturaUuid { get; set; }
        public string belgeNumarasi { get; set; }
        public string faturaTarihi { get; set; }
        public string saat { get; set; }
        public string paraBirimi { get; set; }
        public string dovzTLkur { get; set; }
        public string faturaTipi { get; set; }
        public string hangiTip { get; set; }
        public string vknTckn { get; set; }
        public string aliciUnvan { get; set; }
        public string aliciAdi { get; set; }
        public string aliciSoyadi { get; set; }
        public string binaAdi { get; set; }
        public string binaNo { get; set; }
        public string kapiNo { get; set; }
        public string kasabaKoy { get; set; }
        public string vergiDairesi { get; set; }
        public string ulke { get; set; }
        public string bulvarcaddesokak { get; set; }
        public string irsaliyeNumarasi { get; set; }
        public string irsaliyeTarihi { get; set; }
        public string mahalleSemtIlce { get; set; }
        public string sehir { get; set; }
        public string postaKodu { get; set; }
        public string tel { get; set; }
        public string fax { get; set; }
        public string eposta { get; set; }
        public string websitesi { get; set; }
        public List<object> iadeTable { get; set; } = new List<object>();
        public string ihracKayitliKarsiBelgeNo { get; set; }
        public string vergiCesidi { get; set; }
        public List<MalHizmetTable> malHizmetTable { get; set; }
        public string tip { get; set; }
        public string matrah { get; set; }
        public string malhizmetToplamTutari { get; set; }
        public string toplamIskonto { get; set; } = "0";
        public string hesaplanankdv { get; set; }
        public string vergilerToplami { get; set; }
        public string vergilerDahilToplamTutar { get; set; }
        public string odenecekTutar { get; set; }
        public string not { get; set; }
        public string siparisNumarasi { get; set; }
        public string siparisTarihi { get; set; }
        public string fisNo { get; set; }
        public string fisTarihi { get; set; }
        public string fisSaati { get; set; }
        public string fisTipi { get; set; }
        public string zRaporNo { get; set; }
        public string okcSeriNo { get; set; }

        public Fatura()
        {
            malHizmetTable = new List<MalHizmetTable>();
        }
    }
}
