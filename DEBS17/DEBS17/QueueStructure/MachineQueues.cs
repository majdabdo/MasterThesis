using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DEBS17
{
    class MachineQueues
    {
        #region Variables Definition
        public readonly int MaxPropertiesNumber = 120;
        private int machineNumber; 
        private QueueInfo[] queuesInfo;
        private Queue<QueueEvent>[] queues;
        private k_means[] kmeanInstances;
        private List<int> statfulProperties;
        #endregion

        #region Setters & Getters
        public int MachineNumber
        {
            get { return machineNumber; }
            set { machineNumber = value; }
        }
        internal QueueInfo[] QueuesInfo
        {
            get { return queuesInfo; }
            set { queuesInfo = value; }
        }
        internal Queue<QueueEvent>[] Queues
        {
            get { return queues; }
            set { queues = value; }
        }
        internal k_means[] KmeanInstances
        {
            get { return kmeanInstances; }
            set { kmeanInstances = value; }
        }
        public List<int> StatfulProperties
        {
            get { return statfulProperties; }
            set { statfulProperties = value; }
        }
        #endregion

        /// <summary>
        /// Class Constructor
        /// </summary>
        /// <param name="value">Machine Name</param>
        public MachineQueues(int value)
        {
            MachineNumber = value;
            QueuesInfo = new QueueInfo[MaxPropertiesNumber];
            Queues = new Queue<QueueEvent>[MaxPropertiesNumber]; //RECO: Onchanged Queue notification
            KmeanInstances = new k_means[MaxPropertiesNumber]; //array contains at Max 120 of K-mean instances
            StatfulProperties = new List<int>(); // List contains stateful properties only for each machine

        }
        public bool IsStatefulDimension(int PropertyIndex)
        {
            if ((this.QueuesInfo[PropertyIndex] !=null) && (this.QueuesInfo[PropertyIndex].NumberOfClusters != int.MinValue) && (this.QueuesInfo[PropertyIndex].ProbabilityThreshold != double.MinValue))
                return true;
            return false;
                

        }
        public void AddThresholdValue(int PropertyNumber, double ThresholdValue)
        {
            if (QueuesInfo[PropertyNumber] == null)
                QueuesInfo[PropertyNumber] = new QueueInfo();
            QueuesInfo[PropertyNumber].ProbabilityThreshold = ThresholdValue;
            if (!StatfulProperties.Contains(PropertyNumber))
                statfulProperties.Add(PropertyNumber);

        }
        public void AddQueuePoint(int PropertyIndex, double value, DateTime TimeStamp, string TimeStampLabel)
        {
            if (Queues[PropertyIndex] == null)
                Queues[PropertyIndex] = new Queue<QueueEvent>();
            
            Queues[PropertyIndex].Enqueue(new QueueEvent(value,TimeStamp,TimeStampLabel));
        }
        public void AddNumberOfClustersValue(int PropertyNumber, int NumberOfClusters)
        {
            if (QueuesInfo[PropertyNumber] == null)
                QueuesInfo[PropertyNumber] = new QueueInfo();
            QueuesInfo[PropertyNumber].NumberOfClusters = NumberOfClusters;
            if (!StatfulProperties.Contains(PropertyNumber))
                statfulProperties.Add(PropertyNumber);

        }
    }

}
