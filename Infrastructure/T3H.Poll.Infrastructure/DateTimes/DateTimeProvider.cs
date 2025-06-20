﻿using T3H.Poll.CrossCuttingConcerns.DateTimes;
using IDateTimeProvider = T3H.Poll.CrossCuttingConcerns.DateTimes.IDateTimeProvider;

namespace T3H.Poll.Infrastructure.DateTimes;

public class DateTimeProvider : IDateTimeProvider
{
    public DateTime Now => DateTime.Now;

    public DateTime UtcNow => DateTime.UtcNow;

    public DateTimeOffset OffsetNow => DateTimeOffset.Now;

    public DateTimeOffset OffsetUtcNow => DateTimeOffset.UtcNow;
}
