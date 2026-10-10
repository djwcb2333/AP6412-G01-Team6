using System;

public sealed class EnemySpawnSchedule
{
	public const double InitialDelaySeconds = 1.5;

	private readonly double _durationSeconds;
	private double _nextSpawnSeconds;

	public EnemySpawnSchedule(double durationSeconds)
	{
		if (!double.IsFinite(durationSeconds) || durationSeconds <= 0.0)
		{
			throw new ArgumentOutOfRangeException(nameof(durationSeconds));
		}

		_durationSeconds = durationSeconds;
		Reset();
	}

	public void Reset()
	{
		_nextSpawnSeconds = InitialDelaySeconds;
	}

	public static double IntervalAt(double elapsedSeconds)
	{
		if (elapsedSeconds >= 40.0)
		{
			return 1.0;
		}

		return elapsedSeconds >= 20.0 ? 1.75 : 2.5;
	}

	public int CountDueSpawns(double elapsedSeconds)
	{
		if (!double.IsFinite(elapsedSeconds) || elapsedSeconds < 0.0)
		{
			throw new ArgumentOutOfRangeException(nameof(elapsedSeconds));
		}

		if (elapsedSeconds >= _durationSeconds)
		{
			return 0;
		}

		int count = 0;
		while (_nextSpawnSeconds <= elapsedSeconds
			&& _nextSpawnSeconds < _durationSeconds)
		{
			count++;
			_nextSpawnSeconds += IntervalAt(_nextSpawnSeconds);
		}

		return count;
	}
}
