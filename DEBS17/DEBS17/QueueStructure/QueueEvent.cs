using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DEBS17
{
    class QueueEvent
    {
        #region Variables Definition
        private double value;
        private DateTime timeStamp;
        private string timeStampLabel; // needed for anomlay's output
        #endregion

        #region Setters & Getters
        public double Value
        {
            get { return this.value; }
            set { this.value = value; }
        }
        public DateTime TimeStamp
        {
            get { return timeStamp; }
            set { timeStamp = value; }
        }
        public string TimeStampLabel
        {
            get { return timeStampLabel; }
            set { timeStampLabel = value; }
        }
        #endregion

        /// <summary>
        /// Class Constructor with no input parameters
        /// </summary>
        public QueueEvent()
        { }

        /// <summary>
        /// Class Constructor
        /// </summary>
        /// <param name="value">Senser Value</param>
        /// <param name="TimeStamp">TimeStamp assosiated with the value</param>
        /// <param name="TimeStampLabel">TimeStamp's string Label assosiated with the value</param>
        public QueueEvent(double value, DateTime TimeStamp, string TimeStampLabel)
        {
            this.Value = value;
            this.TimeStamp = TimeStamp;
            this.TimeStampLabel = TimeStampLabel;
        }
    }
}
