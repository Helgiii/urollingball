using UnityEngine;
using System.Collections.Generic;
using System;

namespace mygame
{
	public class Level : MonoBehaviour
	{
		protected int m_gameState = 0;

		protected float m_cameraHalfHeight;
		protected float m_cameraHalfWidth;

		//2 rows: top and bottom, current blocks we keep in memory
		protected List<int[]> m_blocks;
		protected int m_firstBlockIndex;//index of m_blocks[0] on the worldblocks axis

		protected float m_passedDistance = 0.0f;
		protected int m_scores;
		protected int m_jumpDangerScores;//width in world units


		//ball
		protected GameObject m_ball;
		protected float m_ballTopY;
		protected float m_ballBottomY;
		protected int m_ballState;
		protected float m_ballMotionTimer;

		protected float m_spikeWidth;


		//platforms
		protected float[] m_platYs = { 0, 0 };//top and bottom positions

		//pool of released game objects: objecttype, array
		protected Dictionary<System.Type, List<BaseObject>> m_pool;

		//active objects
		protected List<BaseObject> m_objects;
		protected Prefabs m_prefabs;

		//collision detection
		protected Vector2 m_castDir = new Vector2(1, 0);
		protected Vector2 m_castOrigin = new Vector2();


		//CONSTANTS

		//we could use enums here, but it is hard to create child classes and add new states with them
		public const int GSTATE_ON = 1;
		public const int GSTATE_FAILED = 2;
		public const int GSTATE_PAUSED = 3;
		public const int GSTATE_NONINIT = 4;
		//GSTATE_MAX = 5
		
		protected const int BALLSTATE_UP = 1;
		protected const int BALLSTATE_DOWN = 2;
		protected const int BALLSTATE_MOVING_UP = 3;
		protected const int BALLSTATE_MOVING_DOWN = 4;


		void Awake()
		{

			//Physics2D.alwaysShowColliders = true;

			//camera size in units
			Camera camera = Camera.main;
			m_cameraHalfHeight = camera.orthographicSize;
			m_cameraHalfWidth = camera.aspect * m_cameraHalfHeight;

			float paddingFactor = 0.7f;
			m_platYs[0] = m_cameraHalfHeight * paddingFactor - Constants.BLOCK_HEIGHT_HALF;
			m_platYs[1] = -m_cameraHalfHeight * paddingFactor + Constants.BLOCK_HEIGHT_HALF;

			m_ballTopY = m_platYs[0] - Constants.BLOCK_HEIGHT_HALF - Constants.BALL_SIZE_HALF;
			m_ballBottomY = m_platYs[1] + Constants.BLOCK_HEIGHT_HALF + Constants.BALL_SIZE_HALF;

			//test
			//m_ballTopY = 1.0f; m_ballBottomY = -1.0f;

			m_ball = GameObject.Find("Ball");
			m_objects = new List<BaseObject>();

			m_pool = new Dictionary<System.Type, List<BaseObject>>();
			int i;
			for (i = 0; i < Constants.OBJECT_TYPES.Length; ++i)
				m_pool[Constants.OBJECT_TYPES[i]] = new List<BaseObject>();

			m_prefabs = gameObject.GetComponent<Prefabs>();

			//add one spike and store its size
			BaseObject gobj = CreateNewObject(Constants.OT_SPIKE);
			m_spikeWidth = gobj.GetComponent<SpriteRenderer>().bounds.size.x;
			m_pool[Constants.OT_SPIKE].Add(gobj.GetComponent<BaseObject>());
			gobj.gameObject.SetActive(false);

		
			m_blocks = new List<int[]>();
			int cntBlocks = Mathf.CeilToInt((m_cameraHalfWidth * 2) / Constants.BLOCK_WIDTH) + Constants.OBJECT_MAX_BLOCKS*2;

			for (i=0; i<cntBlocks; ++i)
				m_blocks.Add( new int[2]{0,0} );
			
			ReinitLevel();
			SetState(GSTATE_NONINIT);
		}


