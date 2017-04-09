using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DEBS17
{
    public sealed class Singleton
    {
        private Singleton()
        { }

        private static readonly List<MachineQueues> machineQueues = new List<MachineQueues>();
        private static readonly Writer writer = new Writer();
        private static int m;
        private static int n;
        private static int w;
        private static int k;

        #region Setters & getters
        internal static int M
        {
            get { return Singleton.m; }
            set { Singleton.m = value; }
        }
        internal static int N
        {
            get { return Singleton.n; }
            set { Singleton.n = value; }
        }
        public static int W
        {
            get { return Singleton.w; }
            set { Singleton.w = value; }
        }
        public static int K
        {
            get { return Singleton.k; }
            set { Singleton.k = value; }
        }
        #endregion
        internal static Writer Writer
        {
            get { return Singleton.writer; }
        }
        internal static List<MachineQueues> MachineQueues // instead of public
        {
            get { return Singleton.machineQueues; }
        }


    }

    //    private static volatile Singleton instance;
    //private static object syncRoot = new Object();

    //private Singleton() {}

    //public static Singleton Instance
    //{
    //   get 
    //   {
    //      if (instance == null) 
    //      {
    //         lock (syncRoot) 
    //         {
    //            if (instance == null) 
    //               instance = new Singleton();
    //         }
    //      }

    //      return instance;
    //   }
    //}
}
