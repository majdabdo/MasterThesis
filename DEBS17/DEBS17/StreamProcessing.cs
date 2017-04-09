using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace DEBS17
{
    class StreamProcessing
    {
        //readonly char TripleSplitter = ' '; // split each line(triple) to its nodes by detecting space_charachter.
        readonly char[] ColumnSplitter = { ':', '>' };
        readonly char[] UnderScrollSpliter = { '_', '>' };
        readonly char[] SharpSplitter = { '#', '>' };
        readonly char QuotationSplitter = '"';
        readonly char[] DateTimeSplitters = { 'T', '+', '-', ':' };
        string[]  SubjectParts, PredicateParts, ObjectParts;
        string[] StringArray;
        int index;
        int TempIndex;
        int ObservationGroupNumber = -1;

        private delegate void NotifyGpaDelegate(object o, BasicDeliverEventArgs e);

        private ObservationGroup ObservationGroup;

        public int ExecutionCount;
        public double SumOfAllQueriesTime;
        public double OverallExecution;
        TimeSpan timeSpan1;
        TimeSpan timeSpan2;
        DateTime start1;
        DateTime start2;
        public StreamProcessing()
        {
            timeSpan1 = new TimeSpan();
            timeSpan2 = new TimeSpan();
            ExecutionCount = 0;
            SumOfAllQueriesTime = 0;
            OverallExecution = 0;
        }
        public void ReadOGFromRabbitMQ(string OG)
        {
            start2 = DateTime.Now;
            //string[] Lines = OG.Split("\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            ObservationGroup = new ObservationGroup();
            ObservationGroup.observationsData = new ObservationsData();
            string[] Lines = Regex.Split(OG, " .");
            for (int i = 0; i < Lines.Length - 1; i = i + 3)
            {
                PredicateParts = Lines[i + 1].Split(SharpSplitter);
                switch (PredicateParts[1])
                {
                    case "valueLiteral": //http://www.agtinternational.com/ontologies/IoTCore#valueLiteral
                        SubjectParts = Lines[i].Split(SharpSplitter);
                        ObjectParts = Lines[i + 2].Split(QuotationSplitter);
                        if (SubjectParts[1].Substring(0, 5) == "Value") //"9433.11"^^<http://www.w3.org/2001/XMLSchema#float>
                        {
                            StringArray = SubjectParts[1].Split(UnderScrollSpliter);//Value_0
                            index = ObservationGroup.observationsData.ItemSearch(Convert.ToInt32(StringArray[1]), ObservationGroup.observationsData.ValueLabels);
                            if (index == -1)
                            {
                                TempIndex = ObservationGroup.observationsData.ItemSearch(Convert.ToInt32(StringArray[1]), ObservationGroup.observationsData.TempValueLabels);//search if corresponding "ValueLiteral" has already been saved
                                if (TempIndex == -1)// NO Temp Value_0 entry has already come with triple of "Output"
                                {
                                    ObservationGroup.observationsData.TempObservationsIndex++; // take a new tempIndex
                                    TempIndex = ObservationGroup.observationsData.TempObservationsIndex;
                                    ObservationGroup.observationsData.TempValueLabels[TempIndex] = Convert.ToInt32(StringArray[1]);//save ValueLabel
                                }
                                //Value_0 entry has already come with triple of "ValueLiteral" thus we have already a reserved Index

                                StringArray = ObjectParts[1].Split(SharpSplitter);
                                if (StringArray[1] != "string")//if (RDF_Processing.MachinesQueues[MachineIndex].IsStatefulDimension(PropertyIndex))
                                    ObservationGroup.observationsData.TempValues[TempIndex] = Convert.ToDouble(ObjectParts[0]);

                            }
                            else
                            {
                                StringArray = ObjectParts[1].Split(SharpSplitter);
                                if (StringArray[1] != "string")//if (RDF_Processing.MachinesQueues[MachineIndex].IsStatefulDimension(PropertyIndex))
                                    ObservationGroup.observationsData.Values[index] = Convert.ToDouble(ObjectParts[0]);
                            }
                        }

                        else if (SubjectParts[1].Substring(0, 5) == "Times")//"2017-01-01T01:00:10+01:00"^^<http://www.w3.org/2001/XMLSchema#dateTime>
                        {
                            ObservationGroup.TimeStampLabel = SubjectParts[1];
                            StringArray = ObjectParts[0].Split(DateTimeSplitters);
                            DateTime TimeStamp = new DateTime(Convert.ToInt32(StringArray[0]), Convert.ToInt32(StringArray[1]), Convert.ToInt32(StringArray[2]), Convert.ToInt32(StringArray[3]), Convert.ToInt32(StringArray[4]), Convert.ToInt32(StringArray[5]));
                            ObservationGroup.TimeStamp = TimeStamp;
                        }
                        break;

                    case "observationResult": //<http://project-hobbit.eu/resources/debs2017#Observation_0> <http://purl.oclc.org/NET/ssnx/ssn#observationResult> <http://project-hobbit.eu/resources/debs2017#Output_0>
                        StringArray = Lines[i].Split(UnderScrollSpliter);//Observation_0
                        index = ObservationGroup.observationsData.ItemSearch(Convert.ToInt32(StringArray[1]), ObservationGroup.observationsData.Observations);
                        if (index == -1)
                        {
                            ObservationGroup.observationsData.ObservationsIndex++;
                            index = ObservationGroup.observationsData.ObservationsIndex;
                        }

                        ObservationGroup.observationsData.Observations[index] = Convert.ToInt32(StringArray[1]);
                        StringArray = Lines[i + 2].Split(UnderScrollSpliter);//Output_0
                        ObservationGroup.observationsData.Outputs[index] = Convert.ToInt32(StringArray[1]);
                        break;


                    case "hasValue": //<http://project-hobbit.eu/resources/debs2017#Output_0> <http://purl.oclc.org/NET/ssnx/ssn#hasValue> <http://project-hobbit.eu/resources/debs2017#Value_0>
                        StringArray = Lines[i + 2].Split(UnderScrollSpliter); //Value_0
                        TempIndex = ObservationGroup.observationsData.ItemSearch(Convert.ToInt32(StringArray[1]), ObservationGroup.observationsData.TempValueLabels); //search if corresponding "observationResult"has already been saved
                        if (TempIndex == -1)
                        {
                            StringArray = Lines[i].Split(UnderScrollSpliter);//Output_0
                            index = ObservationGroup.observationsData.ItemSearch(Convert.ToInt32(StringArray[1]), ObservationGroup.observationsData.Outputs);//search if corresponding "ValueLiteral" has already been saved
                            if (index == -1)// NO Output_0 entry has already come with triple of "ObservationResult"
                            {
                                ObservationGroup.observationsData.TempObservationsIndex++; // take a new tempIndex
                                TempIndex = ObservationGroup.observationsData.TempObservationsIndex;
                                ObservationGroup.observationsData.TempOutputs[TempIndex] = Convert.ToInt32(StringArray[1]);//save TempOutput
                                StringArray = Lines[i + 2].Split(UnderScrollSpliter);//Value_0
                                ObservationGroup.observationsData.TempValueLabels[TempIndex] = Convert.ToInt32(StringArray[1]);//save TempValue
                            }
                            else //Output_0 entry has already come with triple of "ObservationResult" thus we have already a reserved Index
                            {
                                StringArray = Lines[i + 2].Split(UnderScrollSpliter);//Value_0
                                ObservationGroup.observationsData.ValueLabels[index] = Convert.ToInt32(StringArray[1]);
                            }

                        }
                        else //Value entry has already come with triple of "ValueLiteral"
                        {
                            StringArray = Lines[i].Split(UnderScrollSpliter);//Output_0
                            ObservationGroup.observationsData.TempOutputs[TempIndex] = Convert.ToInt32(StringArray[1]);
                        }
                        break;

                    case "observedProperty": //<http://project-hobbit.eu/resources/debs2017#Observation_0> <http://purl.oclc.org/NET/ssnx/ssn#observedProperty> <http://www.agtinternational.com/ontologies/WeidmullerMetadata#_3>
                        StringArray = Lines[i].Split(UnderScrollSpliter);//Observation_0
                        index = ObservationGroup.observationsData.ItemSearch(Convert.ToInt32(StringArray[1]), ObservationGroup.observationsData.Observations);
                        if (index == -1)
                        {
                            ObservationGroup.observationsData.ObservationsIndex++;
                            index = ObservationGroup.observationsData.ObservationsIndex;
                            ObservationGroup.observationsData.Observations[index] = Convert.ToInt32(StringArray[1]);
                        }
                        StringArray = Lines[i + 2].Split(UnderScrollSpliter);
                        ObservationGroup.observationsData.ObservedProperties[index] = Convert.ToInt32(StringArray[2]);
                        break;

                    case "machine": //<http://project-hobbit.eu/resources/debs2017#ObservationGroup_0> <http://www.agtinternational.com/ontologies/I4.0#machine> <wmm:MoldingMachine_57>
                        StringArray = Lines[i + 2].Split(UnderScrollSpliter); //only the machine name needed
                        ObservationGroup.MachineNumber = Convert.ToInt32(StringArray[1]);
                        break;

                    default:
                        ObjectParts = Lines[i + 2].Split(SharpSplitter);
                        if ((ObjectParts[1] == "MoldingMachineObservationGroup") || (ObjectParts[1] == "AssemblyMachineObservationGroup")) //<http://project-hobbit.eu/resources/debs2017#ObservationGroup_0> <http://www.w3.org/1999/02/22-rdf-syntax-ns#type> <http://www.agtinternational.com/ontologies/I4.0#MoldingMachineObservationGroup>
                        {
                            //Assumption: ObservationGropu has a unique number.
                            StringArray = Lines[i].Split(UnderScrollSpliter);
                            ObservationGroupNumber = Convert.ToInt32(StringArray[1]);

                            ObservationGroup.observationGroupNumber = ObservationGroupNumber; //assign GroupNumber
                            ObservationGroup.observationsData.ObservationGroupIndex = ObservationGroupNumber;//GroupNumber to ObservationData
                        }
                        break;
                }
            }
            if (ObservationGroup.observationsData.TempObservationsIndex > -1)
                MatchMissingObservations(ObservationGroup.observationsData);
            FillQueuesWithObservations();
            timeSpan2 = DateTime.Now - start2;
            OverallExecution += timeSpan2.TotalMilliseconds;
        }

        private void MatchMissingObservations(ObservationsData observationData)
        {
            for (int i = 0; i < observationData.TempValueLabels.Length; i++)
                if (observationData.TempValueLabels[i] != int.MinValue)
                    for (int j = 0; j < observationData.Observations.Length; j++)
                        if (ObservationGroup.observationsData.Outputs[j] == ObservationGroup.observationsData.TempOutputs[i])
                        {
                            ObservationGroup.observationsData.Values[j] = ObservationGroup.observationsData.TempValues[i];
                            break;
                        }
        }

        public void FillQueuesWithObservations() // RECO: think about filling queues during RDF reading
        {
            int MachineIndex = -1;
            Singleton.Writer.WriteResultsOnScreen(Convert.ToString(ObservationGroup.observationGroupNumber + " "));
            for (int Index = 0; Index < Singleton.MachineQueues.Count; Index++) // Search for the desired machine among the machines List which this OG belongs to.
                if (Singleton.MachineQueues[Index].MachineNumber == ObservationGroup.MachineNumber)
                {
                    MachineIndex = Index;
                    break;
                }

            ObservationsData observationsData = ObservationGroup.observationsData; // get a reference of ObservationsData
            for (int i = 0; i < Singleton.MachineQueues[MachineIndex].StatfulProperties.Count; i++)//for (int Property = 0; Property < 120; Property++)
            {
                int Property = Singleton.MachineQueues[MachineIndex].StatfulProperties[i];
                int index = observationsData.ItemSearch(Property, observationsData.ObservedProperties); // search for the index of the property within observedProperties array.

                //RECO: CHeck the conditions Again
                if ((index != -1) && (Singleton.MachineQueues[MachineIndex].QueuesInfo[Property] != null)) // if we didn't found the index (result index = -1) AND we have Metadata related to this property (QueueInfo) 
                {
                    Singleton.MachineQueues[MachineIndex].AddQueuePoint(Property, observationsData.Values[index], ObservationGroup.TimeStamp, ObservationGroup.TimeStampLabel);// push the different observation values(value,timestamp,timestamplabel) into the corresponding property's queue of respective machine.
                    ExecutionCount++;
                    start1 = DateTime.Now;
                    Singleton.MachineQueues[MachineIndex].KmeanInstances[Property].NewEvent();
                    timeSpan1 = DateTime.Now - start1;
                    SumOfAllQueriesTime += timeSpan1.TotalMilliseconds;
                }
            }
        }

        //#region NotRelevant
        //public void ReadTripleFromRabbitMQ(string Line)
        //{
        //    start2 = DateTime.Now;
        //    Components = Line.Split(TripleSplitter); //split each line(triple) to its three components(Subject - Predicate - Object).
        //    if (Components.Length == 4) //Make sure we have non-empty line and contains indeed three components PLUS a space at the end !!
        //    {
        //        PredicateParts = Components[1].Split(SharpSplitter);
        //        switch (PredicateParts[1])
        //        {
        //            case "valueLiteral": //http://www.agtinternational.com/ontologies/IoTCore#valueLiteral
        //                SubjectParts = Components[0].Split(SharpSplitter);
        //                ObjectParts = Components[2].Split(QuotationSplitter); // there is "" at the begining !
        //                if (SubjectParts[1].Substring(0, 5) == "Value") //"9433.11"^^<http://www.w3.org/2001/XMLSchema#float>
        //                {
        //                    StringArray = SubjectParts[1].Split(UnderScrollSpliter);
        //                    index = ObservationGroup.observationsData.ItemSearch(Convert.ToInt32(StringArray[1]), ObservationGroup.observationsData.ValueLabels);
        //                    //if (index ==-1) return exception
        //                    StringArray = ObjectParts[2].Split(SharpSplitter);//RECO
        //                    if (StringArray[1] != "string")//if (RDF_Processing.MachinesQueues[MachineIndex].IsStatefulDimension(PropertyIndex))
        //                        ObservationGroup.observationsData.Values[index] = Convert.ToDouble(ObjectParts[1]);
        //                }
        //                else //if (SubjectParts[1].Substring(0, 5) == "Times")//"2017-01-01T01:00:10+01:00"^^<http://www.w3.org/2001/XMLSchema#dateTime>
        //                {
        //                    ObservationGroup.TimeStampLabel = SubjectParts[1];
        //                    StringArray = ObjectParts[1].Split(DateTimeSplitters);
        //                    DateTime TimeStamp = new DateTime(Convert.ToInt32(StringArray[0]), Convert.ToInt32(StringArray[1]), Convert.ToInt32(StringArray[2]), Convert.ToInt32(StringArray[3]), Convert.ToInt32(StringArray[4]), Convert.ToInt32(StringArray[5]));
        //                    ObservationGroup.TimeStamp = TimeStamp;
        //                }
        //                //else if (SubjectParts[1].Substring(0, 5) == "Cycle") //"13"^^<http://www.w3.org/2001/XMLSchema#int>
        //                //    ObservationGroup.Cycle = Convert.ToDouble(ObjectParts[1]);
        //                break;

        //            case "contains":  // <http://project-hobbit.eu/resources/debs2017#ObservationGroup_0> <http://www.agtinternational.com/ontologies/I4.0#contains> <http://project-hobbit.eu/resources/debs2017#Observation_0>
        //                StringArray = Components[2].Split(UnderScrollSpliter);
        //                if (ObservationGroup.observationsData == null)
        //                    ObservationGroup.observationsData = new ObservationsData(ObservationGroup.observationGroupNumber);
        //                ObservationGroup.observationsData.ObservationsIndex++;
        //                ObservationGroup.observationsData.Observations[ObservationGroup.observationsData.ObservationsIndex] = Convert.ToInt32(StringArray[1]);
        //                break;

        //            case "observationResult": //<http://project-hobbit.eu/resources/debs2017#Observation_0> <http://purl.oclc.org/NET/ssnx/ssn#observationResult> <http://project-hobbit.eu/resources/debs2017#Output_0>
        //                StringArray = Components[0].Split(UnderScrollSpliter);
        //                index = ObservationGroup.observationsData.ItemSearch(Convert.ToInt32(StringArray[1]), ObservationGroup.observationsData.Observations);
        //                //if (index ==-1) return exception
        //                StringArray = Components[2].Split(UnderScrollSpliter);
        //                ObservationGroup.observationsData.Outputs[index] = Convert.ToInt32(StringArray[1]);
        //                break;

        //            case "observedProperty": //<http://project-hobbit.eu/resources/debs2017#Observation_0> <http://purl.oclc.org/NET/ssnx/ssn#observedProperty> <http://www.agtinternational.com/ontologies/WeidmullerMetadata#_3>
        //                StringArray = Components[0].Split(UnderScrollSpliter);
        //                index = ObservationGroup.observationsData.ItemSearch(Convert.ToInt32(StringArray[1]), ObservationGroup.observationsData.Observations);
        //                //if (index ==-1) return exception //reco
        //                StringArray = Components[2].Split(UnderScrollSpliter);
        //                ObservationGroup.observationsData.ObservedProperties[index] = Convert.ToInt32(StringArray[2]);
        //                break;

        //            case "hasValue": //<http://project-hobbit.eu/resources/debs2017#Output_0> <http://purl.oclc.org/NET/ssnx/ssn#hasValue> <http://project-hobbit.eu/resources/debs2017#Value_0>
        //                StringArray = Components[0].Split(UnderScrollSpliter);
        //                index = ObservationGroup.observationsData.ItemSearch(Convert.ToInt32(StringArray[1]), ObservationGroup.observationsData.Outputs);
        //                //if (index ==-1) return exception
        //                StringArray = Components[2].Split(UnderScrollSpliter);
        //                ObservationGroup.observationsData.ValueLabels[index] = Convert.ToInt32(StringArray[1]);
        //                break;

        //            case "machine": //<http://project-hobbit.eu/resources/debs2017#ObservationGroup_0> <http://www.agtinternational.com/ontologies/I4.0#machine> <wmm:MoldingMachine_57>
        //                StringArray = Components[2].Split(UnderScrollSpliter); //only the machine name needed
        //                ObservationGroup.MachineNumber = Convert.ToInt32(StringArray[1]);
        //                break;

        //            default:
        //                ObjectParts = Components[2].Split(SharpSplitter);
        //                if (ObjectParts[1] == "MoldingMachineObservationGroup") //<http://project-hobbit.eu/resources/debs2017#ObservationGroup_0> <http://www.w3.org/1999/02/22-rdf-syntax-ns#type> <http://www.agtinternational.com/ontologies/I4.0#MoldingMachineObservationGroup>
        //                {
        //                    if (ObservationGroup != null) //filling the ObservationGroup at the first iteration before copying.
        //                        FillQueuesWithObservations();
        //                    //Assumption: ObservationGropu has a unique number.
        //                    StringArray = Components[0].Split(UnderScrollSpliter);
        //                    ObservationGroupNumber = Convert.ToInt32(StringArray[1]);
        //                    ObservationGroup = new ObservationGroup(ObservationGroupNumber);
        //                }
        //                break;
        //        }
        //    }
        //    timeSpan2 = DateTime.Now - start2;
        //    OverallExecution += timeSpan2.TotalMilliseconds;
        //}
        //public void ObservationFromFile(string FilePath)
        //{
        //    string[] Components, SubjectParts, PredicateParts, ObjectParts;
        //    string[] StringArray;
        //    int index;
        //    int ObservationGroupNumber = -1;

        //    StreamReader streamReader = new StreamReader(FilePath);

        //    while (!streamReader.EndOfStream)
        //    {
        //        string Line = streamReader.ReadLine();
        //        Components = Line.Split(TripleSplitter); //split each line(triple) to its three components(Subject - Predicate - Object).
        //        if (Components.Length == 4) //Make sure we have non-empty line and contains indeed three components PLUS a space at the end !!
        //        {
        //            PredicateParts = Components[1].Split(SharpSplitter);
        //            switch (PredicateParts[1])
        //            {
        //                case "valueLiteral": //http://www.agtinternational.com/ontologies/IoTCore#valueLiteral
        //                    SubjectParts = Components[0].Split(SharpSplitter);
        //                    ObjectParts = Components[2].Split(QuotationSplitter);
        //                    if (SubjectParts[1].Substring(0, 5) == "Value") //"9433.11"^^<http://www.w3.org/2001/XMLSchema#float>
        //                    {
        //                        StringArray = SubjectParts[1].Split(UnderScrollSpliter);
        //                        index = ObservationGroup.observationsData.ItemSearch(Convert.ToInt32(StringArray[1]), ObservationGroup.observationsData.ValueLabels);
        //                        //if (index ==-1) return exception
        //                        StringArray = ObjectParts[2].Split(SharpSplitter);//RECO
        //                        if (StringArray[1] != "string")//if (RDF_Processing.MachinesQueues[MachineIndex].IsStatefulDimension(PropertyIndex))
        //                            ObservationGroup.observationsData.Values[index] = Convert.ToDouble(ObjectParts[1]);
        //                    }
        //                    else if (SubjectParts[1].Substring(0, 5) == "Times")//"2017-01-01T01:00:10+01:00"^^<http://www.w3.org/2001/XMLSchema#dateTime>
        //                    {
        //                        ObservationGroup.TimeStampLabel = SubjectParts[1];
        //                        StringArray = ObjectParts[1].Split(DateTimeSplitters);
        //                        DateTime TimeStamp = new DateTime(Convert.ToInt32(StringArray[0]), Convert.ToInt32(StringArray[1]), Convert.ToInt32(StringArray[2]), Convert.ToInt32(StringArray[3]), Convert.ToInt32(StringArray[4]), Convert.ToInt32(StringArray[5]));
        //                        ObservationGroup.TimeStamp = TimeStamp;
        //                    }

        //                    //else if (SubjectParts[1].Substring(0, 5) == "Cycle") //"13"^^<http://www.w3.org/2001/XMLSchema#int>
        //                    //    ObservationGroup.Cycle = Convert.ToDouble(ObjectParts[1]);
        //                    break;

        //                case "contains":  // <http://project-hobbit.eu/resources/debs2017#ObservationGroup_0> <http://www.agtinternational.com/ontologies/I4.0#contains> <http://project-hobbit.eu/resources/debs2017#Observation_0>
        //                    StringArray = Components[2].Split(UnderScrollSpliter);
        //                    if (ObservationGroup.observationsData == null)
        //                        ObservationGroup.observationsData = new ObservationsData(ObservationGroup.observationGroupNumber);
        //                    ObservationGroup.observationsData.ObservationsIndex++;
        //                    ObservationGroup.observationsData.Observations[ObservationGroup.observationsData.ObservationsIndex] = Convert.ToInt32(StringArray[1]);
        //                    break;

        //                case "observationResult": //<http://project-hobbit.eu/resources/debs2017#Observation_0> <http://purl.oclc.org/NET/ssnx/ssn#observationResult> <http://project-hobbit.eu/resources/debs2017#Output_0>
        //                    StringArray = Components[0].Split(UnderScrollSpliter);
        //                    index = ObservationGroup.observationsData.ItemSearch(Convert.ToInt32(StringArray[1]), ObservationGroup.observationsData.Observations);
        //                    //if (index ==-1) return exception
        //                    StringArray = Components[2].Split(UnderScrollSpliter);
        //                    ObservationGroup.observationsData.Outputs[index] = Convert.ToInt32(StringArray[1]);
        //                    break;

        //                case "observedProperty": //<http://project-hobbit.eu/resources/debs2017#Observation_0> <http://purl.oclc.org/NET/ssnx/ssn#observedProperty> <http://www.agtinternational.com/ontologies/WeidmullerMetadata#_3>
        //                    StringArray = Components[0].Split(UnderScrollSpliter);
        //                    index = ObservationGroup.observationsData.ItemSearch(Convert.ToInt32(StringArray[1]), ObservationGroup.observationsData.Observations);
        //                    //if (index ==-1) return exception //reco
        //                    StringArray = Components[2].Split(UnderScrollSpliter);
        //                    ObservationGroup.observationsData.ObservedProperties[index] = Convert.ToInt32(StringArray[1]);
        //                    break;

        //                case "hasValue": //<http://project-hobbit.eu/resources/debs2017#Output_0> <http://purl.oclc.org/NET/ssnx/ssn#hasValue> <http://project-hobbit.eu/resources/debs2017#Value_0>
        //                    StringArray = Components[0].Split(UnderScrollSpliter);
        //                    index = ObservationGroup.observationsData.ItemSearch(Convert.ToInt32(StringArray[1]), ObservationGroup.observationsData.Outputs);
        //                    //if (index ==-1) return exception
        //                    StringArray = Components[2].Split(UnderScrollSpliter);
        //                    ObservationGroup.observationsData.ValueLabels[index] = Convert.ToInt32(StringArray[1]);
        //                    break;

        //                case "machine": //<http://project-hobbit.eu/resources/debs2017#ObservationGroup_0> <http://www.agtinternational.com/ontologies/I4.0#machine> <wmm:MoldingMachine_57>
        //                    StringArray = Components[2].Split(UnderScrollSpliter); //only the machine name needed
        //                    ObservationGroup.MachineNumber = Convert.ToInt32(StringArray[1]);
        //                    break;

        //                default:
        //                    ObjectParts = Components[2].Split(SharpSplitter);
        //                    if (ObjectParts[1] == "MoldingMachineObservationGroup") //<http://project-hobbit.eu/resources/debs2017#ObservationGroup_0> <http://www.w3.org/1999/02/22-rdf-syntax-ns#type> <http://www.agtinternational.com/ontologies/I4.0#MoldingMachineObservationGroup>
        //                    {
        //                        if (ObservationGroup != null) //filling the ObservationGroup at the first iteration before copying.
        //                            FillQueuesWithObservations();
        //                        //Assumption: ObservationGropup has a unique number.
        //                        StringArray = Components[0].Split(UnderScrollSpliter);
        //                        ObservationGroupNumber = Convert.ToInt32(StringArray[1]);
        //                        ObservationGroup = new ObservationGroup(ObservationGroupNumber);
        //                    }
        //                    break;
        //            }
        //        }
        //    }
        //    FillQueuesWithObservations();
        //}
        //#endregion NotRelevant
    }
}
