using Sandbox;

[Library( "ent_stunwall", Title = "Stun Wall", Spawnable = true )]
public partial class StunEntity : BaseTrigger
{
	public TimeSince cooldown;
	public override void Touch( Entity other )
	{
		if ( other is not SandboxPlayer pl ) return;
		if(cooldown >= 1f )
		{
			pl.Stun();
			Log.Info( pl.Name + " was Stunned" );
		}

		base.Touch( other );
	}
}
