using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.AspNet.WebHooks;
using System.Net;
using System.Net.Http;

namespace WebJobTestPacker
{
    public class Functions
    {
        // This function will get triggered/executed when a new message is written 
        // on an Azure Queue called queue.
        public static void ProcessQueueMessage([QueueTrigger("queue")] string message, TextWriter log)
        {
            log.WriteLine(message);
        }

        public async Task SlackIniatedWebHook([WebHookTrigger("packer/webhook")] WebHookContext context, [Queue("PackerWork")] ICollector<SlackWork> messages)
        {
            string body = await context.Request.Content.ReadAsStringAsync();

            context.Response = new HttpResponseMessage(HttpStatusCode.Accepted);
            context.Response.Content = new StringContent("Message received! Hello world!");
        }

        public class SlackWork
        {
            public int id { get; set; }
            public string username { get; set; }
            public int work { get; set; }
            public string replyUrl { get; set; }
        }
    }
}
