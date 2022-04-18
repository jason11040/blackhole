using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

public class Health : Panel
{
	public Label HealthLabel;
	public Panel PlayerInfo;
	public Panel HealthBarBackground;
	public Panel HealthBar;

	public Health()
	{
		//PlayerInfo = Add.Panel( "vitalsPanel" );

		HealthBarBackground = Add.Panel( "healthbarback" );
		HealthBar = HealthBarBackground.Add.Panel( "healthbar" );
		HealthLabel = HealthBarBackground.Add.Label( "100", "health" );

	}

	public float startLerpHealth;
	public float curHealth = -1;

	public override void Tick()
	{

		SandboxPlayer player = Local.Pawn as SandboxPlayer;
		if ( player == null ) return;

		if ( curHealth != player.Health )
		{
			startLerpHealth = curHealth;
			curHealth = player.Health;
		}

		startLerpHealth = MathX.LerpTo( startLerpHealth, player.Health, Time.Delta * 1f );
		HealthBar.Style.Width = Length.Percent( (startLerpHealth * 100) / player.MaxHealth );
		if ( player.Health <= 0 )
		{
			HealthBar.Style.Opacity = 0;
		}
		else
		{
			HealthBar.Style.Opacity = 1;
		}


		HealthLabel.Text = "Health: " + $"{player.Health.CeilToInt()}" + "/" + player.MaxHealth;
		HealthLabel.SetClass( "danger", player.Health < 40.0f );
		HealthBar.Style.Dirty();
	}
}
