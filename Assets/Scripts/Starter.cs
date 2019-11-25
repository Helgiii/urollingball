using UnityEngine;
using UnityEngine.UI;

namespace mygame
{

	public class Starter : MonoBehaviour
	{
		void Start()
		{
			UnityEngine.SceneManagement.SceneManager.LoadScene("Game");

			//GameObject.Find("CanvasHud").GetComponent<Ui>().HideAllScreens();
		}
	}
}