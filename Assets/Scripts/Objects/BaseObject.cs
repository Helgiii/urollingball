using UnityEngine;
using UnityEngine.UI;

namespace mygame
{
	public class BaseObject : MonoBehaviour
	{
		//the aabb right side x
		public virtual float GetRightmostX()
		{
			return 0;
		}

		/*

		//the object is put to object pool
		public virtual void Deactivate()
		{

		}

		public virtual void Activate()
		{
			//reenable everything
		}
	*/

	}
}