		public void ReinitLevel()
		{
			int row, col;

			//release all objects from scene
			while (m_objects.Count > 0)
			{
				m_objects[0].gameObject.SetActive(false);
				m_pool[m_objects[0].GetType()].Add(m_objects[0]);
				m_objects.RemoveAt(0);
			}

			//clear all blocks
			for (row = 0; row < 2; ++row)
			for (col = 0; col < m_blocks.Count; ++col)
				m_blocks[col][row] = 0;

			m_firstBlockIndex = 0;
			m_scores = 0;
			m_passedDistance = 0;

			//move camera
			Camera cam = Camera.main;
			Quaternion rot = new Quaternion();
			cam.transform.SetPositionAndRotation(new Vector3(m_cameraHalfWidth, 0, -10), rot);


			TryGenerateBlocks();

			//add starting safe zone to first page
			for (row=0; row<2; ++row)
			for (col = 0; col < 9; ++col)
				m_blocks[col][row] = Constants.BT_PLATFORM;

			TryGenerateObjects(0);


			//ball
			m_ball.transform.position = new Vector3(GetBallXFromCamera(cam), m_ballBottomY, 0);
			m_ballState = BALLSTATE_DOWN;
			m_ballMotionTimer = 0.0f;
			m_ball.GetComponent<TrailRenderer>().Clear();//remove the trail 

			//TestDumpPage();
		}

		protected void OnHitDanger()
		{
			//game over
			//Debug.Log("hit something");
			//Debug.Log("distance passed: " + m_passedDistance.ToString());

			Globals globs = Globals.Instance;

			if (m_scores > globs.m_maxScores)
				globs.m_maxScores = m_scores;

			int dist = Mathf.FloorToInt(m_passedDistance);
			if (dist > globs.m_maxDistance)
				globs.m_maxDistance = dist;

			SetState(GSTATE_FAILED);
		}

		public void SetState(int state)
		{
			m_gameState = state;

			//pause anim (all updates)
			Time.timeScale = (state == GSTATE_ON) ? 1 : 0;

			GameObject.Find("CanvasHud").GetComponent<UserInterface>().ProcessNewGameState(m_gameState);
		}

		public int GetState() { return m_gameState; }
		public int GetScores() { return m_scores; }
		public float GetDistanceInt(){ return Mathf.FloorToInt(m_passedDistance); }



		protected void ProcessPassedBlocks()
		{
			int row, col;

			//if camera has moved, see if we need to add more blocks and generate objects
			int camLeftBlockI = Mathf.FloorToInt((Camera.main.transform.position.x - m_cameraHalfWidth) / Constants.BLOCK_WIDTH);

			//get blocks we have passed by
			int cnt = camLeftBlockI - m_firstBlockIndex;
			if (cnt <= 0)
				return;//camera has not moved yet

			if (cnt < m_blocks.Count)
			{
				for (row = 0; row < 2; ++row)
					for (col = 0; col < cnt; ++col)
						m_blocks[col][row] = 0;
				
				//move left part to the right
				m_blocks.InsertRange(m_blocks.Count, m_blocks.GetRange(0, cnt));
				m_blocks.RemoveRange(0, cnt);
			}
			else
			{
				for (row = 0; row < 2; ++row)
					for (col = 0; col < m_blocks.Count; ++col)
						m_blocks[col][row] = 0;
			}

			m_firstBlockIndex += cnt;
			//Debug.Log("first block index: " + m_firstBlockIndex.ToString());
		}

		protected int TryGenerateBlocks()
		{
			int col, blockType;

			//find first 0 block on top row
			int startIndice = -1;
			for (col = 0; col < m_blocks.Count; ++col)
			{
				if (m_blocks[col][0] == 0)
				{
					startIndice = col;
					break;
				}
			}

			if (startIndice == -1)
				return -1;

			Globals globs = Globals.Instance;
			
			//generate top blocks instead of empty ones
			int camRightBlockI = Mathf.CeilToInt((Camera.main.transform.position.x + m_cameraHalfWidth) / Constants.BLOCK_WIDTH);
			int rowDanger, rowSafe;
			col = startIndice;
			while (col < m_blocks.Count && m_firstBlockIndex + col <= camRightBlockI)
			{
				int blockCnt = globs.m_random.Next(2, Constants.OBJECT_MAX_BLOCKS+1);

				//decide where to put danger and safe block
				if (globs.m_random.NextDouble() > 0.5d)
				{
					rowDanger = 0;
					rowSafe = 1;
				}
				else
				{
					rowDanger = 1;
					rowSafe = 0;
				}

				//create blocks
				blockType = Constants.DEATHLY_BLOCKS[globs.m_random.Next(0, Constants.DEATHLY_BLOCKS.Length)];

				int max = Mathf.Min(col + blockCnt, m_blocks.Count);
				for (int k = col; k < max; ++k)
				{
					m_blocks[k][rowDanger] = blockType;
					m_blocks[k][rowSafe] = Constants.BT_PLATFORM;
				}
				col = max;
			}


			//TestDumpPage();
			//Debug.Assert(TestCheckBlocksPlayble());

			return startIndice;
		}


