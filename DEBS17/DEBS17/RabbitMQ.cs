using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RabbitMQ.Client.Events;
using RabbitMQ.Client;

namespace DEBS17
{
    class RabbitMQ
    {
        StreamProcessing ObservationStream;

        public RabbitMQ()
        {

            ObservationStream = new StreamProcessing();
        }
        public StreamProcessing ReceiveFromRabbitMQ()
        {

            var Factory = new ConnectionFactory() { HostName = "localhost" };
            using (var Connection = Factory.CreateConnection())
            using (var Channel = Connection.CreateModel())
            {
                Channel.QueueDeclare(queue: "test_OG.04.04", durable: false, exclusive: false, autoDelete: false, arguments: null);
                EventingBasicConsumer Consumer = new EventingBasicConsumer(Channel);
                Consumer.Received += Consumer_Received;

                Channel.BasicConsume(queue: "test_OG.04.04", noAck: false, consumer: Consumer);
                //Console.WriteLine(" Press [enter] to exit.");
                Console.ReadLine();

            }
            Console.WriteLine("Finished");
            Console.ReadLine();
            return ObservationStream;
        }

        void Consumer_Received(object sender, BasicDeliverEventArgs e)
        {
            
            var Body = e.Body;
            var Message = Encoding.UTF8.GetString(Body);
            ObservationStream.ReadOGFromRabbitMQ(Message);
        }
    }
}
