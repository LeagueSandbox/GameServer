using dtStatus = System.UInt32;

public static partial class Detour{
    // High level status.
    public const uint DT_FAILURE = 1u << 31;			// Operation failed.
    public const uint DT_SUCCESS = 1u << 30;			// Operation succeed.
    public const uint DT_IN_PROGRESS = 1u << 29;		// Operation still in progress.

    // Detail information for status.
    public const uint DT_STATUS_DETAIL_MASK = 0x0ffffff;
    public const uint DT_WRONG_MAGIC = 1 << 0;		// Input data is not recognized.
    public const uint DT_WRONG_VERSION = 1 << 1;	// Input data is in wrong version.
    public const uint DT_OUT_OF_MEMORY = 1 << 2;	// Operation ran out of memory.
    public const uint DT_INVALID_PARAM = 1 << 3;	// An input parameter was invalid.
    public const uint DT_BUFFER_TOO_SMALL = 1 << 4;	// Result buffer for the query was too small to store all results.
    public const uint DT_OUT_OF_NODES = 1 << 5;		// Query ran out of nodes during search.
    public const uint DT_PARTIAL_RESULT = 1 << 6;	// Query did not reach the end location, returning best guess. 


    // Returns true of status is success.
    public static bool dtStatusSucceed(dtStatus status)
    {
	    return (status & DT_SUCCESS) != 0;
    }

    // Returns true of status is failure.
    public static bool dtStatusFailed(dtStatus status)
    {
	    return (status & DT_FAILURE) != 0;
    }

    // Returns true of status is in progress.
    public static bool dtStatusInProgress(dtStatus status)
    {
	    return (status & DT_IN_PROGRESS) != 0;
    }

    // Returns true if specific detail is set.
    public static bool dtStatusDetail(dtStatus status, uint detail)
    {
	    return (status & detail) != 0;
    }

}
