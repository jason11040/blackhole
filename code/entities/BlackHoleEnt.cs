using Sandbox;
using System;
using System.Linq;

[Library( "ent_blackholeent", Title = "Black Hole", Spawnable = true )]
public partial class BlackHoleEnt : ModelEntity
{
	public float PullRadius { get; set; } = 40000f;
	public float DeathRadius { get; set; } = 1100f;
	public float PullSpeed { get; set; }
	public int Gravity { get; set; }

	public override async void Spawn()
	{
		base.Spawn();
		DeleteOthers();
		SetModel( "models/ball/ball.vmdl" );
		SetupPhysicsFromModel( PhysicsMotionType.Static, false );
		Scale = 100f;
		RenderColor = Color.Black;
		while ( PullSpeed < 100f )
		{
			await GameTask.DelaySeconds( 1 );
			PullSpeed += 0.001f;
			Gravity = (int)(PullSpeed * 1000);
			DebugOverlay.ScreenText( "Gravity Speed: " + Gravity.ToString(), 1f );
		}
	}

	[Event.Tick]
	public void PullPlayer()
	{
		//Log.Info( PullSpeed );
		var nearEnts = Entity.FindInSphere( Position, PullRadius );
		{
			//DebugOverlay.Sphere( Position, PullRadius, Color.Red );
			foreach ( var ent in nearEnts )
			{
				if ( ent is Player )
				{
					ent.Position = Vector3.Lerp( ent.Position, Transform.Position, PullSpeed * Time.Delta );
				}
				if ( Gravity >= 50f )
				{
					Log.Info( "Pull harder" );
					ent.Position = Vector3.Lerp( ent.Position, Transform.Position, PullSpeed * Time.Delta * 2 );
				}
				/*if ( ent.GroundEntity == null )
				{
					Log.Info( "Grounded: " + (ent.GroundEntity != null) );
					ent.Position = Vector3.Lerp( ent.Position, Transform.Position, PullSpeed * Time.Delta * 2 );
				}*/
			}

			var nearDeath = Entity.FindInSphere( Position, DeathRadius );
			{
				//DebugOverlay.Sphere( Position, DeathRadius, Color.Blue );
				foreach ( var ent in nearDeath )
				{
					if ( ent is Player )
					{
						ent.TakeDamage( DamageInfo.Generic( 1f ) );
					}
				}
			}
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
