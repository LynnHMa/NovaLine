using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NovaLine.Utils
{
    public static class TaskExt
    {
        public static Task RunAll(this IEnumerable<Task> tasks)
        {
            if (tasks == null) throw new ArgumentNullException(nameof(tasks));

            var taskList = tasks.ToList();

            startAll(taskList);

            return Task.WhenAll(taskList);
        }
        public static Task RunAny(this IEnumerable<Task> tasks)
        {
            if (tasks == null) throw new ArgumentNullException(nameof(tasks));

            var taskList = tasks.ToList();

            startAll(taskList);

            return Task.WhenAny(taskList);
        }
        private static void startAll(List<Task> taskList)
        {
            foreach (var task in taskList)
            {
                if (task.Status == TaskStatus.Created)
                {
                    task.Start(TaskScheduler.Default);
                }
            }
        }
    }
}
