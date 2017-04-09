using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DEBS17
{
    class Writer
    {
        string FileDirectory = "C:/Users/AFFFOOOOOOD/Desktop/Master Thesis/";
        string FileName = "/OutputData.ttl";
        private int ID = -1;
        private int[] AnomalyInfoPositions = new int[] { 5, 9, 13, 17 };
//        private int[] AnomalyIDPositions = new int[] { 1, 3, 7, 11, 15 };
        private int[] AnomalyIDPositions = new int[] { 1, 3, 6, 9, 12 };
        private  List<string> OutputFormat = new List<string>(new string[] { "<http://project-hobbit.eu/resources/debs2017#Anomaly_",
            "> <http://www.w3.org/1999/02/22-rdf-syntax-ns#type> <http://www.agtinternational.com/ontologies/DEBSAnalyticResults#Anomaly> .\n<http://project-hobbit.eu/resources/debs2017#Anomaly_" ,
        " <http://www.agtinternational.com/ontologies/I4.0#machine> <http://www.agtinternational.com/ontologies/WeidmullerMetadata#Machine_",
        "> .\n<http://project-hobbit.eu/resources/debs2017#Anomaly_","> <http://www.agtinternational.com/ontologies/DEBSAnalyticResults#inAbnormalDimension> <http://www.agtinternational.com/ontologies/WeidmullerMetadata#_",
        "> .\n<http://project-hobbit.eu/resources/debs2017#Anomaly_","> <http://www.agtinternational.com/ontologies/DEBSAnalyticResults#hasTimeStamp> <http://project-hobbit.eu/resources/debs2017#",
        "> .\n<http://project-hobbit.eu/resources/debs2017#Anomaly_","> <http://www.agtinternational.com/ontologies/DEBSAnalyticResults#hasProbabilityOfObservedAbnormalSequence> \"","\"^^<http://www.w3.org/2001/XMLSchema#double> ."});
        
        System.IO.StreamWriter file;
        public Writer()
        {
            file = new System.IO.StreamWriter(FileDirectory + FileName);
        }

        public void OutputAnomaly(string[] Infos)
        {
            List<string> OutputList = new List<string>(OutputFormat);
//            OutputList = OutputFormat;
            string ID = GetAnomalyID();
            int Position;
            int Length = OutputList.Count();
            for (int i = 0; i <AnomalyIDPositions.Length; i++)
            {
                Position = AnomalyIDPositions[i];
                OutputList.Insert(Position, ID);
                
            }

            for (int i = 0; i < AnomalyInfoPositions.Length; i++)
            {
                Position = AnomalyInfoPositions[i];
                OutputList.Insert(Position, Infos[i]);
            }
            string OutputStr  = string.Join("",OutputList);
            WriteResultsOnFile(OutputStr+"\n");
        }

        private string GetAnomalyID()
        {
            ID++;
            return Convert.ToString(ID);
        }
        public void WriteResultsOnFile(string str)
        {
            file.Write(str);
        }
        public void WriteResultsOnScreen(string str)
        {
            Console.Write(str);
        }
        public void OpenFile()
        {
            System.Diagnostics.Process.Start(FileDirectory + FileName);
        }
        public void close()
        {
            file.Close();
        }
    }
}