		protected void TryGenerateObjects(int startIndice)
		{
			if (startIndice == -1)
				return;

			//generate objects
			int row, col, blockType;
			int startIndex, startBlockType;

			//create platforms
			//we consider spikes blocks as platforms too, so we merge the nearby spikes and platforms
			//to get one large common platform
			for (row = 0; row < 2; ++row)
			{
				startIndex = -1;
				for (col = startIndice; col < m_blocks.Count; ++col)
				{
					blockType = m_blocks[col][row];

					int platIndex = Array.IndexOf<int>(Constants.PLATFORM_BLOCKS, blockType);
					if (startIndex == -1 && platIndex != -1)
						startIndex = col;

					if (startIndex >= 0)
					{
						if (platIndex == -1)
						{
							BlockSeqToObjects(Constants.BT_PLATFORM, col - startIndex, m_firstBlockIndex + startIndex, row);
							startIndex = -1;
						}
						else if (col == m_blocks.Count - 1)
						{
							BlockSeqToObjects(Constants.BT_PLATFORM, col - startIndex + 1, m_firstBlockIndex + startIndex, row);
							startIndex = -1;
						}
					}

					if (blockType == 0)
						break;
				}
			}

			//create hole colliders, spikes visuals, spike group collider
			for (row = 0; row < 2; ++row)
			{
				startIndex = -1;
				startBlockType = 0;
				for (col = startIndice; col < m_blocks.Count; ++col)
				{
					blockType = m_blocks[col][row];
					if (startIndex == -1 && blockType>0 && blockType != Constants.BT_PLATFORM)
					{
						startIndex = col;
						startBlockType = blockType;
					}

					if (startIndex >= 0)
					{
						//block type we have met is different
						if (blockType != startBlockType)
						{
							//add object
							BlockSeqToObjects(startBlockType, col - startIndex, m_firstBlockIndex + startIndex, row);

							//and start a new object, if it is not a platform
							if (blockType != Constants.BT_PLATFORM && blockType > 0)
							{
								startBlockType = blockType;
								startIndex = col;
							}
							else startIndex = -1;
						}

						if (startIndex >= 0 && col == m_blocks.Count - 1)
							BlockSeqToObjects(startBlockType, col - startIndex + 1, m_firstBlockIndex + startIndex, row);
					}

					if (blockType == 0)
						break;
				}
			}
		}


		//create gameobjects from a sequence of blocks
		protected void BlockSeqToObjects(int blockType, int lenInBlocks, int startBlockIndex, int row)
		{
			float xx = startBlockIndex * Constants.BLOCK_WIDTH;
			float yy = m_platYs[row];

			if (blockType == Constants.BT_PLATFORM)
			{
				BaseObject bobj = GetFreeObject(Constants.OT_PLATFORM);

				float objWidth = lenInBlocks * Constants.BLOCK_WIDTH;
				bobj.gameObject.transform.localScale = new Vector2(objWidth, Constants.BLOCK_HEIGHT);
				bobj.gameObject.transform.position = new Vector2(xx + objWidth / 2, yy);

				m_objects.Add(bobj);
			}
			else if (blockType == Constants.BT_HOLE)
			{
				BaseObject bobj = GetFreeObject(Constants.OT_HOLE);

				float blGroupWidth = lenInBlocks * Constants.BLOCK_WIDTH;
				float objWidth = blGroupWidth - Constants.BLOCK_WIDTH / 2.5f * 2;

				bobj.gameObject.transform.localScale = new Vector2(objWidth, Constants.BLOCK_HEIGHT + 0.1f);
				bobj.gameObject.transform.position = new Vector2(xx + blGroupWidth / 2, yy);

				m_objects.Add(bobj);
			}
			else if (blockType == Constants.BT_SPIKES)
			{
				//see how many spikes we need to place
				float blGroupWidth = lenInBlocks * Constants.BLOCK_WIDTH;
				int cnt = Mathf.FloorToInt(blGroupWidth / m_spikeWidth);
				float spikesPadding = (blGroupWidth - cnt * m_spikeWidth) / 2.0f;

				//create spikess
				int i;
				for (i = 0; i < cnt; ++i)
				{
					BaseObject bobj = GetFreeObject(Constants.OT_SPIKE);
					Vector3 scale = bobj.gameObject.transform.localScale;

					if (row == 0)
					{
						if (scale.y < 0) scale.y *= -1;
						yy = m_platYs[row] - Constants.BLOCK_HEIGHT_HALF;
					}
					else if (row == 1)
					{
						if (scale.y > 0) scale.y *= -1;
						yy = m_platYs[row] + Constants.BLOCK_HEIGHT_HALF;
					}
					bobj.gameObject.transform.localScale = scale;
					
					Vector2 newPos = new Vector2(xx + spikesPadding + i * m_spikeWidth + m_spikeWidth / 2.0f, yy);
					bobj.gameObject.transform.position = newPos;

					m_objects.Add(bobj);
				}
			}
		}

