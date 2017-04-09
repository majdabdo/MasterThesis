using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;
using RabbitMQ.Client.Events;
using RabbitMQ.Client;


namespace DEBS17
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.SetBufferSize(Console.BufferWidth, 30000);
            string TimingMessage = "";
            string FileDirectory = "C:/Users/AFFFOOOOOOD/Desktop/Master Thesis/Datasets/04.04.2017_full_10M/";
            string MetaDataFile = "molding_machine_10M.metadata.nt";//metadata.ttl   sample_metadata_1machine.nt
            string MeasurementsFile = "molding_machine_10M.nt";
            //string line;
            int Repetitions = 0;
            double AverageTime = 0;
            DateTime TimerStart;
            TimeSpan MetaDataTimeSpan;

            Console.WriteLine("Enter Maximal K-means Execution Times(M):");
            Singleton.M = Convert.ToInt32(Console.ReadLine());
            Console.WriteLine("Enter Transition Count(N):");
            Singleton.N = Convert.ToInt32(Console.ReadLine());
            Console.WriteLine("Enter Window size(W):");
            Singleton.W = Convert.ToInt32(Console.ReadLine())-1;
            TimerStart = DateTime.Now;
            MetaDataReading MetaDataReading = new MetaDataReading();
            MetaDataReading.Read(FileDirectory + MetaDataFile);
            MetaDataTimeSpan = DateTime.Now - TimerStart;
            TimingMessage += string.Format("MetaData Reading took: {0} ms \n", MetaDataTimeSpan.TotalMilliseconds);
            TimingMessage += string.Format("MetaData File: {0}\nObservations File: {1}\n", MetaDataFile, MeasurementsFile);
            //Singleton.Writer.WriteResultsOnFile(TimingMessage);

            RabbitMQ MQ = new RabbitMQ();//would be System Adapter
            StreamProcessing ObservationsStream = MQ.ReceiveFromRabbitMQ();//init

            TimingMessage = string.Format("  Observations Reading + All phases execution         All Iterations time       One Iteration time(Average)            Times of Execution\n");
            AverageTime = Convert.ToDouble(ObservationsStream.SumOfAllQueriesTime / ObservationsStream.ExecutionCount);
            AverageTime = Math.Round(AverageTime, 4);
            ObservationsStream.SumOfAllQueriesTime = Math.Round(ObservationsStream.SumOfAllQueriesTime, 3);
            ObservationsStream.OverallExecution = Math.Round(ObservationsStream.OverallExecution, 3);
            TimingMessage += string.Format("{4}]         {0} ms                                     {1} ms                 {2} ms                            {3}\n", ObservationsStream.OverallExecution, ObservationsStream.SumOfAllQueriesTime, AverageTime, ObservationsStream.ExecutionCount, Repetitions);


            
            //Singleton.Writer.WriteResultsOnFile(TimingMessage);
            Singleton.Writer.close();
            Singleton.Writer.OpenFile();

        }
    }
}
//ParametersMessage += string.Format("Parameters:\n------------\n   Maximal K-means Execution(M)    Transition Count(N)    Window Size\n");
//ParametersMessage += string.Format("{3}]        {0}                          {1}                    {2}\n", Singleton.M, Singleton.N, Singleton.W, Repetitions);
//Singleton.Writer.WriteResultsOnFile(ParametersMessage);