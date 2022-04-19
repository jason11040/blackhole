using Sandbox;

[Library( "ent_stunwall", Title = "Stun Wall", Spawnable = true )]
public partial class StunEntity : BaseTrigger
{
	public TimeSince cooldown;
	protected Output OnHurtPlayer { get; set; }
	protected Output OnHurt { get; set; }
	[Event.Tick.Server]
	protected virtual void Touch()
	{
		if ( !Enabled )
			return;

		foreach ( var entity in TouchingEntities )
		{
			if ( !entity.IsValid() )
				continue;

			entity.TakeDamage( DamageInfo.Generic( 1 * Time.Delta ).WithAttacker( this ) );
			if(cooldown >= 2f )
			{
				if ( entity.Tags.Has( "player" ) )
				{
					//Log.Info(cooldown.ToString() + entity );
					OnHurtPlayer.Fire( entity );
					var player = (SandboxPlayer)entity;
					player.Stun();
					cooldown = 0;
				}
				else
				{
					OnHurt.Fire( entity );
				}
			}
		}
	}
}
