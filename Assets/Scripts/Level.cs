using UnityEngine;
using System.Collections.Generic;
using System;

namespace mygame
{
	public class Level : MonoBehaviour
	{
		GameState _gameState = GameState.NonInit;
		public GameState gameState
		{
			get { return _gameState; }
			set
			{
				_gameState = value;

				// Pause anim (all updates)
				Time.timeScale = (_gameState == GameState.On) ? 1 : 0;

				GameObject.Find("CanvasHud").GetComponent<UserInterface>().ProcessNewGameState(_gameState);
			}
		}


		protected float cameraHalfHeight;
		protected float cameraHalfWidth;

		// 2 rows: top and bottom, current blocks we keep in memory
		protected List<BlockType[]> blocks;

		// Index of blocks[0] on the worldblocks axis
		protected int firstBlockIndex;

		protected float passedDistance = 0.0f;

		public int scores { get; private set; } = 0;

		// Width in world units
		protected int jumpDangerScores;


		// Ball
		protected GameObject ball;
		protected float ballTopY;
		protected float ballBottomY;
		protected BallState ballState;
		protected float ballMotionTimer;

		protected float spikeWidth;


		// Platforms
		// Top and bottom positions
		protected float[] platYs = { 0, 0 };

		// Pool of released game objects: objecttype, array
		protected Dictionary<System.Type, List<BaseObject>> pool;

		// Active objects
		protected List<BaseObject> objects;
		protected Prefabs prefabs;

		// Collision detection
		protected Vector2 castDir = new Vector2(1, 0);
		protected Vector2 castOrigin = new Vector2();


		// CONSTANTS

		public enum GameState
		{
			On = 1,
			Failed,
			Paused,
			NonInit
		}
		
		public enum BallState
		{
			Up = 1,
			Down,
			MovingUp,
			MovingDown
		}

		void Awake()
		{
			//Physics2D.alwaysShowColliders = true;

			// Camera size in units
			Camera camera = Camera.main;
			cameraHalfHeight = camera.orthographicSize;
			cameraHalfWidth = camera.aspect * cameraHalfHeight;

			var paddingFactor = 0.7f;
			platYs[0] = cameraHalfHeight * paddingFactor - Constants.BLOCK_HEIGHT_HALF;
			platYs[1] = -cameraHalfHeight * paddingFactor + Constants.BLOCK_HEIGHT_HALF;

			ballTopY = platYs[0] - Constants.BLOCK_HEIGHT_HALF - Constants.BALL_SIZE_HALF;
			ballBottomY = platYs[1] + Constants.BLOCK_HEIGHT_HALF + Constants.BALL_SIZE_HALF;

			// Test
			//ballTopY = 1.0f; ballBottomY = -1.0f;

			ball = GameObject.Find("Ball");
			objects = new List<BaseObject>();

			pool = new Dictionary<System.Type, List<BaseObject>>();
			
			for (var i = 0; i < Constants.OBJECT_TYPES.Length; ++i)
				pool[Constants.OBJECT_TYPES[i]] = new List<BaseObject>();

			prefabs = gameObject.GetComponent<Prefabs>();

			// Add one spike and store its size
			BaseObject gobj = CreateNewObject(Constants.OT_SPIKE);
			spikeWidth = gobj.GetComponent<SpriteRenderer>().bounds.size.x;
			pool[Constants.OT_SPIKE].Add(gobj.GetComponent<BaseObject>());
			gobj.gameObject.SetActive(false);

		
			blocks = new List<BlockType[]>();
			int cntBlocks = Mathf.CeilToInt((cameraHalfWidth * 2) / Constants.BLOCK_WIDTH) + Constants.OBJECT_MAX_BLOCKS*2;

			for (var i=0; i<cntBlocks; ++i)
				blocks.Add( new BlockType[2]{0,0} );
			
			ReinitLevel();
			gameState = GameState.NonInit;
		}


