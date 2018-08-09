using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

//Example implementation of rcContext
[Serializable()]
public partial class Recast{
    [Serializable()]
    /// Recast build context.
    public class BuildContext : rcContext
    {
        private Stopwatch[] m_Stopwatches = new Stopwatch[(int)rcTimerLabel.RC_MAX_TIMERS];

        public bool m_Dirty = false;

        private class LogMessage{
            public rcLogCategory m_Category;
            public string m_Message;

            public LogMessage(rcLogCategory category, string msg){
                m_Category = category;
                m_Message = msg;
            }
        }

	    const int MAX_MESSAGES = 1000;
	    List<LogMessage> m_Messages = new List<LogMessage>();
	
        public BuildContext()
        {
            for (int i = 0; i < (int)rcTimerLabel.RC_MAX_TIMERS; ++i) {
                m_Stopwatches[i] = new Stopwatch();
            }
	        resetTimers();
        }


        protected override void doResetLog()
        {
	        m_Messages.Clear();
        }

        protected override void doLog(rcLogCategory category, string msg)
        {
	        m_Messages.Add(new LogMessage(category, msg));
            if (m_Messages.Count > MAX_MESSAGES){
                m_Messages.RemoveAt(0);
            }

            m_Dirty = true;
        }

        protected override void doResetTimers()
        {
	        for (int i = 0; i < (int)rcTimerLabel.RC_MAX_TIMERS; ++i)
		        m_Stopwatches[i].Reset();
        }

        protected override void doStartTimer(rcTimerLabel label)
        {
	        m_Stopwatches[(int)label].Start();
        }

        protected override void doStopTimer(rcTimerLabel label)
        {
	        m_Stopwatches[(int)label].Stop();
        }

        protected override long doGetAccumulatedTime(rcTimerLabel label)
        {
	        return m_Stopwatches[(int)label].ElapsedMilliseconds;
        }

        protected override double doGetAccumulatedTimeHiResolution(rcTimerLabel label) {

            if (!Stopwatch.IsHighResolution) {
                return -1.0;
            }

            if (m_Stopwatches[(int)label].IsRunning) {
                //UnityEngine.Debug.LogError("Stopwatch " + label.ToString() + " was not stopped prior to output");
            }

            double seconds = (double)m_Stopwatches[(int)label].ElapsedTicks / (double)Stopwatch.Frequency;

            return seconds * 1000.0;
        }


        private void logLine(rcTimerLabel label, string name, double pc, int level = 0)
        {
            double t = getAccumulatedTimeHiResolution(label);
	        if (t <= 0.0f) 
                return;
            string raster = "";
            
            if (level > 0) {
                raster += new string(' ', 4*level);
            }
            raster += name + ": ";
            if (raster.Length < 25) {
                raster += new string(' ', 25 - raster.Length);
            }

            int secondColumn = 40;
            if (level != 0) {
                raster += "<color=grey>";
                secondColumn += 12;
            }
            raster += string.Format("{0:N2}", t) + "ms ";
            if (raster.Length < secondColumn) {
                raster += new string(' ', secondColumn - raster.Length);
            }
            raster += "(" + string.Format("{0:N1}", t * pc) + "%)";
            if (level != 0) {
                raster += "</color>";
            }
            log(rcLogCategory.RC_LOG_PROGRESS, raster);

            m_PercentCheck += t * pc;
            m_SumCheck += t;
        }

        private double m_PercentCheck = 0.0;
        private double m_SumCheck = 0.0;
        public void logBuildTimes()
        {
            double totalTime = getAccumulatedTimeHiResolution(rcTimerLabel.RC_TIMER_TOTAL);
            double pc = 100.0 / totalTime;

            m_PercentCheck = 0.0;
            m_SumCheck = 0.0;

            log(rcLogCategory.RC_LOG_PROGRESS, "Build Times");
            logLine(rcTimerLabel.RC_TIMER_RASTERIZE_TRIANGLES, "- Rasterize", pc);
            logLine(rcTimerLabel.RC_TIMER_BUILD_COMPACTHEIGHTFIELD, "- Build Compact", pc);
            logLine(rcTimerLabel.RC_TIMER_FILTER_BORDER, "- Filter Border", pc);
            logLine(rcTimerLabel.RC_TIMER_FILTER_WALKABLE, "- Filter Walkable", pc);
            logLine(rcTimerLabel.RC_TIMER_ERODE_AREA, "- Erode Area", pc);
            logLine(rcTimerLabel.RC_TIMER_MEDIAN_AREA, "- Median Area", pc);
            logLine(rcTimerLabel.RC_TIMER_MARK_BOX_AREA, "- Mark Box Area", pc);
            logLine(rcTimerLabel.RC_TIMER_MARK_CONVEXPOLY_AREA, "- Mark Convex Area", pc);
            logLine(rcTimerLabel.RC_TIMER_MARK_CYLINDER_AREA, "- Mark Cylinder Area", pc);
            logLine(rcTimerLabel.RC_TIMER_BUILD_DISTANCEFIELD, "- Build Distance Field", pc);
            logLine(rcTimerLabel.RC_TIMER_BUILD_DISTANCEFIELD_DIST, "- Distance", pc, 1);
            logLine(rcTimerLabel.RC_TIMER_BUILD_DISTANCEFIELD_BLUR, "- Blur", pc, 1);
            logLine(rcTimerLabel.RC_TIMER_BUILD_REGIONS, "- Build Regions", pc);
            logLine(rcTimerLabel.RC_TIMER_BUILD_REGIONS_WATERSHED, "- Watershed", pc, 1);
            logLine(rcTimerLabel.RC_TIMER_BUILD_REGIONS_EXPAND, "- Expand", pc, 2);
            logLine(rcTimerLabel.RC_TIMER_BUILD_REGIONS_FLOOD, "- Find Basins", pc, 2);
            logLine(rcTimerLabel.RC_TIMER_BUILD_REGIONS_FILTER, "- Filter", pc, 1);
            logLine(rcTimerLabel.RC_TIMER_BUILD_LAYERS, "- Build Layers", pc);
            logLine(rcTimerLabel.RC_TIMER_BUILD_CONTOURS, "- Build Contours", pc);
            logLine(rcTimerLabel.RC_TIMER_BUILD_CONTOURS_TRACE, "- Trace", pc, 1);
            logLine(rcTimerLabel.RC_TIMER_BUILD_CONTOURS_SIMPLIFY, "- Simplify", pc, 1);
            logLine(rcTimerLabel.RC_TIMER_BUILD_POLYMESH, "- Build Polymesh", pc);
            logLine(rcTimerLabel.RC_TIMER_BUILD_POLYMESHDETAIL, "- Build Polymesh Detail", pc);
            logLine(rcTimerLabel.RC_TIMER_MERGE_POLYMESH, "- Merge Polymeshes", pc);
            logLine(rcTimerLabel.RC_TIMER_MERGE_POLYMESHDETAIL, "- Merge Polymesh Details", pc);
            log(rcLogCategory.RC_LOG_PROGRESS, "=== TOTAL: " + totalTime + " ms ===");
        }

        public string dumpLog()
        {
	        StringBuilder sb = new StringBuilder();

            foreach(LogMessage msg in m_Messages){
                sb.AppendLine(msg.m_Category.ToString() + ": " + msg.m_Message);
            }
            return sb.ToString();
        }

        public int getLogCount()
        {
	        return m_Messages.Count;
        }
    };
}