using UnityEngine;

namespace mygame
{
	public class Constants
	{
		//block types
		public static readonly int BT_PLATFORM = 1;
		public static readonly int BT_SPIKES = 2;
		public static readonly int BT_HOLE = 3;

		//groups of block types
		public static readonly int[] DEATHLY_BLOCKS = { BT_HOLE, BT_SPIKES };
		public static readonly int[] PLATFORM_BLOCKS = { BT_PLATFORM, BT_SPIKES };

		public static readonly float BLOCK_WIDTH = 1.0f;//0.5F;
		public static readonly float BLOCK_WIDTH_HALF = BLOCK_WIDTH / 2.0f;
		public static readonly float BLOCK_HEIGHT = 0.5f;
		public static readonly float BLOCK_HEIGHT_HALF = BLOCK_HEIGHT / 2.0f;

		//ball
		public static readonly float BALL_SIZE = 1.0f;
		public static readonly float BALL_SIZE_HALF = BALL_SIZE / 2.0f;

		//object (prefabs) types
		public static readonly int OT_PLATFORM = 1;
		public static readonly int OT_SPIKE = 2;
		public static readonly int OT_HOLE = 3;

		public static readonly int[] OBJECT_TYPES = { OT_PLATFORM, OT_SPIKE, OT_HOLE };

		//game params
		public const float SPEED_MIN = 2.0f;
		public const float SPEED_MAX = 8.0f;//units per second
		public const float DISTANCE_MAX = 200;//53 * 100 * 1;
		public const float JUMP_DURATION_SLOW = 0.5f;//seconds
		public const float JUMP_DURATION_FAST = 0.18f;
	}
}