		public void ReinitLevel()
		{
			// Release all objects from scene
			while (objects.Count > 0)
			{
				objects[0].gameObject.SetActive(false);
				pool[objects[0].GetType()].Add(objects[0]);
				objects.RemoveAt(0);
			}

			// Clear all blocks
			for (var row = 0; row < 2; ++row)
			for (var col = 0; col < blocks.Count; ++col)
				blocks[col][row] = 0;

			firstBlockIndex = 0;
			scores = 0;
			passedDistance = 0;

			// Move camera
			Camera cam = Camera.main;
			var rot = new Quaternion();
			cam.transform.SetPositionAndRotation(new Vector3(cameraHalfWidth, 0, -10), rot);


			TryGenerateBlocks();

			// Add starting safe zone to first page
			for (var row=0; row<2; ++row)
			for (var col = 0; col < 9; ++col)
				blocks[col][row] = BlockType.Platform;

			TryGenerateObjects(0);


			// Ball
			ball.transform.position = new Vector3(GetBallXFromCamera(cam), ballBottomY, 0);
			ballState = BallState.Down;
			ballMotionTimer = 0.0f;
			ball.GetComponent<TrailRenderer>().Clear(); 

			//TestDumpPage();
		}

		protected void OnHitDanger()
		{
			// Game over
			//Debug.Log("hit something");
			//Debug.Log("distance passed: " + passedDistance.ToString());

			Globals globs = Globals.Instance;

			if (scores > globs.maxScores)
				globs.maxScores = scores;

			int dist = Mathf.FloorToInt(passedDistance);
			if (dist > globs.maxDistance)
				globs.maxDistance = dist;

			gameState = GameState.Failed;
		}


		public float GetDistanceInt(){ return Mathf.FloorToInt(passedDistance); }



		protected void ProcessPassedBlocks()
		{
			// If camera has moved, see if we need to add more blocks and generate objects
			int camLeftBlockI = Mathf.FloorToInt((Camera.main.transform.position.x - cameraHalfWidth) / Constants.BLOCK_WIDTH);

			// Get blocks we have passed by
			int cnt = camLeftBlockI - firstBlockIndex;
			if (cnt <= 0)
				return;

			if (cnt < blocks.Count)
			{
				for (var row = 0; row < 2; ++row)
					for (var col = 0; col < cnt; ++col)
						blocks[col][row] = 0;
				
				// Move left part to the right
				blocks.InsertRange(blocks.Count, blocks.GetRange(0, cnt));
				blocks.RemoveRange(0, cnt);
			}
			else
			{
				for (var row = 0; row < 2; ++row)
					for (var col = 0; col < blocks.Count; ++col)
						blocks[col][row] = 0;
			}

			firstBlockIndex += cnt;
			//Debug.Log("first block index: " + firstBlockIndex.ToString());
		}

