﻿using System.Text;
using RabbitMQ.Client.Events;
using RabbitMQ.Client;
using Newtonsoft.Json;
using OpenQA.Selenium;
using OrderService.Core.RabbitMqDto;

namespace OrderServiceBot
{
    public class MqLisener
    {
        public const string QUEUE_URLS = "eshop_queue";
        public const string QUEUE_RESULTS = "eshop_result";

        IBotScrapper scapeBot;

        private void InitQueue(IModel channel, string queueName)
        {
            channel.QueueDeclare(queue: queueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);


        }

        private void PublishProduct(IModel channel, object productData)
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


            scapeBot = new BotScrapper();
            scapeBot.Init();

            

            var factory = new ConnectionFactory { HostName = Environment.GetEnvironmentVariable("HOSTNAME") };
            
            Console.WriteLine(factory.HostName);

            var connection = factory.CreateConnection();
            var channel = connection.CreateModel();

            InitQueue(channel, QUEUE_URLS);
            InitQueue(channel, QUEUE_RESULTS);

            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (model, ea) =>
            {
                string userId = "";

                try
                {
                    var body = ea.Body.ToArray();
                    var json = Encoding.UTF8.GetString(body);
                    var rabbitProductRequest = JsonConvert.DeserializeObject<RabbitRequestProductData>(json);

                    userId = rabbitProductRequest.connectionId;

                    if (rabbitProductRequest == null)
                    {
                        Console.WriteLine("request product is null");
                        return;
                    }

                    Console.WriteLine(rabbitProductRequest.productUrl);

                    var product = scapeBot.Scrape(rabbitProductRequest);
                    PublishProduct(channel, product);

                    channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                }
                catch(NoSuchElementException ex)
                {
                    Console.WriteLine("no element, return error for customer, selector:" + ex.Message);
                    channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                    PublishProduct(channel, new Message(userId, "BOT cannot get item information"));
                }
                catch (JsonReaderException)
                {
                    Console.WriteLine("invalid json");
                    channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                    PublishProduct(channel, new Message(userId, "BOT cannot get item information"));
                }
                catch(Exception ex)
                {
                    if (ex.Message.Contains("invalid argument"))
                    {
                        Console.WriteLine("invalid argument exception");
                        PublishProduct(channel, new Message(userId, "invalid argument (url?)"));

                        channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                        

                    }
                    else
                    {
                        //requeue message that error 
                        channel.BasicNack(deliveryTag: ea.DeliveryTag, multiple: false, requeue: true);
                    }

                    Console.WriteLine(ex.Message);
                    scapeBot.Dispose();

                    Thread.Sleep(1000);

                    scapeBot.Init();

                }

            };



            channel.BasicConsume(queue: QUEUE_URLS, autoAck: false, consumer: consumer);

            Console.WriteLine("Started a consumer");
        }
        public void DoWork()
        {
            StartBot();

            while (true)
            {
                Thread.Sleep(10000);
            }
        }                                            
    }                   
}
