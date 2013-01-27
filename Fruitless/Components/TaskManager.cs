using System;
using System.Collections.Generic;

namespace Fruitless.Components {
    public class TaskManager : TimelineComponent {
        public static TaskManager Main {
            get;
            private set;
        }

        // cache of queues we can reuse to avoid garbage
        readonly Stack<TaskQueue> queueCache = new Stack<TaskQueue>();

        // active queues
        readonly List<TaskQueue> queues = new List<TaskQueue>();

        public TaskManager() {
            if (Main == null) {
                Main = this;
            }
        }

        public override void Advance(TimeSpan delta) {
            // update all queues (backwards since we will be modifying the list)
            for (int i = queues.Count - 1; i >= 0; i--) {
                queues[i].Advance(delta);

                // if the queue is complete put it in the cache and remove it from the list
                if (queues[i].IsComplete) {
                    queueCache.Push(queues[i]);
                    queues.RemoveAt(i);
                }
            }
        }

        public void StopAllTaskQueues() {
            // stop each queue and put it into the cache
            foreach (var q in queues) {
                q.Stop();
                queueCache.Push(q);
            }

            // clear our active list
            queues.Clear();
        }

        public TaskQueue WaitFor(float seconds) {
            // find a cached queue or make a new one
            TaskQueue queue = queueCache.Count > 0 ? queueCache.Pop() : new TaskQueue();

            // store the queue in our active list
            queues.Add(queue);

            // queue up a wait action and hand it back to the caller
            return queue.ThenWaitFor(seconds);
        }

        public TaskQueue WaitUntil(UntilTaskPredicate condition) {
            // find a cached queue or make a new one
            TaskQueue queue = queueCache.Count > 0 ? queueCache.Pop() : new TaskQueue();

            // store the queue in our active list
            queues.Add(queue);

            // queue up a conditional action and hand it back to the caller
            return queue.ThenWaitUntil(condition);
        }
    }
}
