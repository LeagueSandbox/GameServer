using System.Diagnostics;
using System;

public static partial class Recast{
    /// @par
    ///
    /// Allows the formation of walkable regions that will flow over low lying 
    /// objects such as curbs, and up structures such as stairways. 
    /// 
    /// Two neighboring spans are walkable if: <tt>rcAbs(currentSpan.smax - neighborSpan.smax) < waklableClimb</tt>
    /// 
    /// @warning Will override the effect of #rcFilterLedgeSpans.  So if both filters are used, call
    /// #rcFilterLedgeSpans after calling this filter. 
    ///
    /// @see rcHeightfield, rcConfig
    public static void rcFilterLowHangingWalkableObstacles(rcContext ctx, int walkableClimb, rcHeightfield solid)
    {
	    Debug.Assert(ctx != null, "rcContext is null");

	    ctx.startTimer(rcTimerLabel.RC_TIMER_FILTER_LOW_OBSTACLES);
	
	    int w = solid.width;
	    int h = solid.height;
	
	    for (int y = 0; y < h; ++y)
	    {
		    for (int x = 0; x < w; ++x)
		    {
			    rcSpan ps = null;
			    bool previousWalkable = false;
			    byte previousArea = RC_NULL_AREA;
			
			    for (rcSpan s = solid.spans[x + y*w]; s != null; ps = s, s = s.next)
			    {
				    bool walkable = s.area != RC_NULL_AREA;
				    // If current span is not walkable, but there is walkable
				    // span just below it, mark the span above it walkable too.
				    if (!walkable && previousWalkable)
				    {
					    if (Math.Abs((int)s.smax - (int)ps.smax) <= walkableClimb){
						    s.area = previousArea;
                        }
				    }
				    // Copy walkable flag so that it cannot propagate
				    // past multiple non-walkable objects.
				    previousWalkable = walkable;
				    previousArea = s.area;
			    }
		    }
	    }

	    ctx.stopTimer(rcTimerLabel.RC_TIMER_FILTER_LOW_OBSTACLES);
    }

    /// @par
    ///
    /// A ledge is a span with one or more neighbors whose maximum is further away than @p walkableClimb
    /// from the current span's maximum.
    /// This method removes the impact of the overestimation of conservative voxelization 
    /// so the resulting mesh will not have regions hanging in the air over ledges.
    /// 
    /// A span is a ledge if: <tt>rcAbs(currentSpan.smax - neighborSpan.smax) > walkableClimb</tt>
    /// 
    /// @see rcHeightfield, rcConfig
    public static void rcFilterLedgeSpans(rcContext ctx, int walkableHeight, int walkableClimb,
						    rcHeightfield solid)
    {
	    Debug.Assert(ctx != null, "rcContext is null");
	
	    ctx.startTimer(rcTimerLabel.RC_TIMER_FILTER_BORDER);

	    int w = solid.width;
	    int h = solid.height;
	    int MAX_HEIGHT = 0xffff;
	
	    // Mark border spans.
	    for (int y = 0; y < h; ++y)
	    {
		    for (int x = 0; x < w; ++x)
		    {
			    for (rcSpan s = solid.spans[x + y*w]; s != null; s = s.next)
			    {
				    // Skip non walkable spans.
				    if (s.area == RC_NULL_AREA){
					    continue;
                    }
				
				    int bot = (int)(s.smax);
				    int top = s.next != null ? (int)(s.next.smin) : MAX_HEIGHT;
				
				    // Find neighbours minimum height.
				    int minh = MAX_HEIGHT;

				    // Min and max height of accessible neighbours.
				    int asmin = s.smax;
				    int asmax = s.smax;

				    for (int dir = 0; dir < 4; ++dir)
				    {
					    int dx = x + rcGetDirOffsetX(dir);
					    int dy = y + rcGetDirOffsetY(dir);
					    // Skip neighbours which are out of bounds.
					    if (dx < 0 || dy < 0 || dx >= w || dy >= h)
					    {
						    minh = Math.Min(minh, -walkableClimb - bot);
						    continue;
					    }

					    // From minus infinity to the first span.
					    rcSpan ns = solid.spans[dx + dy*w];
					    int nbot = -walkableClimb;
					    int ntop = ns != null ? (int)ns.smin : MAX_HEIGHT;
					    // Skip neightbour if the gap between the spans is too small.
					    if (Math.Min(top,ntop) - Math.Max(bot,nbot) > walkableHeight)
						    minh = Math.Min(minh, nbot - bot);
					
					    // Rest of the spans.
					    for (ns = solid.spans[dx + dy*w]; ns != null; ns = ns.next)
					    {
						    nbot = (int)ns.smax;
						    ntop = ns.next != null ? (int)ns.next.smin : MAX_HEIGHT;
						    // Skip neightbour if the gap between the spans is too small.
						    if (Math.Min(top,ntop) - Math.Max(bot,nbot) > walkableHeight)
						    {
							    minh = Math.Min(minh, nbot - bot);
						
							    // Find min/max accessible neighbour height. 
							    if (Math.Abs(nbot - bot) <= walkableClimb)
							    {
								    if (nbot < asmin) asmin = nbot;
								    if (nbot > asmax) asmax = nbot;
							    }
							
						    }
					    }
				    }
				
				    // The current span is close to a ledge if the drop to any
				    // neighbour span is less than the walkableClimb.
				    if (minh < -walkableClimb){
					    s.area = RC_NULL_AREA;
                    }
					
				    // If the difference between all neighbours is too large,
				    // we are at steep slope, mark the span as ledge.
				    if ((asmax - asmin) > walkableClimb)
				    {
					    s.area = RC_NULL_AREA;
				    }
			    }
		    }
	    }
	
	    ctx.stopTimer(rcTimerLabel.RC_TIMER_FILTER_BORDER);
    }	

    /// @par
    ///
    /// For this filter, the clearance above the span is the distance from the span's 
    /// maximum to the next higher span's minimum. (Same grid column.)
    /// 
    /// @see rcHeightfield, rcConfig
    public static void rcFilterWalkableLowHeightSpans(rcContext ctx, int walkableHeight, rcHeightfield solid)
    {
	    Debug.Assert(ctx != null, "rcContext is null");
	
	    ctx.startTimer(rcTimerLabel.RC_TIMER_FILTER_WALKABLE);
	
	    int w = solid.width;
	    int h = solid.height;
	    int MAX_HEIGHT = 0xffff;
	
	    // Remove walkable flag from spans which do not have enough
	    // space above them for the agent to stand there.
	    for (int y = 0; y < h; ++y)
	    {
		    for (int x = 0; x < w; ++x)
		    {
			    for (rcSpan s = solid.spans[x + y*w]; s != null; s = s.next)
			    {
				    int bot = (int)(s.smax);
				    int top = s.next != null ? (int)(s.next.smin) : MAX_HEIGHT;
                    if ((top - bot) <= walkableHeight) {
                        s.area = RC_NULL_AREA;
                    }
			    }
		    }
	    }
	
	    ctx.stopTimer(rcTimerLabel.RC_TIMER_FILTER_WALKABLE);
    }
}