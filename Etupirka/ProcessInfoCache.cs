using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Etupirka
{
    class ProcessInfo
    {
        public int id { get; set; }

        public string processName { get; set; }

        public string path { get; set; }

        // True if we have access to this process.
        public bool accesible { get; set; }

        // True if the process info is pinned. Unpinned entries will be automatically cleaned up.
        public bool pinned { get; set; }
    }

    class ProcessInfoCleanup : IDisposable
    {
        public ProcessInfoCleanup(ProcessInfoCache cache)
        {
            this.cache = cache;
        }

        public void Dispose()
        {
            cache.cleanUp();
        }

        private ProcessInfoCache cache;
    }

    class ProcessInfoCache
    {
        // Returns an object that automatically cleans up process info that is not used in the scope.
        public ProcessInfoCleanup scopedAccess()
        {
            foreach (var entry in processInfoById)
            {
                entry.Value.pinned = false;
            }

            return new ProcessInfoCleanup(this);
        }

        public void cleanUp()
        {
            var entriesToRemove = processInfoById.Where((entry) => !entry.Value.pinned).ToList();
            foreach (var entry in entriesToRemove)
            {
                processInfoById.Remove(entry.Key);
            }
        }

        // Returns the path of the process, or an empty string if the process is not accesible.
        public string getProcessPath(Process p)
        {
            if (processInfoById.ContainsKey(p.Id))
            {
                ProcessInfo info = processInfoById[p.Id];
                if (info.processName == p.ProcessName)
                {
                    info.pinned = true;
                    return info.path;
                }
            }

            var processInfo = new ProcessInfo
            {
                id = p.Id,
                accesible = false,
                processName = p.ProcessName,
                pinned = true
            };

            try
            {
                processInfo.path = p.MainModule.FileName.ToLower();
                processInfo.accesible = true;
            }
            catch { }

            processInfoById[p.Id] = processInfo;
            return processInfo.path;
        }

        private Dictionary<int, ProcessInfo> processInfoById = new Dictionary<int, ProcessInfo>();
    }
}
