using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace DEBS17
{
    class MetaDataReading
    {
        readonly char TripleSplitter = ' '; // split each line(triple) to its nodes by detecting space_charachter.
        readonly char[] ColumnSplitter = { ':', '>' };
        readonly char[] UnderScrollSpliter = { '_', '>' };
        readonly char[] SharpSplitter = { '#', '>' };
        readonly char QuotationSplitter = '"';
        readonly char[] DateTimeSplitters = { 'T', '+', '-', ':' };
        private MachineQueues MachineQueues;


        //public static void ReadUsingRDFDOTNET(string FilePath)
        //{
        //    try
        //    {

        //        IUriNode SearchNode, MoldingMachineInfo, ValueLiteralNode;
        //        string NodeFragment;
        //        double ProbabilityThreshold;
        //        int NumberOfClusters, PropertyNumber;
        //        string[] StringList;
        //        int Index;
        //        int MachineNumber;

        //        TurtleParser ttlparser = new TurtleParser();
        //        Graph graph = new Graph();
        //        ttlparser.Load(graph, FilePath);

        //        List<Triple> SearchForMoldingMachinesList = new List<Triple>();
        //        List<Triple> MoldingMachinesInfo = new List<Triple>();
        //        List<Triple> NumberOfClustersPerMachine = new List<Triple>();
        //        List<Triple> ProbabilityThresholdPerMachine = new List<Triple>();

        //        SearchNode = graph.GetUriNode("wmm:MoldingMachine");
        //        SearchForMoldingMachinesList = (List<Triple>)graph.GetTriplesWithObject(SearchNode);
        //        foreach (Triple SearchTriple in SearchForMoldingMachinesList)
        //        {
        //            SearchNode = (UriNode)SearchTriple.Subject;
        //            NodeFragment = SearchNode.Uri.Fragment;
        //            Index = NodeFragment.IndexOf('_');
        //            MachineNumber = Convert.ToInt32(NodeFragment.Substring(Index + 1, NodeFragment.Length - Index - 1));
        //            MachineQueues MachineQueue = new MachineQueues(MachineNumber);

        //            MoldingMachinesInfo = (List<Triple>)graph.GetTriplesWithSubject(SearchNode);
        //            foreach (Triple MoldingMachineTriple in MoldingMachinesInfo)// iterate through all Molding Machines to save its info(Number of Cluster + Pobability Threshold)
        //            {
        //                MoldingMachineInfo = (UriNode)MoldingMachineTriple.Predicate;
        //                NodeFragment = MoldingMachineInfo.Uri.Fragment;
        //                PropertyNumber = -1;
        //                if (NodeFragment == "#hasNumberOfClusters") // the node whose its predicate is '#hasNumberOfClusters'
        //                {
        //                    NumberOfClusters = 0;
        //                    NumberOfClustersPerMachine = (List<Triple>)graph.GetTriplesWithSubject(MoldingMachineTriple.Object);
        //                    foreach (Triple InfoTriple in NumberOfClustersPerMachine)
        //                    {
        //                        ValueLiteralNode = (UriNode)InfoTriple.Predicate;
        //                        NodeFragment = ValueLiteralNode.Uri.Fragment;
        //                        if (NodeFragment == "#valueLiteral")
        //                        {
        //                            StringList = InfoTriple.Object.ToString().Split('^');
        //                            NumberOfClusters = Convert.ToInt32(StringList[0]);
        //                            if (PropertyNumber != -1)
        //                                MachineQueue.AddNumberOfClustersValue(PropertyNumber, NumberOfClusters);
        //                        }
        //                        if (NodeFragment == "#numberOfClustersForProperty")
        //                        {
        //                            StringList = InfoTriple.Object.ToString().Split('_');
        //                            PropertyNumber = Convert.ToInt32(StringList[1]);
        //                            if (NumberOfClusters != 0)
        //                                MachineQueue.AddNumberOfClustersValue(PropertyNumber, NumberOfClusters);
        //                        }
        //                    }
        //                }

        //                if (NodeFragment == "#hasProbabilityThreshold") // the node whose its predicate is '#hasProbabilityThreshold'
        //                {
        //                    ProbabilityThreshold = 0;
        //                    ProbabilityThresholdPerMachine = (List<Triple>)graph.GetTriplesWithSubject(MoldingMachineTriple.Object);
        //                    foreach (Triple InfoTriple in ProbabilityThresholdPerMachine)
        //                    {
        //                        ValueLiteralNode = (UriNode)InfoTriple.Predicate;
        //                        NodeFragment = ValueLiteralNode.Uri.Fragment;
        //                        if (NodeFragment == "#valueLiteral")
        //                        {
        //                            StringList = InfoTriple.Object.ToString().Split('^');
        //                            ProbabilityThreshold = Convert.ToDouble(StringList[0]);
        //                            if (PropertyNumber != -1)
        //                                MachineQueue.AddThresholdValue(PropertyNumber, ProbabilityThreshold);
        //                        }
        //                        if (NodeFragment == "#isThresholdForProperty")
        //                        {
        //                            StringList = InfoTriple.Object.ToString().Split('_');
        //                            PropertyNumber = Convert.ToInt32(StringList[1]);
        //                            if (ProbabilityThreshold != 0)
        //                                MachineQueue.AddThresholdValue(PropertyNumber, ProbabilityThreshold);
        //                        }
        //                    }
        //                }

        //            }
        //            Singleton.MachineQueues.Add(MachineQueue);
        //        }

        //    }
        //    catch (RdfParseException parseEx)
        //    {
        //        //This indicates a parser error e.g unexpected character, premature end of input, invalid syntax etc.
        //        Console.WriteLine("Parser Error");
        //        Console.WriteLine(parseEx.Message);
        //    }
        //    catch (RdfException rdfEx)
        //    {
        //        //This represents a RDF error e.g. illegal triple for the given syntax, undefined namespace
        //        Console.WriteLine("RDF Error");
        //        Console.WriteLine(rdfEx.Message);
        //    }
        //    //for (int i = 0; i < ObservationGroups.Count; i++)
        //    //    Console.WriteLine(ObservationGroups[i].PrintContent());

        //    //Console.ReadLine();
        //}

        public void Read(string FilePath)
        {

            string[] Components, SubjectParts, PredicateParts, ObjectParts;
            string[] StringArray;
            int MachineNumber;

            StreamReader streamReader = new StreamReader(FilePath);

            while (!streamReader.EndOfStream)
            {
                string Line = streamReader.ReadLine();
                Components = Line.Split(TripleSplitter); //split each line(triple) to its three components(Subject - Predicate - Object).
                if (Components.Length == 4) //Make sure we have non-empty line and contains indeed three components PLUS a space at the end !!
                {
                    PredicateParts = Components[1].Split(SharpSplitter);
                    ObjectParts = Components[2].Split(SharpSplitter);
                    if (PredicateParts[1] == "valueLiteral") //<http://www.agtinternational.com/ontologies/WeidmullerMetadata#ProbabilityThreshold_0_5> <http://www.agtinternational.com/ontologies/IoTCore#valueLiteral> "0.73"^^<http://www.w3.org/2001/XMLSchema#double> .
                    {
                        SubjectParts = Components[0].Split(UnderScrollSpliter);
                        ObjectParts = Components[2].Split(QuotationSplitter);
                        MachineQueues.AddThresholdValue(Convert.ToInt32(SubjectParts[2]), Convert.ToDouble(ObjectParts[1]));
                    }
                    else if (PredicateParts[1] == "hasNumberOfClusters") //<http://www.agtinternational.com/ontologies/WeidmullerMetadata#_0_5> <http://www.agtinternational.com/ontologies/WeidmullerMetadata#hasNumberOfClusters> "13"^^<http://www.w3.org/2001/XMLSchema#int> .
                    {
                        SubjectParts = Components[0].Split(UnderScrollSpliter);
                        ObjectParts = Components[2].Split(QuotationSplitter);
                        MachineQueues.AddNumberOfClustersValue(Convert.ToInt32(SubjectParts[2]), Convert.ToInt32(ObjectParts[1]));

                    }
                    else if (ObjectParts[1] == "MoldingMachine") //<http://www.agtinternational.com/ontologies/WeidmullerMetadata#Machine_0> <http://www.w3.org/1999/02/22-rdf-syntax-ns#type> <http://www.agtinternational.com/ontologies/WeidmullerMetadata#MoldingMachine> .
                    {
                        StringArray = Components[0].Split(UnderScrollSpliter);
                        MachineNumber = Convert.ToInt32(StringArray[1]);
                        if (MachineQueues != null)
                            Singleton.MachineQueues.Add(MachineQueues);

                        MachineQueues = new MachineQueues(MachineNumber);

                    }
                }
            }
            Singleton.MachineQueues.Add(MachineQueues);
            InitiateKmeans();
        }

        private void InitiateKmeans()
        {
            int  Property, K;
            for (int i = 0; i < Singleton.MachineQueues.Count; i++) //loop over the list of MachineQueues 
                for (int PropertyIndex = 0; PropertyIndex < Singleton.MachineQueues[i].StatfulProperties.Count; PropertyIndex++)
                {
                    Property = Singleton.MachineQueues[i].StatfulProperties[PropertyIndex]; // get the property real number
                    K = Singleton.MachineQueues[i].QueuesInfo[Property].NumberOfClusters;
                    Singleton.MachineQueues[i].KmeanInstances[Property] = new k_means(K, i, Property);  //we instantiate a new K means attached with the property
                }
        }
    }
}
