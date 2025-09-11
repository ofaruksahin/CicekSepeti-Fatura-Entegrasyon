using Core.Utilities;
using EArchiveClient.DTO.Request;
using EArchiveClient.DTO.Response;
using Newtonsoft.Json;
using RestSharp;
using System;

namespace EArchiveClient.Commands
{
    /// <summary>
    /// 
    /// </summary>
    public class CreateDraftInvoiceCommand
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="dto"></param>
        public BaseResponse<CreateDraftInvoiceResponse> Execute(CreateDraftInvoiceRequest dto)
        {
            BaseResponse<CreateDraftInvoiceResponse> result = new BaseResponse<CreateDraftInvoiceResponse>();
            ConnectionInfo connectionInfo = ConnectionInfo.Instance;
            try
            {
                var client = new RestClient(connectionInfo.EArchiveURL+"earsiv-services/dispatch");
                client.Timeout = -1;
                var request = new RestRequest(Method.POST);
                request.AddHeader("Connection", "keep-alive");
                request.AddHeader("sec-ch-ua", "\" Not;A Brand\";v=\"99\", \"Google Chrome\";v=\"91\", \"Chromium\";v=\"91\"");
                request.AddHeader("Accept", "application/json, text/javascript, */*; q=0.01");
                request.AddHeader("DNT", "1");
                request.AddHeader("sec-ch-ua-mobile", "?0");
                client.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.114 Safari/537.36";
                request.AddHeader("Content-Type", "application/x-www-form-urlencoded; charset=UTF-8");
                request.AddHeader("Origin", connectionInfo.EArchiveURL.TrimEnd('/'));
                request.AddHeader("Sec-Fetch-Site", "same-origin");
                request.AddHeader("Sec-Fetch-Mode", "cors");
                request.AddHeader("Sec-Fetch-Dest", "empty");
                //request.AddHeader("Referer", connectionInfo.EArchiveURL+"index.jsp?token=21c80f50fa0c8a65a3aa68cefda68f0c3d173509f9e8614908d84f04deb32185bfedebb3d36639a43d55abd722823180335e16eccc9f45e7f7bb4f372574a9d6&v=1624476996318");
                request.AddHeader("Referer", connectionInfo.EArchiveURL+"index.jsp?token="+dto.Token+"&v=1756843433623");
                request.AddHeader("Accept-Language", "tr-TR,tr;q=0.9,en-US;q=0.8,en;q=0.7");
                request.AddHeader("Cookie", "JSESSIONID=-test; JSESSIONID=fffqwe");
                var body = @"cmd=EARSIV_PORTAL_FATURA_OLUSTUR&callid="+Guid.NewGuid().ToString().Substring(0,15)+"&pageName=RG_BASITFATURA&token="+dto.Token+"&jp="+System.Web.HttpUtility.UrlEncode(dto.Invoice);
                request.AddParameter("application/x-www-form-urlencoded; charset=UTF-8", body, ParameterType.RequestBody);
                IRestResponse response = client.Execute(request);
                result.StatusCode = response.StatusCode;
                result.Message = response.Content;
                result.Data = JsonConvert.DeserializeObject<CreateDraftInvoiceResponse>(response.Content);
            }
            catch (Exception)
            {

            }
            return result;
        }
    }
}
