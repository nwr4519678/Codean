namespace Platform.Judge.Models;

public record ExecutionLimits(
    double CpuTimeLimitSec = 2.0,
    long MemoryLimitMb = 256,
    long OutputSizeLimitKb = 1024
);
