using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VDS.RDF;
using VDS.RDF.Ontology;
using VDS.RDF.Parsing;
using VDS.RDF.Parsing.Handlers;

namespace DEBS17
{
    class InputStreamProcessing
    {
        private Dictionary<int, ObservationGroup> ObservationGroups;
        public InputStreamProcessing()
        {
            ObservationGroups = new Dictionary<int, ObservationGroup>(); // instance to save all Molding_Machines information
        }

        public void ObservationStreamReading(string FilePath)
        {
            KeyValuePair<int, ObservationGroup> LastObservationGroupInstance;

            string[] Components, SubjectParts, PredicateParts, ObjectParts;
            string[] StringArray;
            int index;
            int LastObservationGroupNumber = -1;
            //IGraph g = new Graph();

            StreamReader streamReader = new StreamReader(FilePath);
            char TripleSplitter = ' '; // split each line(triple) to its nodes by detecting space_charachter.
            char[] NodeSplitters = { '>', '#', '<', '"' };
            char ColumnSplitter = ':';
            char UnderScrollSpliter = '_';

            while (!streamReader.EndOfStream)
            {
                string Line = streamReader.ReadLine();
                Components = Line.Split(TripleSplitter); //split each line(triple) to its three components(Subject - Predicate - Object).
                if (Components.Length == 4) //Make sure we have non-empty line and contains indeed three components PLUS a space at the end !!
                {
                    SubjectParts = Components[0].Split(NodeSplitters);
                    PredicateParts = Components[1].Split(NodeSplitters);
                    ObjectParts = Components[2].Split(NodeSplitters);
                    if (PredicateParts[2] == "valueLiteral")//debs:Timestamp_? IoTCore:valueLiteral "2016-07-18T23:59:58"^^xsd:dateTime.
                    {
                        if (SubjectParts[2].Substring(0, 6) == "Value_")
                        {
                            LastObservationGroupInstance = ObservationGroups.ElementAt(LastObservationGroupNumber);
                            StringArray = SubjectParts[2].Split(UnderScrollSpliter);
                            index = LastObservationGroupInstance.Value.observationsData.ItemSearch(Convert.ToInt32(StringArray[1]), ref LastObservationGroupInstance.Value.observationsData.ValueLabels);
                            //if (index ==-1) return exception
                            if (ObjectParts[4] != "string")
                                LastObservationGroupInstance.Value.observationsData.Values[index] = Convert.ToDouble(ObjectParts[1]);
                        }
                        else if (SubjectParts[2].Substring(0, 6) == "Timest")
                        {
                            LastObservationGroupInstance = ObservationGroups.ElementAt(LastObservationGroupNumber);
                            LastObservationGroupInstance.Value.TimeStampLabel = SubjectParts[2];
                            LastObservationGroupInstance.Value.TimeStamp = ObjectParts[1];
                        }

                        else if (SubjectParts[2].Substring(0, 6) == "Cycle_")
                        {
                            LastObservationGroupInstance = ObservationGroups.ElementAt(LastObservationGroupNumber);
                            LastObservationGroupInstance.Value.Cycle = Convert.ToDouble(ObjectParts[1]);
                        }
                    }

                    else if (PredicateParts[2] == "contains")
                    {
                        LastObservationGroupInstance = ObservationGroups.ElementAt(LastObservationGroupNumber);
                        StringArray = ObjectParts[2].Split(UnderScrollSpliter);
                        if (LastObservationGroupInstance.Value.observationsData == null)
                            LastObservationGroupInstance.Value.observationsData = new ObservationsData(LastObservationGroupInstance.Value.observationGroupNumber);
                        LastObservationGroupInstance.Value.observationsData.ObservationsIndex++;
                        LastObservationGroupInstance.Value.observationsData.Observations[LastObservationGroupInstance.Value.observationsData.ObservationsIndex] = Convert.ToInt32(StringArray[1]);
                    }

                    else if (PredicateParts[2] == "observationResult")
                    {
                        LastObservationGroupInstance = ObservationGroups.ElementAt(LastObservationGroupNumber);
                        StringArray = SubjectParts[2].Split(UnderScrollSpliter);
                        index = LastObservationGroupInstance.Value.observationsData.ItemSearch(Convert.ToInt32(StringArray[1]), ref LastObservationGroupInstance.Value.observationsData.Observations);
                        //if (index ==-1) return exception
                        StringArray = ObjectParts[2].Split(UnderScrollSpliter);
                        LastObservationGroupInstance.Value.observationsData.Outputs[index] = Convert.ToInt32(StringArray[1]);
                    }
                    else if (PredicateParts[2] == "observedProperty")
                    {
                        LastObservationGroupInstance = ObservationGroups.ElementAt(LastObservationGroupNumber);
                        StringArray = SubjectParts[2].Split(UnderScrollSpliter);
                        index = LastObservationGroupInstance.Value.observationsData.ItemSearch(Convert.ToInt32(StringArray[1]), ref LastObservationGroupInstance.Value.observationsData.Observations);
                        //if (index ==-1) return exception
                        StringArray = ObjectParts[2].Split(UnderScrollSpliter);
                        LastObservationGroupInstance.Value.observationsData.ObservedProperties[index] = Convert.ToInt32(StringArray[1]);
                    }
                    else if (PredicateParts[2] == "hasValue")
                    {
                        LastObservationGroupInstance = ObservationGroups.ElementAt(LastObservationGroupNumber);
                        StringArray = SubjectParts[2].Split(UnderScrollSpliter);
                        index = LastObservationGroupInstance.Value.observationsData.ItemSearch(Convert.ToInt32(StringArray[1]), ref LastObservationGroupInstance.Value.observationsData.Outputs);
                        //if (index ==-1) return exception
                        StringArray = ObjectParts[2].Split(UnderScrollSpliter);
                        LastObservationGroupInstance.Value.observationsData.ValueLabels[index] = Convert.ToInt32(StringArray[1]);
                    }
                    else if (ObjectParts[2] == "MoldingMachineObservationGroup") //debs:ObservationGroup_? rdf:type i40:MoldingMachineObservationGroup.
                    {
                        StringArray = SubjectParts[2].Split(UnderScrollSpliter);
                        LastObservationGroupNumber = Convert.ToInt32(StringArray[1]);
                        ObservationGroups.Add(LastObservationGroupNumber, new ObservationGroup(LastObservationGroupNumber));

                    }
                    else if (PredicateParts[2] == "machine")
                    {
                        LastObservationGroupInstance = ObservationGroups.ElementAt(LastObservationGroupNumber);
                        StringArray = ObjectParts[1].Split(ColumnSplitter); //only the machine name needed
                        LastObservationGroupInstance.Value.MoldingMachine = StringArray[1];
                    }

                }
            }
        }
    }
}
