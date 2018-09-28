using UnityEngine;

namespace Game.Pool
{
	public sealed class PoolGameObject : IPoolGameObject
	{
		public SpawnPool spawnPool { get; private set; }

		public string id { get; private set; }

		public Transform transform { get; private set; }

		public bool isFromPool { get; private set; }

		public Vector3 ogiPosition { get; private set; }

		public Quaternion ogiDirection { get; private set; }

		public Vector3 ogiScale { get; private set; }

		internal PoolGameObject( SpawnPool spawnPool, Transform transform )
		{
			this.spawnPool = spawnPool;
			this.id = this.spawnPool.id;
			this.transform = transform;

			if ( this.transform != null )
			{
				this.ogiPosition = this.transform.localPosition;
				this.ogiDirection = this.transform.localRotation;
				this.ogiScale = this.transform.localScale;
			}
		}

		public void Active( bool isFromPool )
		{
			this.isFromPool = isFromPool;

		    //this.transform.localPosition = this.ogiPosition;
			//this.transform.localRotation = this.ogiDirection;
			//this.transform.localScale = this.ogiScale;
			this.transform?.gameObject.SetActive( true );
		}

		public void Deactive()
		{
			if ( this.transform == null )
				return;

			Animation[] anis = this.transform.GetComponentsInChildren<Animation>( true );
			foreach ( Animation ani in anis )
				ani.Stop();

			ParticleSystem[] particles = this.transform.GetComponentsInChildren<ParticleSystem>( true );
			foreach ( ParticleSystem particle in particles )
				particle.Stop( true );

			this.transform.gameObject.SetActive( false );
		}
	}
}