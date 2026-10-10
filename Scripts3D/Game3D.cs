using Godot;
using System;

public partial class Game3D : Node3D
{
	// OUR victory condition: the player must survive for 60 seconds.
	private const float SurvivalSeconds = 60f;

	private readonly RandomNumberGenerator _rng =
		new();

	private PackedScene _chaserScene = null!;

	private PackedScene _strikerScene = null!;

	private PackedScene _bulletScene = null!;

	private Node3D _entities = null!;

	private Player3D _player = null!;

	private readonly EnemySpawnSchedule _spawnSchedule = new(SurvivalSeconds);

	private float _remainingTime;

	private int _kills;

	private int _score;

	private bool _running;

	// GODOT lifecycle callback, called when this scene enters the tree.
	public override void _Ready()
	{
		Engine.TimeScale = 1.0;

		_rng.Randomize();

		_chaserScene =
			GD.Load<PackedScene>(
				"res://Scenes3D/ChaserEnemy3D.tscn");

		_strikerScene =
			GD.Load<PackedScene>(
				"res://Scenes3D/StrikerEnemy3D.tscn");

		_bulletScene =
			GD.Load<PackedScene>(
				"res://Scenes3D/Bullet3D.tscn");

		_entities =
			GetNode<Node3D>("Entities");

		_player =
			GetNode<Player3D>("Player");

		// OUR signal-based design: actors request actions; Game3D owns spawning.
		_player.ShotRequested +=
			SpawnPlayerBullet;

		_player.Died +=
			EndWithDefeat;

		StartGame();
	}

	public override void _PhysicsProcess(
		double deltaValue)
	{
		if (!_running)
		{
			return;
		}

		_remainingTime = Math.Max(
			0f,
			_remainingTime - (float)deltaValue);

		if (_remainingTime <= 0f)
		{
			EndWithVictory();
			return;
		}

		int dueSpawns = _spawnSchedule.CountDueSpawns(
			SurvivalSeconds - _remainingTime);

		for (int i = 0; i < dueSpawns; i++)
		{
			SpawnEnemy();
		}
	}

	private void StartGame()
	{
		ClearEntities();

		_kills = 0;

		_score = 0;

		_remainingTime = SurvivalSeconds;

		_spawnSchedule.Reset();

		_running = true;

		_player.ResetPlayer(
			new Vector3(0f, 0.5f, 0f));
	}

	private void SpawnEnemy()
	{
		PackedScene selectedScene =
			_kills < 4
				? _chaserScene
				: _strikerScene;

		Enemy3D enemy =
			selectedScene.Instantiate<Enemy3D>();

		_entities.AddChild(enemy);

		enemy.GlobalPosition =
			RandomArenaEdgePosition();

		enemy.Configure(_player);

		enemy.Destroyed +=
			OnEnemyDestroyed;

		enemy.ShotRequested +=
			SpawnEnemyBullet;
	}

	private Vector3 RandomArenaEdgePosition()
	{
		const float MaxX = 8.5f;
		const float MaxZ = 5.5f;

		int side =
			_rng.RandiRange(0, 3);

		if (side == 0)
		{
			return new Vector3(
				_rng.RandfRange(-MaxX, MaxX),
				0.5f,
				-MaxZ);
		}

		if (side == 1)
		{
			return new Vector3(
				MaxX,
				0.5f,
				_rng.RandfRange(-MaxZ, MaxZ));
		}

		if (side == 2)
		{
			return new Vector3(
				_rng.RandfRange(-MaxX, MaxX),
				0.5f,
				MaxZ);
		}

		return new Vector3(
			-MaxX,
			0.5f,
			_rng.RandfRange(-MaxZ, MaxZ));
	}

	private void SpawnPlayerBullet(
		Vector3 position,
		Vector3 direction)
	{
		// OUR PLAYER BULLET: friendly, 14 units/s, 24 HP damage.
		SpawnBullet(
			position,
			direction,
			false,
			14f,
			24f);
	}

	private void SpawnEnemyBullet(
		Vector3 position,
		Vector3 direction)
	{
		// OUR ENEMY BULLET: hostile, 8 units/s, 11 HP damage.
		SpawnBullet(
			position,
			direction,
			true,
			8f,
			11f);
	}

	private void SpawnBullet(
		Vector3 position,
		Vector3 direction,
		bool hostile,
		float speed,
		float damage)
	{
		Bullet3D bullet =
			_bulletScene.Instantiate<Bullet3D>();

		_entities.AddChild(bullet);

		bullet.GlobalPosition =
			position;

		bullet.Configure(
			direction,
			speed,
			damage,
			hostile);
	}

	// OUR scoring rule: each destroyed enemy counts as one kill and adds its points.
	private void OnEnemyDestroyed(int points)
	{
		if (!_running)
		{
			return;
		}

		_kills++;

		_score += points;

		GD.Print(
			$"Kills: {_kills}"
			+ $" | Score: {_score}");
	}

	private void EndWithVictory()
	{
		_running = false;

		_player.Active = false;

		GD.Print("VICTORY: Survived 60 seconds!");
	}

	private void EndWithDefeat()
	{
		if (!_running)
		{
			return;
		}

		_running = false;

		GD.Print("GAME OVER!!!!");
	}

	private void ClearEntities()
	{
		foreach (Node child
			in _entities.GetChildren())
		{
			child.QueueFree();
		}
	}
}
