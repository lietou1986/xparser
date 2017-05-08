using Newtonsoft.Json;
using System;
using System.Data;
using System.IO;

namespace Dorado.Queue.Persistence
{
    public class PersistentQueueItem<T> where T : class, new()
    {
        private T payload;
        private static Type payloadType = typeof(T);
        private static JsonSerializer jsonSerializer = new JsonSerializer();

        public int Id
        {
            get;
            set;
        }

        public DateTime EnqueueTime
        {
            get;
            set;
        }

        public long Priority
        {
            get;
            set;
        }

        public int Try
        {
            get;
            set;
        }

        public T Payload
        {
            get
            {
                return this.payload;
            }
            set
            {
                Guard.ArgumentNotNull<T>(value);
                this.payload = value;
            }
        }

        internal PersistentQueueItem()
        {
        }

        public PersistentQueueItem(T payload)
        {
            this.Payload = payload;
            this.EnqueueTime = DateTime.Now;
            this.Priority = this.EnqueueTime.Ticks;
            this.Try = 0;
        }

        public string PayloadToJson()
        {
            string result;
            using (StringWriter writer = new StringWriter())
            {
                PersistentQueueItem<T>.jsonSerializer.Serialize(writer, this.Payload);
                result = writer.ToString();
            }
            return result;
        }

        public static PersistentQueueItem<T> FromDataReader(IDataReader reader)
        {
            Guard.ArgumentNotNull<IDataReader>(reader);
            return new PersistentQueueItem<T>
            {
                Id = (int)reader["Id"],
                EnqueueTime = (DateTime)reader["EnqueueTime"],
                Priority = (long)reader["Priority"],
                Try = (int)reader["Try"],
                Payload = PersistentQueueItem<T>.PayloadFromJson((string)reader["Payload"])
            };
        }

        private static T PayloadFromJson(string json)
        {
            T result;
            using (StringReader reader = new StringReader(json))
            {
                result = (T)PersistentQueueItem<T>.jsonSerializer.Deserialize(reader, PersistentQueueItem<T>.payloadType);
            }
            return result;
        }
    }
}