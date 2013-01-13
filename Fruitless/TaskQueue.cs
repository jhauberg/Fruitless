using System;
using System.Collections.Generic;

namespace Fruitless {
    public delegate bool UntilTaskPredicate(float elapsedTime);

    public class TaskQueue : IAdvanceable<TimeSpan> {
        // manage each step of the task queue
        enum StepType {
            Action,
            TimeWait,
            ConditionWait
        }

        class Step {
            // type of step and timer for tracking how long we've been running the step
            public StepType Type { get; set; }
            public float Timer { get; set; }

            // one of these three will matter depending on the step type
            public Action Action { get; set; }
            public float Length { get; set; }
            public UntilTaskPredicate Condition { get; set; }
        }

        // cache of steps we can reuse to avoid garbage
        static readonly Stack<Step> stepCache = new Stack<Step>();

        // helper for getting a new step
        static Step NewStep() {
            Step step = stepCache.Count > 0 ?
                stepCache.Pop() :
                new Step();

            step.Timer = 0f;

            return step;
        }

        // steps for this queue
        readonly Queue<Step> steps = new Queue<Step>();

        // whether or not the queue is complete
        public bool IsComplete {
            get {
                return steps.Count == 0;
            }
        }

        public void Advance(TimeSpan delta) {
            // if we're out of steps, we can't update anything
            if (steps.Count == 0) {
                return;
            }

            // see the next step and update its timer
            Step s = steps.Peek();

            s.Timer += (float)delta.TotalSeconds;

            // handle the step
            switch (s.Type) {
                default:
                    break;

                // actions just get invoked
                case StepType.Action: {
                        s.Action();
                        s.Action = null;
                        stepCache.Push(steps.Dequeue());
                    }
                    break;

                // timers check for a specific time before moving on
                case StepType.TimeWait: {
                        if (s.Timer >= s.Length) {
                            stepCache.Push(steps.Dequeue());
                        }
                    }
                    break;

                // conditions require a delegate to return true to move on
                case StepType.ConditionWait: {
                        if (s.Condition(s.Timer)) {
                            s.Condition = null;

                            stepCache.Push(steps.Dequeue());
                        }
                    }
                    break;
            }
        }

        // adds an action to the queue
        public TaskQueue Then(Action action) {
            Step s = NewStep();

            s.Type = StepType.Action;
            s.Action = action;

            steps.Enqueue(s);

            return this;
        }

        // adds a timer to the queue
        public TaskQueue ThenWaitFor(float seconds) {
            Step s = NewStep();

            s.Type = StepType.TimeWait;
            s.Length = seconds;

            steps.Enqueue(s);

            return this;
        }

        // adds a conditional action to the queue
        public TaskQueue ThenWaitUntil(UntilTaskPredicate condition) {
            Step s = NewStep();

            s.Type = StepType.ConditionWait;
            s.Condition = condition;

            steps.Enqueue(s);

            return this;
        }

        // stops a task queue
        public void Stop() {
            while (steps.Count > 0) {
                Step s = steps.Dequeue();

                s.Action = null;
                s.Condition = null;

                stepCache.Push(s);
            }
        }
    }
}