		protected BaseObject GetFreeObject(System.Type objType, bool bAutoSetActive = true)
		{
			BaseObject bobj = null;

			if (m_pool[objType].Count > 0)
			{
				//get the first existing object from pool, reinit this obj outside
				bobj = m_pool[objType][0];
				m_pool[objType].RemoveAt(0);
				bobj.gameObject.SetActive(bAutoSetActive);
				return bobj;
			}

			//create a new obj since we dont have enough free ones
			bobj = CreateNewObject(objType);
			bobj.gameObject.SetActive(bAutoSetActive);//
			return bobj;
		}

		//can overload this in subclasses
		protected BaseObject CreateNewObject(System.Type objType)
		{
			Transform tr = null;

			if (objType == Constants.OT_PLATFORM)
				tr = Instantiate(m_prefabs.m_objPlatform);

			else if (objType == Constants.OT_HOLE)
				tr = Instantiate(m_prefabs.m_objHole);

			else if (objType == Constants.OT_SPIKE)
				tr = Instantiate(m_prefabs.m_objSpike);

			else
				return null;

			tr.parent = GameObject.Find("Objects").transform;
			return tr.gameObject.GetComponent<BaseObject>();
		}


		protected void Update()
		{
			if (m_gameState != GSTATE_ON)
				return;


			//calc current motion speed based on distance passed
			float distanceProgress = m_passedDistance / Constants.DISTANCE_MAX;
			if (distanceProgress > 1)
				distanceProgress = 1.0f;
			float speed = Constants.SPEED_MIN + (Constants.SPEED_MAX - Constants.SPEED_MIN) * distanceProgress;
			float jumpDuration = Constants.JUMP_DURATION_SLOW - (Constants.JUMP_DURATION_SLOW - Constants.JUMP_DURATION_FAST) * distanceProgress;

			//test
			//speed = Constants.SPEED_MAX;
			//jumpDuration = Constants.JUMP_DURATION_FAST;

			float dt = Time.deltaTime;
			float offset = dt * speed;

			//check if we have hit some object while moving camera 
			m_castOrigin.x = m_ball.transform.position.x;
			m_castOrigin.y = m_ball.transform.position.y;
			RaycastHit2D hit = Physics2D.CircleCast(m_castOrigin, Constants.BALL_SIZE_HALF, m_castDir, offset);
			if (hit.collider != null)
			{
				//tune/fix position of the ball so that we dont penetrate the object visually
				Debug.Log("hit object: " + hit.collider.attachedRigidbody.gameObject.name);

				OnHitDanger();
				return;
			}


			//move camera (and the ball) to new pos
			Camera camera = Camera.main;
			camera.transform.Translate(offset, 0, 0);

			ProcessPassedBlocks();

			int startIndice = TryGenerateBlocks();
			TryGenerateObjects(startIndice);

			//set new ball position relative to cameraview
			Vector3 pos = m_ball.transform.position;
			pos.x = GetBallXFromCamera(camera);

			//calc new ball y
			if (m_ballState == BALLSTATE_MOVING_DOWN)
			{
				m_ballMotionTimer += dt;
				if (m_ballMotionTimer >= jumpDuration)
				{
					m_jumpDangerScores += CalcDangerScores(CalcBallBlockIndex(), 1);
					AddScores(m_jumpDangerScores);

					pos.y = m_ballBottomY;
					m_ballMotionTimer = 0.0f;
					m_ballState = BALLSTATE_DOWN;
				}
				else
					pos.y = m_ballTopY + (m_ballBottomY - m_ballTopY) * (m_ballMotionTimer / jumpDuration);
			}
			else if (m_ballState == BALLSTATE_MOVING_UP)
			{
				m_ballMotionTimer += dt;
				if (m_ballMotionTimer >= jumpDuration)
				{
					m_jumpDangerScores += CalcDangerScores(CalcBallBlockIndex(), 0);
					AddScores(m_jumpDangerScores);

					pos.y = m_ballTopY;
					m_ballMotionTimer = 0.0f;
					m_ballState = BALLSTATE_UP;
				}
				else
					pos.y = m_ballBottomY + (m_ballTopY - m_ballBottomY) * (m_ballMotionTimer / jumpDuration);
			}
			m_ball.transform.position = pos;

			m_passedDistance += offset;


			//check for objects that are left behind and will no longer be seen
			BaseObject bobj;
			int i = 0;
			float camleftx = camera.transform.position.x - m_cameraHalfWidth;
			while (i < m_objects.Count)
			{
				bobj = m_objects[i];

				if (bobj.GetRightmostX() < camleftx)
				{
					bobj.gameObject.SetActive(false);
					m_pool[bobj.GetType()].Add(bobj);
					m_objects.RemoveAt(i);
					continue;
				}

				++i;
			}

			UpdateHud();
		}

