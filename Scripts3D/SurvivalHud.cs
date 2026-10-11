using Godot;
using System;

// Displays round state and sends restart requests; gameplay rules stay in Game3D.
public partial class SurvivalHud : CanvasLayer
{
	[Signal]
	public delegate void RestartRequestedEventHandler();

	private Label _timeLabel = null!;
	private Label _healthLabel = null!;
	private Label _scoreLabel = null!;
	private Label _rewardLabel = null!;
	private ColorRect _resultOverlay = null!;
	private Label _resultTitle = null!;
	private Label _resultDetails = null!;
	private Button _restartButton = null!;
	private Timer _rewardTimer = null!;

	public override void _Ready()
	{
		_timeLabel = GetNode<Label>("StatusPanel/StatusMargin/StatusRow/TimeLabel");
		_healthLabel = GetNode<Label>("StatusPanel/StatusMargin/StatusRow/HealthLabel");
		_scoreLabel = GetNode<Label>("StatusPanel/StatusMargin/StatusRow/ScoreLabel");
		_rewardLabel = GetNode<Label>("RewardLabel");
		_resultOverlay = GetNode<ColorRect>("ResultOverlay");
		_resultTitle = GetNode<Label>("ResultOverlay/Center/ResultPanel/ResultMargin/ResultColumn/ResultTitle");
		_resultDetails = GetNode<Label>("ResultOverlay/Center/ResultPanel/ResultMargin/ResultColumn/ResultDetails");
		_restartButton = GetNode<Button>("ResultOverlay/Center/ResultPanel/ResultMargin/ResultColumn/RestartButton");
		_rewardTimer = GetNode<Timer>("RewardTimer");

		_restartButton.Pressed += () => EmitSignal(SignalName.RestartRequested);
		_rewardTimer.Timeout += () => _rewardLabel.Visible = false;
		HideResult();
	}

	public void UpdateStats(float remainingSeconds, int health, int maxHealth,
		int kills, int score)
	{
		_timeLabel.Text = $"TIME  {Mathf.CeilToInt(remainingSeconds):00}s";
		_healthLabel.Text = $"HEALTH  {health}/{maxHealth}  "
			+ new string('♥', health) + new string('♡', maxHealth - health);
		_scoreLabel.Text = $"KILLS  {kills}    SCORE  {score}";
	}

	public void ShowReward(string message)
	{
		_rewardLabel.Text = message;
		_rewardLabel.Visible = true;
		_rewardTimer.Start(1.5);
	}

	public void ShowResult(bool victory, int score)
	{
		_rewardTimer.Stop();
		_rewardLabel.Visible = false;
		_resultTitle.Text = victory ? "VICTORY" : "GAME OVER";
		_resultDetails.Text = victory
			? $"You survived the arena!  Final score: {score}"
			: $"You ran out of health.  Final score: {score}";
		_resultOverlay.Visible = true;
		_restartButton.GrabFocus();
	}

	public void HideResult()
	{
		_rewardTimer.Stop();
		_rewardLabel.Visible = false;
		_resultOverlay.Visible = false;
	}
}
