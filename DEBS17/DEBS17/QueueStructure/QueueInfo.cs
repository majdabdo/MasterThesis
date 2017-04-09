using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DEBS17
{
    class QueueInfo
    {
        #region Variables Definition
        private double probabilityThreshold;
        private int numberOfClusters;
        #endregion

        #region Setters & Getters
        public double ProbabilityThreshold
        {
            get { return probabilityThreshold; }
            set { probabilityThreshold = value; }
        }
        public int NumberOfClusters
        {
            get { return numberOfClusters; }
            set { numberOfClusters = value; }
        }
        #endregion

        /// <summary>
        /// Class Constructor set all its Properties to Type.MinValue
        /// </summary>
        public QueueInfo()
        {
            this.ProbabilityThreshold = double.MinValue;
            this.NumberOfClusters = int.MinValue;
        }
    }
}