		protected int TryGenerateBlocks()
		{
			var blockType = BlockType.None;
			int col;

			// Find first 0 block on top row
			int startIndice = -1;
			for (col = 0; col < blocks.Count; ++col)
			{
				if (blocks[col][0] == BlockType.None)
				{
					startIndice = col;
					break;
				}
			}

			if (startIndice == -1)
				return -1;

			Globals globs = Globals.Instance;
			
			// Generate top blocks instead of empty ones
			int camRightBlockI = Mathf.CeilToInt((Camera.main.transform.position.x + cameraHalfWidth) / Constants.BLOCK_WIDTH);
			int rowDanger, rowSafe;
			col = startIndice;
			while (col < blocks.Count && firstBlockIndex + col <= camRightBlockI)
			{
				int blockCnt = globs.random.Next(2, Constants.OBJECT_MAX_BLOCKS+1);

				// Decide where to put danger and safe block
				if (globs.random.NextDouble() > 0.5d)
				{
					rowDanger = 0;
					rowSafe = 1;
				}
				else
				{
					rowDanger = 1;
					rowSafe = 0;
				}

				// Create blocks
				blockType = Constants.DEATHLY_BLOCKS[globs.random.Next(0, Constants.DEATHLY_BLOCKS.Length)];

				int max = Mathf.Min(col + blockCnt, blocks.Count);
				for (var k = col; k < max; ++k)
				{
					blocks[k][rowDanger] = blockType;
					blocks[k][rowSafe] = BlockType.Platform;
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

			// Generate objects
			BlockType blockType;
			int startIndex;
			BlockType startBlockType;

			// Create platforms
			// We consider spikes blocks as platforms too, so we merge the nearby spikes and platforms
			// To get one large common platform
			for (var row = 0; row < 2; ++row)
			{
				startIndex = -1;
				for (var col = startIndice; col < blocks.Count; ++col)
				{
					blockType = blocks[col][row];

					int platIndex = Array.IndexOf<BlockType>(Constants.PLATFORM_BLOCKS, blockType);
					if (startIndex == -1 && platIndex != -1)
						startIndex = col;

					if (startIndex >= 0)
					{
						if (platIndex == -1)
						{
							BlockSeqToObjects(BlockType.Platform, col - startIndex, firstBlockIndex + startIndex, row);
							startIndex = -1;
						}
						else if (col == blocks.Count - 1)
						{
							BlockSeqToObjects(BlockType.Platform, col - startIndex + 1, firstBlockIndex + startIndex, row);
							startIndex = -1;
						}
					}

					if (blockType == BlockType.None)
						break;
				}
			}

			// Create hole colliders, spikes visuals, spike group collider
			for (var row = 0; row < 2; ++row)
			{
				startIndex = -1;
				startBlockType = 0;
				for (var col = startIndice; col < blocks.Count; ++col)
				{
					blockType = blocks[col][row];
					if (startIndex == -1 && blockType>0 && blockType != BlockType.Platform)
					{
						startIndex = col;
						startBlockType = blockType;
					}

					if (startIndex >= 0)
					{
						// Block type we have met is different
						if (blockType != startBlockType)
						{
							// Add object
							BlockSeqToObjects(startBlockType, col - startIndex, firstBlockIndex + startIndex, row);

							// And start a new object, if it is not a platform
							if (blockType != BlockType.Platform && blockType > 0)
							{
								startBlockType = blockType;
								startIndex = col;
							}
							else startIndex = -1;
						}

						if (startIndex >= 0 && col == blocks.Count - 1)
							BlockSeqToObjects(startBlockType, col - startIndex + 1, firstBlockIndex + startIndex, row);
					}

					if (blockType == BlockType.None)
						break;
				}
			}
		}


		// Create gameobjects from a sequence of blocks
		protected void BlockSeqToObjects(BlockType blockType, int lenInBlocks, int startBlockIndex, int row)
		{
			float xx = startBlockIndex * Constants.BLOCK_WIDTH;
			float yy = platYs[row];

			if (blockType == BlockType.Platform)
			{
				BaseObject bobj = GetFreeObject(Constants.OT_PLATFORM);

				float objWidth = lenInBlocks * Constants.BLOCK_WIDTH;
				bobj.gameObject.transform.localScale = new Vector2(objWidth, Constants.BLOCK_HEIGHT);
				bobj.gameObject.transform.position = new Vector2(xx + objWidth / 2, yy);

				objects.Add(bobj);
			}
			else if (blockType == BlockType.Hole)
			{
				BaseObject bobj = GetFreeObject(Constants.OT_HOLE);

				float blGroupWidth = lenInBlocks * Constants.BLOCK_WIDTH;
				float objWidth = blGroupWidth - Constants.BLOCK_WIDTH / 2.5f * 2;

				bobj.gameObject.transform.localScale = new Vector2(objWidth, Constants.BLOCK_HEIGHT + 0.1f);
				bobj.gameObject.transform.position = new Vector2(xx + blGroupWidth / 2, yy);

				objects.Add(bobj);
			}
			else if (blockType == BlockType.Spikes)
			{
				// See how many spikes we need to place
				float blGroupWidth = lenInBlocks * Constants.BLOCK_WIDTH;
				int cnt = Mathf.FloorToInt(blGroupWidth / spikeWidth);
				float spikesPadding = (blGroupWidth - cnt * spikeWidth) / 2.0f;

				// Create spikess
				for (var i = 0; i < cnt; ++i)
				{
					BaseObject bobj = GetFreeObject(Constants.OT_SPIKE);
					Vector3 scale = bobj.gameObject.transform.localScale;

					if (row == 0)
					{
						if (scale.y < 0) scale.y *= -1;
						yy = platYs[row] - Constants.BLOCK_HEIGHT_HALF;
					}
					else if (row == 1)
					{
						if (scale.y > 0) scale.y *= -1;
						yy = platYs[row] + Constants.BLOCK_HEIGHT_HALF;
					}
					bobj.gameObject.transform.localScale = scale;
					
					bobj.gameObject.transform.position = new Vector2(xx + spikesPadding + i * spikeWidth + spikeWidth / 2.0f, yy);

					objects.Add(bobj);
				}
			}
		}

		protected BaseObject GetFreeObject(System.Type objType, bool bAutoSetActive = true)
		{
			BaseObject bobj = null;

			if (pool[objType].Count > 0)
			{
				// Get the first existing object from pool, reinit this obj outside
				bobj = pool[objType][0];
				pool[objType].RemoveAt(0);
				bobj.gameObject.SetActive(bAutoSetActive);
				return bobj;
			}

			// Create a new obj since we dont have enough free ones
			bobj = CreateNewObject(objType);
			bobj.gameObject.SetActive(bAutoSetActive);
			return bobj;
		}

		// Can overload this in subclasses
		protected BaseObject CreateNewObject(System.Type objType)
		{
			Transform tr = null;

			if (objType == Constants.OT_PLATFORM)
				tr = Instantiate(prefabs.objPlatform);

			else if (objType == Constants.OT_HOLE)
				tr = Instantiate(prefabs.objHole);

			else if (objType == Constants.OT_SPIKE)
				tr = Instantiate(prefabs.objSpike);

			else
				return null;

			tr.parent = GameObject.Find("Objects").transform;
			return tr.gameObject.GetComponent<BaseObject>();
		}


		protected void Update()
		{
			if (gameState != GameState.On)
				return;


			// Calc current motion speed based on distance passed
			float distanceProgress = passedDistance / Constants.DISTANCE_MAX;
			if (distanceProgress > 1)
				distanceProgress = 1.0f;
			float speed = Constants.SPEED_MIN + (Constants.SPEED_MAX - Constants.SPEED_MIN) * distanceProgress;
			float jumpDuration = Constants.JUMP_DURATION_SLOW - (Constants.JUMP_DURATION_SLOW - Constants.JUMP_DURATION_FAST) * distanceProgress;

			// Test
			//speed = Constants.SPEED_MAX;
			//jumpDuration = Constants.JUMP_DURATION_FAST;

			float dt = Time.deltaTime;
			float offset = dt * speed;

			// Check if we have hit some object while moving camera 
			castOrigin.x = ball.transform.position.x;
			castOrigin.y = ball.transform.position.y;
			RaycastHit2D hit = Physics2D.CircleCast(castOrigin, Constants.BALL_SIZE_HALF, castDir, offset);
			if (hit.collider != null)
			{
				// Tune/fix position of the ball so that we dont penetrate the object visually
				Debug.Log("hit object: " + hit.collider.attachedRigidbody.gameObject.name);

				OnHitDanger();
				return;
			}


			// Move camera (and the ball) to new pos
			Camera camera = Camera.main;
			camera.transform.Translate(offset, 0, 0);

			ProcessPassedBlocks();

			int startIndice = TryGenerateBlocks();
			TryGenerateObjects(startIndice);

			// Set new ball position relative to cameraview
			Vector3 pos = ball.transform.position;
			pos.x = GetBallXFromCamera(camera);

			// Calc new ball y
			if (ballState == BallState.MovingDown)
			{
				ballMotionTimer += dt;
				if (ballMotionTimer >= jumpDuration)
				{
					jumpDangerScores += CalcDangerScores(CalcBallBlockIndex(), 1);
					AddScores(jumpDangerScores);

					pos.y = ballBottomY;
					ballMotionTimer = 0.0f;
					ballState = BallState.Down;
				}
				else
					pos.y = ballTopY + (ballBottomY - ballTopY) * (ballMotionTimer / jumpDuration);
			}
			else if (ballState == BallState.MovingUp)
			{
				ballMotionTimer += dt;
				if (ballMotionTimer >= jumpDuration)
				{
					jumpDangerScores += CalcDangerScores(CalcBallBlockIndex(), 0);
					AddScores(jumpDangerScores);

					pos.y = ballTopY;
					ballMotionTimer = 0.0f;
					ballState = BallState.Up;
				}
				else
					pos.y = ballBottomY + (ballTopY - ballBottomY) * (ballMotionTimer / jumpDuration);
			}
			ball.transform.position = pos;

			passedDistance += offset;


			// Check for objects that are left behind and will no longer be seen
			BaseObject bobj;
			float camleftx = camera.transform.position.x - cameraHalfWidth;
			var i = 0;
			while (i < objects.Count)
			{
				bobj = objects[i];

				if (bobj.GetRightmostX() < camleftx)
				{
					bobj.gameObject.SetActive(false);
					pool[bobj.GetType()].Add(bobj);
					objects.RemoveAt(i);
					continue;
				}

				++i;
			}

			UpdateHud();
		}

		public void TryJump()
		{
			if (gameState != GameState.On)
				return;

			if (ballState == BallState.Down)
			{
				jumpDangerScores = CalcDangerScores(CalcBallBlockIndex(), 1);
				ballState = BallState.MovingUp;
			}
			else if (ballState == BallState.Up)
			{
				jumpDangerScores = CalcDangerScores(CalcBallBlockIndex(), 0);
				ballState = BallState.MovingDown;
			}
		}

		// This could be bigger than 1 page, since we have 2 pages and are moving fast
		int CalcBallBlockIndex()
		{
			int blockIndex = Mathf.FloorToInt(ball.transform.position.x / Constants.BLOCK_WIDTH) - firstBlockIndex;

			//Debug.Log(blockIndex.ToString());
			return blockIndex;
		}

		// If no danger found returns blockcnt to the end of the 2pages
		int CalcDangerScores(int startingIndex, int row)
		{
			for (var col = startingIndex; col < blocks.Count; ++col)
			{
				BlockType blockType = blocks[col][row];
				int index = Array.IndexOf<BlockType>(Constants.DEATHLY_BLOCKS, blockType);
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
			return camera.transform.position.x - cameraHalfWidth + cameraHalfWidth / 3;
		}

		void AddScores(int scores)
		{
			if (scores == 0)
				return;

			//Debug.Log("scores added: " + scores.ToString());
			this.scores += scores;

			UpdateHud();
		}

		/*
		protected void TestDumpPage()
		{
			for (var row = 0; row < 2; ++row)
			{
				string txt = "row" + row.ToString() + " [";
				for (var col = 0; col < blocks.Count; ++col)
				{
					txt += blocks[col][row].ToString();
					if (col != blocks.Count - 1)
						txt += ",";
				}
				txt += "]";
				Debug.Log(txt);
			}
		}
		*/

		/*
		protected bool TestCheckBlocksPlayble()
		{
			for (var col=0; col<blocks.Count; ++col)
			{
				if (Array.IndexOf<int>(Constants.DEATHLY_BLOCKS, blocks[col][0]) != -1
					&& Array.IndexOf<int>(Constants.DEATHLY_BLOCKS, blocks[col][1]) != -1)
				{
					return false;
				}
			}
			return true;
		}
		*/
	}
}