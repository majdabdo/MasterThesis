using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DEBS17
{
    class MarkovModel
    {
        #region Variables Definition
        private int NumberOfClusters;
        private int NumberOfTransitions;
        private double ProbabilityThreshold;
        private double AlertProbability;
        private int[] DataPointCluster;
        int AnomalyIndex;
        int StartNode;
        private double[,] TransitionMatrix;
        //double[,] NTransitionMatrix;
        private int[] SumOfOutgoingArrowsPerState;
        private string MarkovChainMessage;
        #endregion

        public MarkovModel(int NumberOfTransitions, double ProbabilityThreshold)
        {
            this.NumberOfTransitions = NumberOfTransitions;
            this.ProbabilityThreshold = ProbabilityThreshold;
            MarkovChainMessage = "";


        }

        public string BuildNMarkovChainModel(int NumberOfClusters, int[] DataPointCluster, int MachineNumber, string Timestamp,int StartNode, int ObservedProperty)
        {
            this.NumberOfClusters = NumberOfClusters;
            TransitionMatrix = new double[NumberOfClusters, NumberOfClusters]; // Probability of transitions during last W, square Matrix with the size of the number of clusters
            SumOfOutgoingArrowsPerState = new int[NumberOfClusters]; // to maitain how many outgoing arrows each state has, useful to calculate the Probability of each cell faster.
            MarkovChainMessage = "";
            this.StartNode = StartNode;

            this.DataPointCluster = new int[DataPointCluster.Count()];
            this.DataPointCluster = DataPointCluster; // array of data point's cluster of last W time window, Kmeans worked on.
            CalcuteTransitionMatrix();
            if (RomanAnomalyFound())
            {
                Alert(MachineNumber, Timestamp, ObservedProperty);
                return MarkovChainMessage;
            }
            return "";
        }

        /// <summary>
        /// Calculate the Transition Matrix which 
        /// which consists for each cell:  the number of outgoing arrows from starting cluster(row index) to the desitnation cluster / sum of all outgoing arrows from the starting cluster.
        /// </summary>
        private void CalcuteTransitionMatrix()
        {
            for (int index = 1; index < DataPointCluster.Length; index++)
            {
                TransitionMatrix[DataPointCluster[index - 1], DataPointCluster[index]]++;
                SumOfOutgoingArrowsPerState[DataPointCluster[index - 1]]++;
                //TokenPosition = DataPointCluster[index]; // marker to the last cluster state we have reached until now.
            }
            MarkovChainMessage += "Transition Matrix:\n";
            for (int Row = 0; Row < TransitionMatrix.GetLength(0); Row++)
            {
                for (int Column = 0; Column < TransitionMatrix.GetLength(1); Column++)
                {
                    if (TransitionMatrix[Row, Column] != 0)
                    {
                        TransitionMatrix[Row, Column] = TransitionMatrix[Row, Column] / SumOfOutgoingArrowsPerState[Row];
                        MarkovChainMessage += Convert.ToString(TransitionMatrix[Row, Column]) + " ";
                    }
                    else MarkovChainMessage += "0 ";
                }
                MarkovChainMessage += "\n";
            }

        }
        private bool AnomalyFound()
        {
            int StartNode = 0;
            AlertProbability = 1;
            bool FirstIteration = true;
            while (StartNode + 1 < DataPointCluster.Length) //  No transition is possible from the last event of the array!
            {
                if (FirstIteration)
                {
                    FirstIteration = false;
                    for (int Index = StartNode; Index < StartNode + NumberOfTransitions; Index++) // doing up to N transitions from StartNode index
                    {
                        if (Index + 1 == DataPointCluster.Length) break;// we reached the end of the array before doing N transitions
                        AlertProbability *= TransitionMatrix[DataPointCluster[Index], DataPointCluster[Index + 1]];
                        if (AlertProbability < ProbabilityThreshold)
                        {
                            AnomalyIndex = StartNode;
                            return true;
                        }
                    }
                }
                else if (StartNode + NumberOfTransitions < DataPointCluster.Length) //not the first iteration and start node can walk N transitions onwards
                {
                    // divide(delete) by the first transition of the previous chain.
                    AlertProbability /= TransitionMatrix[DataPointCluster[StartNode - 1], DataPointCluster[StartNode]];
                    //multiply(add) transition at the end of the previous chain.
                    AlertProbability *= TransitionMatrix[DataPointCluster[StartNode - 1 + NumberOfTransitions], DataPointCluster[StartNode + NumberOfTransitions]];
                    if (AlertProbability < ProbabilityThreshold)
                    {
                        AnomalyIndex = StartNode;
                        return true;
                    }
                }
                StartNode++;
            }
            return false;
        }

        private bool RomanAnomalyFound()
        {
            //consider only last N transitions of the window
            AlertProbability = 1;
            for (int Index = StartNode; Index < StartNode + NumberOfTransitions; Index++) // doing up to N transitions from StartNode index
            {
                if (Index + 1 == DataPointCluster.Length) break;// we reached the end of the array before doing N transitions
                AlertProbability *= TransitionMatrix[DataPointCluster[Index], DataPointCluster[Index + 1]];
                if (AlertProbability < ProbabilityThreshold)
                {
                    AnomalyIndex = StartNode;
                    return true;
                }
            }
            return false;
        }


        private void Alert(int MachineNumber, string Timestamp, int ObservedProperty)
        {

            string[] AnomalyInfo = new string[] { Convert.ToString(MachineNumber), Convert.ToString(MachineNumber) + "_" + Convert.ToString(ObservedProperty), Timestamp, Convert.ToString(AlertProbability) };
            Singleton.Writer.OutputAnomaly(AnomalyInfo);
            //MarkovChainMessage += "\nANOMALIES:\n";
            //MarkovChainMessage += string.Format("MoldingMachine_{0} ObservedProperty_{1} {2} AbnormalSequenceProbability={3} at SequenceIndex={4} \n", MachineNumber, ObservedProperty, Timestamp, Convert.ToString(AlertProbability), AnomalyIndex);
        }

        private bool AnomalyFound_OLD()
        {
            int StartNode = 0;
            AlertProbability = 1;

            while (StartNode + 1 < DataPointCluster.Length) //  No transition is possible from the last event of the array!
            {

                for (int Index = StartNode; Index < StartNode + NumberOfTransitions; Index++) // doing up to N transitions from StartNode index
                {
                    if (Index + 1 == DataPointCluster.Length) // we reach the end of the array before doing N transitions
                        break;

                    AlertProbability *= TransitionMatrix[DataPointCluster[Index], DataPointCluster[Index + 1]];
                    if (AlertProbability < ProbabilityThreshold)
                        return true;
                }

                AlertProbability = 1;
                StartNode++;
            }
            return false;
        }

        #region old_understanding
        //private void OLD_CalcuteNTransitionMatrix()
        //{
        //    for (int i = 0; i < NumberOfTransitions; i++) // multiply the matrix by its self N times.
        //        MatrixSquare();
        //}

        ///// <summary>
        ///// Schoolbook matrix multiplication O(n^3) complexity.
        ///// </summary>
        //private void MatrixSquare()
        //{
        //    for (int i = 0; i < NTransitionMatrix.GetLength(0); i++)
        //    {
        //        for (int j = 0; j < NTransitionMatrix.GetLength(1); j++)
        //        {
        //            NTransitionMatrix[i, j] = 0;
        //            for (int k = 0; k < TransitionMatrix.GetLength(1); k++)
        //                NTransitionMatrix[i, j] = NTransitionMatrix[i, j] + TransitionMatrix[i, k] * TransitionMatrix[k, j];
        //        }
        //    }
        //    TransitionMatrix = (double[,])NTransitionMatrix.Clone();
        //}

        ///// <summary>
        ///// To output the anomaly detected in case Ntransition Matrix has a value less then ProbabilityThreshold
        ///// </summary>
        ///// <param name="MachineName"></param>
        ///// <param name="Timestamp">e.g. TimeStamp_3</param>
        ///// <param name="ObservedProperty">only the number of the property(dimension)</param>
        //private string OLD_AnomalyDetected(string MachineName, string Timestamp, int ObservedProperty)
        //{
        //    string AnomalyMessage = "";
        //    for (int Column = 0; Column < NTransitionMatrix.GetLength(1); Column++)
        //        if (NTransitionMatrix[TokenPosition, Column] < ProbabilityThreshold)
        //            AnomalyMessage += MachineName + " ObservedProperty_" + ObservedProperty + " " + Timestamp + " AbnormalSequenceProbability=" + Convert.ToString(NTransitionMatrix[TokenPosition, Column] + "\n");
        //    return AnomalyMessage;


        //    //for (int Row = 0; Row < TransitionMatrix.GetLength(0); Row++)
        //    //    for (int Column = 0; Column < TransitionMatrix.GetLength(1); Column++)
        //    //        if (NTransitionMatrix[Row, Column] < ProbabilityThreshold)
        //    //            Console.WriteLine(MachineName + "   ObservedProperty_" + ObservedProperty + "    " + Timestamp + "hasProbabilityOfObservedAbnormalSequence" + Convert.ToString(NTransitionMatrix[Row, Column]));

        //    //throw new NotImplementedException();
        //}
        #endregion
    }
}
