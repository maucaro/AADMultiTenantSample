using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.Configuration;
using System.Security.Cryptography.X509Certificates;


namespace WebAppTestPacker3.Utils
{
    public static class AppSettings
    {
        public static X509Certificate2 cert { get; }
        private static readonly string certThumbprint = ConfigurationManager.AppSettings["ida:CertThumbprint"];

        static AppSettings()
        {
            cert = null;
            X509Store store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
            try
            {
                store.Open(OpenFlags.ReadOnly);
                X509Certificate2Collection certCollection = store.Certificates;
                X509Certificate2Collection signingCert = certCollection.Find(X509FindType.FindByThumbprint, certThumbprint, false);
                if (signingCert.Count == 0)
                {
                    return;
                }
                cert = signingCert[0];
            }
            finally
            {
                store.Close();
            }
        }
    }
}