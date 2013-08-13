using System.Collections.Generic;
using Psy.Core;
using Psy.Core.Logging;

namespace Vortex.Interface.Debugging
{
    public class TimingStats
    {
#if DEBUG
        private const bool Disabled = false;
#else
        private const bool Disabled = true;
#endif
        private readonly Dictionary<string, double> _taskToStartTime;
        private readonly Dictionary<string, double> _taskToStopTime;
        private readonly List<string> _taskList;

        private readonly List<TimingStats> _additionalStats;

        private readonly double _startTime;
        private double _stopTime;

        private readonly string _groupName;

        public TimingStats(string groupName)
        {
            _startTime = Timer.GetTime();
            _groupName = groupName;

            _taskToStartTime = new Dictionary<string, double>();
            _taskToStopTime = new Dictionary<string, double>();
            
            _taskList = new List<string>();
            _additionalStats = new List<TimingStats>();
        }

        public void MergeStats(TimingStats stats)
        {
            _additionalStats.Add(stats);
        }

        public void StartingTask(string taskName)
        {
            if (Disabled)
                return;
            var time = Timer.GetTime();
            _taskToStartTime.Add(taskName, time);
            _taskList.Add(taskName);
        }

        public void CompletedTask(string taskName)
        {
            if (Disabled)
                return;
            var time = Timer.GetTime();
            _taskToStopTime.Add(taskName, time);
            _stopTime = time;
        }

        public void LogStats(long warningLevel, long errorLevel, long criticalLevel)
        {
            var level = LoggerLevel.Trace;
            var totalTime = _stopTime - _startTime;
            var completed = true;

            if (totalTime > warningLevel)
                level = LoggerLevel.Warning;
            if (totalTime > errorLevel)
                level = LoggerLevel.Error;
            if (totalTime > criticalLevel)
                level = LoggerLevel.Critical;

            var now = Timer.GetTime();
            foreach (var name in _taskList)
            {
                var startTime = _taskToStartTime[name];
                double stopTime;
                var stopped = _taskToStopTime.TryGetValue(name, out stopTime);
                completed &= stopped;

                if (level == LoggerLevel.Trace)
                    continue;

                Logger.Write(
                    stopped
                        ? string.Format("Task {0}:{1} completed in {2} ms", _groupName, name, stopTime - startTime)
                        : string.Format("Task {0}:{1} ran for {2} ms", _groupName, name, now - startTime),
                    level);
            }

            foreach (var item in _additionalStats)
                item.LogStats(warningLevel, errorLevel, criticalLevel);

            if (level != LoggerLevel.Trace)
            {
                Logger.Write(
                    completed
                        ? string.Format("Total time to complete {0}: {1} ms", _groupName, _stopTime - _startTime)
                        : string.Format("Total time spent working on {0} so far: {1} ms", _groupName, now - _startTime),
                    level);
            }
        }
    }
}
