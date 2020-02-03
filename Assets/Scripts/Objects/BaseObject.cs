using UnityEngine;
using UnityEngine.UI;

namespace mygame
{
	public class BaseObject : MonoBehaviour
	{
		// The aabb right side x
		public virtual float GetRightmostX()
		{
			return 0;
		}
	}
}