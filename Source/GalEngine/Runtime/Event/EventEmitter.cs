using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GalEngine
{
    public abstract class EventEmitter
    {
        private List<EventListener> mEventListeners;

        protected Queue<BaseEvent> mEventQueue;

        public int EventCount { get => mEventQueue.Count; }

        public EventEmitter()
        {
            mEventQueue = new Queue<BaseEvent>();

            mEventListeners = new List<EventListener>();
        }

        public virtual void ForwardEvent(BaseEvent baseEvent)
        {
            mEventQueue.Enqueue(baseEvent);
            
            mEventListeners.ForEach((listener) => listener.mEventQueue.Enqueue(baseEvent));
        }

        public void AddEventListener(EventListener eventListener)
        {
            mEventListeners.Add(eventListener);
        }

        public void RemoveEventListener(EventListener eventListener)
        {
            mEventListeners.Remove(eventListener);
        }

        public BaseEvent GetEvent(bool remove = false)
        {
            if (remove == true) return mEventQueue.Dequeue();
            return mEventQueue.Peek();
        }
    }
}
