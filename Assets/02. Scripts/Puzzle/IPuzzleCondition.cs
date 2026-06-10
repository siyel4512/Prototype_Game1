using System;

public interface IPuzzleCondition
{
    bool IsMet { get; }
    event Action OnConditionMet;
}
