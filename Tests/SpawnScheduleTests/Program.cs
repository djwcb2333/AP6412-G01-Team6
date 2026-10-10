// Dependency-free behavioral tests compile the production scheduler directly.
var tests = new (string Name, Action Run)[]
{
    ("Opening delay and repeated timestamps", () =>
    {
        var schedule = new EnemySpawnSchedule(60);
        Equal(0, schedule.CountDueSpawns(0));
        Equal(0, schedule.CountDueSpawns(1.499999));
        Equal(1, schedule.CountDueSpawns(1.5));
        Equal(0, schedule.CountDueSpawns(1.5));
        Equal(0, schedule.CountDueSpawns(3.999999));
        Equal(1, schedule.CountDueSpawns(4));
    }),
    ("Difficulty interval boundaries", () =>
    {
        Equal(2.5, EnemySpawnSchedule.IntervalAt(0));
        Equal(2.5, EnemySpawnSchedule.IntervalAt(19.999999));
        Equal(1.75, EnemySpawnSchedule.IntervalAt(20));
        Equal(1.75, EnemySpawnSchedule.IntervalAt(39.999999));
        Equal(1.0, EnemySpawnSchedule.IntervalAt(40));
        Equal(1.0, EnemySpawnSchedule.IntervalAt(59.999999));
    }),
    ("Scheduled deadlines preserve phase across 20 and 40 seconds", () =>
    {
        var schedule = new EnemySpawnSchedule(60);
        Equal(8, schedule.CountDueSpawns(19));
        Equal(0, schedule.CountDueSpawns(20));
        Equal(0, schedule.CountDueSpawns(21.499999));
        Equal(1, schedule.CountDueSpawns(21.5));
        Equal(1, schedule.CountDueSpawns(23.25));
        Equal(9, schedule.CountDueSpawns(39));
        Equal(0, schedule.CountDueSpawns(40));
        Equal(0, schedule.CountDueSpawns(40.749999));
        Equal(1, schedule.CountDueSpawns(40.75));
        Equal(1, schedule.CountDueSpawns(41.75));
    }),
    ("One update catches up multiple due spawns", () =>
    {
        var schedule = new EnemySpawnSchedule(60);
        Equal(4, schedule.CountDueSpawns(10));
        Equal(0, schedule.CountDueSpawns(10));
        Equal(1, schedule.CountDueSpawns(12));
    }),
    ("Paused authority clock creates no extra enemies", () =>
    {
        var schedule = new EnemySpawnSchedule(60);
        Equal(4, schedule.CountDueSpawns(10));
        for (int i = 0; i < 300; i++)
            Equal(0, schedule.CountDueSpawns(10));
        Equal(0, schedule.CountDueSpawns(11.499999));
        Equal(1, schedule.CountDueSpawns(11.5));
    }),
    ("60 FPS updates produce 39 enemies", () => Equal(39, CountWithStep(1.0 / 60))),
    ("30 FPS updates produce 39 enemies", () => Equal(39, CountWithStep(1.0 / 30))),
    ("Two updates per second produce 39 enemies", () => Equal(39, CountWithStep(0.5))),
    ("Irregular update hitches produce 39 enemies", () =>
    {
        var schedule = new EnemySpawnSchedule(60);
        int total = 0;
        foreach (double elapsed in new[] { 0.0, 1.49, 1.5, 3, 11.8, 20.1, 34.7, 40.1, 59.999 })
            total += schedule.CountDueSpawns(elapsed);
        Equal(39, total);
    }),
    ("Single pre-terminal update produces the same 39 enemies", () =>
    {
        var schedule = new EnemySpawnSchedule(60);
        Equal(39, schedule.CountDueSpawns(59.999));
        Equal(0, schedule.CountDueSpawns(59.999));
    }),
    ("Counts increase in each difficulty stage", () =>
    {
        var schedule = new EnemySpawnSchedule(60);
        Equal(8, schedule.CountDueSpawns(19.999));
        Equal(11, schedule.CountDueSpawns(39.999));
        Equal(20, schedule.CountDueSpawns(59.999));
    }),
    ("Last scheduled spawn and terminal timestamp", () =>
    {
        var schedule = new EnemySpawnSchedule(60);
        Equal(38, schedule.CountDueSpawns(59.749999));
        Equal(1, schedule.CountDueSpawns(59.75));
        Equal(0, schedule.CountDueSpawns(60));
        Equal(0, schedule.CountDueSpawns(61));
    }),
    ("Terminal update skips catch-up even on a fresh scheduler", () =>
    {
        var schedule = new EnemySpawnSchedule(60);
        Equal(0, schedule.CountDueSpawns(60));
        Equal(0, schedule.CountDueSpawns(120));
    }),
    ("Reset starts a new independent round", () =>
    {
        var schedule = new EnemySpawnSchedule(60);
        Equal(39, schedule.CountDueSpawns(59.999));
        Equal(0, schedule.CountDueSpawns(60));
        schedule.Reset();
        Equal(0, schedule.CountDueSpawns(0));
        Equal(0, schedule.CountDueSpawns(1.499999));
        Equal(1, schedule.CountDueSpawns(1.5));
        Equal(38, schedule.CountDueSpawns(59.999));
    }),
    ("Short rounds do not emit at or after their duration", () =>
    {
        Equal(0, new EnemySpawnSchedule(1).CountDueSpawns(0.999));
        Equal(0, new EnemySpawnSchedule(1.5).CountDueSpawns(1.5));
        var schedule = new EnemySpawnSchedule(4);
        Equal(1, schedule.CountDueSpawns(3.999));
        Equal(0, schedule.CountDueSpawns(4));
    }),
    ("Invalid round durations are rejected", () =>
    {
        foreach (double invalid in new[] { 0.0, -1, double.NaN, double.PositiveInfinity, double.NegativeInfinity })
            Throws<ArgumentOutOfRangeException>(() => new EnemySpawnSchedule(invalid));
    }),
    ("Invalid elapsed times are rejected without advancing state", () =>
    {
        var schedule = new EnemySpawnSchedule(60);
        foreach (double invalid in new[] { -0.001, double.NaN, double.PositiveInfinity, double.NegativeInfinity })
            Throws<ArgumentOutOfRangeException>(() => schedule.CountDueSpawns(invalid));
        Equal(1, schedule.CountDueSpawns(1.5));
    }),
};

int failures = 0;
foreach (var test in tests)
{
    try
    {
        test.Run();
        Console.WriteLine($"PASS: {test.Name}");
    }
    catch (Exception error)
    {
        failures++;
        Console.Error.WriteLine($"FAIL: {test.Name}: {error.Message}");
    }
}
Console.WriteLine($"RESULT: {tests.Length - failures}/{tests.Length} tests passed.");
return failures == 0 ? 0 : 1;

static int CountWithStep(double step)
{
    var schedule = new EnemySpawnSchedule(60);
    int total = 0;
    for (double elapsed = 0; elapsed < 59.999; elapsed += step)
        total += schedule.CountDueSpawns(elapsed);
    return total + schedule.CountDueSpawns(59.999);
}

static void Equal<T>(T expected, T actual)
{
    if (!EqualityComparer<T>.Default.Equals(expected, actual))
        throw new Exception($"Expected {expected}; actual {actual}.");
}

static void Throws<T>(Action action) where T : Exception
{
    try
    {
        action();
    }
    catch (T)
    {
        return;
    }
    throw new Exception($"Expected {typeof(T).Name}.");
}
