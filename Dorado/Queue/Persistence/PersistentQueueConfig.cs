using Dorado.Utils;
using System;
using System.Xml.Serialization;

namespace Dorado.Queue.Persistence
{
    [XmlRoot("PersistentQueue")]
    public class PersistentQueueConfig
    {
        public static string PersistenceRootPath
        {
            get
            {
                string queueRootPath = AppDomain.CurrentDomain.BaseDirectory + "QueueData\\";
                IOUtility.CreateDirectory(queueRootPath);
                return queueRootPath;
            }
        }
    }
}