		public void TryJump()
		{
			if (m_gameState != GSTATE_ON)
				return;

			if (m_ballState == BALLSTATE_DOWN)
			{
				m_jumpDangerScores = CalcDangerScores(CalcBallBlockIndex(), 1);
				m_ballState = BALLSTATE_MOVING_UP;
			}
			else if (m_ballState == BALLSTATE_UP)
			{
				m_jumpDangerScores = CalcDangerScores(CalcBallBlockIndex(), 0);
				m_ballState = BALLSTATE_MOVING_DOWN;
			}
		}

		//this could be bigger than 1 page, since we have 2 pages and are moving fast
		int CalcBallBlockIndex()
		{
			int blockIndex = Mathf.FloorToInt(m_ball.transform.position.x / Constants.BLOCK_WIDTH) - m_firstBlockIndex;

			//Debug.Log(blockIndex.ToString());
			return blockIndex;
		}

		//if no danger found returns blockcnt to the end of the 2pages
		int CalcDangerScores(int startingIndex, int row)
		{
			for (int col = startingIndex; col < m_blocks.Count; ++col)
			{
				int blockType = m_blocks[col][row];
				int index = Array.IndexOf<int>(Constants.DEATHLY_BLOCKS, blockType);
				if (index >=0)
				{
					int dist = col - startingIndex;
					if (dist > 5)
						return 0;
					return 6 - dist;
				}
			}
			return 0;
		}

		void UpdateHud()
		{
			GameObject.Find("CanvasHud").GetComponent<UserInterface>().UpdateHud();
		}

		float GetBallXFromCamera(Camera camera)
		{
			return camera.transform.position.x - m_cameraHalfWidth + m_cameraHalfWidth / 3;
		}

		void AddScores(int scores)
		{
			if (scores == 0)
				return;

			//Debug.Log("scores added: " + scores.ToString());
			m_scores += scores;

			UpdateHud();
		}

		
		protected void TestDumpPage()
		{
			int row, col;
			for (row = 0; row < 2; ++row)
			{
				string txt = "row" + row.ToString() + " [";
				for (col = 0; col < m_blocks.Count; ++col)
				{
					txt += m_blocks[col][row].ToString();
					if (col != m_blocks.Count - 1)
						txt += ",";
				}
				txt += "]";
				Debug.Log(txt);
			}
		}


		/*
		protected bool TestCheckBlocksPlayble()
		{
			int col;
			for (col=0; col<m_blocks.Count; ++col)
			{
				if (ArrayUtility.Contains(Constants.DEATHLY_BLOCKS, m_blocks[col][0])
					&& ArrayUtility.Contains(Constants.DEATHLY_BLOCKS, m_blocks[col][1]))
				{
					return false;
				}
			}
			return true;
		}
		*/
	}
}