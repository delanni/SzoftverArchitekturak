using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PocsKft.Models
{
    public class LockManager
    {
        private static volatile LockManager instance;
        private static object syncRoot = new Object();
        private LockManager() { }
        public static LockManager LockManagerInstance
        {
            get
            {
                lock (syncRoot)
                {
                    if (instance == null)
                        instance = new LockManager();
                }
                return instance;
            }
        }
    }
}