using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DEBS17
{
    class ObservationGroup
    {
        #region Variables Definition
        private DateTime timeStamp;
        private string timeStampLabel;
        //private double cycle;
        private int machineNumber;
        private ObservationsData ObservationsData;
        private int ObservationGroupNumber;
        #endregion

        #region Setters & Getters
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
        //public double Cycle
        //{
        //    get { return cycle; }
        //    set { cycle = value; }
        //}
        public int MachineNumber
        {
            get { return machineNumber; }
            set { machineNumber = value; }
        }
        internal ObservationsData observationsData
        {
            get { return ObservationsData; }
            set { ObservationsData = value; }
        }
        public int observationGroupNumber
        {
            get { return ObservationGroupNumber; }
            set { ObservationGroupNumber = value; }
        }
        #endregion

        public ObservationGroup()
        { }
        public ObservationGroup(int value)
        {
            ObservationGroupNumber = value;
        }
        public void isMainInfoProvided()
        { }

        public string PrintContent()
        {
            string content = "";
            content += "ObservationGroupNumber=" + ObservationGroupNumber + "\n" + TimeStampLabel + "=" + TimeStamp +  "\n" + MachineNumber;
            content += "\n\n------------has observations:------------------\n\n ";
            for (int j = 0; j < 120; j++)
            {
                content += "observed_Property:" + observationsData.ObservedProperties[j] + "\n " + observationsData.Observations[j] + "\n " + observationsData.Outputs[j] + "\n " + observationsData.ValueLabels[j] + "=" + Convert.ToString(observationsData.Values[j]);
                content += "\n\n ";
            }
            return content;
        }
    }
}


