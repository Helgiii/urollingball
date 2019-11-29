using UnityEngine;

namespace mygame
{
	public class Constants
	{
		//block types
		public const int BT_PLATFORM = 1;
		public const int BT_SPIKES = 2;
		public const int BT_HOLE = 3;

		//groups of block types
		public static readonly int[] DEATHLY_BLOCKS = { BT_HOLE, BT_SPIKES };
		public static readonly int[] PLATFORM_BLOCKS = { BT_PLATFORM, BT_SPIKES };

		public const float BLOCK_WIDTH = 1.0f;//0.5F;
		public const float BLOCK_WIDTH_HALF = BLOCK_WIDTH / 2.0f;
		public const float BLOCK_HEIGHT = 0.5f;
		public const float BLOCK_HEIGHT_HALF = BLOCK_HEIGHT / 2.0f;

		//ball
		public const float BALL_SIZE = 1.0f;
		public const float BALL_SIZE_HALF = BALL_SIZE / 2.0f;

		//object (prefabs) types
		public static readonly System.Type OT_PLATFORM = typeof(Platform);
		public static readonly System.Type OT_SPIKE = typeof(Spike);
		public static readonly System.Type OT_HOLE = typeof(Hole);

		public static readonly System.Type[] OBJECT_TYPES = { OT_PLATFORM, OT_SPIKE, OT_HOLE };

		//game params
		public const float SPEED_MIN = 2.0f;
		public const float SPEED_MAX = 8.0f;//units per second
		public const float DISTANCE_MAX = 200;//53 * 100 * 1;
		public const float JUMP_DURATION_SLOW = 0.5f;//seconds
		public const float JUMP_DURATION_FAST = 0.18f;
	}
}