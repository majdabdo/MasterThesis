using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DEBS17
{
    class ObservationsData
    {
        #region Variables Definition
        private int[] observations;//120 observation
        private int[] outputs;
        private int[] valueLabels;
        private double[] values;

        private int[] tempOutputs;
        private int[] tempValueLabels;
        private double[] tempValues;

        private int[] observedProperties;
        private int observationGroupIndex;
        private int observationsIndex = -1;
        private int tempObservationsIndex =-1;

        #endregion

        #region Setters & Getters
        public int[] Observations
        {
            get { return observations; }
            set { observations = value; }
        }
        public int[] Outputs
        {
            get { return outputs; }
            set { outputs = value; }
        }
        public int[] TempOutputs
        {
            get { return tempOutputs; }
            set { tempOutputs = value; }
        }
        public int[] ValueLabels
        {
            get { return valueLabels; }
            set { valueLabels = value; }
        }
        public int[] TempValueLabels
        {
            get { return tempValueLabels; }
            set { tempValueLabels = value; }
        }
        public double[] Values
        {
            get { return values; }
            set { values = value; }
        }
        public double[] TempValues
        {
            get { return tempValues; }
            set { tempValues = value; }
        } 
        public int[] ObservedProperties
        {
            get { return observedProperties; }
            set { observedProperties = value; }
        }
        public int ObservationGroupIndex
        {
            get { return observationGroupIndex; }
            set { observationGroupIndex = value; }
        }
        public int ObservationsIndex
        {
            get { return observationsIndex; }
            set { observationsIndex = value; }
        }

        public int TempObservationsIndex
        {
            get { return tempObservationsIndex; }
            set { tempObservationsIndex = value; }
        }
        #endregion

        /// <summary>
        /// Class Constructor
        /// </summary>
        /// <param name="value">ObservationGroupIndex</param>
        public ObservationsData()
        {
            ObservedProperties = new int[120];
            Observations = new int[120];
            Outputs = new int[120];
            ValueLabels = new int[120];
            Values = new double[120];

            TempOutputs = new int[120];
            TempValueLabels = new int[120];
            TempValues = new double[120];

            for (int i = 0; i < Values.Length; i++)
            {
                ObservedProperties[i] = int.MinValue;
                Observations[i] = int.MinValue;
                Outputs[i] = int.MinValue;
                ValueLabels[i] = int.MinValue;
                Values[i] = double.MinValue;

                TempOutputs[i] = int.MinValue;
                TempValueLabels[i] = int.MinValue;
                TempValues[i] = double.MinValue;
            }

        }
        public int ItemSearch(int item, int[] array)
        {
            int i;
            //if (array[observationsIndex] == item) return observationsIndex;
            for (i = 0; i < array.Length; i++)
                if (item == array[i])
                    return i;
            return -1;
        }
    }
}
