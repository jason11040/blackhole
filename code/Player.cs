using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;

partial class SandboxPlayer : Player
{
	private TimeSince timeSinceDropped;
	private TimeSince timeSinceJumpReleased;
	private DamageInfo lastDamage;
	[Net, Predicted] public int GravityScale { get; set; }
	[Net, Predicted] public bool InAir { get; set; }
	[Net] public bool Stunned { get; set; } = false;
	public TimeSince stuncooldown;

	[Net]
	public int MaxHealth { get; set; }
	[Net]
	public int GravitySpeed { get; set; }

	/// <summary>
	/// The clothing container is what dresses the citizen
	/// </summary>
	public ClothingContainer Clothing = new();

	/// <summary>
	/// Default init
	/// </summary>
	public SandboxPlayer()
	{
		Inventory = new Inventory( this );
		Health = 100;
		MaxHealth = 100;
	}

	/// <summary>
	/// Initialize using this client
	/// </summary>
	public SandboxPlayer( Client cl ) : this()
	{
		// Load clothing from client data
		Clothing.LoadFromClient( cl );
	}

	public override void Respawn()
	{
		SetModel( "models/citizen/citizen.vmdl" );

		Controller = new WalkController();
		Animator = new StandardPlayerAnimator();
		ReceiveLoadout();
		if ( DevController is NoclipController )
		{
			DevController = null;
		}

		EnableAllCollisions = true;
		EnableDrawing = true;
		EnableHideInFirstPerson = true;
		EnableShadowInFirstPerson = true;

		Clothing.DressEntity( this );

		(Controller as WalkController).DefaultSpeed = 600;
		(Controller as WalkController).Gravity = 600;
		(Controller as WalkController).SprintSpeed = 600;
		(Controller as WalkController).AutoJump = true;
		Health = MaxHealth;

		CameraMode = new FirstPersonCamera();

		base.Respawn();
	}

	private void ReceiveLoadout()
	{
		var random = new Random();
		var weapons = TypeLibrary.GetTypes<Weapon>()
			.Where( weapon => !weapon.IsAbstract );
		int randweapons = random.Next( weapons.Count());
		foreach ( var weapon in weapons )
		{
			Inventory.Add( TypeLibrary.Create<Weapon>( weapon ) );
			if(weapon.ToString() == "Weapon" )
			{
				Inventory.DeleteContents();
			}
		}
	}

	public void Stun()
	{
		Stunned = true;
		stuncooldown = 0;
	}

	public override void BuildInput( InputBuilder input )
	{
		if ( Stunned )
		{
			input.StopProcessing = true;
			input.ClearButtons();
			input.Clear();
			
			//Log.Info( stuncooldown.ToString() );
		}

		base.BuildInput( input );
	}

	[Event.Tick]
	public void StunChecker()
	{
		if ( Stunned && stuncooldown >= 1f )
		{
			stuncooldown = 0;
			Stunned = false;
		}
	}

	public override void OnKilled()
	{
		base.OnKilled();

		if ( lastDamage.Flags.HasFlag( DamageFlags.Vehicle ) )
		{
			Particles.Create( "particles/impact.flesh.bloodpuff-big.vpcf", lastDamage.Position );
			Particles.Create( "particles/impact.flesh-big.vpcf", lastDamage.Position );
			PlaySound( "kersplat" );
		}

		BecomeRagdollOnClient( Velocity, lastDamage.Flags, lastDamage.Position, lastDamage.Force, GetHitboxBone( lastDamage.HitboxIndex ) );

		Controller = null;

		EnableAllCollisions = false;
		EnableDrawing = false;

		CameraMode = new SpectateRagdollCamera();

		foreach ( var child in Children )
		{
			child.EnableDrawing = false;
		}

		Inventory.DropActive();
		Inventory.DeleteContents();
	}

	public override void TakeDamage( DamageInfo info )
	{
		if ( GetHitboxGroup( info.HitboxIndex ) == 1 )
		{
			info.Damage *= 10.0f;
		}

		lastDamage = info;

		TookDamage( lastDamage.Flags, lastDamage.Position, lastDamage.Force );

		base.TakeDamage( info );
	}

	[ClientRpc]
	public void TookDamage( DamageFlags damageFlags, Vector3 forcePos, Vector3 force )
	{
	}

	public override PawnController GetActiveController()
	{
		if ( DevController != null ) return DevController;

		return base.GetActiveController();
	}

	public override void Simulate( Client cl )
	{
		base.Simulate( cl );

		if ( Input.ActiveChild != null )
		{
			ActiveChild = Input.ActiveChild;
		}

		if ( LifeState != LifeState.Alive )
			return;

		var controller = GetActiveController();
		if ( controller != null )
			EnableSolidCollisions = !controller.HasTag( "noclip" );

		//Log.Info( "Grounded: " + (GroundEntity != null) );
		if(GroundEntity == null )
		{
			InAir = false;
		}
		else
		{
			InAir = true;
		}

		TickPlayerUse();
		SimulateActiveChild( cl, ActiveChild );

		if ( Input.Pressed( InputButton.View ) )
		{
			if ( CameraMode is ThirdPersonCamera )
			{
				CameraMode = new FirstPersonCamera();
			}
			else
			{
				CameraMode = new ThirdPersonCamera();
			}
		}

		if ( Input.Pressed( InputButton.Drop ) )
		{
			var dropped = Inventory.DropActive();
			if ( dropped != null )
			{
				dropped.PhysicsGroup.ApplyImpulse( Velocity + EyeRotation.Forward * 500.0f + Vector3.Up * 100.0f, true );
				dropped.PhysicsGroup.ApplyAngularImpulse( Vector3.Random * 100.0f, true );

				timeSinceDropped = 0;
			}
		}

		if ( Input.Released( InputButton.Jump ) )
		{
			if ( timeSinceJumpReleased < 0.3f )
			{
				Game.Current?.DoPlayerNoclip( cl );
			}

			timeSinceJumpReleased = 0;
		}

		if ( Input.Left != 0 || Input.Forward != 0 )
		{
			timeSinceJumpReleased = 1;
		}
	}

	public override void StartTouch( Entity other )
	{
		if ( timeSinceDropped < 1 ) return;

		base.StartTouch( other );
	}

	[ConCmd.Server( "inventory_current" )]
	public static void SetInventoryCurrent( string entName )
	{
		var target = ConsoleSystem.Caller.Pawn as Player;
		if ( target == null ) return;

		var inventory = target.Inventory;
		if ( inventory == null )
			return;

		for ( int i = 0; i < inventory.Count(); ++i )
		{
			var slot = inventory.GetSlot( i );
			if ( !slot.IsValid() )
				continue;

			if ( slot.ClassName != entName )
				continue;

			inventory.SetActiveSlot( i, false );

			break;
		}
	}
	[ConCmd.Server]
	public static void Deleteinventory()
	{
		var player = ConsoleSystem.Caller.Pawn as SandboxPlayer;

		player?.Inventory.DeleteContents();
	}

}
