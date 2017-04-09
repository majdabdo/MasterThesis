using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DEBS17
{
    class k_means
    {
        #region Variables Definition
        private int NumberOfClusters;
        private int NumberOfTransitions;
        private double ProbabilityThreshold;
        private double[] clustersSum;
        private int[] numberOfElementsInEachCluster;
        private double[] oldClusterCenters;
        private List<double> ClusterCenters;
        private int MachineIndex;//RECO: redundant with MachineNumber
        private int PropertyIndex;

        private uint iteration;
        private int WindowSize;
        private int counter;
        private int MachineNumber;
        private int ObservedProperty;
        private string OutputMessage;
        private int MaximalRepetitions;
        private List<QueueEvent> Window;
        private double[] QueueValues;
        private int[] DataPointCluster;

        MarkovModel markovModel;
        #endregion

        public k_means(int numberOfClusters, int MachineIndex, int PropertyIndex)
        {
            this.NumberOfClusters = numberOfClusters;
            Singleton.K = numberOfClusters;
            this.MaximalRepetitions = Singleton.M;
            this.NumberOfTransitions = Singleton.N; //RECO: to be inserted from outside!
            this.WindowSize = Singleton.W;

            this.MachineIndex = MachineIndex;
            this.PropertyIndex = PropertyIndex;
            this.counter = 0;
            OutputMessage = "";
            this.ProbabilityThreshold = Singleton.MachineQueues[MachineIndex].QueuesInfo[PropertyIndex].ProbabilityThreshold;
            this.MachineNumber = Singleton.MachineQueues[MachineIndex].MachineNumber;
            this.ObservedProperty = PropertyIndex;
            Window = new List<QueueEvent>();

            markovModel = new MarkovModel(NumberOfTransitions, ProbabilityThreshold);
        }

        // this function should be called(notified) whenever a new value is inserted into the queue.
        public void NewEvent()
        {
            TimeSpan difference;
            // Dequeue the oldest event in the queue (each machine has queues equals to the number of Properties).
            QueueEvent Event = Singleton.MachineQueues[MachineIndex].Queues[PropertyIndex].Dequeue();
            Window.Add(Event); // Add the event to the window list (at the end)
            // calculate time difference between first event's timestamp and the last one(just added).
            difference = Window.Last().TimeStamp - Window.First().TimeStamp;
            while (difference.TotalSeconds > WindowSize)
            {   // the difference still exceed the window size
                Window.RemoveAt(0); // delete the oldest event from the window.
                difference = Window.Last().TimeStamp - Window.First().TimeStamp; // recalculate the difference of last and first events within the window.
            }
            // Now we have valid window size... we can start Kmeans
            Start();//RECO: should be locked later, so not many incoming events execute at once
        }

        public void Start()
        {
            int StartNode = 0;
            iteration = 0;
            string Message = "";
            initialClusterCentersAssigning();
            do
            {
                iteration++; // number of executions.
                AssignDataPoints();
                UpdateCentroidsCenters();
            }
            while (!IsFinished());

            if (Window.Count >= NumberOfTransitions + 1)
                StartNode = Window.Count - NumberOfTransitions - 1;
            Message = markovModel.BuildNMarkovChainModel(ClusterCenters.Count, DataPointCluster, MachineNumber, Window[StartNode].TimeStampLabel,StartNode, ObservedProperty); // return anomalies information only if found.
            //if (counter == 0) AddParameters();
            //AddInformation(Message); // to include the results of K-means execution(clusters)
            //Singleton.Writer.WriteResultsOnScreen(".");
            //Singleton.Writer.WriteResultsOnFile(Message);
            //Singleton.Writer.WriteResultsOnFile(OutputMessage);
            counter++;
        }

        private void initialClusterCentersAssigning()
        {
            ClusterCenters = new List<double>();// list contains all cluster centers - I used (List) for its flexibility
            QueueValues = new double[Window.Count];

            for (int i = 0; i < Window.Count; i++)
            {
                QueueValues[i] = Window[i].Value; // RECO: copy in better way
                if (!(ClusterCenters.Count == NumberOfClusters) && !(ClusterCenters.Contains(QueueValues[i])))
                    ClusterCenters.Add(QueueValues[i]);// in General should equal to K. But in case  a given window has less than K distinct values then the number of clusters must be equal to the number of distinct values in the window.
            }
            // used to compare with ClusterCenters list to detect any change between two sucessive iterations.
            oldClusterCenters = new double[ClusterCenters.Count()];
            // Contains cluster index whose each data point is belongs to. - initially all cells have (zero) value i.e. they belongs to the first cluster (whose index is 0)
            DataPointCluster = new int[QueueValues.Length];
            //number of elements each cluster has - Useful to calculate the mean value for the next iteration.
            numberOfElementsInEachCluster = new int[ClusterCenters.Count()];
            //total sum of all values each cluster has - Useful to calculate the mean value for the next iteration.
            clustersSum = new double[ClusterCenters.Count()];
            // SO the number of elements are the length of incoming data point List.
            numberOfElementsInEachCluster[0] = QueueValues.Length;
            // and the total sum of the firsr cluster(whose index is 0) is the sum of incoming data points List values.
            clustersSum[0] = QueueValues.Sum();
        }


        private void AssignDataPoints()
        {
            int InitialClusterIndex;
            int tempIndex;
            double Distance;
            double tempDistance;
            double element;
            double ClusterCenterOfElement;

            for (int DataPointIndex = 0; DataPointIndex < QueueValues.Length; DataPointIndex++) //loop over all datapoints
            {
                tempIndex = -1;
                element = QueueValues[DataPointIndex]; // datapoint to be considered in this loop.
                InitialClusterIndex = DataPointCluster[DataPointIndex];
                ClusterCenterOfElement = ClusterCenters[DataPointCluster[DataPointIndex]];//cluster's center whose this element belongs to.
                Distance = Math.Abs(element - ClusterCenterOfElement); // distance between this datapoint and its cluster center - useful to compare with the distances to other cluster centers

                for (int clusterIndex = 0; clusterIndex < ClusterCenters.Count; clusterIndex++) // loop over all cluster centers array
                {
                    tempDistance = Math.Abs(element - ClusterCenters[clusterIndex]); // distance measuring
                    if (tempDistance < Distance)
                    {
                        Distance = tempDistance; // consider this distance as smaller and save it.
                        tempIndex = clusterIndex; // save this cluster center index
                    }
                    // If a data point has the exact same distance to more than one cluster center, it must be assigned to the cluster which has the highest center value.
                    else if ((tempDistance == Distance) && (ClusterCenters[clusterIndex] > ClusterCenterOfElement))
                    {
                        Distance = tempDistance;
                        tempIndex = clusterIndex;
                    }
                }
                if (tempIndex != -1) // there was shorter distance than the initial one. relocate the point to the new centroids
                {
                    DataPointCluster[DataPointIndex] = tempIndex; // change the cluster index whose datapoint is the nearest to.
                    clustersSum[tempIndex] += element; // adds up its value to the total sum of its belonging cluster.
                    clustersSum[InitialClusterIndex] -= element; // subtracts its value from the total sum of the previous belonging cluster.
                    numberOfElementsInEachCluster[tempIndex]++;
                    numberOfElementsInEachCluster[InitialClusterIndex]--;
                }
            }
            //throw new NotImplementedException();
        }

        private void UpdateCentroidsCenters()
        {
            for (int ClusterIndex = 0; ClusterIndex < ClusterCenters.Count; ClusterIndex++)
            {
                oldClusterCenters[ClusterIndex] = ClusterCenters[ClusterIndex];
                if (numberOfElementsInEachCluster[ClusterIndex] == 0) // in order not divide by ZERO
                    ClusterCenters[ClusterIndex] = 0;
                else
                    ClusterCenters[ClusterIndex] = clustersSum[ClusterIndex] / numberOfElementsInEachCluster[ClusterIndex];
            }
            //throw new NotImplementedException();
        }

        private bool IsFinished()
        {
            if (iteration > MaximalRepetitions) return true; //we exceed the Maximal repetition
            if (!ClusterCenters.SequenceEqual(oldClusterCenters)) // Previous Cluster centers are not identical with newer ones
                return false;
            return true; // Are identical !
        }

        #region Rabish

        //public bool CanStart()
        //{
        //    QueuePoint point;
        //    if (QueuePoints.Count == 0) // for the first time only.
        //    {
        //        point = RDF_Processing.MachinesQueues[MachineIndex].Queues[PropertyIndex].Dequeue();
        //        QueuePoints.Add(point);
        //        return false;
        //    }
        //    point = RDF_Processing.MachinesQueues[MachineIndex].Queues[PropertyIndex].Dequeue();
        //    QueuePoints.Add(point);
        //    TimeSpan difference = RDF_Processing.currentTimeStamp - QueuePoints.First().TimeStamp; // difference between first point's timestamp and current application one.
        //    if (difference.TotalSeconds > WindowSize) // has exceed the window size, then we can start executing (minumum one queue point)
        //        return true;

        //    //RDF_Processing.MachinesQueues[MachineIndex].Queues[PropertyIndex].Dequeue();
        //    return false;
        //}
        //public void Start()
        //{
        //    //if (QueuePoints.Count == 0 ) // last survivor(item) was already deleted ... and no values has been added further. Slow die
        //    //    return;
        //    this.isBusy = true;
        //    initialClusterCentersAssigning();
        //    iteration = 0;
        //    string Message = "";
        //    do
        //    {
        //        iteration++; // number of executions.
        //        AssignDataPoints();
        //        UpdateCentroidsCenters();
        //    }
        //    while (!IsFinished());
        //    markovChain = new MarkovChain(numberOfClusters, 2, Singleton.MoldingQueues[MachineIndex].QueuesInfo[PropertyIndex].ProbabilityThreshold);
        //    Message = markovChain.BuildNMarkovChainModel(DataPointCluster, Singleton.MoldingQueues[MachineIndex].MachineNumber, QueuePoints[QueuePoints.Count - 1].TimeStampLabel, Singleton.MoldingQueues[MachineIndex].QueuesInfo[PropertyIndex].ObservedProperty);
        //    QueuePoints.RemoveAt(0);
        //    PrintInformation(Message);
        //    this.isBusy = false;
        //}
        //private void DequeueFirstWPoints()
        //{
        //    QueuePoint point;
        //    DateTime BeginingTS, EndTS;
        //    TimeSpan difference;
        //    while (Singleton.MoldingQueues[MachineIndex].Queues[PropertyIndex].Count == 0) ;// will be deleted - busy waiting
        //    point = Singleton.MoldingQueues[MachineIndex].Queues[PropertyIndex].Peek();
        //    BeginingTS = point.TimeStamp;
        //    do
        //    {
        //        Singleton.MoldingQueues[MachineIndex].Queues[PropertyIndex].Dequeue();
        //        QueuePoints.Add(point);
        //        while (Singleton.MoldingQueues[MachineIndex].Queues[PropertyIndex].Count == 0) ; // will be deleted - busy waiting
        //        point = Singleton.MoldingQueues[MachineIndex].Queues[PropertyIndex].Peek();
        //        EndTS = point.TimeStamp;
        //        difference = EndTS - BeginingTS;
        //    }
        //    while (difference.TotalSeconds <= WindowSize);

        //}

        //private bool TimeWindowIsShifted()
        //{
        //    QueuePoints.RemoveAt(0);
        //    if (Singleton.MoldingQueues[MachineIndex].Queues[PropertyIndex].Count == 0)//RECO: need to be reconsidered
        //        return false;
        //    // IMPORTANT: This list is Updated in each W time units.
        //    DateTime BeginingTS = QueuePoints[0].TimeStamp;
        //    QueuePoint Point = Singleton.MoldingQueues[MachineIndex].Queues[PropertyIndex].Peek();
        //    DateTime EndTS = Point.TimeStamp;
        //    TimeSpan difference = EndTS - BeginingTS;
        //    while (difference.TotalSeconds <= WindowSize)
        //    {
        //        Singleton.MoldingQueues[MachineIndex].Queues[PropertyIndex].Dequeue();
        //        QueuePoints.Add(Point);
        //        if (Singleton.MoldingQueues[MachineIndex].Queues[PropertyIndex].Count == 0)//RECO: need to be reconsidered
        //            return true;
        //        Point = Singleton.MoldingQueues[MachineIndex].Queues[PropertyIndex].Peek();
        //        EndTS = Point.TimeStamp;
        //        difference = EndTS - BeginingTS;
        //    }
        //    return true;

        //}

        #endregion
        private void AddInformation(string Message)
        {
            OutputMessage += string.Format("\r\nExecution_{0} of Machine{1} - ObservedProperty:{2}  -  TimeStamp:{3} time:{4}\r\n\r\n", counter, MachineNumber, ObservedProperty, Window[Window.Count - 1].TimeStampLabel, Window[Window.Count - 1].TimeStamp);

            OutputMessage += string.Format("Cluster centeres are: \r\n");
            for (int index = 0; index < ClusterCenters.Count; index++)
                OutputMessage += string.Format("CC_{0} = ({1})   ", index, ClusterCenters[index]);

            OutputMessage += "\r\n\r\nInput Values:\r\n";
            for (int i = 0; i < QueueValues.Length; i++)
                OutputMessage += string.Format("{0} in Cluster {1} at {2}={3}\r\n", QueueValues[i], DataPointCluster[i], Window[i].TimeStampLabel, Window[i].TimeStamp);

            OutputMessage += string.Format("\r\n{0}------------------------------- ", Message);
        }

        public void AddParameters()
        {
            OutputMessage += "Parameters:\n";
            OutputMessage += string.Format("Window Size: {0}\nNumber Of Transitions(N): {1}\nMaximum Number of Iterations(N): {2}\n", WindowSize, NumberOfTransitions, MaximalRepetitions);
            OutputMessage += string.Format("Number of Clusters: {0}\n\n", NumberOfClusters);
        }
    }
}
