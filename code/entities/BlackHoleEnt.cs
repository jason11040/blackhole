using Sandbox;
using System;
using System.Linq;

[Library( "ent_blackholeent", Title = "Black Hole", Spawnable = true )]
public partial class BlackHoleEnt : ModelEntity
{
	public float PullRadius { get; set; } = 40000f;
	public float PullRadius2 { get; set; } = 1100f;
	public float DeathRadius { get; set; } = 1100f;
	public float PullSpeed { get; set; }
	public float BHScale { get; set;}
	[Net] public int Gravity { get; set; }
	public TimeSince gravUpdate;
	public TimeSince sinceSpawn;

	public override void Spawn()
	{
		base.Spawn();
		DeleteOthers();
		SetModel( "models/ball/ball.vmdl" );
		SetupPhysicsFromModel( PhysicsMotionType.Static, false );
		Scale = BHScale;
		RenderColor = Color.Black;
		sinceSpawn = 0;
	}
	[Event.Tick]
	public void sendstats(Entity pl)
	{
		Log.Info( "looking for player" );
		if ( pl is SandboxPlayer )
		{
			var player = (SandboxPlayer)pl;
			Gravity = player.GravitySpeed;
			Log.Info("grav: " + Gravity + " playergrav: " + player.GravitySpeed);
		}
	}

	[Event.Tick.Server]
	public void GravityUpdate()
	{
		if ( gravUpdate > 5f )
		{
			gravUpdate = 0;
			PullSpeed += 0.001f;
			Gravity = (int)(PullSpeed * 1000);
			DebugOverlay.ScreenText( "Gravity Speed: " + Gravity.ToString(), 5f );
		}
		
	}

	[Event.Tick.Server]
	public void PullPlayer()
	{
		if ( sinceSpawn < 10f ) return;
		//Log.Info( PullSpeed );
		PullRadius2 = PullRadius2 += 40 * Time.Delta;
		BHScale = BHScale += 1 * Time.Delta;
		//DebugOverlay.Sphere( Position, PullRadius2, Color.Red );
		//Log.Info( PullRadius2 );
		//Log.Info( BHScale );
		var nearEnts = Entity.FindInSphere( Position, PullRadius );
		{
			//DebugOverlay.Sphere( Position, PullRadius, Color.Red );
			foreach ( var ent in nearEnts )
			{
				if ( ent is SandboxPlayer )
				{
					//DebugOverlay.Sphere( Position, 200, Color.Red );
					var player = (SandboxPlayer)ent;
					sendstats( player );
					if ( !player.InAir )
					{
						//Log.Info( "Pulling with in air: " + (PullSpeed * 5) );
						ent.Position = Vector3.Lerp( ent.Position, Position, (PullSpeed * 5) * Time.Delta );
					}
					else
					{
						//Log.Info( "Pulling with: " + PullSpeed );
						ent.Position = Vector3.Lerp( ent.Position, Position, PullSpeed * Time.Delta );
					}
				}
			}
			var nearBH = Entity.FindInSphere( Position, PullRadius2 );
			{
				foreach ( var ent in nearBH )
				{
					if ( ent is SandboxPlayer )
					{
						var player = (SandboxPlayer)ent;
						if ( !player.InAir )
						{
							//Log.Info( "Pulling with in air: " + (PullSpeed * 5) );
							ent.Position = Vector3.Lerp( ent.Position, Position, (PullSpeed * 20) * Time.Delta );
						}
						else
						{
							//Log.Info( "Pulling with: " + PullSpeed );
							ent.Position = Vector3.Lerp( ent.Position, Position, (PullSpeed * 20) * Time.Delta );
						}
					}
				}
			}
			var nearDeath = Entity.FindInSphere( Position, DeathRadius );
				{
					//DebugOverlay.Sphere( Position, DeathRadius, Color.Blue );
					foreach ( var ent in nearDeath )
					{
						if ( ent is SandboxPlayer )
						{
							ent.TakeDamage( DamageInfo.Generic( 1f ) );
						}
					}
				}
			}
		}
	[Event( "server.tick" )]
	public void Simulate()
	{
		if(this.Scale <= 150 )
		{
			if ( this.Scale != BHScale )
			{
				this.Scale = BHScale;
			}
		}
		else
		{
			return;
		}
	}

	public override void Touch( Entity other )
	{
		var player = other as Player;
		//Log.Info( "Touched" );
		other.TakeDamage( DamageInfo.Generic(5f));
	}
	private void DeleteOthers()
	{
		// Only allow one of these to be spawned at a time
		foreach ( var ent in All.OfType<BlackHoleEnt>()
			.Where( x => x.IsValid() && x != this ) )
		{
			PullSpeed = 0;
			ent.Delete();
		}
	}
}
