﻿namespace T3H.Poll.CrossCuttingConcerns.CircuitBreaker;

public interface ICircuitBreakerManager
{
    ICircuitBreaker GetCircuitBreaker(string name, TimeSpan openTime);

    void LogSuccess(ICircuitBreaker circuitBreaker);

    void LogFailure(ICircuitBreaker circuitBreaker, int maximumNumberOfFailures, TimeSpan period);
}
