using System;
using System.Threading.Tasks;
using UnityEngine;

namespace Xrcadia.App
{
    public static class TaskExtensions
    {
        /// <summary>
        /// Fire-and-forget a Task started from a synchronous context (e.g. a state's Tick or a
        /// UI button handler) while still surfacing exceptions to the console. Used for
        /// transitions that are kicked off from outside an async flow so we never silently
        /// swallow a failed transition.
        /// </summary>
        public static async void Forget(this Task task)
        {
            try
            {
                await task;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Xrcadia] Unobserved task failed: {ex}");
            }
        }
    }
}
