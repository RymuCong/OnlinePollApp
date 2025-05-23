﻿namespace T3H.Poll.CrossCuttingConcerns.CircuitBreaker;

public interface ICircuitBreaker
{
    public string Name { get; set; }

    public CircuitStatus Status { get; set; }

    public DateTimeOffset LastStatusUpdated { get; set; }

    void EnsureOkStatus();
}

public enum CircuitStatus
{
    Closed = 1,
    Open = 2,
    HalfOpen = 3,
}
