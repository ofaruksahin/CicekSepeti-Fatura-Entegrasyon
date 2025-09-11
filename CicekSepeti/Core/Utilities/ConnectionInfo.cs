using Microsoft.Extensions.Configuration;

namespace Core.Utilities
{
    /// <summary>
    /// 
    /// </summary>
    public class ConnectionInfo
    {
        /// <summary>
        /// 
        /// </summary>
        static volatile ConnectionInfo _instance;

        /// <summary>
        /// 
        /// </summary>
        public static ConnectionInfo Instance
        {
            get
            {
                return _instance ?? (_instance = new ConnectionInfo());
            }
        }

        /// <summary>
        /// 
        /// </summary>
        readonly IConfiguration configuration;

        /// <summary>
        /// 
        /// </summary>
        private ConnectionInfo()
        {
            configuration = Environment.Environment.Instance.GetConfiguration();
        }

        /// <summary>
        /// 
        /// </summary>
        public string MySQLServerConnectionString => (string)configuration.GetValue(typeof(string), "mysql_server_connection_string");

        /// <summary>
        /// 
        /// </summary>
        public string RedisConnectionString => (string)configuration.GetValue(typeof(string), "redis_connection_string");

        /// <summary>
        /// 
        /// </summary>
        public string EArchiveURL => (string)configuration.GetValue(typeof(string), "earchive_url");
        /// <summary>
        /// 
        /// </summary>
        public string DownloadInvoiceType => (string)configuration.GetValue(typeof(string), "download_invoice_type");

        /// <summary>
        /// 
        /// </summary>
        public string RabbitMQHost => (string)configuration.GetValue(typeof(string), "rabbitmq_ip");

        /// <summary>
        /// 
        /// </summary>
        public string RabbitMQUser => (string)configuration.GetValue(typeof(string), "rabbitmq_user");

        /// <summary>
        /// 
        /// </summary>
        public string RabbitMQPassword => (string)configuration.GetValue(typeof(string), "rabbitmq_password");

        /// <summary>
        /// 
        /// </summary>
        public string MinioIp => (string)configuration.GetValue(typeof(string), "minio_ip");

        /// <summary>
        /// 
        /// </summary>
        public string MinioUserName => (string)configuration.GetValue(typeof(string), "minio_user");

        /// <summary>
        /// 
        /// </summary>
        public string MinioPassword => (string)configuration.GetValue(typeof(string), "minio_password");

        /// <summary>
        /// 
        /// </summary>
        public string CompanyName => (string)configuration.GetValue(typeof(string), "company_name");
        
        /// <summary>
        /// 
        /// </summary>
        public string LocalFilePath => (string)configuration.GetValue(typeof(string), "local_file_path");

        /// <summary>
        /// 
        /// </summary>
        public string CdnUrl => (string)configuration.GetValue(typeof(string), "cdn_url");
    }
}
