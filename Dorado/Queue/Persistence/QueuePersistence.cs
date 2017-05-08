using System.Collections.Generic;
using System.Threading;

namespace Dorado.Queue.Persistence
{
    public abstract class QueuePersistence<T> where T : class, new()
    {
        protected int count;

        public string PersistPath
        {
            get;
            private set;
        }

        public int Count
        {
            get
            {
                return this.count;
            }
        }

        public QueuePersistence(string persistPath)
        {
            Guard.ArgumentNotNull(persistPath);
            this.PersistPath = persistPath;
            this.count = this.Init(persistPath);
        }

        protected abstract int Init(string persistPath);

        public void Save(PersistentQueueItem<T> item)
        {
            this.MultiSave(new PersistentQueueItem<T>[]
            {
                item
            });
        }

        protected abstract void MultiSaveImpl(params PersistentQueueItem<T>[] items);

        public void MultiSave(params PersistentQueueItem<T>[] items)
        {
            Guard.ArgumentNotNull<PersistentQueueItem<T>[]>(items);
            Guard.ArgumentPositive(items.Length);
            Guard.ArgumentValuesNotNull<PersistentQueueItem<T>>(items);
            this.MultiSaveImpl(items);
            Interlocked.Add(ref this.count, items.Length);
        }

        public PersistentQueueItem<T> Load()
        {
            List<PersistentQueueItem<T>> items = this.MultiLoad(1);
            if (items.Count <= 0)
            {
                return null;
            }
            return items[0];
        }

        protected abstract List<PersistentQueueItem<T>> MultiLoadImpl(int batch);

        public List<PersistentQueueItem<T>> MultiLoad(int batch)
        {
            Guard.ArgumentPositive(batch);
            List<PersistentQueueItem<T>> list = this.MultiLoadImpl(batch);
            if (list.Count > 0)
            {
                Interlocked.Add(ref this.count, -list.Count);
            }
            return list;
        }

        protected abstract void RemoveImpl(PersistentQueueItem<T> item);

        public void Remove(PersistentQueueItem<T> item)
        {
            Guard.ArgumentNotNull<PersistentQueueItem<T>>(item);
            Guard.ArgumentPositive(item.Id);
            this.RemoveImpl(item);
        }

        protected abstract void FailImpl(PersistentQueueItem<T> item);

        public void Fail(PersistentQueueItem<T> item)
        {
            Guard.ArgumentNotNull<PersistentQueueItem<T>>(item);
            Guard.ArgumentPositive(item.Id);
            this.FailImpl(item);
            Interlocked.Increment(ref this.count);
        }

        protected abstract void DiscardImpl(PersistentQueueItem<T> item);

        public void Discard(PersistentQueueItem<T> item)
        {
            Guard.ArgumentNotNull<PersistentQueueItem<T>>(item);
            Guard.ArgumentPositive(item.Id);
            this.DiscardImpl(item);
        }

        protected abstract void PurgeImpl();

        public void Purge()
        {
            this.PurgeImpl();
            Interlocked.Exchange(ref this.count, 0);
        }
    }
}