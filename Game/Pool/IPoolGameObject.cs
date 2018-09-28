using UnityEngine;

namespace Game.Pool
{
	public interface IPoolGameObject
	{
		SpawnPool spawnPool { get; }

		string id { get; }

		Transform transform { get; }

		bool isFromPool { get; }

		Vector3 ogiPosition { get; }

		Quaternion ogiDirection { get; }

		Vector3 ogiScale { get; }

		void Active( bool isFromPool );

		void Deactive();
	}
}