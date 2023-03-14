using SeleniumUndetectedChromeDriver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RabbitMQ.Client.Events;
using RabbitMQ.Client;
using Newtonsoft.Json;

namespace OrderServiceBot
{
    public class MqLisener
    {
        public const string QUEUE_URLS = "eshop_queue";
        public const string QUEUE_RESULTS = "eshop_result";

        private void InitQueue(IModel channel, string queueName)
        {
            channel.QueueDeclare(queue: queueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);


        }

        private void PublishProduct(IModel channel, ProductData productData)
        {
            string jsonObject = JsonConvert.SerializeObject(productData);
            var body = Encoding.UTF8.GetBytes(jsonObject);

            channel.BasicPublish(exchange: string.Empty,
                routingKey: QUEUE_RESULTS,
                basicProperties: null,
                body: body);
        }

        public void StartBot()
        {
            IBotScrapper scapeBot = new BotScrapper();
            scapeBot.Init();

            var factory = new ConnectionFactory { HostName = "localhost" };
            var connection = factory.CreateConnection();
            var channel = connection.CreateModel();

            InitQueue(channel, QUEUE_URLS);
            InitQueue(channel, QUEUE_RESULTS);

            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var url = Encoding.UTF8.GetString(body);
                Console.WriteLine(url);

                var product = scapeBot.Scrape(url);
                PublishProduct(channel, product);

                channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
            };


            channel.BasicConsume(queue: QUEUE_URLS, autoAck: false, consumer: consumer);

            Console.WriteLine("Started a consumer");
        }
        public void DoWork()
        {
            StartBot();

            while (true)
            {
                Console.WriteLine("waiting..");
                Thread.Sleep(10000);
            }
        }                                            
    }                   
}
